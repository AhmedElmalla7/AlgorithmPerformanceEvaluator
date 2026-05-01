using System;
using System.Collections.Generic;
using System.Linq;
using AlgorithmPerformanceEvaluator.Models;

namespace AlgorithmPerformanceEvaluator.Services
{
    public class ComplexityAnalyzer
    {
        private static readonly (string Name, string Desc, double Slope)[] _classes =
        {
            ("O(1)",         "Constant",     0.05),
            ("O(log n)",     "Logarithmic",  0.35),
            ("O(n)",         "Linear",       1.00),
            ("O(n log n)",   "Linearithmic", 1.30),
            ("O(n²)",        "Quadratic",    2.00), // الميل المثالي لـ n^2 هو 2
            ("O(n³)",        "Cubic",        3.00), // الميل المثالي لـ n^3 هو 3
            ("O(2ⁿ)",        "Exponential",  5.00), // الميل للأسي يكون حاداً جداً
        };

        public EvaluationResult Analyze(EvaluationResult result)
        {
            if (result.InputSizes.Count < 2) return result;

            double slope = LogLogSlope(result.InputSizes, result.AvgTimes);

            // البحث عن أقرب فئة رياضية للميل المحسوب
            var bestMatch = _classes.OrderBy(c => Math.Abs(c.Slope - slope)).First();

            result.Complexity = bestMatch.Name;
            result.Description = bestMatch.Desc;

            // حساب نسبة الثقة
            double diff = Math.Abs(bestMatch.Slope - slope);
            result.Confidence = Math.Round(Math.Max(30, 100 - (diff * 45)), 1);

            return result;
        }

        private double LogLogSlope(List<int> sizes, List<double> times)
        {
            var x = sizes.Select(s => Math.Log(s)).ToArray();
            var y = times.Select(t => Math.Log(Math.Max(t, 0.0001))).ToArray();

            int n = x.Length;
            double sumX = x.Sum(), sumY = y.Sum();
            double sumXY = x.Zip(y, (a, b) => a * b).Sum();
            double sumX2 = x.Sum(a => a * a);

            double denom = (n * sumX2 - sumX * sumX);
            return Math.Abs(denom) < 1e-10 ? 0 : (n * sumXY - sumX * sumY) / denom;
        }
    }
}