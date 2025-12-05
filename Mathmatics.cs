int sum = 7 + 5;
int difference = 7 - 5;
int product = 7 * 5;
int quotient = 7 / 5;

Console.WriteLine("Sum: " + sum);
Console.WriteLine("Difference: " + difference);
Console.WriteLine("Product: " + product);
Console.WriteLine("Quotient: " + quotient);

decimal decimalQuotient = 7.0m / 5;
Console.WriteLine($"Decimal quotient: {decimalQuotient}");

Console.WriteLine($"Modulus of 200 / 5 : {200 % 5}");
Console.WriteLine($"Modulus of 7 / 5 : {7 % 5}");

// PEMDAS example
// Parentheses, Exponents, Multiplication, Division, Addition, Subtraction
int value1 = 3 + 4 * 5;
int value2 = (3 + 4) * 5;
Console.WriteLine(value1);
Console.WriteLine(value2);

int value3 = 0;     // value is now 0.
value3 = value3 + 5; // value is now 5.
value3 += 5;        // value is now 10.

int value4 = 0;     // value is now 0.
value4 = value4 + 1; // value is now 1.
value4++;           // value is now 2.

// Challenge
int value5 = 1;

value5 = value5 + 1;
Console.WriteLine("First increment: " + value5);

value5 += 1;
Console.WriteLine("Second increment: " + value5);

value5++;
Console.WriteLine("Third increment: " + value5);

value5 = value5 - 1;
Console.WriteLine("First decrement: " + value5);

value5 -= 1;
Console.WriteLine("Second decrement: " + value5);

value5--;
Console.WriteLine("Third decrement: " + value5);

//Challenge 2
int fahrenheit = 94;
decimal celsius = (fahrenheit - 32) * 5 / 9m;
Console.WriteLine($"The temperature is {celsius} Celsius.");