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

        //Interacts with the Azure Storage to manage the orchestration state(reading/writing), queues, tracking and tables.
        var orchestrationService = new AzureStorageOrchestrationService(orchestrationServiceSettings);

        //This worker is responsible for executing the orchestration logic and activities.
        //It listens to the orchestrator queue for new messages, executes the orchestration and activity code, and manages the lifecycle of orchestration instances.
        var taskHubWorker = new TaskHubWorker(orchestrationService);

        // We can register multiple orchestrations and activities with the TaskHubWorker instance.
        taskHubWorker.AddTaskOrchestrations(typeof(SampleOrchestration));
        taskHubWorker.AddTaskActivities(typeof(TaskA), typeof(TaskB));

        // this method is responsible for creating the necessary resources in azure storage if they do not already exist. This includes the creation of the three queues:
        //	< taskhubname >/ orchestrator
        //	< taskhubname >/ tracking
        //	< taskhubname >/ worker
        await orchestrationService.CreateIfNotExistsAsync();

        await taskHubWorker.StartAsync();

        // This client is used to interact with the orchestration service from the perspective of an external caller.
        // It is responsible for initiating new orchestrations, querying the status of existing orchestrations, and waiting for orchestration completion.
        var client = new TaskHubClient(orchestrationService);

        // This method sends an ExecutionStarted message to the Orchestrator queue with details like
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


/**
 *The hub is essentially the orchestrator for orchestrations.
 *it manages the lifecycle of orchestration instances, handles the communication between various components,
 *and ensures reliability and consistency across distributed systems.
 *
 *state management:
 *the hub tracks the state of each orchestration instance, including its current status, history, and any pending tasks.
 *it ensures that the state is persisted reliably, so that the orchestration can be resumed in case of failures.
 *
 *Task Coordination:
 *The hub receives messages from the orchestration queue, which signal various events (e.g., orchestration start, task completion).
 *It coordinates the execution of tasks within an orchestration by dispatching work to worker processes and tracking their progress.
 *
 *Reliability and Fault Tolerance:
 *The hub is designed to handle failures gracefully. It ensures that tasks are retried in case of transient failures and that the state is not lost if the hub itself fails.
 *It leverages queues to ensure messages (e.g., task completion) are not lost and can be processed when the hub recovers.
 *
 *Concurrency Control:
 *The hub manages concurrency and ensures that orchestrations are executed in a controlled manner.
 *It handles the scheduling of tasks and ensures that tasks are executed in the correct order, respecting dependencies and retries.
 *
 *Event Handling:
 *The hub listens for events from various sources, such as timers, external events, or task completions.
 *It reacts to these events by updating the state and progressing the orchestration as needed.
 **/
