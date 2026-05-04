using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgorithmPerformanceEvaluator.Logic
{
    public static class DataGenerator
    {
        private static readonly Random _rng = new();

        public static int[] Random(int size) => Enumerable.Range(0, size).Select(_ => _rng.Next(1, size * 10)).ToArray();
        public static int[] Sorted(int size) => Enumerable.Range(1, size).ToArray();
        public static int[] Reversed(int size) => Enumerable.Range(1, size).Reverse().ToArray();

        // Parse comma-separated string into integer array
        public static int[] Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return Array.Empty<int>();
            return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse).ToArray();
        }

        // Standard sizes for O(n) or O(n log n)
        public static List<int> GetDefaultSizes() => new() { 10000, 50000, 100000, 500000, 1000000 };

        // Moderate sizes for O(n²) or O(n³) to prevent timeouts
        public static List<int> GetSmallSizes() => new() { 100, 200, 350, 500, 700 };

        // Tiny sizes for O(2ⁿ) as it grows extremely fast
        public static List<int> GetExponentialSizes() => new() { 10, 15, 18, 22, 25 };

        // Repeat base array elements to reach target sizes for Manual Mode
        public static List<int[]> Expand(int[] baseArray, List<int> sizes)
        {
            var result = new List<int[]>();
            foreach (var size in sizes)
            {
                var newArr = new int[size];
                for (int i = 0; i < size; i++)
                    newArr[i] = baseArray[i % baseArray.Length];
                result.Add(newArr);
            }
            return result;
        }

        // Generate scaled sizes based on user input length
        public static List<int> GetSmartSizes(int baseSize) =>
            new() { baseSize, baseSize * 2, baseSize * 4, baseSize * 8, baseSize * 16 };
    }
}