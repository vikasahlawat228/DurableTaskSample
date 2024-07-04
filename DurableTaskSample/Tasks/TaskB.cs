using DurableTask.Core;

public class TaskB : TaskActivity<string, string>
{
    protected override string Execute(TaskContext context, string input)
    {
        Console.WriteLine($"Executing TaskB with input: {input}");
        return $"TaskB result from input: {input}";
    }
}
