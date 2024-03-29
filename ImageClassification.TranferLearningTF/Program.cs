﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImageClassification.ImageData;

namespace ImageClassification.TranferLearningTF
{
    class Program
    {
        static void Main(string[] args)
        {
            TransferLearner.TrainAndPredict(DisplayResultsToConcole);
        }

        private static bool DisplayResultsToConcole(IEnumerable<ImagePrediction> imagePredictionData, string message = null)
        {
            if (imagePredictionData != null)
            {
                foreach (ImagePrediction prediction in imagePredictionData)
                {
                    Console.WriteLine($"Image: {Path.GetFileName(prediction.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score.Max()} ");
                }
            }

            if (!string.IsNullOrEmpty(message)) Console.WriteLine(message);

            return true;
        }
    }
}
