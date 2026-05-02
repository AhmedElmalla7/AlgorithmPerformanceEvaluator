using System;
using System.Collections.Generic;
using System.Linq;
using AlgorithmPerformanceEvaluator.Models;

namespace AlgorithmPerformanceEvaluator.Services
{
    public class ComplexityAnalyzer
    {
        // Each class: (Label, Description, expected log-log slope)
        private static readonly (string Name, string Desc, double Slope)[] _classes =
        {
            ("O(1)",       "Constant",      0.00),
            ("O(log n)",   "Logarithmic",   0.35),
            ("O(n)",       "Linear",        1.00),
            ("O(n log n)", "Linearithmic",  1.20),
            ("O(n²)",      "Quadratic",     2.00),
            ("O(n³)",      "Cubic",         3.00),
            ("O(2ⁿ)",      "Exponential",   6.00),
        };

        public EvaluationResult Analyze(EvaluationResult result)
        {
            if (result.InputSizes.Count < 2)
            {
                result.Complexity = "N/A";
                result.Description = "Not enough data points";
                result.Confidence = 0;
                return result;
            }

            // Use average times; fall back to worst if avg is all zeros
            var times = result.AvgTimes.Any(t => t > 0)
                ? result.AvgTimes
                : result.WorstTimes;

            // Filter out zero/negative measurements which break log
            var validPairs = result.InputSizes
                .Zip(times, (s, t) => (Size: s, Time: t))
                .Where(p => p.Size > 0 && p.Time > 1e-9)
                .ToList();

            if (validPairs.Count < 2)
            {
                result.Complexity = "O(1)";
                result.Description = "Constant (too fast to measure accurately)";
                result.Confidence = 60;
                return result;
            }

            double slope = LogLogSlope(
                validPairs.Select(p => (double)p.Size).ToList(),
                validPairs.Select(p => p.Time).ToList()
            );

            // Find best matching complexity class
            var bestMatch = _classes.OrderBy(c => Math.Abs(c.Slope - slope)).First();

            result.Complexity = bestMatch.Name;
            result.Description = bestMatch.Desc;

            // Confidence: starts at 100, penalised by distance from ideal slope
            double diff = Math.Abs(bestMatch.Slope - slope);
            double r2 = ComputeR2(
                validPairs.Select(p => (double)p.Size).ToList(),
                validPairs.Select(p => p.Time).ToList(),
                bestMatch.Slope
            );

            // Blend slope-distance penalty with R² quality
            double confidenceFromSlope = Math.Max(0, 100 - diff * 35);
            double confidenceFromR2 = r2 * 100;
            result.Confidence = Math.Round((confidenceFromSlope * 0.6 + confidenceFromR2 * 0.4)
                                           .Clamp(30, 99), 1);

            return result;
        }

        // Log-log linear regression slope  (slope ≈ exponent p in T(n) = a·nᵖ)
        private static double LogLogSlope(List<double> sizes, List<double> times)
        {
            var x = sizes.Select(s => Math.Log(s)).ToArray();
            var y = times.Select(t => Math.Log(Math.Max(t, 1e-9))).ToArray();

            int n = x.Length;
            double sx = x.Sum();
            double sy = y.Sum();
            double sxy = x.Zip(y, (a, b) => a * b).Sum();
            double sx2 = x.Sum(a => a * a);

            double denom = n * sx2 - sx * sx;
            if (Math.Abs(denom) < 1e-10) return 0;

            return (n * sxy - sx * sy) / denom;
        }

        // Coefficient of determination R² for the chosen complexity model
        // We fit  log(T) = a + slope·log(N)  and compute R² on log scale
        private static double ComputeR2(List<double> sizes, List<double> times, double slope)
        {
            var logN = sizes.Select(s => Math.Log(s)).ToArray();
            var logT = times.Select(t => Math.Log(Math.Max(t, 1e-9))).ToArray();

            int n = logN.Length;
            double meanLogT = logT.Average();

            // Intercept: a = mean(logT) - slope * mean(logN)
            double a = meanLogT - slope * logN.Average();

            double ssTot = logT.Sum(y => Math.Pow(y - meanLogT, 2));
            double ssRes = logN.Zip(logT, (x, y) => Math.Pow(y - (a + slope * x), 2)).Sum();

            if (ssTot < 1e-12) return 1.0;   // perfectly flat — constant
            return Math.Max(0, 1.0 - ssRes / ssTot);
        }
    }

    internal static class DoubleExtensions
    {
        public static double Clamp(this double value, double min, double max)
            => Math.Max(min, Math.Min(max, value));
    }
}