using DurableTask.Core;

public class TaskA : TaskActivity<string, string>
{
    protected override string Execute(TaskContext context, string input)
    {
        Console.WriteLine($"Executing TaskA with input: {input}");
        Task.Delay(10000).GetAwaiter().GetResult();
        return $"TaskA result from input: {input}";
    }
}
