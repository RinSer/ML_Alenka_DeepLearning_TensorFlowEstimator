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

        // Передаёт промежуточные результаты на терминал или в логер
        private static Func<IEnumerable<ImagePrediction>, string, bool> DisplayResults;
        // Финальный набор данных, обработанных моделью
        private static List<ImageWebData> _showResults;

        public static List<ImageWebData> TrainAndPredict(Func<IEnumerable<ImagePrediction>, string, bool> displayResults)
        {
            _showResults = new List<ImageWebData>();

            DisplayResults = displayResults;

            // Инициализируем ML контекст
            MLContext mlContext = new MLContext(seed: 1);

            var model = ReuseAndTuneInceptionModel(mlContext, _trainTagsTsv, _trainImagesFolder, _inceptionPb, _outputImageClassifierZip);

            ClassifyImages(mlContext, _predictImageListTsv, _predictImagesFolder, _outputImageClassifierZip, model);

            ClassifySingleImage(mlContext, _predictSingleImage, _outputImageClassifierZip, model);

            return _showResults;
        }

        /// <summary>
        /// Настроить модель классификатора и загрузить её в память
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="dataLocation"></param>
        /// <param name="imagesFolder"></param>
        /// <param name="inputModelLocation"></param>
        /// <param name="outputModelLocation"></param>
        /// <returns></returns>
        public static ITransformer ReuseAndTuneInceptionModel(MLContext mlContext, string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            // Загружаем данные для переобучения модели
            IDataView data = mlContext.Data.LoadFromTextFile<ImageDatum>(path: dataLocation, hasHeader: false);

            DisplayResults(null, $"Путь к файлу с лейблами для тренировочных изображений: {dataLocation}");

            // Готовим первоначальные данные, чтобы они соответствовали ожиданиям модели,
            // настраиваем конвеер обучения модели, 
            var estimator = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelTokey, inputColumnName: "Label")
                    // Загружаем изображения в память в виде BitMaps
                    .Append(mlContext.Transforms.LoadImages(outputColumnName: "input", imageFolder: _trainImagesFolder, inputColumnName: nameof(ImageData.ImageDatum.ImagePath)))
                    // Изменяем их размер согласно настройкам модели
                    .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input", imageWidth: InceptionSettings.ImageWidth, imageHeight: InceptionSettings.ImageHeight, inputColumnName: "input"))
                    // Конвертируем пиксели изображений в числовой вектор
                    .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input", interleavePixelColors: InceptionSettings.ChannelsLast, offsetImage: InceptionSettings.Mean))
                    // Загружаем готовую Tensorflow Inception model
                    // softmax2_pre_activation - функция, определяющая вероятность попадания
                    // изображения в одну из категорий
                    .Append(mlContext.Model.LoadTensorFlowModel(inputModelLocation).
                        ScoreTensorFlowModel(outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }, addBatchDimensionInput: true))
                    // Добавляем алгоритм обучения, получающий на вход данные о чертах изображений
                    // из модели Tensorflow и числовые лейблы
                    .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: LabelTokey, featureColumnName: "softmax2_pre_activation"))
                    // Переводим числовый лейблы обратно в строчные 
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue(PredictedLabelValue, "PredictedLabel"))
                    .AppendCacheCheckpoint(mlContext);

            DisplayResults(null, $"Путь к папке с тренировочными изображениями: {_trainImagesFolder}");
            DisplayResults(null, $"Размер изображений для модели: {InceptionSettings.ImageWidth}x{InceptionSettings.ImageHeight}");
            DisplayResults(null, $"Путь к модели: {inputModelLocation}");

            DisplayResults(null, $"Обучение модели...");
            // Обучение модели на имеющихся данных
            ITransformer model = estimator.Fit(data);

            // Получаем предсказываемые лейблы для тренировочных изображений
            var predictions = model.Transform(data);

            var imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, false, true);

            DisplayResults(null, "Лейблы для тренировочных данных:");
            DisplayResults(imagePredictionData, null);

            _showResults.AddRange(imagePredictionData.Select(ipd => new ImageWebData
            {
                ImagePath = ipd.ImagePath,
                Label = ipd.PredictedLabelValue,
                Probability = ipd.Score.Max()
            }));

            // Показатели качества классификатора
            var multiclassContext = mlContext.MulticlassClassification;
            var metrics = multiclassContext.Evaluate(predictions, labelColumnName: LabelTokey, predictedLabelColumnName: "PredictedLabel");

            DisplayResults(null, $"LogLoss = {metrics.LogLoss}");
            DisplayResults(null, $"PerClassLogLoss = {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");
            
            return model;
        }

        /// <summary>
        /// Определение классов нескольких изображений
        /// с помощью предзагруженной модели
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="dataLocation"></param>
        /// <param name="imagesFolder"></param>
        /// <param name="outputModelLocation"></param>
        /// <param name="model"></param>
        public static void ClassifyImages(MLContext mlContext, string dataLocation, string imagesFolder, string outputModelLocation, ITransformer model)
        {
            DisplayResults(null, "Начинаю обработку тестовых данных...");

            var ImageDatum = ReadFromTsv(dataLocation, imagesFolder);
            var ImageDatumView = mlContext.Data.LoadFromEnumerable<ImageDatum>(ImageDatum);

            var predictions = model.Transform(ImageDatumView);
            var imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, false, true);

            DisplayResults(null, "Лейблы для тестовых данных:");
            DisplayResults(imagePredictionData, null);

            _showResults.AddRange(imagePredictionData.Select(ipd => new ImageWebData
            {
                ImagePath = ipd.ImagePath,
                Label = ipd.PredictedLabelValue,
                Probability = ipd.Score.Max()
            }));
        }

        /// <summary>
        /// Определение класса одного изображения с
        /// помощью загруженной модели
        /// </summary>
        /// <param name="mlContext"></param>
        /// <param name="imagePath"></param>
        /// <param name="outputModelLocation"></param>
        /// <param name="model"></param>
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

        /// <summary>
        /// Считывает из .tsv файла пути к файлам изображений
        /// </summary>
        /// <param name="file"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        public static IEnumerable<ImageDatum> ReadFromTsv(string file, string folder)
        {
            return File.ReadAllLines(file)
                .Select(line => line.Split('\t'))
                .Select(line => new ImageDatum()
                {
                    ImagePath = Path.Combine(folder, line[0])
                });
        }

        /// <summary>
        /// Настройки для Inception model
        /// </summary>
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
