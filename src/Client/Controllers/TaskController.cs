using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Client.Models;
using Grpc.Net.Client;
using BackendApi;

namespace Client.Controllers
{
    public class TaskController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public TaskController(ILogger<HomeController> logger)
        {
            _logger = logger;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public async Task<IActionResult> Index(string JobId)
        {
            using var channel = GrpcChannel.ForAddress("http://" + Environment.GetEnvironmentVariable("API_HOST"));
            var client = new Job.JobClient(channel);
            var reply = await client.GetProcessingResultAsync(new GetProcessingResultRequest { Id = JobId });
            return View("Task", new TaskViewModel { Id = JobId, Meausre = reply.Measure, Status = reply.Status });
        }
    }
}