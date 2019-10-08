using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.ImageSources
{
    public class TestLineImageSource : IImageSource
    {
        public TestLineImageSource(ImageMaxValues maxValues)
        {
            MaxValues = maxValues;
        }


        public ImageType ImageType => ImageType.Vector;

        public bool Streaming => false;



        public ImageMaxValues MaxValues { get; }



        public IEnumerable<ImageFrame> GetFrames()
        {

            var frames = new List<ImageFrame>();
            
            var points = new List<ImagePoint>(MaxValues.MaxX - MaxValues.MinX);
            
            for (int i = MaxValues.MinX; i <= MaxValues.MaxX; i++)
            {
                var point = new ImagePoint()
                {
                    X = i,
                    Y = i,
                    Z = MaxValues.MaxX - i,

                    R = MaxValues.MaxRGB / 2,
                    G = MaxValues.MaxRGB / 2,
                    B = MaxValues.MaxRGB / 2,

                    Blanking = false
                };

                points.Add(point);

            }

            points[points.Count - 2].Blanking = true;
            points[points.Count - 1].Blanking = true;

            frames.Add(new ImageFrame()
            {
                Duration = -1,
                Number = 1,
                Points = points
            });

            return frames;

        }

    }
}
