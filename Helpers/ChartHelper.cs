using System;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using AlgorithmPerformanceEvaluator.Models;

namespace AlgorithmPerformanceEvaluator.Helpers
{
    public static class ChartHelper
    {
        public static void Render(CartesianChart chart, EvaluationResult result)
        {
            // Y-axis: smart ms formatter — no more 0.000140000000001 nonsense
            chart.AxisY[0].LabelFormatter = val => FormatMs(val);
            chart.AxisY[0].Title = "Time (ms)";

            // X-axis: force every point to show its real N label
            chart.AxisX[0].Labels = result.InputSizes.ConvertAll(n => n.ToString("N0")).ToArray();
            chart.AxisX[0].Title = "Input Size (N)";
            chart.AxisX[0].Separator = new Separator { Step = 1 };

            chart.Series = new SeriesCollection
            {
                new LineSeries {
                    Title             = "Best",
                    Values            = new ChartValues<double>(result.BestTimes),
                    Stroke            = new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50)),
                    Fill              = Brushes.Transparent,
                    PointGeometrySize = 8
                },
                new LineSeries {
                    Title             = "Average",
                    Values            = new ChartValues<double>(result.AvgTimes),
                    Stroke            = new SolidColorBrush(Color.FromRgb(0xDE, 0xFF, 0x9A)),
                    Fill              = Brushes.Transparent,
                    PointGeometrySize = 8
                },
                new LineSeries {
                    Title             = "Worst",
                    Values            = new ChartValues<double>(result.WorstTimes),
                    Stroke            = new SolidColorBrush(Color.FromRgb(0xF7, 0x81, 0x66)),
                    Fill              = Brushes.Transparent,
                    StrokeDashArray   = new DoubleCollection { 4, 2 },
                    PointGeometrySize = 8
                }
            };
        }

        // Formats milliseconds in a readable way:
        //   >= 1 ms    →  "12.34 ms"
        //   >= 0.001   →  "0.4567 ms"
        //   tiny       →  "1.23E-5 ms"
        private static string FormatMs(double ms)
        {
            if (ms == 0) return "0 ms";
            double abs = Math.Abs(ms);
            if (abs >= 1) return $"{ms:F2} ms";
            if (abs >= 0.001) return $"{ms:F4} ms";
            return $"{ms:E2} ms";
        }
    }
}