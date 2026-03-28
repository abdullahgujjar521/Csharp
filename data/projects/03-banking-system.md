# Banking System Project in C#

A comprehensive banking system that demonstrates financial transaction management, security, and data persistence.

## Project Overview

This banking system will include:
- Account management
- Transaction processing
- Security and authentication
- Reporting and analytics
- Data persistence
- Error handling and validation

## Features

### Core Banking Features
- Multiple account types (Checking, Savings, Credit)
- Account creation and management
- Deposit and withdrawal operations
- Transfer between accounts
- Balance inquiries
- Account statements

### Advanced Features
- Transaction history
- Interest calculation
- Overdraft protection
- Account limits and restrictions
- Scheduled transactions
- Multi-currency support

### Security Features
- User authentication
- Role-based access
- Transaction limits
- Audit logging
- Encryption for sensitive data

### Reporting Features
- Account statements
- Transaction reports
- Balance summaries
- Tax documents
- Export functionality

## Project Structure

```
BankingSystem/
├── Models/
│   ├── Account.cs
│   ├── Customer.cs
│   ├── Transaction.cs
│   ├── AccountType.cs
│   └── TransactionType.cs
├── Services/
│   ├── IBankingService.cs
│   ├── BankingService.cs
│   ├── IAuthenticationService.cs
│   ├── AuthenticationService.cs
│   ├── IReportingService.cs
│   └── ReportingService.cs
├── Repositories/
│   ├── IAccountRepository.cs
│   ├── AccountRepository.cs
│   ├── ITransactionRepository.cs
│   └── TransactionRepository.cs
├── Security/
│   ├── PasswordHasher.cs
│   ├── EncryptionService.cs
│   └── AuditLogger.cs
├── UI/
│   └── BankingUI.cs
└── Program.cs
```

## Core Classes

### Account Model
```csharp
public abstract class Account
{
    public int AccountNumber { get; set; }
    public string AccountHolderName { get; set; }
    public decimal Balance { get; protected set; }
    public AccountType AccountType { get; protected set; }
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsActive { get; set; }
    public decimal DailyWithdrawalLimit { get; protected set; }
    public decimal MinimumBalance { get; protected set; }
    
    public virtual bool Withdraw(decimal amount)
    {
        if (amount <= 0)
            return false;
            
        if (Balance - amount < MinimumBalance)
            return false;
            
        Balance -= amount;
        LastModified = DateTime.Now;
        return true;
    }
    
    public virtual void Deposit(decimal amount)
    {
        if (amount > 0)
        {
            Balance += amount;
            LastModified = DateTime.Now;
        }
    }
    
    public abstract decimal CalculateInterest();
}

public class CheckingAccount : Account
{
    public decimal OverdraftLimit { get; set; }
    
    public CheckingAccount()
    {
        AccountType = AccountType.Checking;
        DailyWithdrawalLimit = 1000;
        MinimumBalance = -OverdraftLimit;
    }
    
    public override bool Withdraw(decimal amount)
    {
        if (amount <= 0)
            return false;
            
        if (Balance - amount < -OverdraftLimit)
            return false;
            
        Balance -= amount;
        LastModified = DateTime.Now;
        return true;
    }
    
    public override decimal CalculateInterest()
    {
        return 0; // Checking accounts typically don't earn interest
    }
}

public class SavingsAccount : Account
{
    public decimal InterestRate { get; set; }
    
    public SavingsAccount()
    {
        AccountType = AccountType.Savings;
        DailyWithdrawalLimit = 500;
        MinimumBalance = 100;
        InterestRate = 0.02m; // 2% annual interest
    }
    
    public override decimal CalculateInterest()
    {
        return Balance * InterestRate / 12; // Monthly interest
    }
}
```

### Customer Model
```csharp
public class Customer
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
    public List<Account> Accounts { get; set; } = new List<Account>();
    
    public string FullName => $"{FirstName} {LastName}";
    public int Age => DateTime.Now.Year - DateOfBirth.Year;
}
```

### Transaction Model
```csharp
public class Transaction
{
    public int TransactionId { get; set; }
    public int AccountNumber { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public int? TargetAccountNumber { get; set; } // For transfers
    public string ReferenceNumber { get; set; }
    public string Status { get; set; }
    
    public override string ToString()
    {
        return $"{TransactionDate:MM/dd/yyyy HH:mm} | {TransactionType} | {Amount:C} | Balance: {Balance:C} | {Description}";
    }
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    Interest,
    Fee,
    Adjustment
}
```

### Banking Service
```csharp
public interface IBankingService
{
    Task<Account> CreateAccountAsync(Customer customer, AccountType accountType);
    Task<bool> DepositAsync(int accountNumber, decimal amount);
    Task<bool> WithdrawAsync(int accountNumber, decimal amount);
    Task<bool> TransferAsync(int fromAccount, int toAccount, decimal amount);
    Task<Account> GetAccountAsync(int accountNumber);
    Task<IEnumerable<Transaction>> GetTransactionHistoryAsync(int accountNumber, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> CloseAccountAsync(int accountNumber);
    Task<decimal> CalculateInterestAsync(int accountNumber);
}

public class BankingService : IBankingService
{
    private readonly IAccountRepository accountRepository;
    private readonly ITransactionRepository transactionRepository;
    private readonly IAuditLogger auditLogger;
    
    public BankingService(IAccountRepository accountRepo, ITransactionRepository transactionRepo, IAuditLogger logger)
    {
        accountRepository = accountRepo;
        transactionRepository = transactionRepo;
        auditLogger = logger;
    }
    
    public async Task<Account> CreateAccountAsync(Customer customer, AccountType accountType)
    {
        Account account = accountType switch
        {
            AccountType.Checking => new CheckingAccount(),
            AccountType.Savings => new SavingsAccount(),
            _ => throw new ArgumentException("Invalid account type")
        };
        
        account.AccountNumber = GenerateAccountNumber();
        account.AccountHolderName = customer.FullName;
        account.CreatedDate = DateTime.Now;
        account.IsActive = true;
        
        await accountRepository.AddAsync(account);
        await auditLogger.LogAsync($"Account {account.AccountNumber} created for customer {customer.CustomerId}");
        
        return account;
    }
    
    public async Task<bool> DepositAsync(int accountNumber, decimal amount)
    {
        if (amount <= 0)
            return false;
            
        var account = await accountRepository.GetAsync(accountNumber);
        if (account == null || !account.IsActive)
            return false;
            
        account.Deposit(amount);
        await accountRepository.UpdateAsync(account);
        
        var transaction = new Transaction
        {
            AccountNumber = accountNumber,
            TransactionType = TransactionType.Deposit,
            Amount = amount,
            Balance = account.Balance,
            Description = "Deposit",
            TransactionDate = DateTime.Now,
            ReferenceNumber = GenerateReferenceNumber(),
            Status = "Completed"
        };
        
        await transactionRepository.AddAsync(transaction);
        await auditLogger.LogAsync($"Deposit of {amount:C} to account {accountNumber}");
        
        return true;
    }
    
    // Other methods implementation...
}
```

## Implementation Steps

### Step 1: Create Core Models
1. Define Account, Customer, and Transaction classes
2. Create account type hierarchies
3. Implement validation rules

### Step 2: Implement Data Layer
1. Create repository interfaces and implementations
2. Add data persistence (JSON or database)
3. Implement transaction logging

### Step 3: Build Business Logic
1. Implement banking service
2. Add transaction processing
3. Implement security measures

### Step 4: Create User Interface
1. Design console-based banking interface
2. Implement authentication
3. Add reporting features

## Usage Examples

### Creating an Account
```csharp
var customer = new Customer
{
    FirstName = "John",
    LastName = "Doe",
    Email = "john.doe@email.com",
    Username = "johndoe"
};

var account = await bankingService.CreateAccountAsync(customer, AccountType.Savings);
```

### Processing a Transaction
```csharp
bool success = await bankingService.DepositAsync(account.AccountNumber, 1000);
if (success)
{
    Console.WriteLine("Deposit successful");
}
```

### Getting Transaction History
```csharp
var transactions = await bankingService.GetTransactionHistoryAsync(account.AccountNumber);
foreach (var transaction in transactions)
{
    Console.WriteLine(transaction.ToString());
}
```

## Security Considerations

### Password Security
```csharp
public class PasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

### Transaction Limits
- Daily withdrawal limits
- Per-transaction limits
- Frequency limits
- Geographic restrictions

### Audit Logging
```csharp
public interface IAuditLogger
{
    Task LogAsync(string message);
    Task LogAsync(string message, string userId);
    Task LogTransactionAsync(Transaction transaction);
}
```

## Error Handling

The system includes comprehensive error handling for:
- Invalid account numbers
- Insufficient funds
- Transaction limits exceeded
- Network failures
- Data corruption
- Security breaches

## Extension Ideas

1. **Mobile Banking**: Create a mobile app interface
2. **Web Banking**: ASP.NET Core web application
3. **Investment Services**: Add investment account types
4. **Loan Management**: Implement loan processing
5. **Bill Payment**: Add bill payment services
6. **Currency Exchange**: Multi-currency support
7. **Fraud Detection**: AI-powered fraud detection
8. **API Integration**: Connect with external banking APIs

## Learning Objectives

This project helps you learn:
- Financial domain modeling
- Transaction processing
- Security best practices
- Data persistence strategies
- Error handling and validation
- Audit logging
- Business rule implementation
- Repository pattern
- Service layer architecture

## Best Practices Demonstrated

- Domain-driven design
- Repository pattern
- Service layer architecture
- Dependency injection
- Secure coding practices
- Transaction management
- Audit logging
- Input validation
- Error handling
- Separation of concerns
