using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageClassification.Model;
using Microsoft.AspNetCore.Mvc;

namespace ShowTime.Controllers
{
    [Route("api/[controller]")]
    public class AlenkaController : Controller
    {
        private string assetsPath => GetAbsolutePath(@"../../../assets");

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
                var result = modelBuilder.BuildAndReturn();
                return Ok(result);
            }
            catch (Exception ex)
            {
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
                var result = modelScorer.ClassifyImages4Web();
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(ex);
            }
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