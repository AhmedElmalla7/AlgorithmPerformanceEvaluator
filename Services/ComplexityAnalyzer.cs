using System;
using System.Collections.Generic;
using System.Linq;
using AlgorithmPerformanceEvaluator.Models;

namespace AlgorithmPerformanceEvaluator.Services
{
    public class ComplexityAnalyzer
    {
        // Target slopes for Log-Log regression
        private static readonly (string Name, string Desc, double Slope)[] _classes =
        {
            ("O(1)",       "Constant",      0.00),
            ("O(log n)",   "Logarithmic",   0.30),
            ("O(n)",       "Linear",        1.00),
            ("O(n log n)", "Linearithmic",  1.15),
            ("O(n²)",      "Quadratic",     2.00),
            ("O(n³)",      "Cubic",         3.00),
            ("O(2ⁿ)",      "Exponential",   5.00),
        };

        public EvaluationResult Analyze(EvaluationResult result)
        {
            if (result.InputSizes.Count < 2) return result;

            // Prefer average times for stability
            var times = result.AvgTimes.Any(t => t > 0) ? result.AvgTimes : result.WorstTimes;

            // Log-Log regression requires values > 0
            var validPoints = result.InputSizes
                .Zip(times, (s, t) => (Size: (double)s, Time: t))
                .Where(p => p.Time > 1e-9)
                .ToList();

            if (validPoints.Count < 2)
            {
                result.Complexity = "O(1)";
                result.Description = "Execution too fast to measure";
                result.Confidence = 90;
                return result;
            }

            double slope = CalculateSlope(validPoints);
            var bestMatch = _classes.OrderBy(c => Math.Abs(c.Slope - slope)).First();

            result.Complexity = bestMatch.Name;
            result.Description = bestMatch.Desc;

            // Calculate accuracy score
            double r2 = CalculateR2(validPoints, bestMatch.Slope);
            result.Confidence = Math.Round(Math.Clamp(r2 * 100, 30, 99), 1);

            return result;
        }

        private static double CalculateSlope(List<(double Size, double Time)> points)
        {
            var x = points.Select(p => Math.Log(p.Size)).ToArray();
            var y = points.Select(p => Math.Log(p.Time)).ToArray();

            double n = x.Length;
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (a, b) => a * b).Sum();
            double sumX2 = x.Sum(a => a * a);

            double divisor = (n * sumX2 - sumX * sumX);
            return Math.Abs(divisor) < 1e-10 ? 0 : (n * sumXY - sumX * sumY) / divisor;
        }

        private static double CalculateR2(List<(double Size, double Time)> points, double expectedSlope)
        {
            var logN = points.Select(p => Math.Log(p.Size)).ToArray();
            var logT = points.Select(p => Math.Log(p.Time)).ToArray();

            double avgLogT = logT.Average();
            double intercept = avgLogT - (expectedSlope * logN.Average());

            double ssTot = logT.Sum(t => Math.Pow(t - avgLogT, 2));
            double ssRes = logN.Zip(logT, (n, t) => Math.Pow(t - (intercept + expectedSlope * n), 2)).Sum();

            return ssTot < 1e-12 ? 1.0 : Math.Max(0, 1.0 - (ssRes / ssTot));
        }
    }
}