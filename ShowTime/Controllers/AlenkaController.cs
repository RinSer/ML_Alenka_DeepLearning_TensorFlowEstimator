using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ImageClassification.ImageData;
using ImageClassification.Model;
using ImageClassification.TranferLearningTF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ShowTime.Hubs;

namespace ShowTime.Controllers
{
    [Route("api/[controller]")]
    public class AlenkaController : Controller
    {
        private string assetsPath => GetAbsolutePath(@"../../../assets");
        private readonly IHubContext<ConsoleHub> _hub;
        private readonly Func<string, Task> _logTrainTask;
        private readonly Func<string, Task> _logPredictTask;
        private readonly Func<string, Task> _logTransferTask;

        public AlenkaController(IHubContext<ConsoleHub> hub)
        {
            _hub = hub;
            _logTrainTask = message => 
                _hub.Clients.All.SendAsync("get_train_log", message);
            _logPredictTask = message =>
                _hub.Clients.All.SendAsync("get_predict_log", message);
            _logTransferTask = message =>
                _hub.Clients.All.SendAsync("get_transfer_log", message);
        }

        [HttpGet("[action]")]
        public IActionResult Train()
        {
            var tagsTsv = Path.Combine(assetsPath, "inputs", "data", "tags.tsv");
            var imagesFolder = Path.Combine(assetsPath, "inputs", "data");
            var inceptionPb = Path.Combine(assetsPath, "inputs", "inception", "tensorflow_inception_graph.pb");
            var imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");

            try
            {
                var modelBuilder = new ModelBuilder(tagsTsv, imagesFolder, inceptionPb, imageClassifierZip);
                var result = modelBuilder.BuildAndReturn(_logTrainTask);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logTrainTask(ex.Message);
                return BadRequest(ex);
            }
        }

        [HttpGet("[action]")]
        public IActionResult Predict()
        {
            var tagsTsv = Path.Combine(assetsPath, "outputs", "data", "images_list.tsv");
            var outImagesFolder = Path.Combine(assetsPath, "outputs", "data");
            var imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");

            try
            {
                var modelScorer = new ModelScorer(tagsTsv, outImagesFolder, imageClassifierZip);
                var result = modelScorer.ClassifyImages4Web(_logPredictTask);
                return Ok(result);
            }
            catch(Exception ex)
            {
                _logPredictTask(ex.Message);
                return BadRequest(ex);
            }
        }

        [HttpGet("[action]")]
        public IActionResult Transfer()
        {
            try
            {
                var result = TransferLearner.TrainAndPredict(DisplayResultsToFront);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logTransferTask(ex.Message);
                return BadRequest(ex);
            }
        }

        private bool DisplayResultsToFront(IEnumerable<ImagePrediction> imagePredictionData, string message)
        {
            if (imagePredictionData != null)
            {
                foreach (ImagePrediction prediction in imagePredictionData)
                {
                    Thread.Sleep(100);
                    _logTransferTask(
                        $"Изображению {Path.GetFileName(prediction.ImagePath)} присвоен лейбл {prediction.PredictedLabelValue} с вероятностью {prediction.Score.Max()}"
                    );
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                Thread.Sleep(100);
                _logTransferTask(message);
            }

            return true;
        }

        private string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.GetFullPath(
                Path.Combine(assemblyFolderPath, relativePath));

            return fullPath;
        }
    }
}