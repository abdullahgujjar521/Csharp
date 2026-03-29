# Todo App Project in C#

A comprehensive todo application that demonstrates data management, file I/O, and object-oriented programming principles.

## Project Overview

This todo application will include:
- Task creation and management
- Priority levels
- Due dates
- Task status tracking
- Data persistence
- Search and filter functionality
- Category management

## Features

### Core Features
- Create, read, update, delete tasks (CRUD operations)
- Mark tasks as complete/incomplete
- Set priority levels (High, Medium, Low)
- Assign due dates
- Add task descriptions
- Task categories

### Advanced Features
- Search tasks by title or description
- Filter by status, priority, or category
- Sort tasks by various criteria
- Task statistics
- Data export/import
- Command-line interface

### Data Management
- JSON file storage
- Automatic data backup
- Data validation
- Error handling

## Project Structure

```
TodoApp/
├── Models/
│   ├── Task.cs
│   ├── TaskStatus.cs
│   ├── Priority.cs
│   └── Category.cs
├── Services/
│   ├── ITodoService.cs
│   ├── TodoService.cs
│   ├── IFileService.cs
│   └── JsonFileService.cs
├── UI/
│   └── TodoUI.cs
├── Utils/
│   └── ValidationHelper.cs
└── Program.cs
```

## Core Classes

### Task Model
```csharp
public class Task
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public Priority Priority { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Category { get; set; }
    
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.Now && Status != TaskStatus.Completed;
    public int DaysUntilDue => DueDate.HasValue ? (DueDate.Value - DateTime.Now).Days : -1;
}
```

### Enums
```csharp
public enum TaskStatus
{
    NotStarted,
    InProgress,
    Completed,
    Cancelled
}

public enum Priority
{
    Low,
    Medium,
    High,
    Critical
}
```

### Todo Service
```csharp
public interface ITodoService
{
    Task<int> AddTaskAsync(Task task);
    Task<bool> UpdateTaskAsync(Task task);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<Task> GetTaskAsync(int taskId);
    Task<IEnumerable<Task>> GetAllTasksAsync();
    Task<IEnumerable<Task>> GetTasksByStatusAsync(TaskStatus status);
    Task<IEnumerable<Task>> SearchTasksAsync(string searchTerm);
    Task<TaskStatistics> GetStatisticsAsync();
}

public class TodoService : ITodoService
{
    private readonly IFileService fileService;
    private readonly string dataFilePath;
    private List<Task> tasks = new List<Task>();
    
    public TodoService(IFileService fileService, string dataFilePath)
    {
        this.fileService = fileService;
        this.dataFilePath = dataFilePath;
        LoadTasksAsync().Wait();
    }
    
    public async Task<int> AddTaskAsync(Task task)
    {
        task.Id = tasks.Count > 0 ? tasks.Max(t => t.Id) + 1 : 1;
        task.CreatedDate = DateTime.Now;
        tasks.Add(task);
        await SaveTasksAsync();
        return task.Id;
    }
    
    // Other methods implementation...
}
```

### File Service
```csharp
public interface IFileService
{
    Task SaveAsync<T>(T data, string filePath);
    Task<T> LoadAsync<T>(string filePath);
    Task<bool> FileExistsAsync(string filePath);
}

public class JsonFileService : IFileService
{
    public async Task SaveAsync<T>(T data, string filePath)
    {
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }
    
    public async Task<T> LoadAsync<T>(string filePath)
    {
        if (!await FileExistsAsync(filePath))
            return default(T);
            
        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json);
    }
    
    public async Task<bool> FileExistsAsync(string filePath)
    {
        return File.Exists(filePath);
    }
}
```

## Implementation Steps

### Step 1: Create Core Models
1. Define Task class with properties
2. Create enums for status and priority
3. Add validation attributes

### Step 2: Implement Data Layer
1. Create file service for JSON operations
2. Implement todo service with CRUD operations
3. Add data validation and error handling

### Step 3: Build User Interface
1. Create console-based menu system
2. Implement input handling
3. Add display formatting

### Step 4: Add Advanced Features
1. Search and filter functionality
2. Statistics and reporting
3. Data import/export

## Usage Examples

### Adding a Task
```csharp
var task = new Task
{
    Title = "Complete C# project",
    Description = "Finish the todo application",
    Priority = Priority.High,
    DueDate = DateTime.Now.AddDays(7),
    Category = "Programming"
};

int taskId = await todoService.AddTaskAsync(task);
```

### Searching Tasks
```csharp
var searchResults = await todoService.SearchTasksAsync("C#");
foreach (var task in searchResults)
{
    Console.WriteLine($"{task.Id}: {task.Title} - {task.Status}");
}
```

### Getting Statistics
```csharp
var stats = await todoService.GetStatisticsAsync();
Console.WriteLine($"Total tasks: {stats.TotalTasks}");
Console.WriteLine($"Completed: {stats.CompletedTasks}");
Console.WriteLine($"Pending: {stats.PendingTasks}");
```

## Data Format

### JSON Structure
```json
{
  "tasks": [
    {
      "id": 1,
      "title": "Learn C#",
      "description": "Study C# fundamentals",
      "status": "InProgress",
      "priority": "High",
      "createdDate": "2024-01-01T10:00:00",
      "dueDate": "2024-01-15T17:00:00",
      "completedDate": null,
      "category": "Learning"
    }
  ]
}
```

## Command Structure

### Main Commands
- `add` - Add new task
- `list` - List all tasks
- `update` - Update existing task
- `delete` - Delete task
- `complete` - Mark task as complete
- `search` - Search tasks
- `stats` - Show statistics
- `export` - Export tasks to file
- `import` - Import tasks from file

### Filter Options
- `--status <status>` - Filter by status
- `--priority <priority>` - Filter by priority
- `--category <category>` - Filter by category
- `--due <days>` - Filter by due date
- `--sort <field>` - Sort by field

## Error Handling

The application includes comprehensive error handling for:
- Invalid input formats
- File I/O operations
- Data validation errors
- Duplicate task IDs
- Corrupted data files

## Extension Ideas

1. **Web Interface**: Create an ASP.NET Core web version
2. **Database Storage**: Replace JSON with SQLite or SQL Server
3. **Task Dependencies**: Add task dependency management
4. **Notifications**: Email or desktop notifications
5. **Collaboration**: Multi-user support with shared tasks
6. **Mobile App**: Xamarin or MAUI mobile application
7. **API**: RESTful API for integration with other apps

## Learning Objectives

This project helps you learn:
- Object-oriented programming principles
- Async/await patterns
- File I/O operations
- JSON serialization
- LINQ queries
- Error handling and validation
- Command-line interface design
- Data persistence strategies

## Best Practices Demonstrated

- Separation of concerns
- Dependency injection
- Async programming
- Data validation
- Error handling
- Clean architecture
- SOLID principles
- Test-driven design principles
