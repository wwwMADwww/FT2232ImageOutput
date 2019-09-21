using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{
    public class RandomImageSource : IImageSource
    {
        public RandomImageSource(ImageMaxValues maxValues)
        {
            MaxValues = maxValues;
        }


        public ImageType ImageType => ImageType.Vector;

        public bool Streaming => false;



        public ImageMaxValues MaxValues { get; }



        public IEnumerable<ImageFrame> GetFrames()
        {

            var frames = new List<ImageFrame>();
            

            frames.Add(new ImageFrame()
            {
                Duration = -1,
                Number = 1,
                Points = GetPoints()
            });

            return frames;

        }

        IEnumerable<ImagePoint> GetPoints()
        {
            var random = new Random((int) DateTime.Now.Ticks);

            for (int i = 0; i< 4096; i++)
            {

                yield return new ImagePoint()
                {
                    X = random.Next(MaxValues.MinX, MaxValues.MaxX),
                    Y = random.Next(MaxValues.MinY, MaxValues.MaxY),
                    Z = random.Next(MaxValues.MinZ, MaxValues.MaxZ),

                    R = MaxValues.MaxRGB, // random.Next(0, MaxValues.MaxRGB),
                    G = MaxValues.MaxRGB, // random.Next(0, MaxValues.MaxRGB),
                    B = MaxValues.MaxRGB, // random.Next(0, MaxValues.MaxRGB),

                    Blanking = false /// (random.Next() & 1) == 1
                };

            }

            yield break;

        }

    }
}
