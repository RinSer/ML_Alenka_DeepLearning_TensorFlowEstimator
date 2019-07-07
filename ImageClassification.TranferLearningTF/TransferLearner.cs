using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Data.IO;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms.Image;
using ImageClassification.ImageData;

namespace ImageClassification.TranferLearningTF
{
    public static class TransferLearner
    {
        static readonly string _assetsPath = Path.Combine(Environment.CurrentDirectory, 
            Environment.CurrentDirectory.Contains("bin") ? "../../../assetss" : "assetss");
        static readonly string _trainTagsTsv = Path.Combine(_assetsPath, "inputs-train", "data", "tags.tsv");
        static readonly string _predictImageListTsv = Path.Combine(_assetsPath, "inputs-predict", "data", "images_list.tsv");
        static readonly string _trainImagesFolder = Path.Combine(_assetsPath, "inputs-train", "data");
        static readonly string _predictImagesFolder = Path.Combine(_assetsPath, "inputs-predict", "data");
        static readonly string _predictSingleImage = Path.Combine(_assetsPath, "inputs-predict-single", "data", "alenka.jpg");
        static readonly string _inceptionPb = Path.Combine(_assetsPath, "inputs-train", "inception", "tensorflow_inception_graph.pb");
        static readonly string _inputImageClassifierZip = Path.Combine(_assetsPath, "inputs-predict", "imageClassifier.zip");
        static readonly string _outputImageClassifierZip = Path.Combine(_assetsPath, "outputs", "imageClassifier.zip");
        private static string LabelTokey = nameof(LabelTokey);
        private static string PredictedLabelValue = nameof(PredictedLabelValue);

        private static Func<IEnumerable<ImagePrediction>, string, bool> DisplayResults;
        private static List<ImageWebData> _showResults;

        public static List<ImageWebData> TrainAndPredict(Func<IEnumerable<ImagePrediction>, string, bool> displayResults)
        {
            _showResults = new List<ImageWebData>();

            DisplayResults = displayResults;

            MLContext mlContext = new MLContext(seed: 1);

            var model = ReuseAndTuneInceptionModel(mlContext, _trainTagsTsv, _trainImagesFolder, _inceptionPb, _outputImageClassifierZip);

            ClassifyImages(mlContext, _predictImageListTsv, _predictImagesFolder, _outputImageClassifierZip, model);

            ClassifySingleImage(mlContext, _predictSingleImage, _outputImageClassifierZip, model);

            return _showResults;
        }

        public static ITransformer ReuseAndTuneInceptionModel(MLContext mlContext, string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            var data = mlContext.Data.LoadFromTextFile<ImageDatum>(path: dataLocation, hasHeader: false);

            var estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelTokey, inputColumnName: "Label")
                    .Append(mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: _trainImagesFolder, inputColumnName: nameof(ImageData.ImageDatum.ImagePath)))
                    .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: InceptionSettings.ImageWidth, imageHeight: InceptionSettings.ImageHeight, inputColumnName: "input"))
                    .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: InceptionSettings.ChannelsLast, offsetImage: InceptionSettings.Mean))
                    .Append(mlContext.Model.LoadTensorFlowModel(inputModelLocation).
                        ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
                    .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: LabelTokey, featureColumnName: "softmax2_pre_activation"))
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(PredictedLabelValue, "PredictedLabel"))
                    .AppendCacheCheckpoint(mlContext);

            ITransformer model = estimator.Fit(data);

            var predictions = model.Transform(data);

            var ImageDatum = mlContext.Data.CreateEnumerable<ImageDatum>(data, false, true);
            var imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, false, true);

            DisplayResults(imagePredictionData, null);

            _showResults.AddRange(imagePredictionData.Select(ipd => new ImageWebData
            {
                ImagePath = ipd.ImagePath,
                Label = ipd.PredictedLabelValue,
                Probability = ipd.Score.Max()
            }));

            var multiclassContext = mlContext.MulticlassClassification;
            var metrics = multiclassContext.Evaluate(predictions, labelColumnName: LabelTokey, predictedLabelColumnName: "PredictedLabel");

            DisplayResults(null, $"LogLoss is: {metrics.LogLoss}");
            DisplayResults(null, $"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");
            
            return model;
        }

        public static void ClassifyImages(MLContext mlContext, string dataLocation, string imagesFolder, string outputModelLocation, ITransformer model)
        {
            var ImageDatum = ReadFromTsv(dataLocation, imagesFolder);
            var ImageDatumView = mlContext.Data.LoadFromEnumerable<ImageDatum>(ImageDatum);

            var predictions = model.Transform(ImageDatumView);
            var imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, false, true);

            DisplayResults(imagePredictionData, null);

            _showResults.AddRange(imagePredictionData.Select(ipd => new ImageWebData
            {
                ImagePath = ipd.ImagePath,
                Label = ipd.PredictedLabelValue,
                Probability = ipd.Score.Max()
            }));
        }

        public static void ClassifySingleImage(MLContext mlContext, string imagePath, string outputModelLocation, ITransformer model)
        {
            var ImageDatum = new ImageDatum()
            {
                ImagePath = imagePath
            };
            // Make prediction function (input = ImageDatum, output = ImagePrediction)
            var predictor = mlContext.Model.CreatePredictionEngine<ImageDatum, ImagePrediction>(model);
            var prediction = predictor.Predict(ImageDatum);

            DisplayResults(new List<ImagePrediction> { prediction }, null);

            _showResults.Add(new ImageWebData
            {
                ImagePath = prediction.ImagePath,
                Label = prediction.PredictedLabelValue,
                Probability = prediction.Score.Max()
            });
        }

        public static IEnumerable<ImageDatum> ReadFromTsv(string file, string folder)
        {
            return File.ReadAllLines(file)
                .Select(line => line.Split('\t'))
                .Select(line => new ImageDatum()
                {
                    ImagePath = Path.Combine(folder, line[0])
                });
        }

        private struct InceptionSettings
        {
            public const int ImageHeight = 224;
            public const int ImageWidth = 224;
            public const float Mean = 117;
            public const float Scale = 1;
            public const bool ChannelsLast = true;
        }

    }
}
