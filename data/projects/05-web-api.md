# Web API Project in C#

A comprehensive RESTful API project that demonstrates modern web development using ASP.NET Core, including CRUD operations, authentication, and best practices.

## Project Overview

This Web API project will include:
- RESTful API endpoints
- Entity Framework Core integration
- Authentication and authorization
- Data validation
- Error handling
- API documentation
- Testing support

## Features

### Core API Features
- CRUD operations for entities
- RESTful endpoint design
- HTTP status codes
- Request/response models
- Data transfer objects (DTOs)
- Input validation

### Advanced Features
- JWT authentication
- Role-based authorization
- Pagination
- Filtering and searching
- Sorting
- Versioning
- Rate limiting

### Development Features
- Swagger/OpenAPI documentation
- Health checks
- Logging
- Unit testing
- Integration testing
- Docker support

## Project Structure

```
WebAPIProject/
├── Controllers/
│   ├── UsersController.cs
│   ├── ProductsController.cs
│   └── OrdersController.cs
├── Models/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Product.cs
│   │   └── Order.cs
│   ├── DTOs/
│   │   ├── UserDto.cs
│   │   ├── CreateUserDto.cs
│   │   └── UpdateUserDto.cs
│   └── Enums/
│       └── UserRole.cs
├── Services/
│   ├── Interfaces/
│   │   ├── IUserService.cs
│   │   ├── IProductService.cs
│   │   └── IOrderService.cs
│   └── Implementations/
│       ├── UserService.cs
│       ├── ProductService.cs
│       └── OrderService.cs
├── Repositories/
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Implementations/
│       ├── Repository.cs
│       └── UnitOfWork.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Configurations/
│       ├── UserConfiguration.cs
│       └── ProductConfiguration.cs
├── Middleware/
│   ├── ErrorHandlingMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Configuration/
│   └── ServiceCollectionExtensions.cs
├── Tests/
│   ├── Unit/
│   └── Integration/
└── Program.cs
```

## Core Classes

### Entity Models
```csharp
public class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<Order> Orders { get; set; }
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
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string ShippingAddress { get; set; }
    
    // Navigation properties
    public User User { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    
    // Navigation properties
    public Order Order { get; set; }
    public Product Product { get; set; }
}

public enum UserRole
{
    Customer,
    Admin
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}
```

### DTOs (Data Transfer Objects)
```csharp
public class UserDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(50)]
    public string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    [StringLength(30)]
    public string Username { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }
}

public class UpdateUserDto
{
    [StringLength(50)]
    public string FirstName { get; set; }
    
    [StringLength(50)]
    public string LastName { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProductDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(500)]
    public string Description { get; set; }
    
    [Required]
    [Range(0.01, 10000.00)]
    public decimal Price { get; set; }
    
    [Required]
    [Range(0, int.MaxValue)]
    public int Stock { get; set; }
    
    [StringLength(50)]
    public string Category { get; set; }
}
```

### API Controllers
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;
    
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string search = null)
    {
        var result = await _userService.GetUsersAsync(page, pageSize, search);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var user = await _userService.CreateUserAsync(createUserDto);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "An error occurred while creating the user.");
        }
    }
    
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var user = await _userService.UpdateUserAsync(id, updateUserDto);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "An error occurred while updating the user.");
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "An error occurred while deleting the user.");
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;
    
    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string category = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        var result = await _productService.GetProductsAsync(
            page, pageSize, category, minPrice, maxPrice, sortBy, sortOrder);
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var product = await _productService.CreateProductAsync(createProductDto);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "An error occurred while creating the product.");
        }
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var product = await _productService.UpdateProductAsync(id, updateProductDto);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "An error occurred while updating the product.");
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "An error occurred while deleting the product.");
        }
    }
}
```

### Service Layer
```csharp
public interface IUserService
{
    Task<PagedResult<UserDto>> GetUsersAsync(int page, int pageSize, string search);
    Task<UserDto> GetUserByIdAsync(int id);
    Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
    Task<bool> DeleteUserAsync(int id);
    Task<UserDto> AuthenticateAsync(string username, string password);
}

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    
    public UserService(
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }
    
    public async Task<PagedResult<UserDto>> GetUsersAsync(int page, int pageSize, string search)
    {
        var query = _userRepository.Query();
        
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(u => 
                u.FirstName.Contains(search) || 
                u.LastName.Contains(search) || 
                u.Email.Contains(search));
        }
        
        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var userDtos = _mapper.Map<List<UserDto>>(users);
        
        return new PagedResult<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };
    }
    
    public async Task<UserDto> GetUserByIdAsync(int id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }
    
    public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
    {
        // Check if username or email already exists
        var existingUser = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Username == createUserDto.Username || u.Email == createUserDto.Email);
        
        if (existingUser != null)
        {
            throw new InvalidOperationException("Username or email already exists");
        }
        
        var user = _mapper.Map<User>(createUserDto);
        user.PasswordHash = _passwordHasher.HashPassword(createUserDto.Password);
        user.CreatedAt = DateTime.UtcNow;
        user.IsActive = true;
        
        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();
        
        return _mapper.Map<UserDto>(user);
    }
    
    // Other methods implementation...
}
```

### Authentication Setup
```csharp
public class JwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    
    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["Jwt:SecretKey"];
        _issuer = _configuration["Jwt:Issuer"];
        _audience = _configuration["Jwt:Audience"];
    }
    
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
```

## Configuration

### Program.cs Setup
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings.GetValue<string>("SecretKey");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = jwtSettings.GetValue<string>("Issuer"),
            ValidateAudience = jwtSettings.GetValue<string>("Audience"),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web API", Version = "v1" });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Register custom services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();
```

## API Endpoints

### Users API
- `GET /api/users` - Get all users (with pagination and search)
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user (Admin only)
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user (Admin only)
- `POST /api/auth/login` - Authenticate user

### Products API
- `GET /api/products` - Get all products (with filtering, sorting, pagination)
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product (Admin only)
- `PUT /api/products/{id}` - Update product (Admin only)
- `DELETE /api/products/{id}` - Delete product (Admin only)

### Orders API
- `GET /api/orders` - Get user's orders
- `GET /api/orders/{id}` - Get order by ID
- `POST /api/orders` - Create new order
- `PUT /api/orders/{id}/status` - Update order status

## Testing

### Unit Testing Example
```csharp
[TestFixture]
public class UserServiceTests
{
    private IUserService _userService;
    private Mock<IRepository<User>> _mockUserRepository;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    
    [SetUp]
    public void Setup()
    {
        _mockUserRepository = new Mock<IRepository<User>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        
        _userService = new UserService(
            _mockUserRepository.Object,
            _mockUnitOfWork.Object,
            Mock.Of<IPasswordHasher>(),
            Mock.Of<IMapper>());
    }
    
    [Test]
    public async Task CreateUser_ValidUser_ReturnsUserDto()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Username = "johndoe",
            Password = "password123"
        };
        
        var user = new User { Id = 1, FirstName = "John", LastName = "Doe" };
        _mockUserRepository.Setup(r => r.Query())
            .Returns(new List<User>().AsQueryable());
        _mockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .Returns(Task.FromResult(1));
        
        // Act
        var result = await _userService.CreateUserAsync(createUserDto);
        
        // Assert
        Assert.IsNotNull(result);
        _mockUserRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
```

## Extension Ideas

1. **Real-time Features**: Add SignalR for real-time updates
2. **File Upload**: Add file upload and storage capabilities
3. **Caching**: Implement Redis caching
4. **Background Jobs**: Add Hangfire for background processing
5. **API Versioning**: Implement multiple API versions
6. **GraphQL**: Add GraphQL endpoint alongside REST
7. **Microservices**: Split into microservices architecture
8. **Docker**: Add containerization and orchestration

## Learning Objectives

This project helps you learn:
- ASP.NET Core web API development
- RESTful API design principles
- Entity Framework Core
- Authentication and authorization
- Data transfer objects (DTOs)
- Repository and Unit of Work patterns
- Dependency injection
- Error handling middleware
- API documentation with Swagger
- Unit and integration testing

## Best Practices Demonstrated

- Clean architecture
- Separation of concerns
- Dependency injection
- Async/await patterns
- Proper HTTP status codes
- Input validation
- Error handling
- Logging
- Testing strategies
- Security best practices
- Performance optimization
- Scalability considerations
