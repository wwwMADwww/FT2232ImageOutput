using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{
    public class MatrixBootstrap
    {

        public IImageSource CrateMatrix(string dir, MatrixImageSourceConfig config, ImageMaxValues maxValues)
        {

            List<Symbol> symbols = new List<Symbol>();


            var blanker = new AddBlankingPointsFrameProcessor();

            foreach (var file in Directory.EnumerateFiles(dir, "*.ild"))
            {

                var imagesource = new IldaImageSource(Path.Combine(dir, file));

                var scaler = new ScaleMaxValuesFrameProcessor(imagesource.MaxValues, maxValues);

                var image = imagesource.GetFrames()
                    .Where(f => f.Points.Any())
                    .Select(f => scaler.Process(f))
                    .Select(f => blanker.Process(f));

                foreach (var frame in image)
                {
                    var s = new Symbol()
                    {
                        MaxValues = maxValues,
                        Name = Path.GetFileNameWithoutExtension(file),
                        Points = frame.Points.ToArray()
                    };
                    symbols.Add(s);
                }

            }

            var imageSource = new MatrixImageSource(config, symbols);

            return imageSource;
        }

    }
}
