using System.Windows;
using System.Windows.Media;

namespace AlgorithmPerformanceEvaluator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            manualInputPanel.Visibility = Visibility.Visible;
        }

        private void btnAuto_Click(object sender, RoutedEventArgs e)
        {
            manualInputPanel.Visibility = Visibility.Collapsed;
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Run Clicked!");
        }
    }
}