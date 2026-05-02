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
        private const int    WarmupRuns  = 3;
        private const int    MeasureRuns = 7;
        private const double TimeoutMs   = 3000;   // abort a single run after 3 s

        /// <summary>
        /// Runs <paramref name="fn"/> on <paramref name="input"/> and returns the
        /// trimmed-mean elapsed milliseconds (drops min and max to reduce noise).
        /// Returns 0 if every attempt times out or throws.
        /// </summary>
        private double Measure(Func<int[], object?> fn, int[] input)
        {
            // --- warm-up (not measured) ---
            for (int i = 0; i < WarmupRuns; i++)
            {
                try { fn((int[])input.Clone()); }
                catch { /* ignore */ }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // --- measured runs ---
            var samples = new List<double>(MeasureRuns);
            var sw      = new Stopwatch();

            for (int run = 0; run < MeasureRuns; run++)
            {
                var copy = (int[])input.Clone();
                sw.Restart();
                try { fn(copy); }
                catch { break; }
                sw.Stop();

                double ms = sw.Elapsed.TotalMilliseconds;
                samples.Add(ms);

                if (ms > TimeoutMs) break;   // no point in more runs if already slow
            }

            if (samples.Count == 0) return 0;
            if (samples.Count == 1) return samples[0];

            // Trimmed mean: drop the single fastest and single slowest sample
            samples.Sort();
            var trimmed = samples.Skip(1).Take(samples.Count - 2).ToList();
            return trimmed.Count > 0 ? trimmed.Average() : samples.Average();
        }

        /// <summary>
        /// Auto mode: generates sorted / random / reversed arrays for each size.
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

                    result.BestTimes.Add(Measure(fn, DataGenerator.Sorted(n)));
                    result.AvgTimes.Add(Measure(fn, DataGenerator.Random(n)));
                    result.WorstTimes.Add(Measure(fn, DataGenerator.Reversed(n)));

                    progress?.Report((i + 1) * 100 / sizes.Count);
                }

                return result;
            });
        }

        /// <summary>
        /// Manual mode: user supplies the base array which is expanded to each size.
        /// All three case lists are filled with the same value (single input shape).
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