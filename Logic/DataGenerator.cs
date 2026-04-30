using System;
using System.Collections.Generic;
using System.Linq;

namespace AlgorithmPerformanceEvaluator.Logic
{
    public static class DataGenerator
    {
        private static readonly Random _rng = new();

        // 1. الحالة المتوسطة: أرقام عشوائية تماماً
        public static int[] Random(int size)
        {
            var arr = new int[size];
            for (int i = 0; i < size; i++)
                arr[i] = _rng.Next(1, size * 10);
            return arr;
        }

        // 2. الحالة الفضلى: مصفوفة مترتبة جاهزة (عشان نقيس الـ Best Case)
        public static int[] Sorted(int size)
        {
            var arr = new int[size];
            for (int i = 0; i < size; i++) arr[i] = i + 1;
            return arr;
        }

        // 3. الحالة الأسوأ: مصفوفة مترتبة بالعكس (Worst Case)
        public static int[] Reversed(int size)
        {
            var arr = new int[size];
            for (int i = 0; i < size; i++) arr[i] = size - i;
            return arr;
        }

        // 4. وضع الـ Manual: بيحول النص (مثل "1,2,3") لمصفوفة حقيقية
        public static int[] Parse(string input)
        {
            return input.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => int.TryParse(s, out _))
                        .Select(int.Parse)
                        .ToArray();
        }
        public static List<int[]> Expand(int[] baseArray, List<int> sizes)
        {
            var result = new List<int[]>();

            foreach (var size in sizes)
            {
                var newArr = new int[size];

                for (int i = 0; i < size; i++)
                {
                    newArr[i] = baseArray[i % baseArray.Length];
                }

                result.Add(newArr);
            }

            return result;
        }
    }
}