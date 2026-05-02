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
            // النص الافتراضي المحدث ليتناسب مع المترجم المرن والـ Smart Scaling
            txtCodeEditor.Text =
                "public object MyAlgorithm(int[] arr)\n" +
                "{\n" +
                "    // Tip: The analyzer will auto-detect loops or recursion to scale input sizes.\n" +
                "    \n" +
                "    // Example: \n" +
                "    // Array.Sort(arr);\n" +
                "    \n" +
                "    return arr; \n" +
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
            btnRun.Content = "Analyzing Performance...";
            lblComplexity.Text = "...";

            try
            {
                string code = txtCodeEditor.Text;

                // 1. استخدام المترجم المرن المحدث
                var compiledFunction = await _compiler.CompileFlexibleAsync(code);

                // 2. استخراج اسم الدالة بطريقة آمنة باستخدام الميثود التي أضفناها للسيرفس
                string methodName;
                try
                {
                    methodName = CompilerService.ExtractMethodName(code);
                }
                catch
                {
                    methodName = "MyAlgorithm"; // اسم افتراضي في حالة فشل الاستخراج
                }

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

                    // --- منطق فحص نمط الكود الذكي (Smart Scaling) ---

                    // فحص الـ Recursion: هل اسم الدالة يتكرر داخل الكود؟
                    bool isRecursive = System.Text.RegularExpressions.Regex.Matches(code, $@"\b{methodName}\s*\(").Count >= 2;

                    // فحص عدد الحلقات (Loops) لتحديد التعقيد المتوقع
                    int loopCount = System.Text.RegularExpressions.Regex.Matches(code, @"\bfor\b|\bwhile\b").Count;

                    if (isRecursive)
                    {
                        // أحجام ميكروسكوبية للـ O(2^n) لتجنب الانهيار
                        sizes = DataGenerator.GetExponentialSizes();
                        lblConfidence.Text = "Recursive pattern detected. Using micro-scales.";
                    }
                    else if (loopCount >= 3)
                    {
                        // أحجام صغيرة جداً للـ O(n^3)
                        sizes = new List<int> { 50, 100, 150, 200, 250 };
                        lblConfidence.Text = "Cubic pattern detected. Scaling down input.";
                    }
                    else if (loopCount == 2)
                    {
                        // أحجام متوسطة للـ O(n^2)
                        sizes = DataGenerator.GetSmallSizes();
                        lblConfidence.Text = "Quadratic pattern detected.";
                    }
                    else
                    {
                        // أحجام كبيرة للـ O(n) أو O(n log n)
                        sizes = DataGenerator.GetDefaultSizes();
                        lblConfidence.Text = "Linear/Logarithmic pattern detected.";
                    }

                    // تشغيل الاختبار الفعلي
                    result = await _runner.RunAsync(compiledFunction, sizes);
                }

                // 3. تحليل النتائج رياضياً وعرضها
                var finalResult = _analyzer.Analyze(result);
                DisplayResults(finalResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Execution Error");
                lblComplexity.Text = "O(?)";
                lblConfidence.Text = "Analysis failed.";
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