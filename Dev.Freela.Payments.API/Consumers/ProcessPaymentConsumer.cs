using Dev.Freela.Payments.API.Models;
using Dev.Freela.Payments.API.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Dev.Freela.Payments.API.Consumers
{
    public class ProcessPaymentConsumer : BackgroundService
    {
        private const string QueueName = "Payments";
        private const string PaymentApprovedQueuName = "PaymentsApproved";
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;

        public ProcessPaymentConsumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                        queue: PaymentApprovedQueuName,
                        durable: false,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (sender, eventArgs) =>
            {
                var byteArray = eventArgs.Body.ToArray();

                var paymentInfoJson = Encoding.UTF8.GetString(byteArray);
                var paymentInfo = JsonSerializer.Deserialize<PaymentInfoInputModel>(paymentInfoJson);

                ProcessPayment(paymentInfo);

                var paymentApproved = new PaymentApprovedIntegrationEvent(paymentInfo.ProjectId);
                var paymentApprovedJson = JsonSerializer.Serialize(paymentApproved);
                var paymentApprovedBytes = Encoding.UTF8.GetBytes(paymentApprovedJson);

                _channel.BasicPublish(
                    exchange: "",
                    routingKey: PaymentApprovedQueuName,
                    basicProperties: null,
                    body: paymentApprovedBytes
                );

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(QueueName, false, consumer);

            return Task.CompletedTask;
        }

        private void ProcessPayment(PaymentInfoInputModel paymentInfo)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

                paymentService.Process(paymentInfo);
            }
        }
    }
}
