# Durable Task Sample

This project demonstrates the use of the Durable Task Framework (DTF) with Azure Storage for orchestrating long-running workflows in .NET.

## Prerequisites

- .NET SDK
- Azure Storage Account
- Visual Studio

## Configuration

1. Create an `appsettings.json` file in the root of your project with the following content:

```json
{
  "AzureWebJobsStorage": "<YourAzureStorageConnectionString>",
  "DurableTask": {
	"TaskHubName": "<YourTaskHubName>"
  }
}
```

2. Replace `<YourAzureStorageConnectionString>` with your Azure Storage connection string.
3. Replace `<YourTaskHubName>` with your desired Task Hub name.

## Running the Project

1. Open the project in Visual Studio.
2. Build the solution to restore the necessary packages.
3. Run the project.

## Code Overview

The main entry point of the application is the `Program.cs` file. Here's a brief overview of what each part does:

- **Configuration**: Reads the `appsettings.json` file to get the Azure Storage connection string and Task Hub name.
- **Orchestration Service**: Initializes the `AzureStorageOrchestrationService` with the provided settings.
- **Task Hub Worker**: Registers orchestrations and activities with the `TaskHubWorker` instance.
- **Orchestration Client**: Creates an instance of the orchestration and waits for its completion.

## Orchestrations and Activities

- **SampleOrchestration**: The main orchestration logic.
- **TaskA** and **TaskB**: Sample activities that are executed as part of the orchestration.

## What is a Hub?

The hub in the Durable Task Framework is essentially the orchestrator for orchestrations. It manages the lifecycle of orchestration instances, handles the communication between various components, and ensures reliability and consistency across distributed systems.

## Functionalities of the Hub

### State Management

- The hub tracks the state of each orchestration instance, including its current status, history, and any pending tasks.
- It ensures that the state is persisted reliably, so that the orchestration can be resumed in case of failures.

### Task Coordination

- The hub receives messages from the orchestration queue, which signal various events (e.g., orchestration start, task completion).
- It coordinates the execution of tasks within an orchestration by dispatching work to worker processes and tracking their progress.

### Reliability and Fault Tolerance

- The hub is designed to handle failures gracefully. It ensures that tasks are retried in case of transient failures and that the state is not lost if the hub itself fails.
- It leverages queues to ensure messages (e.g., task completion) are not lost and can be processed when the hub recovers.

### Concurrency Control

- The hub manages concurrency and ensures that orchestrations are executed in a controlled manner.
- It handles the scheduling of tasks and ensures that tasks are executed in the correct order, respecting dependencies and retries.

### Event Handling

- The hub listens for events from various sources, such as timers, external events, or task completions.
- It reacts to these events by updating the state and progressing the orchestration as needed.



