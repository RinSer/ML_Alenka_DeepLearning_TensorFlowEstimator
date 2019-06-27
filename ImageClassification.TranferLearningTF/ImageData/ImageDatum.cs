using Microsoft.ML.Data;

namespace ImageClassification.ImageData
{
    public class ImageDatum
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }
}
