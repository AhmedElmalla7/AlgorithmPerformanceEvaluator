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
        private const int DefaultMeasureRuns = 15; // Increased samples for higher confidence
        private const double TimeoutMs = 1500;    // Lower timeout for better responsiveness

        /// <summary>
        /// Measures function execution time with high precision by filtering outliers.
        /// </summary>
        private double Measure(Func<int[], object?> fn, int[] input)
        {
            // Use 10 runs for small inputs (like 2^n) and 15 for standard inputs
            int actualMeasureRuns = input.Length < 100 ? 10 : DefaultMeasureRuns;

            // --- 1. Warm-up Phase ---
            // Prepare the JIT compiler
            for (int i = 0; i < (input.Length < 100 ? 1 : WarmupRuns); i++)
            {
                try { fn(input); }
                catch { break; }
            }

            // Clear memory to minimize Garbage Collector interference
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // --- 2. Measurement Phase ---
            var samples = new List<double>();
            var sw = new Stopwatch();

            for (int run = 0; run < actualMeasureRuns; run++)
            {
                // Only clone for larger arrays to protect data integrity
                var testInput = input.Length > 500 ? (int[])input.Clone() : input;

                sw.Restart();
                try { fn(testInput); }
                catch { break; }
                sw.Stop();

                double ms = sw.Elapsed.TotalMilliseconds;
                samples.Add(ms);

                // Exit if the run exceeds the timeout limit
                if (ms > TimeoutMs) break;
            }

            if (samples.Count == 0) return 0;
            if (samples.Count < 3) return samples.Average();

            // --- 3. Trimmed Mean (Noise Reduction) ---
            // Sort and remove the top and bottom 20% to eliminate spikes
            samples.Sort();
            int skipCount = Math.Max(1, samples.Count / 5);
            var trimmed = samples.Skip(skipCount).Take(samples.Count - 2 * skipCount).ToList();

            return trimmed.Count > 0 ? trimmed.Average() : samples.Average();
        }

        /// <summary>
        /// Auto Mode: Tests Sorted, Random, and Reversed cases for each size.
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

                    // Measure standard test cases
                    result.BestTimes.Add(Measure(fn, DataGenerator.Sorted(n)));
                    result.AvgTimes.Add(Measure(fn, DataGenerator.Random(n)));
                    result.WorstTimes.Add(Measure(fn, DataGenerator.Reversed(n)));

                    progress?.Report((i + 1) * 100 / sizes.Count);
                }

                return result;
            });
        }

        /// <summary>
        /// Manual Mode: Tests user-provided arrays scaled to different sizes.
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

                    result.AvgTimes.Add(t);
                    result.BestTimes.Add(t);
                    result.WorstTimes.Add(t);
                }

                return result;
            });
        }
    }
}