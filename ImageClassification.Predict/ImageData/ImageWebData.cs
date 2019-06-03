using System;
using System.Collections.Generic;
using System.Text;

namespace ImageClassification.ImageData
{
    public class ImageWebData
    {
        public string ImagePath { get; set; }

        public string Label { get; set; }

        public float Probability { get; set; }
    }
}
