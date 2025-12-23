namespace AspDotNetTestProject
{
    public class MathOperations
    {
        /// <summary>
        /// Calculates the sum of two numbers
        /// </summary>
        /// <param name="a">First number</param>
        /// <param name="b">Second number</param>
        /// <returns>The sum of a and b</returns>
        public static int Sum(int a, int b)
        {
            return a + b;
        }

        /// <summary>
        /// Calculates the sum of multiple numbers
        /// </summary>
        /// <param name="numbers">Array of numbers to sum</param>
        /// <returns>The sum of all numbers</returns>
        public static int Sum(params int[] numbers)
        {
            int total = 0;
            foreach (int num in numbers)
            {
                total += num;
            }
            return total;
        }
    }
}
