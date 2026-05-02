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

        public static int[] Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return Array.Empty<int>();
            return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse).ToArray();
        }

        // 1. الأحجام العادية لـ O(n) و O(n log n)
        public static List<int> GetDefaultSizes() => new() { 10000, 50000, 100000, 500000, 1000000 };

        // 2. أحجام متوسطة لـ O(n²) و O(n³) لضمان عدم حدوث Timeout
        public static List<int> GetSmallSizes() => new() { 100, 200, 350, 500, 700 };

        // 3. أحجام ميكروسكوبية لـ O(2ⁿ) لأنها تنهار بعد n=30
        public static List<int> GetExponentialSizes() => new() { 10, 15, 18, 22, 25 };

        public static List<int[]> Expand(int[] baseArray, List<int> sizes)
        {
            var result = new List<int[]>();
            foreach (var size in sizes)
            {
                var newArr = new int[size];
                for (int i = 0; i < size; i++) newArr[i] = baseArray[i % baseArray.Length];
                result.Add(newArr);
            }
            return result;
        }

        public static List<int> GetSmartSizes(int baseSize) => new() { baseSize, baseSize * 2, baseSize * 4, baseSize * 8, baseSize * 16 };
    }
}