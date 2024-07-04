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


