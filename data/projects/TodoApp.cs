using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TodoAppProject
{
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
        
        public override string ToString()
        {
            string statusIcon = Status switch
            {
                TaskStatus.NotStarted => "○",
                TaskStatus.InProgress => "◐",
                TaskStatus.Completed => "●",
                TaskStatus.Cancelled => "✗",
                _ => "?"
            };
            
            string priorityIcon = Priority switch
            {
                Priority.Low => "↓",
                Priority.Medium => "→",
                Priority.High => "↑",
                Priority.Critical => "!",
                _ => "?"
            };
            
            string dueInfo = "";
            if (DueDate.HasValue)
            {
                if (IsOverdue)
                    dueInfo = $" (OVERDUE by {Math.Abs(DaysUntilDue)} days)";
                else if (DaysUntilDue <= 3)
                    dueInfo = $" (Due in {DaysUntilDue} days)";
                else
                    dueInfo = $" (Due: {DueDate.Value:MM/dd})";
            }
            
            return $"{Id}. [{statusIcon}] {priorityIcon} {Title}{dueInfo}";
        }
    }
    
    public class TaskStatistics
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int CriticalTasks { get; set; }
        public double CompletionRate => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks * 100 : 0;
    }
    
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
            try
            {
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save file: {ex.Message}", ex);
            }
        }
        
        public async Task<T> LoadAsync<T>(string filePath)
        {
            try
            {
                if (!await FileExistsAsync(filePath))
                    return default(T);
                    
                string json = await File.ReadAllTextAsync(filePath);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load file: {ex.Message}", ex);
            }
        }
        
        public async Task<bool> FileExistsAsync(string filePath)
        {
            return File.Exists(filePath);
        }
    }
    
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
        
        private async Task LoadTasksAsync()
        {
            try
            {
                var loadedTasks = await fileService.LoadAsync<List<Task>>(dataFilePath);
                if (loadedTasks != null)
                {
                    tasks = loadedTasks;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not load tasks from file. Starting with empty list. Error: {ex.Message}");
                tasks = new List<Task>();
            }
        }
        
        private async Task SaveTasksAsync()
        {
            try
            {
                await fileService.SaveAsync(tasks, dataFilePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: Could not save tasks to file. Error: {ex.Message}");
            }
        }
        
        public async Task<int> AddTaskAsync(Task task)
        {
            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Task title cannot be empty");
                
            task.Id = tasks.Count > 0 ? tasks.Max(t => t.Id) + 1 : 1;
            task.CreatedDate = DateTime.Now;
            task.Status = TaskStatus.NotStarted;
            
            if (string.IsNullOrWhiteSpace(task.Category))
                task.Category = "General";
                
            tasks.Add(task);
            await SaveTasksAsync();
            return task.Id;
        }
        
        public async Task<bool> UpdateTaskAsync(Task task)
        {
            var existingTask = tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask == null)
                return false;
                
            existingTask.Title = task.Title;
            existingTask.Description = task.Description;
            existingTask.Priority = task.Priority;
            existingTask.DueDate = task.DueDate;
            existingTask.Category = task.Category;
            
            if (task.Status == TaskStatus.Completed && existingTask.Status != TaskStatus.Completed)
            {
                existingTask.CompletedDate = DateTime.Now;
            }
            
            existingTask.Status = task.Status;
            
            await SaveTasksAsync();
            return true;
        }
        
        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return false;
                
            tasks.Remove(task);
            await SaveTasksAsync();
            return true;
        }
        
        public Task<Task> GetTaskAsync(int taskId)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskId);
            return Task.FromResult(task);
        }
        
        public Task<IEnumerable<Task>> GetAllTasksAsync()
        {
            return Task.FromResult(tasks.AsEnumerable());
        }
        
        public Task<IEnumerable<Task>> GetTasksByStatusAsync(TaskStatus status)
        {
            var filteredTasks = tasks.Where(t => t.Status == status);
            return Task.FromResult(filteredTasks);
        }
        
        public Task<IEnumerable<Task>> SearchTasksAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Task.FromResult(tasks.AsEnumerable());
                
            var searchResults = tasks.Where(t => 
                t.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                t.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
                
            return Task.FromResult(searchResults);
        }
        
        public Task<TaskStatistics> GetStatisticsAsync()
        {
            var stats = new TaskStatistics
            {
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == TaskStatus.Completed),
                PendingTasks = tasks.Count(t => t.Status == TaskStatus.NotStarted || t.Status == TaskStatus.InProgress),
                OverdueTasks = tasks.Count(t => t.IsOverdue),
                CriticalTasks = tasks.Count(t => t.Priority == Priority.Critical && t.Status != TaskStatus.Completed)
            };
            
            return Task.FromResult(stats);
        }
    }
    
    public class TodoUI
    {
        private readonly ITodoService todoService;
        private bool running = true;
        
        public TodoUI(ITodoService todoService)
        {
            this.todoService = todoService;
        }
        
        public void Run()
        {
            Console.WriteLine("=== Todo Application ===");
            Console.WriteLine("Type 'help' for available commands");
            Console.WriteLine();
            
            while (running)
            {
                Console.Write("todo> ");
                string input = Console.ReadLine()?.Trim().ToLower();
                
                if (string.IsNullOrEmpty(input))
                    continue;
                    
                ProcessCommand(input);
            }
        }
        
        private void ProcessCommand(string input)
        {
            string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0];
            string[] args = parts.Skip(1).ToArray();
            
            try
            {
                switch (command)
                {
                    case "add":
                    case "a":
                        AddTask();
                        break;
                        
                    case "list":
                    case "l":
                        ListTasks(args);
                        break;
                        
                    case "update":
                    case "u":
                        UpdateTask();
                        break;
                        
                    case "delete":
                    case "d":
                        DeleteTask();
                        break;
                        
                    case "complete":
                    case "c":
                        CompleteTask();
                        break;
                        
                    case "search":
                    case "s":
                        SearchTasks();
                        break;
                        
                    case "stats":
                        ShowStatistics();
                        break;
                        
                    case "help":
                    case "h":
                        ShowHelp();
                        break;
                        
                    case "exit":
                    case "quit":
                    case "q":
                        running = false;
                        break;
                        
                    default:
                        Console.WriteLine($"Unknown command: {command}. Type 'help' for available commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        
        private void AddTask()
        {
            Console.WriteLine("Add new task:");
            
            Console.Write("Title: ");
            string title = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrWhiteSpace(title))
            {
                Console.WriteLine("Title cannot be empty.");
                return;
            }
            
            Console.Write("Description (optional): ");
            string description = Console.ReadLine()?.Trim();
            
            Console.Write("Priority (Low/Medium/High/Critical) [Medium]: ");
            string priorityInput = Console.ReadLine()?.Trim();
            Priority priority = Enum.TryParse<Priority>(priorityInput, true, out var p) ? p : Priority.Medium;
            
            Console.Write("Due date (MM/DD/YYYY) (optional): ");
            string dueDateInput = Console.ReadLine()?.Trim();
            DateTime? dueDate = null;
            if (DateTime.TryParse(dueDateInput, out DateTime dueDateResult))
            {
                dueDate = dueDateResult;
            }
            else if (!string.IsNullOrEmpty(dueDateInput))
            {
                Console.WriteLine("Invalid date format. Due date not set.");
            }
            
            Console.Write("Category (optional): ");
            string category = Console.ReadLine()?.Trim();
            
            var task = new Task
            {
                Title = title,
                Description = description,
                Priority = priority,
                DueDate = dueDate,
                Category = category
            };
            
            int taskId = todoService.AddTaskAsync(task).Result;
            Console.WriteLine($"Task added with ID: {taskId}");
        }
        
        private void ListTasks(string[] args)
        {
            var tasks = todoService.GetAllTasksAsync().Result;
            
            // Apply filters
            if (args.Length > 0)
            {
                string filter = args[0];
                switch (filter.ToLower())
                {
                    case "completed":
                        tasks = tasks.Where(t => t.Status == TaskStatus.Completed);
                        break;
                    case "pending":
                        tasks = tasks.Where(t => t.Status != TaskStatus.Completed);
                        break;
                    case "overdue":
                        tasks = tasks.Where(t => t.IsOverdue);
                        break;
                    case "critical":
                        tasks = tasks.Where(t => t.Priority == Priority.Critical);
                        break;
                }
            }
            
            if (!tasks.Any())
            {
                Console.WriteLine("No tasks found.");
                return;
            }
            
            Console.WriteLine("\n=== Tasks ===");
            foreach (var task in tasks.OrderBy(t => t.Priority).ThenBy(t => t.DueDate))
            {
                Console.WriteLine(task);
                
                if (!string.IsNullOrEmpty(task.Description))
                {
                    Console.WriteLine($"   Description: {task.Description}");
                }
                
                Console.WriteLine($"   Category: {task.Category} | Created: {task.CreatedDate:MM/dd/yyyy}");
                Console.WriteLine();
            }
        }
        
        private void UpdateTask()
        {
            Console.Write("Enter task ID to update: ");
            if (!int.TryParse(Console.ReadLine(), out int taskId))
            {
                Console.WriteLine("Invalid task ID.");
                return;
            }
            
            var task = todoService.GetTaskAsync(taskId).Result;
            if (task == null)
            {
                Console.WriteLine("Task not found.");
                return;
            }
            
            Console.WriteLine($"Current task: {task}");
            Console.WriteLine("Enter new values (press Enter to keep current):");
            
            Console.Write($"Title [{task.Title}]: ");
            string title = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(title))
                task.Title = title;
            
            Console.Write($"Description [{task.Description}]: ");
            string description = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(description))
                task.Description = description;
            
            Console.Write($"Status [{task.Status}] (NotStarted/InProgress/Completed/Cancelled): ");
            string statusInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(statusInput) && Enum.TryParse<TaskStatus>(statusInput, true, out var status))
                task.Status = status;
            
            Console.Write($"Priority [{task.Priority}] (Low/Medium/High/Critical): ");
            string priorityInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(priorityInput) && Enum.TryParse<Priority>(priorityInput, true, out var priority))
                task.Priority = priority;
            
            Console.Write($"Due date [{task.DueDate:MM/dd/yyyy}] (optional): ");
            string dueDateInput = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(dueDateInput) && DateTime.TryParse(dueDateInput, out DateTime dueDate))
                task.DueDate = dueDate;
            
            Console.Write($"Category [{task.Category}]: ");
            string category = Console.ReadLine()?.Trim();
            if (!string.IsNullOrEmpty(category))
                task.Category = category;
            
            bool success = todoService.UpdateTaskAsync(task).Result;
            if (success)
                Console.WriteLine("Task updated successfully.");
            else
                Console.WriteLine("Failed to update task.");
        }
        
        private void DeleteTask()
        {
            Console.Write("Enter task ID to delete: ");
            if (!int.TryParse(Console.ReadLine(), out int taskId))
            {
                Console.WriteLine("Invalid task ID.");
                return;
            }
            
            var task = todoService.GetTaskAsync(taskId).Result;
            if (task == null)
            {
                Console.WriteLine("Task not found.");
                return;
            }
            
            Console.WriteLine($"Task to delete: {task}");
            Console.Write("Are you sure? (y/N): ");
            string confirmation = Console.ReadLine()?.Trim().ToLower();
            
            if (confirmation == "y" || confirmation == "yes")
            {
                bool success = todoService.DeleteTaskAsync(taskId).Result;
                if (success)
                    Console.WriteLine("Task deleted successfully.");
                else
                    Console.WriteLine("Failed to delete task.");
            }
            else
            {
                Console.WriteLine("Deletion cancelled.");
            }
        }
        
        private void CompleteTask()
        {
            Console.Write("Enter task ID to complete: ");
            if (!int.TryParse(Console.ReadLine(), out int taskId))
            {
                Console.WriteLine("Invalid task ID.");
                return;
            }
            
            var task = todoService.GetTaskAsync(taskId).Result;
            if (task == null)
            {
                Console.WriteLine("Task not found.");
                return;
            }
            
            if (task.Status == TaskStatus.Completed)
            {
                Console.WriteLine("Task is already completed.");
                return;
            }
            
            task.Status = TaskStatus.Completed;
            task.CompletedDate = DateTime.Now;
            
            bool success = todoService.UpdateTaskAsync(task).Result;
            if (success)
                Console.WriteLine("Task marked as completed.");
            else
                Console.WriteLine("Failed to update task.");
        }
        
        private void SearchTasks()
        {
            Console.Write("Enter search term: ");
            string searchTerm = Console.ReadLine()?.Trim();
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                Console.WriteLine("Search term cannot be empty.");
                return;
            }
            
            var results = todoService.SearchTasksAsync(searchTerm).Result;
            
            if (!results.Any())
            {
                Console.WriteLine($"No tasks found matching '{searchTerm}'.");
                return;
            }
            
            Console.WriteLine($"\n=== Search Results for '{searchTerm}' ===");
            foreach (var task in results)
            {
                Console.WriteLine(task);
            }
        }
        
        private void ShowStatistics()
        {
            var stats = todoService.GetStatisticsAsync().Result;
            
            Console.WriteLine("\n=== Task Statistics ===");
            Console.WriteLine($"Total tasks: {stats.TotalTasks}");
            Console.WriteLine($"Completed: {stats.CompletedTasks}");
            Console.WriteLine($"Pending: {stats.PendingTasks}");
            Console.WriteLine($"Overdue: {stats.OverdueTasks}");
            Console.WriteLine($"Critical: {stats.CriticalTasks}");
            Console.WriteLine($"Completion rate: {stats.CompletionRate:F1}%");
        }
        
        private void ShowHelp()
        {
            Console.WriteLine("\n=== Todo Application Help ===");
            Console.WriteLine("Commands:");
            Console.WriteLine("  add, a              - Add a new task");
            Console.WriteLine("  list, l [filter]    - List tasks (optional filters: completed, pending, overdue, critical)");
            Console.WriteLine("  update, u            - Update an existing task");
            Console.WriteLine("  delete, d            - Delete a task");
            Console.WriteLine("  complete, c          - Mark task as completed");
            Console.WriteLine("  search, s            - Search tasks");
            Console.WriteLine("  stats                - Show task statistics");
            Console.WriteLine("  help, h              - Show this help message");
            Console.WriteLine("  exit, quit, q        - Exit the application");
            Console.WriteLine();
        }
    }
    
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                string dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TodoApp");
                Directory.CreateDirectory(dataDirectory);
                string dataFilePath = Path.Combine(dataDirectory, "tasks.json");
                
                var fileService = new JsonFileService();
                var todoService = new TodoService(fileService, dataFilePath);
                var ui = new TodoUI(todoService);
                
                ui.Run();
                
                Console.WriteLine("Thank you for using Todo Application!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }
    }
}
