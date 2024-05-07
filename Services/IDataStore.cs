using TodoBackend.Models;

namespace TodoBackend.Services;

internal interface IDataStore
{
    Task<TodoTask[]> GetAll();
    Task<bool> Update(Guid id, TodoTaskStatus newStatus);
    Task<Guid> Create(string name);
    Task<bool> Delete(Guid id);
}