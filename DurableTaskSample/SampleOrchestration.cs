using DurableTask.Core;
using System;
using System.Threading.Tasks;

public class SampleOrchestration : TaskOrchestration<string, string>
{
    public override async Task<string> RunTask(OrchestrationContext context, string input)
    {
        Console.WriteLine($"Orchestration started with input: {input}");
        Console.WriteLine("Is Replaying = " + context.IsReplaying);

        var retryOptions = new RetryOptions(TimeSpan.FromSeconds(5), 3)
        {
            Handle = exception => exception is TimeoutException
        };

        string resultA = null;
        string resultB = null;

        // Case-I series combination 
        // resultA = await context.ScheduleWithRetry<string>(typeof(TaskA), retryOptions, input);
        // resultB = await context.ScheduleTask<string>(typeof(TaskB), resultA);


        // parallel combination (Uncomment this for Case-II and Case-III)
        Task<string> taskA = context.ScheduleTask<string>(typeof(TaskA), input);
        Task<string> taskB = context.ScheduleTask<string>(typeof(TaskB), input);

        // Case-II parallel combination with await 
        // resultA = await taskA;
        // resultB = await taskB;

        // Case - III parallel combination with Task.WhenAll
        List<Task> tasks = new List<Task> { taskA, taskB };
        while (tasks.Count > 0)
        {
            Task finishedTask = await Task.WhenAny(tasks);
            if (finishedTask == taskA)
            {
                Console.WriteLine("TaskA finished");
                resultA = await taskA;
            }
            else if (finishedTask == taskB)
            {
                Console.WriteLine("TaskB finished");
                resultB = await taskB;
            }
            tasks.Remove(finishedTask);
        }

        Console.WriteLine("Orchestration finished");
        return resultB;
    }
}
