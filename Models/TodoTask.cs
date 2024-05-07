namespace TodoBackend.Models;

public enum TodoTaskStatus
{
    Incomplete,
    Complete
}

public record TodoTask(Guid Id, string Name, TodoTaskStatus Status);

