# Calculator Project in C#

A comprehensive calculator project that demonstrates basic C# concepts including arithmetic operations, user input handling, and program structure.

## Project Overview

This calculator project will include:
- Basic arithmetic operations (add, subtract, multiply, divide)
- Advanced operations (power, square root, percentage)
- Memory functions
- Error handling
- User interface

## Features

### Basic Operations
- Addition (+)
- Subtraction (-)
- Multiplication (*)
- Division (/)

### Advanced Operations
- Power (x^y)
- Square root (√)
- Percentage (%)
- Reciprocal (1/x)

### Memory Functions
- Memory Store (MS)
- Memory Recall (MR)
- Memory Clear (MC)
- Memory Add (M+)
- Memory Subtract (M-)

### Additional Features
- Clear (C)
- Clear Entry (CE)
- Backspace
- Sign change (+/-)
- Decimal point support

## Project Structure

```
Calculator/
├── Models/
│   ├── Calculator.cs
│   └── OperationResult.cs
├── Services/
│   ├── ICalculatorService.cs
│   └── CalculatorService.cs
├── UI/
│   └── CalculatorUI.cs
└── Program.cs
```

## Core Classes

### Calculator Model
```csharp
public class Calculator
{
    private double currentValue = 0;
    private double storedValue = 0;
    private string currentOperation = "";
    private bool isNewNumber = true;
    
    public double CurrentValue => currentValue;
    public double StoredValue => storedValue;
    public string CurrentOperation => currentOperation;
    
    public void InputDigit(double digit)
    {
        if (isNewNumber)
        {
            currentValue = digit;
            isNewNumber = false;
        }
        else
        {
            currentValue = currentValue * 10 + digit;
        }
    }
    
    public void InputDecimal()
    {
        // Handle decimal point input
    }
    
    public void SetOperation(string operation)
    {
        // Set the current operation
    }
    
    public OperationResult Calculate()
    {
        // Perform the calculation
    }
}
```

### Operation Result
```csharp
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
```

## Implementation Steps

### Step 1: Create Basic Structure
1. Create the Calculator class
2. Implement basic arithmetic operations
3. Add error handling for division by zero

### Step 2: Add Advanced Features
1. Implement power and square root functions
2. Add memory operations
3. Handle edge cases (negative square roots, etc.)

### Step 3: Create User Interface
1. Design console-based UI
2. Handle user input
3. Display results and error messages

### Step 4: Add Validation
1. Input validation
2. Error handling
3. Edge case management

## Usage Examples

### Basic Calculation
```csharp
var calculator = new Calculator();
calculator.InputDigit(5);
calculator.SetOperation("+");
calculator.InputDigit(3);
var result = calculator.Calculate();
Console.WriteLine($"Result: {result.Result}"); // Output: 8
```

### Advanced Calculation
```csharp
var calculator = new Calculator();
calculator.InputDigit(16);
var sqrtResult = calculator.SquareRoot();
Console.WriteLine($"√16 = {sqrtResult.Result}"); // Output: 4
```

## Error Handling

The calculator includes comprehensive error handling for:
- Division by zero
- Invalid operations
- Overflow errors
- Invalid input formats

## Extension Ideas

1. **Scientific Mode**: Add trigonometric functions
2. **Unit Converter**: Convert between units
3. **History**: Track calculation history
4. **Graphical UI**: Create a Windows Forms or WPF version
5. **Expression Parser**: Parse complex mathematical expressions

## Learning Objectives

This project helps you learn:
- Basic C# syntax and data types
- Class design and encapsulation
- Error handling and validation
- User input processing
- Program structure and organization
- Mathematical operations in C#

## Best Practices Demonstrated

- Separation of concerns (UI vs business logic)
- Proper error handling
- Input validation
- Clean code principles
- Object-oriented design
- Service pattern implementation
