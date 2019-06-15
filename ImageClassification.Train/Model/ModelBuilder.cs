using System;
using System.Linq;
using Microsoft.ML;
using ImageClassification.ImageData;
using static ImageClassification.Model.ConsoleHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace ImageClassification.Model
{
    public class ModelBuilder
    {
        private readonly string dataLocation;
        private readonly string imagesFolder;
        private readonly string inputModelLocation;
        private readonly string outputModelLocation;
        private readonly MLContext mlContext;
        private static string LabelTokey = nameof(LabelTokey);
        private static string ImageReal = nameof(ImageReal);
        private static string PredictedLabelValue = nameof(PredictedLabelValue);

        public ModelBuilder(string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            this.dataLocation = dataLocation;
            this.imagesFolder = imagesFolder;
            this.inputModelLocation = inputModelLocation;
            this.outputModelLocation = outputModelLocation;
            mlContext = new MLContext(seed: 1);
        }

        private struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const float scale = 1;
            public const bool channelsLast = true;
        }

        public void BuildAndTrain()
        {
            var featurizerModelLocation = inputModelLocation;

            ConsoleWriteHeader("Read model");
            Console.WriteLine($"Model location: {featurizerModelLocation}");
            Console.WriteLine($"Images folder: {imagesFolder}");
            Console.WriteLine($"Training file: {dataLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight}), image mean: {ImageNetSettings.mean}");



            var data = mlContext.Data.LoadFromTextFile<ImageNetData>(path:dataLocation, hasHeader: false);

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelTokey,inputColumnName:"Label")
                            .Append(mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: imagesFolder, inputColumnName: nameof(ImageNetData.ImagePath)))
                            .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "input"))
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: ImageNetSettings.channelsLast, offsetImage: ImageNetSettings.mean))
                            .Append(mlContext.Model.LoadTensorFlowModel(featurizerModelLocation).
                                 ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
                            .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName:LabelTokey, featureColumnName:"softmax2_pre_activation"))
                            .Append(mlContext.Transforms.Conversion.MapKeyToValue(PredictedLabelValue,"PredictedLabel"))
                            .AppendCacheCheckpoint(mlContext);

           
            // Train the model
            ConsoleWriteHeader("Training classification model");
            ITransformer model = pipeline.Fit(data);

            // Process the training data through the model
            // This is an optional step, but it's useful for debugging issues
            var trainData = model.Transform(data);
            var loadedModelOutputColumnNames = trainData.Schema
                .Where(col => !col.IsHidden).Select(col => col.Name);
            var trainData2 = mlContext.Data.CreateEnumerable<ImageNetPipeline>(trainData, false, true).ToList();
            trainData2.ForEach(pr => ConsoleWriteImagePrediction(pr.ImagePath,pr.PredictedLabelValue, pr.Score.Max()));

            // Get some performance metric on the model using training data            
            var classificationContext = mlContext.MulticlassClassification;
            ConsoleWriteHeader("Classification metrics");
            var metrics = classificationContext.Evaluate(trainData, labelColumnName: LabelTokey, predictedLabelColumnName: "PredictedLabel");
            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            // Save the model to assets/outputs
            ConsoleWriteHeader("Save model to local file");
            ModelHelpers.DeleteAssets(outputModelLocation);

            mlContext.Model.Save(model, trainData.Schema, outputModelLocation);

            Console.WriteLine($"Model saved: {outputModelLocation}");
        }

        public List<ImageWebData> BuildAndReturn(Func<string, Task> consoleWrite)
        {
            List<ImageWebData> trainingList = new List<ImageWebData>();

            var featurizerModelLocation = inputModelLocation;

            consoleWrite("Считывание модели");
            Thread.Sleep(100);
            consoleWrite($"Файл модели: {featurizerModelLocation}");
            Thread.Sleep(100);
            consoleWrite($"Папка с изображениями: {imagesFolder}");
            Thread.Sleep(100);
            consoleWrite($"Файл с лейблами: {dataLocation}");
            Thread.Sleep(100);
            consoleWrite($"Параметры модели: размер изображений=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight}), среднее: {ImageNetSettings.mean}");

            var data = mlContext.Data.LoadFromTextFile<ImageNetData>(path: dataLocation, hasHeader: false);

            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelTokey, inputColumnName: "Label")
                            .Append(mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: this.imagesFolder, inputColumnName: nameof(ImageNetData.ImagePath)))
                            .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: "input"))
                            .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: ImageNetSettings.channelsLast, offsetImage: ImageNetSettings.mean))
                            .Append(mlContext.Model.LoadTensorFlowModel(featurizerModelLocation).
                                 ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
                            .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: LabelTokey, featureColumnName: "softmax2_pre_activation"))
                            .Append(mlContext.Transforms.Conversion.MapKeyToValue(PredictedLabelValue, "PredictedLabel"))
                            .AppendCacheCheckpoint(mlContext);


            // Train the model
            consoleWrite("Обучается модель классификатора");
            ITransformer model = pipeline.Fit(data);

            // Process the training data through the model
            // This is an optional step, but it's useful for debugging issues
            var trainData = model.Transform(data);
            var loadedModelOutputColumnNames = trainData.Schema
                .Where(col => !col.IsHidden).Select(col => col.Name);
            var trainData2 = mlContext.Data.CreateEnumerable<ImageNetPipeline>(trainData, false, true).ToList();
            trainingList.AddRange(trainData2
                .Select(pr => new ImageWebData
                {
                    ImagePath = pr.ImagePath,
                    Label = pr.PredictedLabelValue,
                    Probability = pr.Score.Max()
                }));
            trainingList.ForEach(img =>
            {
                consoleWrite($"Изображению {img.ImagePath} присвоен лейбл {img.Label} с вероятностью {img.Probability}");
                Thread.Sleep(100);
            });

            // Get some performance metric on the model using training data            
            var classificationContext = mlContext.MulticlassClassification;
            consoleWrite("Метрика классификатора");
            var metrics = classificationContext.Evaluate(trainData, labelColumnName: LabelTokey, predictedLabelColumnName: "PredictedLabel");
            consoleWrite($"LogLoss: {metrics.LogLoss}");
            Thread.Sleep(100);
            consoleWrite($"PerClassLogLoss: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            // Save the model to assets/outputs
            consoleWrite("Модель сохраняется в файл");
            ModelHelpers.DeleteAssets(outputModelLocation);

            mlContext.Model.Save(model, trainData.Schema, outputModelLocation);
            consoleWrite($"Модель записана в файл: {outputModelLocation}");

            return trainingList;
        }
    }
}
