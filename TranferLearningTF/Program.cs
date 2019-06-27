using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TranferLearningTF.DataClasses;

namespace TranferLearningTF
{
    class Program
    {
        static void Main(string[] args)
        {
            TransferLearner.TrainAndPredict(DisplayResultsToConcole);
        }

        private static bool DisplayResultsToConcole(IEnumerable<ImagePrediction> imagePredictionData)
        {
            foreach (ImagePrediction prediction in imagePredictionData)
            {
                Console.WriteLine($"Image: {Path.GetFileName(prediction.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score.Max()} ");
            }
            return true;
        }
    }
}
