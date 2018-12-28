using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ_Server.Models;

namespace RabbitMQ_Server.RabbitMQ
{
    public class RabbitMQClient
    {
        private static IConnection connection;
        private static IModel channel;

        private const string ExchangeName = "Topic_Exchange";

        private const string AllQueueName = "AllTopic_Queue"; //ConversationId
        private const string routingKey = "watchlist.dashboard";
        public RabbitMQClient(string conversationId)
        {
            var _factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "test",
                Password = "1234",
                VirtualHost = "ACID"
            };

            connection = _factory.CreateConnection();
            channel = connection.CreateModel();
            channel.ExchangeDeclare(ExchangeName, "topic");

            channel.QueueDeclare(conversationId, true, false, false, null);
            channel.QueueDeclare(AllQueueName, true, false, false, null);

            // Optinal
            channel.QueueBind(conversationId, ExchangeName, routingKey);
            channel.QueueBind(AllQueueName, ExchangeName, "watchlist.*");
        }

        public void CloseQueue()
        {
            Console.WriteLine("[+] Closing conversation ");
            if (connection != null)
                connection.Close();
        }

        public void PublishToQue(CpuRam payment)
        {
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payment));
            SendMessage(body, routingKey);
        }

        //PublishResult
        private void SendMessage(byte[] body, string routingKey)
        {
            if (channel.IsOpen)
            {
                lock (channel)
                {
                    IBasicProperties header = channel.CreateBasicProperties();
                    header.Headers = new Dictionary<string, object>();
                    header.Headers.Add("Timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff"));
                    //this.Channel.BasicPublish(string.Empty, this.ConversationId, header, body);
                    channel.BasicPublish(ExchangeName, routingKey, header, body);
                }
            }
        }

        ~RabbitMQClient()
        {
            this.CloseQueue();
        }
    }
}
