using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{
    public class MeandreImageSource : IImageSource
    {

        public MeandreImageSource(ImageMaxValues maxValues)
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
            var list = new List<ImagePoint>();
            
            for (int i = MaxValues.MinX; i <= MaxValues.MaxX; i++)
            {

                    list.Add(new ImagePoint()
                    {
                        X = i,
                        Y = (i & 1) == 0 ? MaxValues.MaxY : MaxValues.MinY,
                        Z = MaxValues.MaxZ,

                        R = MaxValues.MaxRGB,
                        G = MaxValues.MaxRGB,
                        B = MaxValues.MaxRGB,

                        Blanking = false
                    });


            }

            return list;

        }

    }


}
