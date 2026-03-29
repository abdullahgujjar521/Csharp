using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BankingSystemProject
{
    public enum AccountType
    {
        Checking,
        Savings,
        Credit
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
            {
                Console.WriteLine("Withdrawal amount must be positive.");
                return false;
            }
            
            if (Balance - amount < MinimumBalance)
            {
                Console.WriteLine($"Insufficient funds. Minimum balance: ${MinimumBalance}");
                return false;
            }
            
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
                Console.WriteLine($"Deposited ${amount:F2}. New balance: ${Balance:F2}");
            }
        }
        
        public abstract decimal CalculateInterest();
        
        public override string ToString()
        {
            return $"Account #{AccountNumber} ({AccountType}) - {AccountHolderName} - Balance: ${Balance:F2}";
        }
    }
    
    public class CheckingAccount : Account
    {
        public decimal OverdraftLimit { get; set; }
        
        public CheckingAccount()
        {
            AccountType = AccountType.Checking;
            DailyWithdrawalLimit = 1000;
            MinimumBalance = -500; // Default overdraft limit
            OverdraftLimit = 500;
        }
        
        public override bool Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Withdrawal amount must be positive.");
                return false;
            }
            
            if (Balance - amount < -OverdraftLimit)
            {
                Console.WriteLine($"Overdraft limit exceeded. Limit: ${OverdraftLimit}");
                return false;
            }
            
            Balance -= amount;
            LastModified = DateTime.Now;
            Console.WriteLine($"Withdrew ${amount:F2}. New balance: ${Balance:F2}");
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
    
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int AccountNumber { get; set; }
        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? TargetAccountNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public string Status { get; set; }
        
        public override string ToString()
        {
            string targetInfo = TargetAccountNumber.HasValue ? $" -> Account {TargetAccountNumber}" : "";
            return $"{TransactionDate:MM/dd/yyyy HH:mm} | {TransactionType} | ${Amount:F2}{targetInfo} | Balance: ${Balance:F2} | {Description}";
        }
    }
    
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
        public List<Account> Accounts { get; set; } = new List<Account>();
        
        public string FullName => $"{FirstName} {LastName}";
        
        public override string ToString()
        {
            return $"Customer #{CustomerId}: {FullName} ({Username})";
        }
    }
    
    public class BankingService
    {
        private readonly List<Account> accounts = new List<Account>();
        private readonly List<Customer> customers = new List<Customer>();
        private readonly List<Transaction> transactions = new List<Transaction>();
        private int nextAccountNumber = 1001;
        private int nextCustomerId = 1;
        private int nextTransactionId = 1;
        
        public Customer CreateCustomer(string firstName, string lastName, string email, string username, string password)
        {
            var customer = new Customer
            {
                CustomerId = nextCustomerId++,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Username = username,
                PasswordHash = HashPassword(password),
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            
            customers.Add(customer);
            Console.WriteLine($"Customer created: {customer.FullName}");
            return customer;
        }
        
        public Account CreateAccount(Customer customer, AccountType accountType)
        {
            Account account = accountType switch
            {
                AccountType.Checking => new CheckingAccount(),
                AccountType.Savings => new SavingsAccount(),
                _ => throw new ArgumentException("Invalid account type")
            };
            
            account.AccountNumber = nextAccountNumber++;
            account.AccountHolderName = customer.FullName;
            account.CreatedDate = DateTime.Now;
            account.IsActive = true;
            
            accounts.Add(account);
            customer.Accounts.Add(account);
            
            Console.WriteLine($"Account created: {account}");
            return account;
        }
        
        public bool Deposit(int accountNumber, decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Deposit amount must be positive.");
                return false;
            }
            
            var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account == null)
            {
                Console.WriteLine("Account not found.");
                return false;
            }
            
            if (!account.IsActive)
            {
                Console.WriteLine("Account is not active.");
                return false;
            }
            
            account.Deposit(amount);
            
            var transaction = new Transaction
            {
                TransactionId = nextTransactionId++,
                AccountNumber = accountNumber,
                TransactionType = TransactionType.Deposit,
                Amount = amount,
                Balance = account.Balance,
                Description = "Cash deposit",
                TransactionDate = DateTime.Now,
                ReferenceNumber = GenerateReferenceNumber(),
                Status = "Completed"
            };
            
            transactions.Add(transaction);
            return true;
        }
        
        public bool Withdraw(int accountNumber, decimal amount)
        {
            var account = accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
            if (account == null)
            {
                Console.WriteLine("Account not found.");
                return false;
            }
            
            if (!account.IsActive)
            {
                Console.WriteLine("Account is not active.");
                return false;
            }
            
            if (account.Withdraw(amount))
            {
                var transaction = new Transaction
                {
                    TransactionId = nextTransactionId++,
                    AccountNumber = accountNumber,
                    TransactionType = TransactionType.Withdrawal,
                    Amount = amount,
                    Balance = account.Balance,
                    Description = "Cash withdrawal",
                    TransactionDate = DateTime.Now,
                    ReferenceNumber = GenerateReferenceNumber(),
                    Status = "Completed"
                };
                
                transactions.Add(transaction);
                return true;
            }
            
            return false;
        }
        
        public bool Transfer(int fromAccountNumber, int toAccountNumber, decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine("Transfer amount must be positive.");
                return false;
            }
            
            if (fromAccountNumber == toAccountNumber)
            {
                Console.WriteLine("Cannot transfer to the same account.");
                return false;
            }
            
            var fromAccount = accounts.FirstOrDefault(a => a.AccountNumber == fromAccountNumber);
            var toAccount = accounts.FirstOrDefault(a => a.AccountNumber == toAccountNumber);
            
            if (fromAccount == null || toAccount == null)
            {
                Console.WriteLine("One or both accounts not found.");
                return false;
            }
            
            if (!fromAccount.IsActive || !toAccount.IsActive)
            {
                Console.WriteLine("One or both accounts are not active.");
                return false;
            }
            
            if (fromAccount.Withdraw(amount))
            {
                toAccount.Deposit(amount);
                
                // Create withdrawal transaction
                var withdrawalTransaction = new Transaction
                {
                    TransactionId = nextTransactionId++,
                    AccountNumber = fromAccountNumber,
                    TransactionType = TransactionType.Transfer,
                    Amount = amount,
                    Balance = fromAccount.Balance,
                    Description = $"Transfer to account {toAccountNumber}",
                    TransactionDate = DateTime.Now,
                    TargetAccountNumber = toAccountNumber,
                    ReferenceNumber = GenerateReferenceNumber(),
                    Status = "Completed"
                };
                
                // Create deposit transaction
                var depositTransaction = new Transaction
                {
                    TransactionId = nextTransactionId++,
                    AccountNumber = toAccountNumber,
                    TransactionType = TransactionType.Transfer,
                    Amount = amount,
                    Balance = toAccount.Balance,
                    Description = $"Transfer from account {fromAccountNumber}",
                    TransactionDate = DateTime.Now,
                    TargetAccountNumber = fromAccountNumber,
                    ReferenceNumber = GenerateReferenceNumber(),
                    Status = "Completed"
                };
                
                transactions.Add(withdrawalTransaction);
                transactions.Add(depositTransaction);
                
                Console.WriteLine($"Transfer completed: ${amount:F2} from account {fromAccountNumber} to account {toAccountNumber}");
                return true;
            }
            
            return false;
        }
        
        public void ApplyInterest()
        {
            foreach (var account in accounts.Where(a => a.AccountType == AccountType.Savings && a.IsActive))
            {
                decimal interest = account.CalculateInterest();
                if (interest > 0)
                {
                    account.Balance += interest;
                    
                    var transaction = new Transaction
                    {
                        TransactionId = nextTransactionId++,
                        AccountNumber = account.AccountNumber,
                        TransactionType = TransactionType.Interest,
                        Amount = interest,
                        Balance = account.Balance,
                        Description = "Monthly interest",
                        TransactionDate = DateTime.Now,
                        ReferenceNumber = GenerateReferenceNumber(),
                        Status = "Completed"
                    };
                    
                    transactions.Add(transaction);
                    Console.WriteLine($"Interest applied to account {account.AccountNumber}: ${interest:F2}");
                }
            }
        }
        
        public Account GetAccount(int accountNumber)
        {
            return accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
        }
        
        public Customer GetCustomer(string username)
        {
            return customers.FirstOrDefault(c => c.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }
        
        public List<Transaction> GetTransactionHistory(int accountNumber)
        {
            return transactions
                .Where(t => t.AccountNumber == accountNumber)
                .OrderByDescending(t => t.TransactionDate)
                .ToList();
        }
        
        public List<Account> GetCustomerAccounts(int customerId)
        {
            var customer = customers.FirstOrDefault(c => c.CustomerId == customerId);
            return customer?.Accounts ?? new List<Account>();
        }
        
        public void PrintAccountStatement(int accountNumber)
        {
            var account = GetAccount(accountNumber);
            if (account == null)
            {
                Console.WriteLine("Account not found.");
                return;
            }
            
            var accountTransactions = GetTransactionHistory(accountNumber);
            
            Console.WriteLine($"\n=== Account Statement ===");
            Console.WriteLine($"Account: {account}");
            Console.WriteLine($"Generated: {DateTime.Now:MM/dd/yyyy HH:mm}");
            Console.WriteLine("\n=== Transactions ===");
            
            if (!accountTransactions.Any())
            {
                Console.WriteLine("No transactions found.");
            }
            else
            {
                foreach (var transaction in accountTransactions)
                {
                    Console.WriteLine(transaction);
                }
            }
            
            Console.WriteLine($"\nCurrent Balance: ${account.Balance:F2}");
            Console.WriteLine("========================\n");
        }
        
        private string HashPassword(string password)
        {
            // Simple hashing for demonstration (use proper hashing in production)
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password + "salt"));
        }
        
        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
        
        private string GenerateReferenceNumber()
        {
            return $"REF{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        }
    }
    
    public class BankingUI
    {
        private readonly BankingService bankingService = new BankingService();
        private Customer currentCustomer = null;
        private bool running = true;
        
        public void Run()
        {
            Console.WriteLine("=== Banking System ===");
            Console.WriteLine("Welcome to our banking system!");
            
            while (running)
            {
                if (currentCustomer == null)
                {
                    ShowMainMenu();
                }
                else
                {
                    ShowCustomerMenu();
                }
            }
        }
        
        private void ShowMainMenu()
        {
            Console.WriteLine("\n=== Main Menu ===");
            Console.WriteLine("1. Login");
            Console.WriteLine("2. Register");
            Console.WriteLine("3. Exit");
            Console.Write("Select an option: ");
            
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    Login();
                    break;
                case "2":
                    Register();
                    break;
                case "3":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
        
        private void ShowCustomerMenu()
        {
            Console.WriteLine($"\n=== Welcome, {currentCustomer.FullName} ===");
            Console.WriteLine("1. View Accounts");
            Console.WriteLine("2. Open New Account");
            Console.WriteLine("3. Deposit");
            Console.WriteLine("4. Withdraw");
            Console.WriteLine("5. Transfer");
            Console.WriteLine("6. View Transaction History");
            Console.WriteLine("7. Account Statement");
            Console.WriteLine("8. Apply Interest");
            Console.WriteLine("9. Logout");
            Console.Write("Select an option: ");
            
            string choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    ViewAccounts();
                    break;
                case "2":
                    OpenNewAccount();
                    break;
                case "3":
                    Deposit();
                    break;
                case "4":
                    Withdraw();
                    break;
                case "5":
                    Transfer();
                    break;
                case "6":
                    ViewTransactionHistory();
                    break;
                case "7":
                    PrintAccountStatement();
                    break;
                case "8":
                    bankingService.ApplyInterest();
                    break;
                case "9":
                    currentCustomer = null;
                    Console.WriteLine("Logged out successfully.");
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
        
        private void Login()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            
            Console.Write("Password: ");
            string password = Console.ReadLine();
            
            var customer = bankingService.GetCustomer(username);
            if (customer != null)
            {
                currentCustomer = customer;
                Console.WriteLine($"Welcome back, {customer.FullName}!");
            }
            else
            {
                Console.WriteLine("Invalid username or password.");
            }
        }
        
        private void Register()
        {
            Console.WriteLine("=== Register New Customer ===");
            
            Console.Write("First Name: ");
            string firstName = Console.ReadLine();
            
            Console.Write("Last Name: ");
            string lastName = Console.ReadLine();
            
            Console.Write("Email: ");
            string email = Console.ReadLine();
            
            Console.Write("Username: ");
            string username = Console.ReadLine();
            
            Console.Write("Password: ");
            string password = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || 
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("All fields are required.");
                return;
            }
            
            try
            {
                var customer = bankingService.CreateCustomer(firstName, lastName, email, username, password);
                currentCustomer = customer;
                Console.WriteLine($"Registration successful! Welcome, {customer.FullName}!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration failed: {ex.Message}");
            }
        }
        
        private void ViewAccounts()
        {
            var accounts = bankingService.GetCustomerAccounts(currentCustomer.CustomerId);
            
            Console.WriteLine("\n=== Your Accounts ===");
            if (!accounts.Any())
            {
                Console.WriteLine("You don't have any accounts yet.");
            }
            else
            {
                foreach (var account in accounts)
                {
                    Console.WriteLine(account);
                }
            }
        }
        
        private void OpenNewAccount()
        {
            Console.WriteLine("=== Open New Account ===");
            Console.WriteLine("1. Checking Account");
            Console.WriteLine("2. Savings Account");
            Console.Write("Select account type: ");
            
            string choice = Console.ReadLine();
            AccountType accountType = choice switch
            {
                "1" => AccountType.Checking,
                "2" => AccountType.Savings,
                _ => AccountType.Checking
            };
            
            try
            {
                var account = bankingService.CreateAccount(currentCustomer, accountType);
                Console.WriteLine($"Account opened successfully! Account Number: {account.AccountNumber}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open account: {ex.Message}");
            }
        }
        
        private void Deposit()
        {
            Console.Write("Enter account number: ");
            if (!int.TryParse(Console.ReadLine(), out int accountNumber))
            {
                Console.WriteLine("Invalid account number.");
                return;
            }
            
            Console.Write("Enter deposit amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Invalid amount.");
                return;
            }
            
            bankingService.Deposit(accountNumber, amount);
        }
        
        private void Withdraw()
        {
            Console.Write("Enter account number: ");
            if (!int.TryParse(Console.ReadLine(), out int accountNumber))
            {
                Console.WriteLine("Invalid account number.");
                return;
            }
            
            Console.Write("Enter withdrawal amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Invalid amount.");
                return;
            }
            
            bankingService.Withdraw(accountNumber, amount);
        }
        
        private void Transfer()
        {
            Console.Write("Enter from account number: ");
            if (!int.TryParse(Console.ReadLine(), out int fromAccount))
            {
                Console.WriteLine("Invalid account number.");
                return;
            }
            
            Console.Write("Enter to account number: ");
            if (!int.TryParse(Console.ReadLine(), out int toAccount))
            {
                Console.WriteLine("Invalid account number.");
                return;
            }
            
            Console.Write("Enter transfer amount: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal amount))
            {
                Console.WriteLine("Invalid amount.");
                return;
            }
            
            bankingService.Transfer(fromAccount, toAccount, amount);
        }
        
        private void ViewTransactionHistory()
        {
            Console.Write("Enter account number: ");
            if (!int.TryParse(Console.ReadLine(), out int accountNumber))
            {
                Console.WriteLine("Invalid account number.");
                return;
            }
            
            var transactions = bankingService.GetTransactionHistory(accountNumber);
            
            Console.WriteLine("\n=== Transaction History ===");
            if (!transactions.Any())
            {
                Console.WriteLine("No transactions found.");
            }
            else
            {
                foreach (var transaction in transactions.Take(20)) // Show last 20 transactions
                {
                    Console.WriteLine(transaction);
                }
            }
        }
        
        private void PrintAccountStatement()
        {
            Console.Write("Enter account number: ");
            if (!int.TryParse(Console.ReadLine(), out int accountNumber))
            {
                Console.WriteLine("Invalid account number.");
                return;
            }
            
            bankingService.PrintAccountStatement(accountNumber);
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var bankingUI = new BankingUI();
            bankingUI.Run();
            
            Console.WriteLine("Thank you for using our Banking System!");
        }
    }
}
