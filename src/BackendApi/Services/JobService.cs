using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System.Text;
using StackExchange.Redis;
using BackendApi.Models;
using System.Text.Json;
using System.Threading;

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
            var resp = new RegisterResponse{ Id = id };
            _jobs[id] = request.Description;

            var model = new RedisModel { Description = request.Description, Data = request.Data };
            string result = JsonSerializer.Serialize(model);
            IDatabase db = _redis.GetDatabase();
            db.StringSet(id, result);

            string message = $"JobCreated|{id}";
            byte[] payload = Encoding.Default.GetBytes(message);
            _connection.Publish("events", payload);

            return Task.FromResult(resp);
        }

        public override Task<GetProcessingResultResponse> GetProcessingResult(GetProcessingResultRequest request, ServerCallContext context)
        {
            IDatabase db = _redis.GetDatabase();
            double measure = -1;
            int i = 0;
            while (i < 5)
            {
                string JSON = db.StringGet(request.Id);
                var model = JsonSerializer.Deserialize<RedisModel>(JSON);
                if (model.Measure != -1)
                {
                    measure = model.Measure;
                    break;
                }
                Thread.Sleep(1000);
                i++;
            }
            var resp = new GetProcessingResultResponse{ Measure = measure };
            if (measure == -1)
            {
                resp.Status = "in_progress";
            }
            else
            {
                resp.Status = "done";
            }

            return Task.FromResult(resp);
        }
    }
}