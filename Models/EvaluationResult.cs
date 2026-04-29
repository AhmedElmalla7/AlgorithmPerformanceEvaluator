using System.Collections.Generic;

namespace AlgorithmPerformanceEvaluator.Models
{
    public class EvaluationResult
    {
        public List<int> InputSizes { get; set; } = new();
        public List<double> AvgTimes { get; set; } = new();
        public List<double> BestTimes { get; set; } = new();
        public List<double> WorstTimes { get; set; } = new();
        public string Complexity { get; set; } = "";
        public string Description { get; set; } = "";
        public double Confidence { get; set; }
    }
}