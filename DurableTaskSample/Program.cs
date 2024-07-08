using System;
using System.Threading.Tasks;
using DurableTask.AzureStorage;
using DurableTask.Core;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Configuration;

class Program
{
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

        // Initializes the orchestration service which interacts with Azure Storage
        // to manage orchestration state, queues, tracking, and tables.
        var orchestrationService = new AzureStorageOrchestrationService(orchestrationServiceSettings);

        // Initializes the task hub worker responsible for executing orchestration logic and activities.
        // It listens to the orchestrator queue for new messages, executes the orchestration and activity code,
        // and manages the lifecycle of orchestration instances.
        var taskHubWorker = new TaskHubWorker(orchestrationService);

        // Registers orchestrations and activities with the TaskHubWorker instance.
        taskHubWorker.AddTaskOrchestrations(typeof(SampleOrchestration));
        taskHubWorker.AddTaskActivities(typeof(TaskA), typeof(TaskB));

        // Ensures the necessary resources in Azure Storage are created if they do not already exist.
        // This includes the creation of the three queues:
        // <taskHubName>/orchestrator, <taskHubName>/tracking, <taskHubName>/worker
        await orchestrationService.CreateIfNotExistsAsync();

        // Starts the task hub worker.
        await taskHubWorker.StartAsync();

        // Initializes the client used to interact with the orchestration service from the perspective of an external caller.
        // It is responsible for initiating new orchestrations, querying the status of existing orchestrations,
        // and waiting for orchestration completion.
        var client = new TaskHubClient(orchestrationService);

        // Initiates a new orchestration by sending an ExecutionStarted message to the Orchestrator queue.
        // This message includes details such as the orchestration name and ID,
        // allowing the Durable Task Framework to operate on the correct instance of the orchestration.
        // Multiple instances of the same orchestration can run simultaneously, each identified by a unique instance ID.
        var instance = await client.CreateOrchestrationInstanceAsync(typeof(SampleOrchestration), "Hello World!");

        // Waits for the orchestration to complete and retrieves the result.
        var result = await client.WaitForOrchestrationAsync(instance, TimeSpan.FromSeconds(30));
        Console.WriteLine($"Orchestration result: {result.Output}");

        // Stops the task hub worker gracefully.
        await taskHubWorker.StopAsync(true);
    }
}
