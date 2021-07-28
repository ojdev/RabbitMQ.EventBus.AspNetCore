using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.EventBus.AspNetCore.Configurations;
using System;
using System.IO;
using System.Net.Sockets;

namespace RabbitMQ.EventBus.AspNetCore.Factories
{
    internal sealed class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        public RabbitMQEventBusConnectionConfiguration Configuration { get; }
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<DefaultRabbitMQPersistentConnection> _logger;
        private IConnection _connection;
        Func<string> _connectionAction;
        private bool _disposed;
        private readonly object sync_root = new object();
        private IModel _model;
        public string Endpoint => _connection?.Endpoint.ToString();
        public DefaultRabbitMQPersistentConnection(RabbitMQEventBusConnectionConfiguration configuration, Func<string> connectionAction, ILogger<DefaultRabbitMQPersistentConnection> logger)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _connectionAction = connectionAction ?? throw new ArgumentNullException(nameof(connectionAction));
            _connectionFactory = new ConnectionFactory
            {
                AutomaticRecoveryEnabled = configuration.AutomaticRecoveryEnabled,
                NetworkRecoveryInterval = configuration.NetworkRecoveryInterval
            };
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }
            if ((_model?.IsOpen ?? false) == false)
            {
                _model = _connection.CreateModel();
                Console.WriteLine("创建一个Channel");
            }
            return _model;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.LogCritical(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            _logger.WriteLog(LogLevel.Information, "RabbitMQ Client is trying to connect");
            lock (sync_root)
            {
                RetryPolicy policy = RetryPolicy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(Configuration.FailReConnectRetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.LogWarning(ex.ToString());
                    }
                );

                policy.Execute(() =>
                {
                    string connectionString = _connectionAction.Invoke();
                    _logger.WriteLog(LogLevel.Information, $"[ConnectionString]:\t{connectionString}");
                    _connectionFactory.Uri = new Uri(connectionString);
                    _connection = _connectionFactory.CreateConnection(clientProvidedName: Configuration.ClientProvidedName);
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;
                    _logger.WriteLog(LogLevel.Information, $"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                    return true;
                }
                else
                {
                    _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
                    return false;
                }
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed)
            {
                return;
            }
            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");
            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed)
            {
                return;
            }
            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed)
            {
                return;
            }
            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
            TryConnect();
        }
    }
}
