using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System.Text;
using StackExchange.Redis;

namespace BackendApi.Services
{
    public class JobService : Job.JobBase
    {
        private readonly static Dictionary<string, string> _jobs = new Dictionary<string, string>();
        private readonly ILogger<JobService> _logger;
        private readonly IConnection _connection;
        private readonly ConnectionMultiplexer _redis;

        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;
            _connection = new ConnectionFactory().CreateConnection("nats://" + Environment.GetEnvironmentVariable("NATS_HOST"));
            _redis = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_HOST"));
        }

        public override Task<RegisterResponse> Register(RegisterRequest request, ServerCallContext context)
        {
            string id = Guid.NewGuid().ToString();
            var resp = new RegisterResponse
            {
                Id = id
            };
            _jobs[id] = request.Description;

            string message = $"JobCreated|{id}";
            byte[] payload = Encoding.Default.GetBytes(message);
            _connection.Publish("events", payload);

            IDatabase db = _redis.GetDatabase();
            db.StringSet(id, request.Description);

            return Task.FromResult(resp);
        }
    }
}