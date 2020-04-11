using System;
using System.Text;
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using System.Linq;
using StackExchange.Redis;

namespace JobLogger
{
    public class SubscriberService
    {
        public void Run(IConnection connection, ConnectionMultiplexer redis)
        {
            var events = connection.Observe("events")
                    .Where(m => m.Data?.Any() == true)
                    .Select(m => Encoding.Default.GetString(m.Data));

            events.Subscribe(msg => {
                string id = msg.Split('|').Last();
                Console.WriteLine(msg);
                IDatabase db = redis.GetDatabase();
                string description = db.StringGet(id);
                Console.WriteLine(description);
            });
        }    
    }
}