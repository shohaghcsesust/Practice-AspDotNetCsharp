namespace AspDotNetTestProject
{
    public class Calculator
    {
        /// <summary>
        /// Demonstrates using the Sum method from MathOperations
        /// </summary>
        public void DemonstrateSummation()
        {
            // Using Sum with two numbers
            int result1 = MathOperations.Sum(5, 10);
            Console.WriteLine($"Sum of 5 and 10: {result1}");

            // Using Sum with multiple numbers
            int result2 = MathOperations.Sum(1, 2, 3, 4, 5);
            Console.WriteLine($"Sum of 1, 2, 3, 4, 5: {result2}");

            // Using Sum with an array
            int[] numbers = { 10, 20, 30, 40 };
            int result3 = MathOperations.Sum(numbers);
            Console.WriteLine($"Sum of array elements: {result3}");
        }
    }
}
