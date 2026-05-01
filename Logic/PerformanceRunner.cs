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
        private double Measure(Func<int[], object?> fn, int[] input)
        {
            var watch = Stopwatch.StartNew();

            // Warm-up سريع مع حماية من التهنيج
            for (int i = 0; i < 2; i++)
            {
                try { fn((int[])input.Clone()); } catch { break; }
                if (watch.ElapsedMilliseconds > 1000) return watch.ElapsedMilliseconds;
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            var sw = new Stopwatch();
            var times = new List<double>();

            // محاولات القياس
            for (int run = 0; run < 5; run++)
            {
                sw.Restart();
                try { fn((int[])input.Clone()); } catch { break; }
                sw.Stop();
                times.Add(sw.Elapsed.TotalMilliseconds);

                // إذا كانت الدالة بطيئة جداً، نكتفي بعدد قليل من الاختبارات
                if (sw.ElapsedMilliseconds > 2000) break;
            }

            return times.Count > 0 ? times.Average() : 0;
        }
        public async Task<EvaluationResult> RunAsync(Func<int[], object?> fn, List<int> sizes, IProgress<int>? progress = null)
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

        public async Task<EvaluationResult> RunManualAsync(Func<int[], object?> fn, List<int[]> dataSets, List<int> sizes)
        {
            return await Task.Run(() =>
            {
                var result = new EvaluationResult();
                for (int i = 0; i < dataSets.Count; i++)
                {
                    result.InputSizes.Add(sizes[i]);
                    var time = Measure(fn, dataSets[i]);
                    result.AvgTimes.Add(time);
                    result.BestTimes.Add(time);
                    result.WorstTimes.Add(time);
                }
                return result;
            });
        }
    }
}