using System;
using System.Collections.Generic;

namespace CalculatorProject
{
    public class OperationResult
    {
        public double Result { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        
        public static OperationResult SuccessResult(double result)
        {
            return new OperationResult
            {
                Result = result,
                Success = true
            };
        }
        
        public static OperationResult ErrorResult(string errorMessage)
        {
            return new OperationResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
    
    public class Calculator
    {
        private double currentValue = 0;
        private double storedValue = 0;
        private string currentOperation = "";
        private bool isNewNumber = true;
        private double memory = 0;
        private readonly List<string> history = new List<string>();
        
        public double CurrentValue => currentValue;
        public double StoredValue => storedValue;
        public double Memory => memory;
        public string CurrentOperation => currentOperation;
        public IReadOnlyList<string> History => history.AsReadOnly();
        
        public void InputDigit(double digit)
        {
            if (isNewNumber)
            {
                currentValue = digit;
                isNewNumber = false;
            }
            else
            {
                // Check if we're dealing with integers
                if (currentValue % 1 == 0 && digit % 1 == 0)
                {
                    currentValue = currentValue * 10 + digit;
                }
                else
                {
                    currentValue += digit;
                }
            }
        }
        
        public void InputDecimal()
        {
            if (isNewNumber)
            {
                currentValue = 0;
                isNewNumber = false;
            }
            
            if (!currentValue.ToString().Contains("."))
            {
                currentValue = double.Parse(currentValue.ToString() + ".");
            }
        }
        
        public void SetOperation(string operation)
        {
            if (!string.IsNullOrEmpty(currentOperation) && !isNewNumber)
            {
                Calculate();
            }
            
            storedValue = currentValue;
            currentOperation = operation;
            isNewNumber = true;
        }
        
        public OperationResult Calculate()
        {
            if (string.IsNullOrEmpty(currentOperation))
            {
                return OperationResult.SuccessResult(currentValue);
            }
            
            try
            {
                double result = PerformOperation(storedValue, currentValue, currentOperation);
                
                // Add to history
                string historyEntry = $"{storedValue} {currentOperation} {currentValue} = {result}";
                history.Add(historyEntry);
                
                currentValue = result;
                currentOperation = "";
                isNewNumber = true;
                
                return OperationResult.SuccessResult(result);
            }
            catch (Exception ex)
            {
                return OperationResult.ErrorResult(ex.Message);
            }
        }
        
        private double PerformOperation(double first, double second, string operation)
        {
            return operation switch
            {
                "+" => first + second,
                "-" => first - second,
                "*" => first * second,
                "/" => second != 0 ? first / second : throw new DivideByZeroException("Cannot divide by zero"),
                "^" => Math.Pow(first, second),
                "%" => first * (second / 100),
                _ => throw new ArgumentException($"Unknown operation: {operation}")
            };
        }
        
        public OperationResult SquareRoot()
        {
            if (currentValue < 0)
            {
                return OperationResult.ErrorResult("Cannot calculate square root of negative number");
            }
            
            double result = Math.Sqrt(currentValue);
            string historyEntry = $"√{currentValue} = {result}";
            history.Add(historyEntry);
            
            currentValue = result;
            isNewNumber = true;
            
            return OperationResult.SuccessResult(result);
        }
        
        public OperationResult Percentage()
        {
            double result = currentValue / 100;
            string historyEntry = $"{currentValue}% = {result}";
            history.Add(historyEntry);
            
            currentValue = result;
            isNewNumber = true;
            
            return OperationResult.SuccessResult(result);
        }
        
        public OperationResult Reciprocal()
        {
            if (currentValue == 0)
            {
                return OperationResult.ErrorResult("Cannot calculate reciprocal of zero");
            }
            
            double result = 1 / currentValue;
            string historyEntry = $"1/{currentValue} = {result}";
            history.Add(historyEntry);
            
            currentValue = result;
            isNewNumber = true;
            
            return OperationResult.SuccessResult(result);
        }
        
        public void ChangeSign()
        {
            currentValue = -currentValue;
        }
        
        public void Clear()
        {
            currentValue = 0;
            storedValue = 0;
            currentOperation = "";
            isNewNumber = true;
        }
        
        public void ClearEntry()
        {
            currentValue = 0;
            isNewNumber = true;
        }
        
        public void Backspace()
        {
            if (!isNewNumber)
            {
                string currentValueStr = currentValue.ToString();
                if (currentValueStr.Length > 1)
                {
                    currentValueStr = currentValueStr.Substring(0, currentValueStr.Length - 1);
                    currentValue = double.Parse(currentValueStr);
                }
                else
                {
                    currentValue = 0;
                    isNewNumber = true;
                }
            }
        }
        
        // Memory operations
        public void MemoryStore()
        {
            memory = currentValue;
            isNewNumber = true;
        }
        
        public void MemoryRecall()
        {
            currentValue = memory;
            isNewNumber = true;
        }
        
        public void MemoryClear()
        {
            memory = 0;
        }
        
        public void MemoryAdd()
        {
            memory += currentValue;
            isNewNumber = true;
        }
        
        public void MemorySubtract()
        {
            memory -= currentValue;
            isNewNumber = true;
        }
        
        public void ClearHistory()
        {
            history.Clear();
        }
        
        public override string ToString()
        {
            if (string.IsNullOrEmpty(currentOperation))
            {
                return currentValue.ToString();
            }
            
            return $"{storedValue} {currentOperation} {(isNewNumber ? "" : currentValue.ToString())}";
        }
    }
    
    public class CalculatorUI
    {
        private readonly Calculator calculator = new Calculator();
        private bool running = true;
        
        public void Run()
        {
            Console.WriteLine("=== C# Calculator ===");
            Console.WriteLine("Commands: +, -, *, /, ^, %, sqrt, reciprocal, clear, exit");
            Console.WriteLine("Memory: ms (store), mr (recall), mc (clear), m+, m-");
            Console.WriteLine("Other: backspace, sign, history, help");
            Console.WriteLine();
            
            while (running)
            {
                Console.Write($"Calculator: {calculator}");
                Console.Write(" > ");
                
                string input = Console.ReadLine()?.ToLower().Trim();
                
                if (string.IsNullOrEmpty(input))
                    continue;
                
                ProcessInput(input);
            }
        }
        
        private void ProcessInput(string input)
        {
            // Try to parse as number first
            if (double.TryParse(input, out double number))
            {
                calculator.InputDigit(number);
                return;
            }
            
            // Handle special commands
            switch (input)
            {
                case "+":
                case "-":
                case "*":
                case "/":
                case "^":
                case "%":
                    calculator.SetOperation(input);
                    break;
                    
                case "=":
                case "calculate":
                    var result = calculator.Calculate();
                    if (result.Success)
                    {
                        Console.WriteLine($"Result: {result.Result}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {result.ErrorMessage}");
                    }
                    break;
                    
                case "sqrt":
                case "√":
                    var sqrtResult = calculator.SquareRoot();
                    if (sqrtResult.Success)
                    {
                        Console.WriteLine($"√ = {sqrtResult.Result}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {sqrtResult.ErrorMessage}");
                    }
                    break;
                    
                case "reciprocal":
                case "1/x":
                    var recResult = calculator.Reciprocal();
                    if (recResult.Success)
                    {
                        Console.WriteLine($"1/x = {recResult.Result}");
                    }
                    else
                    {
                        Console.WriteLine($"Error: {recResult.ErrorMessage}");
                    }
                    break;
                    
                case "percent":
                case "%":
                    var percentResult = calculator.Percentage();
                    Console.WriteLine($"% = {percentResult.Result}");
                    break;
                    
                case "sign":
                case "+/-":
                    calculator.ChangeSign();
                    break;
                    
                case "clear":
                case "c":
                    calculator.Clear();
                    break;
                    
                case "ce":
                    calculator.ClearEntry();
                    break;
                    
                case "backspace":
                case "bs":
                    calculator.Backspace();
                    break;
                    
                case "ms":
                    calculator.MemoryStore();
                    Console.WriteLine("Stored in memory");
                    break;
                    
                case "mr":
                    calculator.MemoryRecall();
                    Console.WriteLine("Recalled from memory");
                    break;
                    
                case "mc":
                    calculator.MemoryClear();
                    Console.WriteLine("Memory cleared");
                    break;
                    
                case "m+":
                    calculator.MemoryAdd();
                    Console.WriteLine("Added to memory");
                    break;
                    
                case "m-":
                    calculator.MemorySubtract();
                    Console.WriteLine("Subtracted from memory");
                    break;
                    
                case "history":
                case "h":
                    ShowHistory();
                    break;
                    
                case "help":
                    ShowHelp();
                    break;
                    
                case "exit":
                case "quit":
                    running = false;
                    break;
                    
                default:
                    Console.WriteLine($"Unknown command: {input}");
                    break;
            }
        }
        
        private void ShowHistory()
        {
            Console.WriteLine("\n=== Calculation History ===");
            if (calculator.History.Count == 0)
            {
                Console.WriteLine("No calculations yet.");
            }
            else
            {
                for (int i = 0; i < calculator.History.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {calculator.History[i]}");
                }
            }
            Console.WriteLine($"Memory: {calculator.Memory}");
            Console.WriteLine();
        }
        
        private void ShowHelp()
        {
            Console.WriteLine("\n=== Calculator Help ===");
            Console.WriteLine("Numbers: Enter any number (e.g., 5, 3.14, -10)");
            Console.WriteLine("Operations: +, -, *, /, ^ (power), % (percentage)");
            Console.WriteLine("Functions:");
            Console.WriteLine("  sqrt or √     - Square root");
            Console.WriteLine("  reciprocal or 1/x - Reciprocal");
            Console.WriteLine("  percent or %   - Percentage");
            Console.WriteLine("  sign or +/-    - Change sign");
            Console.WriteLine("Memory:");
            Console.WriteLine("  ms            - Memory store");
            Console.WriteLine("  mr            - Memory recall");
            Console.WriteLine("  mc            - Memory clear");
            Console.WriteLine("  m+            - Memory add");
            Console.WriteLine("  m-            - Memory subtract");
            Console.WriteLine("Control:");
            Console.WriteLine("  = or calculate - Perform calculation");
            Console.WriteLine("  clear or c    - Clear all");
            Console.WriteLine("  ce            - Clear entry");
            Console.WriteLine("  backspace     - Remove last digit");
            Console.WriteLine("  history or h  - Show history");
            Console.WriteLine("  exit          - Exit calculator");
            Console.WriteLine();
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var calculatorUI = new CalculatorUI();
            calculatorUI.Run();
            
            Console.WriteLine("Thank you for using C# Calculator!");
        }
    }
}
