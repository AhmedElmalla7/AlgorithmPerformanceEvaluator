using System;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using AlgorithmPerformanceEvaluator.Models;

namespace AlgorithmPerformanceEvaluator.Helpers
{
    public static class ChartHelper
    {
        /// <summary>
        /// Configures and renders the performance chart using LiveCharts.
        /// </summary>
        public static void Render(CartesianChart chart, EvaluationResult result)
        {
            // 1. Configure Y-Axis (Time in ms)
            // Uses a custom formatter to prevent messy long decimal strings
            chart.AxisY[0].LabelFormatter = val => FormatMs(val);
            chart.AxisY[0].Title = "Time (ms)";

            // 2. Configure X-Axis (Input Size N)
            // Map the labels directly to our input sizes (e.g., "10,000", "50,000")
            chart.AxisX[0].Labels = result.InputSizes.ConvertAll(n => n.ToString("N0")).ToArray();
            chart.AxisX[0].Title = "Input Size (N)";
            chart.AxisX[0].Separator = new Separator { Step = 1 };

            // 3. Populate Chart Series
            chart.Series = new SeriesCollection
            {
                // Best Case (e.g., Sorted Data) - Green
                new LineSeries {
                    Title             = "Best",
                    Values            = new ChartValues<double>(result.BestTimes),
                    Stroke            = new SolidColorBrush(Color.FromRgb(0x3F, 0xB9, 0x50)),
                    Fill              = Brushes.Transparent,
                    PointGeometrySize = 8
                },
                // Average Case (e.g., Random Data) - Yellowish
                new LineSeries {
                    Title             = "Average",
                    Values            = new ChartValues<double>(result.AvgTimes),
                    Stroke            = new SolidColorBrush(Color.FromRgb(0xDE, 0xFF, 0x9A)),
                    Fill              = Brushes.Transparent,
                    PointGeometrySize = 8
                },
                // Worst Case (e.g., Reversed Data) - Red/Orange
                new LineSeries {
                    Title             = "Worst",
                    Values            = new ChartValues<double>(result.WorstTimes),
                    Stroke            = new SolidColorBrush(Color.FromRgb(0xF7, 0x81, 0x66)),
                    Fill              = Brushes.Transparent,
                    StrokeDashArray   = new DoubleCollection { 4, 2 }, // Dashed line for distinction
                    PointGeometrySize = 8
                }
            };
        }

        /// <summary>
        /// Formats millisecond values into a readable string.
        /// Handles normal, small, and microscopic values.
        /// </summary>
        private static string FormatMs(double ms)
        {
            if (ms == 0) return "0 ms";
            double abs = Math.Abs(ms);

            if (abs >= 1) return $"{ms:F2} ms";      // Example: 12.34 ms
            if (abs >= 0.001) return $"{ms:F4} ms";  // Example: 0.4567 ms

            return $"{ms:E2} ms";                    // Example: 1.23E-5 ms (Scientific notation)
        }
    }
}