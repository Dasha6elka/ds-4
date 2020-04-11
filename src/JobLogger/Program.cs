using System;
using NATS.Client;
using StackExchange.Redis;
using System.Threading;
using System.Threading.Tasks;

namespace JobLogger
{
    class Program
    {
        private static readonly AutoResetEvent _closing = new AutoResetEvent(false);
        static void Main(string[] args)
        {
            var subscriberService = new SubscriberService();

            using (IConnection connection = new ConnectionFactory().CreateConnection("nats://" + Environment.GetEnvironmentVariable("NATS_HOST")))
            using (ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST")))
            {
                subscriberService.Run(connection, redis);
                Console.WriteLine("Events listening started.");

                Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        Console.WriteLine(DateTime.Now.ToString());
                        Thread.Sleep(1000);
                    }
                });
                Console.CancelKeyPress += new ConsoleCancelEventHandler(OnExit);
                _closing.WaitOne();
            }
        }
        protected static void OnExit(object sender, ConsoleCancelEventArgs args)
        {
            Console.WriteLine("Exit");
            _closing.Set();
        }
    }
}
