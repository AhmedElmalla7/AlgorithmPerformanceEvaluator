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
            chart.Series = new SeriesCollection
            {
                // رسم خط الـ Best Case (أخضر)
                new LineSeries {
                    Title = "Best",
                    Values = new ChartValues<double>(result.BestTimes),
                    Stroke = new SolidColorBrush(Color.FromRgb(0x3F,0xB9,0x50)),
                    Fill = Brushes.Transparent
                },
                // رسم خط الـ Average (أصفر/أخضر فاتح)
                new LineSeries {
                    Title = "Average",
                    Values = new ChartValues<double>(result.AvgTimes),
                    Stroke = new SolidColorBrush(Color.FromRgb(0xDE,0xFF,0x9A)),
                    Fill = Brushes.Transparent
                },
                // رسم خط الـ Worst Case (أحمر)
                new LineSeries {
                    Title = "Worst",
                    Values = new ChartValues<double>(result.WorstTimes),
                    Stroke = new SolidColorBrush(Color.FromRgb(0xF7,0x81,0x66)),
                    Fill = Brushes.Transparent,
                    StrokeDashArray = new DoubleCollection { 4, 2 } // خط منقط للتميز
                }
            };

            // وضع أرقام الـ N على المحور الأفقي
            chart.AxisX[0].Labels = result.InputSizes.ConvertAll(n => n.ToString()).ToArray();
        }
    }
}