using System;
using System.Collections.Generic;
using System.Windows;
using AlgorithmPerformanceEvaluator.Helpers;
using AlgorithmPerformanceEvaluator.Logic;
using AlgorithmPerformanceEvaluator.Models;
using AlgorithmPerformanceEvaluator.Services;

namespace AlgorithmPerformanceEvaluator
{
    public partial class MainWindow : Window
    {
        // Services used in the app
        private readonly CompilerService _compiler = new();
        private readonly PerformanceRunner _runner = new();
        private readonly ComplexityAnalyzer _analyzer = new();

        public MainWindow()
        {
            InitializeComponent();

            // Default code shown in editor
            txtCodeEditor.Text =
    "public object MyAlgorithm(int[] arr)\n" +
    "{\n" +
    "    // Write your logic here\n" +
    "    \n" +
    "    \n" +
    "    return arr; // <--- This line prevents the error CS0161\n" +
    "}";
        }

        // ===== MODE BUTTONS =====

        // Switch to Manual Mode (user provides input array)
        // الانتقال للنمط اليدوي
        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            manualInputPanel.Visibility = Visibility.Visible;
            txtCodeEditor.Height = 350;

            // تغيير ألوان الزراير للتوضيح
            btnManual.Background = System.Windows.Media.Brushes.DimGray;
            btnAuto.Background = System.Windows.Media.Brushes.Transparent;
        }

        // الانتقال للنمط التلقائي
        private void btnAuto_Click(object sender, RoutedEventArgs e)
        {
            manualInputPanel.Visibility = Visibility.Collapsed;
            txtCodeEditor.Height = 400;

            // تغيير ألوان الزراير للتوضيح
            btnAuto.Background = System.Windows.Media.Brushes.DimGray;
            btnManual.Background = System.Windows.Media.Brushes.Transparent;
        }

        // ===== RUN BUTTON =====

        // Main entry point for analysis
        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            btnRun.IsEnabled = false;
            btnRun.Content = "Analyzing...";
            lblComplexity.Text = "...";

            try
            {
                string code = txtCodeEditor.Text;

                // استخدام المترجم المرن الجديد
                var compiledFunction = await _compiler.CompileFlexibleAsync(code);

                // استخراج اسم الدالة للفحص لاحقاً (للـ Recursion)
                string methodName = System.Text.RegularExpressions.Regex.Match(code, @"\b\w+\s+(\w+)\s*\(int\s*\[\s*\]").Groups[1].Value;

                EvaluationResult result;

                if (manualInputPanel.Visibility == Visibility.Visible)
                {
                    var baseArray = DataGenerator.Parse(txtArrayInput.Text);
                    var sizes = DataGenerator.GetSmartSizes(baseArray.Length);
                    result = await _runner.RunManualAsync(compiledFunction, DataGenerator.Expand(baseArray, sizes), sizes);
                }
                else
                {
                    List<int> sizes;

                    // --- فحص نمط الكود الذكي ---

                    // هل الدالة تستدعي نفسها؟ (Recursion اكتشاف)
                    bool isRecursive = System.Text.RegularExpressions.Regex.Matches(code, $@"\b{methodName}\s*\(").Count >= 2;

                    int loopCount = System.Text.RegularExpressions.Regex.Matches(code, @"\bfor\b|\bwhile\b").Count;

                    if (isRecursive)
                    {
                        sizes = DataGenerator.GetExponentialSizes();
                        lblConfidence.Text = "Exponential/Recursive pattern detected.";
                    }
                    else if (loopCount >= 3)
                    {
                        sizes = DataGenerator.GetSmallSizes();
                        lblConfidence.Text = "Cubic complexity detected. Scaling down.";
                    }
                    else if (loopCount == 2)
                    {
                        sizes = DataGenerator.GetSmallSizes();
                        lblConfidence.Text = "Quadratic pattern detected.";
                    }
                    else
                    {
                        sizes = DataGenerator.GetDefaultSizes();
                        lblConfidence.Text = "Standard complexity pattern.";
                    }

                    result = await _runner.RunAsync(compiledFunction, sizes);
                }

                var finalResult = _analyzer.Analyze(result);
                DisplayResults(finalResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                lblComplexity.Text = "O(?)";
            }
            finally
            {
                btnRun.IsEnabled = true;
                btnRun.Content = "Run Performance Analysis";
            }
        }

        // القالب الافتراضي عند تشغيل التطبيق
        private void SetDefaultTemplate()
        {
            txtCodeEditor.Text =
                "public object MyAlgorithm(int[] arr)\n" +
                "{\n" +
                "    // Write your code here\n" +
                "    int n = arr.Length;\n" +
                "    for (int i = 0; i < n; i++) {\n" +
                "        // Do something\n" +
                "    }\n" +
                "    return arr;\n" +
                "}";
        }
        // ===== DISPLAY RESULTS =====

        // Update UI with analysis results
        private void DisplayResults(EvaluationResult result)
        {
            lblComplexity.Text = result.Complexity;

            // Show confidence and description
            lblConfidence.Text = $"{result.Confidence}% Confidence  —  {result.Description}";

            // Render chart
            ChartHelper.Render(MyChart, result);
        }
    }
}