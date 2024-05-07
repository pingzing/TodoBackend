using System.Text;
using System.Text.Json;
using TodoBackend.Models;

namespace TodoBackend.Services;

public class JsonFileDataStore : IDataStore
{
    private readonly JsonSerializerOptions _serializerOpts = new JsonSerializerOptions(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "dataStore.json");
    private readonly ILogger<JsonFileDataStore> _logger;

    public JsonFileDataStore(ILogger<JsonFileDataStore> logger)
    {
        if (!File.Exists(_filePath))
        {
            using var fs = new FileStream(_filePath, FileMode.OpenOrCreate);
            string emptyArray = "[]";
            var textBytes = Encoding.UTF8.GetBytes(emptyArray).AsSpan();
            fs.Write(textBytes);
        }

        _logger = logger;
    }

    public async Task<Guid> Create(string name)
    {
        List<TodoTask> tasks = (await ReadFileContents()).ToList();
        Guid newTaskGuid = Guid.NewGuid();
        tasks.Add(new TodoTask(newTaskGuid, name, TodoTaskStatus.Incomplete));
        await WriteContentsToFile(tasks);
        return newTaskGuid;
    }

    public async Task<bool> Delete(Guid id)
    {
        List<TodoTask> tasks = (await ReadFileContents()).ToList();
        var toDelete = tasks.Find(x => x.Id == id);
        if (toDelete == null)
        {
            _logger.LogWarning("Couldn't find task with id {id}. Not deleting.", id);
            return false;
        }
        
        tasks.Remove(toDelete);
        await WriteContentsToFile(tasks);
        return true;
    }

    public Task<TodoTask[]> GetAll()
    {
        return ReadFileContents();
    }

    public async Task<bool> Update(Guid id, TodoTaskStatus newStatus)
    {
        List<TodoTask> tasks = (await ReadFileContents()).ToList();
        var toUpdate = tasks.Find(x => x.Id == id);
        if (toUpdate == null)
        {
            _logger.LogWarning("Couldn't find task with id {id}. Not deleting.", id);
            return false;
        }

        tasks.Remove(toUpdate);
        toUpdate = toUpdate with { Status = newStatus };
        tasks.Add(toUpdate);
        await WriteContentsToFile(tasks);
        return true;
    }

    private async Task<TodoTask[]> ReadFileContents()
    {
        TodoTask[]? deserialized = null;

        await _fileLock.WaitAsync();
        try
        {
            string json = File.ReadAllText(_filePath);
            deserialized = JsonSerializer.Deserialize<TodoTask[]>(json, _serializerOpts);
        }
        finally
        {
            _fileLock.Release();
        }

        if (deserialized == null)
        {
            _logger.LogError("Got null when attempting to deserialize data JSON file.");
            throw new Exception("Unable to read from the data store.");
        }        

        return deserialized;
    }

    private async Task WriteContentsToFile(IEnumerable<TodoTask> tasks)
    {
        await _fileLock.WaitAsync();
        try
        {
            string json = JsonSerializer.Serialize(tasks, _serializerOpts);
            await File.WriteAllTextAsync(_filePath, json, Encoding.UTF8);
        }
        finally
        {
            _fileLock.Release();
        }
    }
}