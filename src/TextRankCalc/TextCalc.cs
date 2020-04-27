using System;
using System.Text;
using NATS.Client;
using NATS.Client.Rx;
using NATS.Client.Rx.Ops;
using System.Linq;
using StackExchange.Redis;
using TextRankCalc.Models;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TextRankCalc
{
    public class SubscriberService
    {
        public void Run(IConnection nats, ConnectionMultiplexer redis)
        {
            var events = nats.Observe("events")
                    .Where(m => m.Data?.Any() == true)
                    .Select(m => Encoding.Default.GetString(m.Data));

            events.Subscribe(msg =>
            {
                IDatabase db = redis.GetDatabase();
                string id = msg.Split('|').Last();
                string JSON = db.StringGet(id);

                RedisModel model = JsonSerializer.Deserialize<RedisModel>(JSON);

                string str = model.Data;

                double vowels = Regex.Matches(model.Data, @"[AEIOUaeiou]").Count;
                double consonants = Regex.Matches(model.Data, @"[QWRTYPSDFGHJKLZXCVBNMqwrtypsdfghjklzxcvbnm]").Count;

                double measure = vowels / Math.Max(consonants, 1);
                model.Measure = measure;

                var result = JsonSerializer.Serialize(model);
                db.StringSet(id, result);
            });
        }
    }
}