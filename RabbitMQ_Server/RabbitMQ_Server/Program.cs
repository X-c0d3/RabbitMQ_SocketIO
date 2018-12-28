using RabbitMQ_Server.Models;
using RabbitMQ_Server.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Timers;

namespace RabbitMQ_Server
{
    class Program
    {
        static string conversationId = "CardPaymentTopic_Queue";
        static List<float> AvailableCPU = new List<float>();
        static List<float> AvailableRAM = new List<float>();

        protected static PerformanceCounter cpuCounter;
        protected static PerformanceCounter ramCounter;
        static List<PerformanceCounter> cpuCounters = new List<PerformanceCounter>();
        static int cores = 0;
        static RabbitMQClient client;

        static void Main(string[] args)
        {
            //SendPayment(payment);
            //ClientListen(payment);

            Console.WriteLine("[+] Starting...");
            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            client = new RabbitMQClient(conversationId);
            try
            {
                System.Timers.Timer t = new System.Timers.Timer(1200);
                t.Elapsed += new ElapsedEventHandler(TimerElapsed);
                t.Start();
                Thread.Sleep(10000);
            }
            catch (Exception e)
            {
                Console.WriteLine("catched exception");
            }
            Console.ReadLine();
        }

        public static void ConsumeCPU()
        {
            int percentage = 60;
            if (percentage < 0 || percentage > 100)
                throw new ArgumentException("percentage");
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                if (watch.ElapsedMilliseconds > percentage)
                {
                    Thread.Sleep(100 - percentage);
                    watch.Reset();
                    watch.Start();
                }
            }
        }

        public static void TimerElapsed(object source, ElapsedEventArgs e)
        {
            float cpu = cpuCounter.NextValue();
            float sum = 0;
            foreach (PerformanceCounter c in cpuCounters)
            {
                sum = sum + c.NextValue();
            }
            sum = sum / (cores);
            float ram = ramCounter.NextValue();
            Console.WriteLine(string.Format("CPU Value 1: {0}, Cpu  2: {1} ,Ram : {2}", sum, cpu, ram));
            AvailableCPU.Add(sum);
            AvailableRAM.Add(ram);


            client.PublishToQue(new CpuRam
            {
                Name = "JIB/" + Guid.NewGuid(),
                CPU = cpu,
                Ram = ram,
                ModifyDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss,fff")
            });
        }


        static void SendDataToQue(CpuRam cpuusage)
        {
            client = new RabbitMQClient(conversationId);
            client.PublishToQue(cpuusage);
            client.CloseQueue();
        }

        static void ClientListen(CpuRam payment)
        {
            RabbitMQDirectClient client = new RabbitMQDirectClient();
            client.ClientListen(payment, conversationId);
        }
    }
}
