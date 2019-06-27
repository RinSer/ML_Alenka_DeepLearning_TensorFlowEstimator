using Microsoft.ML.Data;

namespace TranferLearningTF.DataClasses
{
    public class ImageData
    {
        [LoadColumn(0)]
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }
}
