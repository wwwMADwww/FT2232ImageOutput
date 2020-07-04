using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathCore.WAV;

namespace FT2232ImageOutput.ImageSources
{

    // TODO: custom channel mapping
    public enum WaveFileImageSourceChannelMode { LeftX_RightY, LeftY_RightX }

    public class WaveFileImageSource : IImageSource
    {
        private readonly string _filePath;
        private readonly WaveFileImageSourceChannelMode _channelMode;

        private int _sampleRate;
        private readonly long[] _channelX;
        private readonly long[] _channelY;

        public WaveFileImageSource(
            string filePath, 
            WaveFileImageSourceChannelMode channelMode = WaveFileImageSourceChannelMode.LeftX_RightY
            )
        {

            _channelMode = channelMode;

            
            _filePath = Path.GetFullPath(filePath);

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"File '{_filePath}' does not exists.");

            if (Directory.Exists(_filePath))
                throw new ArgumentException($"Path '{_filePath}' is a directory.");


            var wav = new WavFile(_filePath);

            Console.WriteLine($"wave file '{_filePath}'");
            Console.WriteLine($"duration {wav.FileTimeLength}");
            Console.WriteLine($"length {wav.FullLength}");
            Console.WriteLine($"sample rate {wav.SampleRate}");
            Console.WriteLine($"channels {wav.ChannelsCount}:");

            _sampleRate = wav.SampleRate;

            if (wav.ChannelsCount == 1)
            {
                _channelY = _channelX = wav.GetChannel(0);
            }
            else
            {
                switch (_channelMode)
                {
                    case WaveFileImageSourceChannelMode.LeftX_RightY:
                        _channelX = wav.GetChannel(0);
                        _channelY = wav.GetChannel(1);
                        break;

                    case WaveFileImageSourceChannelMode.LeftY_RightX:
                        _channelX = wav.GetChannel(1);
                        _channelY = wav.GetChannel(0);
                        break;

                    default:
                        throw new ArgumentException($"Unknown channel mode {channelMode}");
                }
            }


            MaxValues = new ImageMaxValues()
            {
                MaxRGB = 1,
                MinX = short.MinValue,
                MinY = short.MinValue,
                MinZ = 0,

                MaxX = short.MaxValue,
                MaxY = short.MaxValue,
                MaxZ = 1
            };
            
            // foreach (var i in _channelX)
            // {
            //     Console.WriteLine(i);
            // }

        }


        public ImageType ImageType => ImageType.Vector;

        public bool Streaming => false;



        public ImageMaxValues MaxValues { get; }



        public IEnumerable<ImageFrame> GetFrames()
        {

            var frameCount = (int)_channelX.Length / (int)(_sampleRate / 100);

            if (frameCount == 0)
                frameCount = 1;

            foreach (var f in Enumerable.Range(0, frameCount))
            {

                yield return new ImageFrame()
                {
                    Duration = -1,
                    Number = f,
                    Points = GetPoints(f * ((int)_sampleRate / 100), (int)_sampleRate / 100) // 3600
                };

            }

            yield break;

        }

        IEnumerable<ImagePoint> GetPoints(int start, int count)
        {

            long stop = start + count;

            if (stop > _channelX.Length)
                stop = _channelX.Length;

            count = (int) stop - start;

            var points = new ImagePoint[count];

            foreach (var i in Enumerable.Range(start, count))
            {

                points[i - start] = new ImagePoint()
                {
                    X = (int) _channelX[i],
                    Y = (int) _channelY[i],
                    Z = MaxValues.MaxZ,

                    R = MaxValues.MaxRGB,
                    G = MaxValues.MaxRGB,
                    B = MaxValues.MaxRGB,

                    Blanking = false
                };

            }

            return points;


        }

    }


}
