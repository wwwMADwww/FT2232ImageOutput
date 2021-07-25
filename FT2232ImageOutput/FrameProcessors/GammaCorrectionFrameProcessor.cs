using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors
{
    public class GammaCorrectionFrameProcessor : IFrameProcessor
    {
        private readonly IDictionary<GammaCorrectionFrameProcessorChannel, float> _gammaMap;
        private readonly ImageMaxValues _maxValues;

        public GammaCorrectionFrameProcessor(IDictionary<GammaCorrectionFrameProcessorChannel, float> gammaMap, ImageMaxValues maxValues)
        {
            _gammaMap = gammaMap;
            _maxValues = maxValues;
        }


        public ImageFrame Process(ImageFrame frame)
        {
            var res = new ImageFrame();
            res.Duration = frame.Duration;
            res.Number = frame.Number;


            res.Points = GetPoints(frame.Points);

            return res;

        }

        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {


            foreach (var point in originalPoints)
            {
                var newPoint = point.Clone();

                foreach (var map in _gammaMap)
                {
                    switch (map.Key)
                    {

                        case GammaCorrectionFrameProcessorChannel.Red:
                            newPoint.R = GammaCorrect(newPoint.R, _maxValues.MaxRGB, map.Value);
                            break;

                        case GammaCorrectionFrameProcessorChannel.Green:
                            newPoint.G = GammaCorrect(newPoint.G, _maxValues.MaxRGB, map.Value);
                            break;

                        case GammaCorrectionFrameProcessorChannel.Blue:
                            newPoint.B = GammaCorrect(newPoint.B, _maxValues.MaxRGB, map.Value);
                            break;

                        case GammaCorrectionFrameProcessorChannel.Z:
                            newPoint.Z = GammaCorrect(newPoint.Z - _maxValues.MinZ, _maxValues.MaxZ - _maxValues.MinZ, map.Value) + _maxValues.MinZ;
                            break;
                    }
                }

                yield return newPoint;
            }


            yield break;

        }


        int GammaCorrect(int value, int maxValue, float gamma)
        {
            return (int) Math.Floor(maxValue * Math.Pow((float)value / (float)maxValue, 1.0 / gamma));
        }

    }

    public enum GammaCorrectionFrameProcessorChannel { Red, Green, Blue, Z }

}
