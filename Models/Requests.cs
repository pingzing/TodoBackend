namespace TodoBackend.Models;

public record UpdateTaskRequest(TodoTaskStatus NewStatus);

public record NewTaskRequest(string Name);

