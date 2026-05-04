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
            txtCodeEditor.Text =
                "public object MyAlgorithm(int[] arr)\n" +
                "{\n" +
                "    // Your logic here\n" +
                "    Array.Sort(arr);\n" +
                "    return arr;\n" +
                "}";
        }

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            SetMode(isManual: true);
        }

        private void btnAuto_Click(object sender, RoutedEventArgs e)
        {
            SetMode(isManual: false);
        }

        private void SetMode(bool isManual)
        {
            manualInputPanel.Visibility = isManual ? Visibility.Visible : Visibility.Collapsed;
            txtCodeEditor.Height = isManual ? 350 : 400;
            btnManual.Background = isManual ? System.Windows.Media.Brushes.DimGray : System.Windows.Media.Brushes.Transparent;
            btnAuto.Background = isManual ? System.Windows.Media.Brushes.Transparent : System.Windows.Media.Brushes.DimGray;
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

                if (manualInputPanel.Visibility == Visibility.Visible)
                {
                    // Manual Mode: Expand user input to test scalability
                    var baseArray = DataGenerator.Parse(txtArrayInput.Text);
                    var sizes = DataGenerator.GetSmartSizes(baseArray.Length);
                    result = await _runner.RunManualAsync(compiledFunction, DataGenerator.Expand(baseArray, sizes), sizes);
                }
                else
                {
                    // Auto Mode: Smart Scaling based on code structure
                    var sizes = DetectSmartSizes(code, methodName);
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

        private List<int> DetectSmartSizes(string code, string methodName)
        {
            // Simple heuristic to avoid crashing the UI with heavy algorithms
            bool isRecursive = Regex.Matches(code, $@"\b{methodName}\s*\(").Count >= 2;
            int loopCount = Regex.Matches(code, @"\b(for|while)\b").Count;

            if (isRecursive)
            {
                lblConfidence.Text = "Recursion detected: using small inputs.";
                return DataGenerator.GetExponentialSizes();
            }
            if (loopCount >= 3)
            {
                lblConfidence.Text = "High nesting detected: scaling down.";
                return new List<int> { 50, 100, 150, 200, 250 };
            }
            if (loopCount == 2)
            {
                lblConfidence.Text = "Quadratic pattern detected.";
                return DataGenerator.GetSmallSizes();
            }

            lblConfidence.Text = "Standard scaling applied.";
            return DataGenerator.GetDefaultSizes();
        }

        private void DisplayResults(EvaluationResult result)
        {
            lblComplexity.Text = result.Complexity;
            lblConfidence.Text = $"{result.Confidence}% Confidence — {result.Description}";
            ChartHelper.Render(MyChart, result);
        }
    }
}