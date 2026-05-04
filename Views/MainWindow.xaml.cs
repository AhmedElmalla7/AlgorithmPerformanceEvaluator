using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using AlgorithmPerformanceEvaluator.Helpers;
using AlgorithmPerformanceEvaluator.Logic;
using AlgorithmPerformanceEvaluator.Models;
using AlgorithmPerformanceEvaluator.Services;

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

            // Set default boilerplate code in the editor
            txtCodeEditor.Text =
                "public object MyAlgorithm(int[] arr)\n" +
                "{\n" +
                "    // Write your algorithm here. \n\n" +
                "    return arr;\n" +
                "}";
        }

        private async void btnRun_Click(object sender, RoutedEventArgs e)
        {
            btnRun.IsEnabled = false;
            btnRun.Content = "Analyzing...";

            try
            {
                string code = txtCodeEditor.Text;
                var compiledFunction = await _compiler.CompileFlexibleAsync(code);
                string methodName = CompilerService.ExtractMethodName(code);

                EvaluationResult result;

                // Toggle between Manual and Auto mode
                if (manualInputPanel.Visibility == Visibility.Visible)
                {
                    var baseArray = DataGenerator.Parse(txtArrayInput.Text);
                    var sizes = DataGenerator.GetSmartSizes(baseArray.Length);
                    result = await _runner.RunManualAsync(compiledFunction, DataGenerator.Expand(baseArray, sizes), sizes);
                }
                else
                {
                    // Detect appropriate input sizes based on code structure
                    var sizes = DetectSmartSizes(code, methodName);
                    result = await _runner.RunAsync(compiledFunction, sizes);
                }

                // Analyze timing results to determine Big O complexity
                var finalResult = _analyzer.Analyze(result);
                DisplayResults(finalResult);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                btnRun.IsEnabled = true;
                btnRun.Content = "Run Performance Analysis";
            }
        }

        private List<int> DetectSmartSizes(string code, string methodName)
        {
            // Check for recursion to use micro-scales for O(2^n) algorithms
            bool isRecursive = Regex.Matches(code, $@"\b{methodName}\s*\(").Count >= 2 ||
                               Regex.IsMatch(code, @"\w+\s+(\w+)\(.*\)\s*\{[\s\S]*\b\1\s*\(");

            if (isRecursive)
            {
                lblConfidence.Text = "Recursion detected: using micro-scales.";
                return DataGenerator.GetExponentialSizes();
            }

            // Count loops to downscale for heavy algorithms like O(n^2) or O(n^3)
            int loopCount = Regex.Matches(code, @"\b(for|while)\b").Count;

            if (loopCount >= 3) return new List<int> { 50, 100, 150, 200, 250 }; // O(n^3)
            if (loopCount == 2) return DataGenerator.GetSmallSizes();            // O(n^2)

            lblConfidence.Text = "Standard scaling applied.";
            return DataGenerator.GetDefaultSizes();
        }

        private void DisplayResults(EvaluationResult result)
        {
            lblComplexity.Text = result.Complexity;
            lblConfidence.Text = $"{result.Confidence}% Confidence — {result.Description}";
            ChartHelper.Render(MyChart, result);
        }

        private void btnManual_Click(object sender, RoutedEventArgs e) => SetMode(true);
        private void btnAuto_Click(object sender, RoutedEventArgs e) => SetMode(false);

        private void SetMode(bool isManual)
        {
            manualInputPanel.Visibility = isManual ? Visibility.Visible : Visibility.Collapsed;
            txtCodeEditor.Height = isManual ? 350 : 400;
        }
    }
}