// Set console to support Unicode characters
Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Hello\nWorld!");
Console.WriteLine("Hello\tWorld!");

Console.WriteLine("Hello \"World\"!");
Console.WriteLine("c:\\source\\repos");

Console.WriteLine("Generating invoices for customer \"Contoso Corp\" ... \n");
Console.WriteLine("Invoice: 1021\t\tComplete!");
Console.WriteLine("Invoice: 1022\t\tComplete!");
Console.Write("\nOutput Directory:\t");

// Verbatim string literal
Console.WriteLine(@"    c:\source\repos    
        (this is where your code goes)");

Console.Write(@"c:\invoices");

// Kon'nichiwa World
Console.WriteLine("\u3053\u3093\u306B\u3061\u306F World!");

// To generate Japanese invoices:
// Nihon no seikyū-sho o seisei suru ni wa:
Console.Write("\n\n\u65e5\u672c\u306e\u8acb\u6c42\u66f8\u3092\u751f\u6210\u3059\u308b\u306b\u306f\uff1a\n\t");
// User command to run an application
Console.WriteLine(@"c:\invoices\app.exe -j");

// Showing Unicode values
char myChar = 'こ';
Console.WriteLine($"\nCharacter: {myChar}");
Console.WriteLine($"Unicode value (decimal): {(int)myChar}");
Console.WriteLine($"Unicode value (hex): \\u{(int)myChar:X4}");

// Show Unicode value for each character in a string
string text = "こんにちは";
Console.WriteLine($"\nUnicode values for '{text}':");
foreach (char c in text)
{
    Console.WriteLine($"  {c} = U+{(int)c:X4} (decimal: {(int)c})");
}

// String interpolation
string greeting = "Hello ";
string firstName = "Aariz";
string messages = $"{greeting}{firstName}";
Console.WriteLine(messages);

// Verbatim string with interpolation
string projectName2 = "First-Project";
Console.WriteLine($@"C:\Output\{projectName2}\Data");

//Challenge
string projectName = "ACME";
string russianMessage = "\u041f\u043e\u0441\u043c\u043e\u0442\u0440\u0435\u0442\u044c \u0440\u0443\u0441\u0441\u043a\u0438\u0439 \u0432\u044b\u0432\u043e\u0434";
string englishMessage = "View English output:";
string engFilePath = $@"C:\Exercise\{projectName}\data.txt";
string rusFilePath = $@"C:\Exercise\{projectName}\ru-RU\data.txt";
Console.WriteLine($"{englishMessage}\n\t{engFilePath}\n\n{russianMessage}\n\t{rusFilePath}");

