using Microsoft.AspNetCore.OpenApi;
using TodoBackend.Models;
using TodoBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDataStore, JsonFileDataStore>();

var app = builder.Build();

// Swagger UI and routing
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet(
        "/tasks",
        async (IDataStore dataStore) =>
        {
            var tasks = await dataStore.GetAll();
            return Results.Ok(tasks);
        }
    )
    .WithName("GetTasks")
    .WithOpenApi();

app.MapPut(
        "tasks/{taskId}",
        async (Guid taskId, UpdateTaskRequest req, IDataStore dataStore) =>
        {
            bool updateSuccess = await dataStore.Update(taskId, req.NewStatus);
            if (!updateSuccess)
            {
                return Results.NotFound();
            }
            return Results.Ok();
        }
    )
    .WithName("UpdateTask")
    .WithOpenApi();

app.MapPost(
        "/tasks",
        async (NewTaskRequest req, IDataStore dataStore) =>
        {
            Guid newTaskGuid = await dataStore.Create(req.Name);
            return Results.Ok(new NewTaskResponse(newTaskGuid, req.Name));
        }
    )
    .WithName("AddTask")
    .WithOpenApi();

app.MapDelete(
        "/tasks/{taskId}",
        async (Guid taskId, IDataStore dataStore) =>
        {
            bool deleteSuccess = await dataStore.Delete(taskId);
            if (!deleteSuccess)
            {
                return Results.NotFound();
            }

            return Results.Ok();
        }
    )
    .WithName("DeleteTask")
    .WithOpenApi();

app.Run();
