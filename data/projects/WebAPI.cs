using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebAPIProject
{
    // Simple models for demonstration
    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
    
    public class OrderItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
    
    public class CreateUserRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Category { get; set; }
    }
    
    public class CreateOrderRequest
    {
        public List<OrderItemRequest> Items { get; set; } = new List<OrderItemRequest>();
    }
    
    public class OrderItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
    
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
    
    public class PagedResponse<T>
    {
        public List<T> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
    
    // Simple in-memory data store for demonstration
    public class DataStore
    {
        private static readonly List<User> _users = new List<User>();
        private static readonly List<Product> _products = new List<Product>();
        private static readonly List<Order> _orders = new List<Order>();
        private static int _nextUserId = 1;
        private static int _nextProductId = 1;
        private static int _nextOrderId = 1;
        
        static DataStore()
        {
            // Initialize with sample data
            InitializeSampleData();
        }
        
        private static void InitializeSampleData()
        {
            // Sample users
            _users.Add(new User
            {
                Id = _nextUserId++,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Username = "johndoe",
                CreatedAt = DateTime.Now.AddDays(-30),
                IsActive = true
            });
            
            _users.Add(new User
            {
                Id = _nextUserId++,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Username = "janesmith",
                CreatedAt = DateTime.Now.AddDays(-15),
                IsActive = true
            });
            
            // Sample products
            _products.Add(new Product
            {
                Id = _nextProductId++,
                Name = "Laptop",
                Description = "High-performance laptop",
                Price = 999.99m,
                Stock = 50,
                Category = "Electronics",
                CreatedAt = DateTime.Now.AddDays(-10),
                IsActive = true
            });
            
            _products.Add(new Product
            {
                Id = _nextProductId++,
                Name = "Mouse",
                Description = "Wireless optical mouse",
                Price = 29.99m,
                Stock = 100,
                Category = "Electronics",
                CreatedAt = DateTime.Now.AddDays(-5),
                IsActive = true
            });
            
            _products.Add(new Product
            {
                Id = _nextProductId++,
                Name = "Keyboard",
                Description = "Mechanical keyboard",
                Price = 79.99m,
                Stock = 75,
                Category = "Electronics",
                CreatedAt = DateTime.Now.AddDays(-3),
                IsActive = true
            });
            
            _products.Add(new Product
            {
                Id = _nextProductId++,
                Name = "Book",
                Description = "Programming book",
                Price = 39.99m,
                Stock = 25,
                Category = "Books",
                CreatedAt = DateTime.Now.AddDays(-1),
                IsActive = true
            });
        }
        
        public static List<User> GetUsers() => _users.ToList();
        public static User GetUserById(int id) => _users.FirstOrDefault(u => u.Id == id);
        public static void AddUser(User user)
        {
            user.Id = _nextUserId++;
            user.CreatedAt = DateTime.Now;
            user.IsActive = true;
            _users.Add(user);
        }
        public static bool UpdateUser(int id, User updatedUser)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null) return false;
            
            user.FirstName = updatedUser.FirstName;
            user.LastName = updatedUser.LastName;
            user.Email = updatedUser.Email;
            user.Username = updatedUser.Username;
            return true;
        }
        public static bool DeleteUser(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null) return false;
            
            return _users.Remove(user);
        }
        
        public static List<Product> GetProducts() => _products.Where(p => p.IsActive).ToList();
        public static Product GetProductById(int id) => _products.FirstOrDefault(p => p.Id == id && p.IsActive);
        public static void AddProduct(Product product)
        {
            product.Id = _nextProductId++;
            product.CreatedAt = DateTime.Now;
            product.IsActive = true;
            _products.Add(product);
        }
        public static bool UpdateProduct(int id, Product updatedProduct)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return false;
            
            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;
            product.Category = updatedProduct.Category;
            return true;
        }
        public static bool DeleteProduct(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null) return false;
            
            product.IsActive = false;
            return true;
        }
        
        public static List<Order> GetOrders() => _orders.ToList();
        public static List<Order> GetOrdersByUserId(int userId) => _orders.Where(o => o.UserId == userId).ToList();
        public static Order GetOrderById(int id) => _orders.FirstOrDefault(o => o.Id == id);
        public static void AddOrder(Order order)
        {
            order.Id = _nextOrderId++;
            order.OrderDate = DateTime.Now;
            order.Status = "Pending";
            _orders.Add(order);
        }
        public static bool UpdateOrderStatus(int id, string status)
        {
            var order = _orders.FirstOrDefault(o => o.Id == id);
            if (order == null) return false;
            
            order.Status = status;
            return true;
        }
    }
    
    // API Controllers
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        
        public UsersController(ILogger<UsersController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet]
        public ActionResult<ApiResponse<List<User>>> GetUsers()
        {
            try
            {
                var users = DataStore.GetUsers();
                return Ok(new ApiResponse<List<User>>
                {
                    Success = true,
                    Data = users,
                    Message = "Users retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new ApiResponse<List<User>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving users"
                });
            }
        }
        
        [HttpGet("{id:int}")]
        public ActionResult<ApiResponse<User>> GetUser(int id)
        {
            try
            {
                var user = DataStore.GetUserById(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }
                
                return Ok(new ApiResponse<User>
                {
                    Success = true,
                    Data = user,
                    Message = "User retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, new ApiResponse<User>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the user"
                });
            }
        }
        
        [HttpPost]
        public ActionResult<ApiResponse<User>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }
                
                // Check if username already exists
                var existingUsers = DataStore.GetUsers();
                if (existingUsers.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "Username already exists"
                    });
                }
                
                if (existingUsers.Any(u => u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "Email already exists"
                    });
                }
                
                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Username = request.Username
                };
                
                DataStore.AddUser(user);
                
                return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new ApiResponse<User>
                {
                    Success = true,
                    Data = user,
                    Message = "User created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new ApiResponse<User>
                {
                    Success = false,
                    Message = "An error occurred while creating the user"
                });
            }
        }
        
        [HttpPut("{id:int}")]
        public ActionResult<ApiResponse<User>> UpdateUser(int id, [FromBody] CreateUserRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }
                
                var updatedUser = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = request.Email,
                    Username = request.Username
                };
                
                var success = DataStore.UpdateUser(id, updatedUser);
                if (!success)
                {
                    return NotFound(new ApiResponse<User>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }
                
                var user = DataStore.GetUserById(id);
                return Ok(new ApiResponse<User>
                {
                    Success = true,
                    Data = user,
                    Message = "User updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new ApiResponse<User>
                {
                    Success = false,
                    Message = "An error occurred while updating the user"
                });
            }
        }
        
        [HttpDelete("{id:int}")]
        public ActionResult<ApiResponse<object>> DeleteUser(int id)
        {
            try
            {
                var success = DataStore.DeleteUser(id);
                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "User deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the user"
                });
            }
        }
    }
    
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        
        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet]
        public ActionResult<ApiResponse<List<Product>>> GetProducts()
        {
            try
            {
                var products = DataStore.GetProducts();
                return Ok(new ApiResponse<List<Product>>
                {
                    Success = true,
                    Data = products,
                    Message = "Products retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                return StatusCode(500, new ApiResponse<List<Product>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving products"
                });
            }
        }
        
        [HttpGet("{id:int}")]
        public ActionResult<ApiResponse<Product>> GetProduct(int id)
        {
            try
            {
                var product = DataStore.GetProductById(id);
                if (product == null)
                {
                    return NotFound(new ApiResponse<Product>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }
                
                return Ok(new ApiResponse<Product>
                {
                    Success = true,
                    Data = product,
                    Message = "Product retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product {ProductId}", id);
                return StatusCode(500, new ApiResponse<Product>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the product"
                });
            }
        }
        
        [HttpPost]
        public ActionResult<ApiResponse<Product>> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<Product>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }
                
                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Stock = request.Stock,
                    Category = request.Category
                };
                
                DataStore.AddProduct(product);
                
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new ApiResponse<Product>
                {
                    Success = true,
                    Data = product,
                    Message = "Product created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new ApiResponse<Product>
                {
                    Success = false,
                    Message = "An error occurred while creating the product"
                });
            }
        }
        
        [HttpPut("{id:int}")]
        public ActionResult<ApiResponse<Product>> UpdateProduct(int id, [FromBody] CreateProductRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<Product>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }
                
                var updatedProduct = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    Price = request.Price,
                    Stock = request.Stock,
                    Category = request.Category
                };
                
                var success = DataStore.UpdateProduct(id, updatedProduct);
                if (!success)
                {
                    return NotFound(new ApiResponse<Product>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }
                
                var product = DataStore.GetProductById(id);
                return Ok(new ApiResponse<Product>
                {
                    Success = true,
                    Data = product,
                    Message = "Product updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, new ApiResponse<Product>
                {
                    Success = false,
                    Message = "An error occurred while updating the product"
                });
            }
        }
        
        [HttpDelete("{id:int}")]
        public ActionResult<ApiResponse<object>> DeleteProduct(int id)
        {
            try
            {
                var success = DataStore.DeleteProduct(id);
                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Product not found"
                    });
                }
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Product deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while deleting the product"
                });
            }
        }
    }
    
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly ILogger<OrdersController> _logger;
        
        public OrdersController(ILogger<OrdersController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet]
        public ActionResult<ApiResponse<List<Order>>> GetOrders()
        {
            try
            {
                var orders = DataStore.GetOrders();
                return Ok(new ApiResponse<List<Order>>
                {
                    Success = true,
                    Data = orders,
                    Message = "Orders retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                return StatusCode(500, new ApiResponse<List<Order>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving orders"
                });
            }
        }
        
        [HttpGet("{id:int}")]
        public ActionResult<ApiResponse<Order>> GetOrder(int id)
        {
            try
            {
                var order = DataStore.GetOrderById(id);
                if (order == null)
                {
                    return NotFound(new ApiResponse<Order>
                    {
                        Success = false,
                        Message = "Order not found"
                    });
                }
                
                return Ok(new ApiResponse<Order>
                {
                    Success = true,
                    Data = order,
                    Message = "Order retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", id);
                return StatusCode(500, new ApiResponse<Order>
                {
                    Success = false,
                    Message = "An error occurred while retrieving the order"
                });
            }
        }
        
        [HttpPost]
        public ActionResult<ApiResponse<Order>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<Order>
                    {
                        Success = false,
                        Message = "Invalid request data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                    });
                }
                
                var order = new Order
                {
                    UserId = 1, // Hardcoded for demo
                    Items = new List<OrderItem>()
                };
                
                decimal totalAmount = 0;
                foreach (var itemRequest in request.Items)
                {
                    var product = DataStore.GetProductById(itemRequest.ProductId);
                    if (product == null)
                    {
                        return BadRequest(new ApiResponse<Order>
                        {
                            Success = false,
                            Message = $"Product with ID {itemRequest.ProductId} not found"
                        });
                    }
                    
                    if (product.Stock < itemRequest.Quantity)
                    {
                        return BadRequest(new ApiResponse<Order>
                        {
                            Success = false,
                            Message = $"Insufficient stock for product {product.Name}"
                        });
                    }
                    
                    var orderItem = new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = itemRequest.Quantity,
                        UnitPrice = product.Price
                    };
                    
                    order.Items.Add(orderItem);
                    totalAmount += orderItem.TotalPrice;
                }
                
                order.TotalAmount = totalAmount;
                DataStore.AddOrder(order);
                
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, new ApiResponse<Order>
                {
                    Success = true,
                    Data = order,
                    Message = "Order created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new ApiResponse<Order>
                {
                    Success = false,
                    Message = "An error occurred while creating the order"
                });
            }
        }
    }
    
    // Simple demo program
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Web API Demo ===");
            Console.WriteLine("This is a console demonstration of Web API concepts.");
            Console.WriteLine("In a real application, this would be an ASP.NET Core Web API project.");
            Console.WriteLine();
            
            // Demonstrate API responses
            DemonstrateUserAPI();
            DemonstrateProductAPI();
            DemonstrateOrderAPI();
            
            Console.WriteLine("=== API Endpoints Available ===");
            Console.WriteLine("GET    /api/users        - Get all users");
            Console.WriteLine("GET    /api/users/{id}    - Get user by ID");
            Console.WriteLine("POST   /api/users        - Create new user");
            Console.WriteLine("PUT    /api/users/{id}    - Update user");
            Console.WriteLine("DELETE /api/users/{id}    - Delete user");
            Console.WriteLine();
            Console.WriteLine("GET    /api/products     - Get all products");
            Console.WriteLine("GET    /api/products/{id} - Get product by ID");
            Console.WriteLine("POST   /api/products     - Create new product");
            Console.WriteLine("PUT    /api/products/{id} - Update product");
            Console.WriteLine("DELETE /api/products/{id} - Delete product");
            Console.WriteLine();
            Console.WriteLine("GET    /api/orders       - Get all orders");
            Console.WriteLine("GET    /api/orders/{id}   - Get order by ID");
            Console.WriteLine("POST   /api/orders       - Create new order");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        
        private static void DemonstrateUserAPI()
        {
            Console.WriteLine("=== User API Demo ===");
            
            // Get all users
            var users = DataStore.GetUsers();
            Console.WriteLine($"Total users: {users.Count}");
            foreach (var user in users.Take(2))
            {
                Console.WriteLine($"  {user.Id}: {user.FirstName} {user.LastName} ({user.Email})");
            }
            
            // Create new user
            var newUser = new User
            {
                FirstName = "Alice",
                LastName = "Johnson",
                Email = "alice@example.com",
                Username = "alicej"
            };
            DataStore.AddUser(newUser);
            Console.WriteLine($"Created user: {newUser.FirstName} {newUser.LastName} (ID: {newUser.Id})");
            Console.WriteLine();
        }
        
        private static void DemonstrateProductAPI()
        {
            Console.WriteLine("=== Product API Demo ===");
            
            // Get all products
            var products = DataStore.GetProducts();
            Console.WriteLine($"Total products: {products.Count}");
            foreach (var product in products.Take(3))
            {
                Console.WriteLine($"  {product.Id}: {product.Name} - ${product.Price} (Stock: {product.Stock})");
            }
            
            // Create new product
            var newProduct = new Product
            {
                Name = "Monitor",
                Description = "24-inch LCD monitor",
                Price = 299.99m,
                Stock = 30,
                Category = "Electronics"
            };
            DataStore.AddProduct(newProduct);
            Console.WriteLine($"Created product: {newProduct.Name} (ID: {newProduct.Id})");
            Console.WriteLine();
        }
        
        private static void DemonstrateOrderAPI()
        {
            Console.WriteLine("=== Order API Demo ===");
            
            // Create a sample order
            var order = new Order
            {
                UserId = 1,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        ProductName = "Laptop",
                        Quantity = 1,
                        UnitPrice = 999.99m
                    },
                    new OrderItem
                    {
                        ProductId = 2,
                        ProductName = "Mouse",
                        Quantity = 2,
                        UnitPrice = 29.99m
                    }
                }
            };
            
            // Calculate total
            order.TotalAmount = order.Items.Sum(item => item.TotalPrice);
            DataStore.AddOrder(order);
            
            Console.WriteLine($"Created order: {order.Id}");
            Console.WriteLine($"  Total amount: ${order.TotalAmount}");
            Console.WriteLine("  Items:");
            foreach (var item in order.Items)
            {
                Console.WriteLine($"    {item.ProductName} x{item.Quantity} = ${item.TotalPrice}");
            }
            Console.WriteLine();
        }
    }
}
