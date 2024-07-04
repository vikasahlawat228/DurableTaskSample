using System;
using System.Threading.Tasks;
using DurableTask.AzureStorage;
using DurableTask.Core;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Configuration;

class Program
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        var configuration = builder.Build();

        string storageConnectionString = configuration["StorageConnectionString"];
        string taskHubName = configuration["TaskHubName"];

        var orchestrationServiceSettings = new AzureStorageOrchestrationServiceSettings
        {
            TaskHubName = taskHubName,
            StorageConnectionString = storageConnectionString
        };

        var orchestrationService = new AzureStorageOrchestrationService(orchestrationServiceSettings);

        var taskHubWorker = new TaskHubWorker(orchestrationService);
        // We can register multiple orchestrations and activities with the TaskHubWorker instance.
        taskHubWorker.AddTaskOrchestrations(typeof(SampleOrchestration));
        taskHubWorker.AddTaskActivities(typeof(TaskA), typeof(TaskB));

        await orchestrationService.CreateIfNotExistsAsync();
        // this method is responsible for creating the necessary resources in azure storage if they do not already exist.this includes the creation of the three queues:
        //	< taskhubname >/ orchestrator
        //	< taskhubname >/ tracking
        //	< taskhubname >/ worker

        await taskHubWorker.StartAsync();

        var client = new TaskHubClient(orchestrationService);

        // This line sends an ExecutionStarted message to the Orchestrator queue with details like
        // the orchestration name and ID, that will help DTF to operate on the correct instance of orchestration.
        // We can have multiple instances of the same orchestration running simultaneously. Each instance is identified by a unique instance ID.
        // ******The client initiates the orchestration by sending a message to the Orchestrator queue. This is done using the TaskHubClient:
        var instance = await client.CreateOrchestrationInstanceAsync(typeof(SampleOrchestration), "Hello World!");
        // But the orchestrator runs on the server side, managed by the TaskHubWorker and AzureStorageOrchestrationService.
        // The orchestrator code is executed by the worker, which listens to the Orchestrator queue for messages and processes them accordingly.

        var result = await client.WaitForOrchestrationAsync(instance, TimeSpan.FromSeconds(30));
        Console.WriteLine($"Orchestration result: {result.Output}");

        await taskHubWorker.StopAsync(true);
    }
}
