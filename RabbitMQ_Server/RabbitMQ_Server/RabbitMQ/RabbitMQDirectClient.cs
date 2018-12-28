
using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ_Server.Models;

namespace RabbitMQ_Server.RabbitMQ
{
    public class RabbitMQDirectClient
    {


        public void ClientListen(CpuRam payment, string conversationId)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "test",
                Password = "1234",
                VirtualHost = "ACID"
            };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // channel.QueueDeclare("rpc_reply", false, false, false, null);
                    channel.QueueDeclare(conversationId, true, false, false, null);
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(conversationId, true, consumer);

                    //##############################
                    //var corrId = Guid.NewGuid().ToString();
                    //var props = channel.CreateBasicProperties();
                    //props.ReplyTo = conversationId;
                    //props.CorrelationId = corrId;
                    //Console.WriteLine("[+] CorrelationId: " + corrId);

                    if (channel.IsOpen)
                    {
                        while (true)
                        {
                            var ea = consumer.Queue.Dequeue();
                            //if (ea.BasicProperties.CorrelationId != corrId)
                            //    continue;

                            var message = Encoding.UTF8.GetString(ea.Body);
                            if (ea.RoutingKey.Contains("payment.cardpayment"))
                            {
                                Console.WriteLine(message);
                            }
                        }
                    }
                }
            };
        }
    }
}
