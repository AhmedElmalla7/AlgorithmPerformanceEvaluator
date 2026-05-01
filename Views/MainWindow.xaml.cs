using AlgorithmPerformanceEvaluator.Helpers; // استدعاء ملف الرسم البياني
using AlgorithmPerformanceEvaluator.Logic;
using AlgorithmPerformanceEvaluator.Models;
using AlgorithmPerformanceEvaluator.Services;
using System;
using System.Collections.Generic;
using System.Windows;

namespace AlgorithmPerformanceEvaluator
{
    public partial class MainWindow : Window
    {
        private readonly CompilerService _compiler = new();
        private readonly PerformanceRunner _runner = new();
        private readonly ComplexityAnalyzer _analyzer = new();

        public MainWindow()
        {
            InitializeComponent();

            // نص افتراضي صحيح لا يسبب خطأ في الـ Compilation
            txtCodeEditor.Text = "// Write your algorithm logic here.\n" +
                         "// Use the 'arr' variable as your input array.\n\n" +
                         "Array.Sort(arr);";
        }

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            // هيظهر الخانة بتاعة الـ Manual ويصغر الـ Editor شوية
            manualInputPanel.Visibility = Visibility.Visible;
            txtCodeEditor.Height = 350;
        }

        private void btnAuto_Click(object sender, RoutedEventArgs e)
        {
            // هيخفي خانة الـ Manual ويرجع الـ Editor لحجمه
            manualInputPanel.Visibility = Visibility.Collapsed;
            txtCodeEditor.Height = 400;
        }

        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            btnRun.IsEnabled = false;
            lblComplexity.Text = "Analyzing...";

            try
            {
                // 1. تجميع الكود
                string code = txtCodeEditor.Text;
                var compiledFunction = await _compiler.CompileAsync(code);

                // بدلاً من 100 و 500 و 1000...
                // أرقام كافية للـ O(n) ومش قاتلة للـ O(n^2)
                var sizes = new List<int> { 100, 200, 300, 400, 500 };

                bool isManual = manualInputPanel.Visibility == Visibility.Visible;

                EvaluationResult results;

                if (isManual)
                {
                    // 🟢 Manual Mode
                    var baseArray = DataGenerator.Parse(txtArrayInput.Text);

                    if (baseArray.Length == 0)
                        throw new Exception("Invalid manual input!");

                    var dataSets = DataGenerator.Expand(baseArray, sizes);

                    results = await _runner.RunManualAsync(compiledFunction, dataSets, sizes);
                }
                else
                {
                    // 🔵 Auto Mode
                    results = await _runner.RunAsync(compiledFunction, sizes);
                }

                // 4. تحليل التعقيد
                var finalAnalysis = _analyzer.Analyze(results);

                // 5. عرض النتائج والرسم البياني
                DisplayResults(finalAnalysis);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "Analysis Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblComplexity.Text = "O(?)";
            }
            finally
            {
                btnRun.IsEnabled = true;
            }
        }

        private void DisplayResults(EvaluationResult result)
        {
            // تحديث النصوص
            lblComplexity.Text = result.Complexity;
            lblConfidence.Text = $"{result.Confidence}% Confidence - {result.Description}";

            // استدعاء الـ Helper لرسم البيانات على MyChart
            ChartHelper.Render(MyChart, result);
        }
    }
}