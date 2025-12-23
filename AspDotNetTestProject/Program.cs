using AspDotNetTestProject;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

Random dice = new Random();
int roll = dice.Next(1, 7);
Console.WriteLine(roll);

// Using the Calculator class with summation methods
Console.WriteLine("\n--- Summation Examples ---");
Calculator calculator = new Calculator();
calculator.DemonstrateSummation();

// Direct usage of MathOperations
Console.WriteLine("\n--- Direct MathOperations Usage ---");
int sum1 = MathOperations.Sum(15, 25);
Console.WriteLine($"Direct sum of 15 and 25: {sum1}");

int sum2 = MathOperations.Sum(100, 200, 300);
Console.WriteLine($"Direct sum of 100, 200, 300: {sum2}");
