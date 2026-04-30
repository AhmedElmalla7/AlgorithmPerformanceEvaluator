using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AlgorithmPerformanceEvaluator.Models;

namespace AlgorithmPerformanceEvaluator.Logic
{
    public class PerformanceRunner
    {
        // دالة القياس: بتكرر التشغيل 5 مرات وتاخد الـ Median عشان الدقة
        private double Measure(Func<int[], object?> fn, int[] input)
        {
            var times = new List<double>();
            var sw = new Stopwatch();

            for (int i = 0; i < 5; i++)
            {
                var copy = (int[])input.Clone(); // بناخد نسخة عشان الأصل ميبوظش
                sw.Restart();
                try { fn(copy); } catch { }
                sw.Stop();
                times.Add(sw.Elapsed.TotalMilliseconds);
            }

            times.Sort();
            return times[2]; // بنرجع القيمة اللي في النص (Median)
        }

        // الدالة الأساسية: بتشغل القياس على أحجام مختلفة (Auto Mode)
        public async Task<EvaluationResult> RunAsync(Func<int[], object?> fn, List<int> sizes, IProgress<int>? progress = null)
        {
            // Warmup سريع عشان الـ JIT Compiler
            try { fn(new int[10]); } catch { }

            return await Task.Run(() =>
            {
                var result = new EvaluationResult();
                for (int i = 0; i < sizes.Count; i++)
                {
                    int n = sizes[i];
                    result.InputSizes.Add(n);

                    // قياس التلات حالات لكل حجم N
                    result.AvgTimes.Add(Measure(fn, DataGenerator.Random(n)));
                    result.BestTimes.Add(Measure(fn, DataGenerator.Sorted(n)));
                    result.WorstTimes.Add(Measure(fn, DataGenerator.Reversed(n)));

                    // تحديث الـ ProgressBar في الواجهة
                    progress?.Report((i + 1) * 100 / sizes.Count);
                }
                return result;
            });
        }
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

                    result.AvgTimes.Add(Measure(fn, dataSets[i]));
                }

                return result;
            });
        }
    }
}