using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AlgorithmPerformanceEvaluator.Models;

namespace AlgorithmPerformanceEvaluator.Logic
{
    public class PerformanceRunner
    {
        private const int WarmupRuns = 3;
        private const int MeasureRuns = 7;
        private const double TimeoutMs = 3000; // Limit each run to 3 seconds

        /// <summary>
        /// Measures the execution time of a function using multiple samples.
        /// Uses a trimmed mean (removes outliers) for higher accuracy.
        /// </summary>
        private double Measure(Func<int[], object?> fn, int[] input)
        {
            // --- 1. Warm-up Phase ---
            // Run the code without measuring to let the JIT compiler optimize it
            for (int i = 0; i < WarmupRuns; i++)
            {
                try { fn((int[])input.Clone()); }
                catch { /* Ignore errors during warmup */ }
            }

            // Cleanup memory before starting actual measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // --- 2. Measurement Phase ---
            var samples = new List<double>(MeasureRuns);
            var sw = new Stopwatch();

            for (int run = 0; run < MeasureRuns; run++)
            {
                var copy = (int[])input.Clone();
                sw.Restart();
                try { fn(copy); }
                catch { break; }
                sw.Stop();

                double ms = sw.Elapsed.TotalMilliseconds;
                samples.Add(ms);

                // Stop if the execution is already too slow
                if (ms > TimeoutMs) break;
            }

            if (samples.Count == 0) return 0;
            if (samples.Count == 1) return samples[0];

            // --- 3. Result Calculation ---
            // Sort samples and remove the fastest and slowest to avoid noise
            samples.Sort();
            var trimmed = samples.Skip(1).Take(samples.Count - 2).ToList();

            return trimmed.Count > 0 ? trimmed.Average() : samples.Average();
        }

        /// <summary>
        /// Automatic Mode: Tests sorted, random, and reversed data for each size.
        /// </summary>
        public async Task<EvaluationResult> RunAsync(
            Func<int[], object?> fn,
            List<int> sizes,
            IProgress<int>? progress = null)
        {
            return await Task.Run(() =>
            {
                var result = new EvaluationResult();

                for (int i = 0; i < sizes.Count; i++)
                {
                    int n = sizes[i];
                    result.InputSizes.Add(n);

                    // Test the 3 standard cases
                    result.BestTimes.Add(Measure(fn, DataGenerator.Sorted(n)));
                    result.AvgTimes.Add(Measure(fn, DataGenerator.Random(n)));
                    result.WorstTimes.Add(Measure(fn, DataGenerator.Reversed(n)));

                    progress?.Report((i + 1) * 100 / sizes.Count);
                }

                return result;
            });
        }

        /// <summary>
        /// Manual Mode: Tests user-provided input data scaled to different sizes.
        /// </summary>
        public async Task<EvaluationResult> RunManualAsync(
            Func<int[], object?> fn,
            List<int[]> dataSets,
            List<int> sizes)
        {
            return await Task.Run(() =>
            {
                var result = new EvaluationResult();

                for (int i = 0; i < dataSets.Count; i++)
                {
                    result.InputSizes.Add(sizes[i]);

                    double t = Measure(fn, dataSets[i]);

                    // In manual mode, all time categories represent the same input shape
                    result.AvgTimes.Add(t);
                    result.BestTimes.Add(t);
                    result.WorstTimes.Add(t);
                }

                return result;
            });
        }
    }
}