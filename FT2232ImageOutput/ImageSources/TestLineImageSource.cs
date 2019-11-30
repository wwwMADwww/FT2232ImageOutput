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
        private readonly ImageMaxValues _lineBounds;

        public TestLineImageSource(ImageMaxValues maxValues, ImageMaxValues lineBounds)
        {
            MaxValues = maxValues;
            _lineBounds = lineBounds;
        }


        public ImageType ImageType => ImageType.Vector;

        public bool Streaming => false;



        public ImageMaxValues MaxValues { get; }



        public IEnumerable<ImageFrame> GetFrames()
        {

            var frames = new List<ImageFrame>();
            
            var points = new List<ImagePoint>(_lineBounds.MaxX - _lineBounds.MinX);

            var zkoeff = ((_lineBounds.MaxX - _lineBounds.MinX) + 1) / ((_lineBounds.MaxZ - _lineBounds.MinZ) + 1);

            for (int i = _lineBounds.MinX; i <= _lineBounds.MaxX; i++)
            {
                var point = new ImagePoint()
                {
                    X = i,
                    Y = i,
                    Z = i / zkoeff, // % (_lineBounds.MaxZ - _lineBounds.MinZ), //_lineBounds.MaxX - i,

                    R = _lineBounds.MaxRGB / 2,
                    G = _lineBounds.MaxRGB / 2,
                    B = _lineBounds.MaxRGB / 2,

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
