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
                ("O(1)",         "Constant", 0.05), // رفعنا الصفر شوية
                ("O(log n)",     "Logarithmic", 0.40),
                ("O(n)",         "Linear", 1.00),
                ("O(n log n)",   "Linearithmic", 1.30),
                ("O(n²)",        "Quadratic", 1.90), // قللنا الـ 2 شوية
                ("O(n³)",        "Cubic", 2.70),     // قللنا الـ 3 عشان يلقطها أسرع
                ("O(2ⁿ)",        "Exponential", 4.50),
            };

        public EvaluationResult Analyze(EvaluationResult result)
        {
            var sizes = result.InputSizes;
            var times = result.AvgTimes;

            if (sizes.Count < 2)
            {
                result.Complexity = "N/A";
                result.Description = "At least two input sizes are required";
                result.Confidence = 0;
                return result;
            }

            double slope = LogLogSlope(sizes, times);
            var best = _classes.MinBy(c => Math.Abs(c.Slope - slope));
            double dist = Math.Abs(best.Slope - slope);
            double conf = Math.Round(Math.Max(40, Math.Min(98, 100 - dist * 35)), 1);

            result.Complexity = best.Name;
            result.Description = best.Desc;
            result.Confidence = conf;

            return result;
        }

        private static double LogLogSlope(List<int> sizes, List<double> times)
        {
            int n = sizes.Count;
            var logN = sizes.Select(s => Math.Log(s)).ToArray();
            var logT = times.Select(t => Math.Log(Math.Max(t, 0.001))).ToArray();

            double sx = logN.Sum();
            double sy = logT.Sum();
            double sxy = logN.Zip(logT, (x, y) => x * y).Sum();
            double sx2 = logN.Select(x => x * x).Sum();
            double denom = n * sx2 - sx * sx;

            return Math.Abs(denom) < 1e-10 ? 1.0 : (n * sxy - sx * sy) / denom;
        }
    }
}