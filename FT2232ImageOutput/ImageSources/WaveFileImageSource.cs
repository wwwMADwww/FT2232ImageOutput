using DSS.ILDA;
using Ratchet.IO.Format;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.ImageSources
{

    public enum WaveFileImageSourceChannelMode { LeftX_RightY, LeftY_RightX }

    public class WaveFileImageSource : IImageSource
    {
        private readonly string _filePath;

        // TODO: fix int bug in library
        private readonly Waveform.Sound<short> _sound;
        private readonly WaveFileImageSourceChannelMode _channelMode;
        private readonly int _channelX;
        private readonly int _channelY;

        public WaveFileImageSource(
            string filePath, 
            WaveFileImageSourceChannelMode channelMode = WaveFileImageSourceChannelMode.LeftX_RightY
            )
        {

            _channelMode = channelMode;

            switch (_channelMode)
            {
                case WaveFileImageSourceChannelMode.LeftX_RightY:
                    _channelX = 0;
                    _channelY = 1;
                    break;

                case WaveFileImageSourceChannelMode.LeftY_RightX:
                    _channelX = 1;
                    _channelY = 0;
                    break;

                default:
                    throw new ArgumentException($"Unknown channel mode {channelMode}");
            }

            _filePath = Path.GetFullPath(filePath);

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"File '{_filePath}' does not exists.");

            if (Directory.Exists(_filePath))
                throw new ArgumentException($"Path '{_filePath}' is a directory.");

            // TODO: fix int bug in library
            // Waveform.Sound<short> s;

            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
            {
                _sound = Waveform.Read<short>(fs);
            }

            Console.WriteLine($"wave file '{_filePath}'");
            Console.WriteLine($"duration {_sound.Duration}");
            Console.WriteLine($"length {_sound.Length}");
            Console.WriteLine($"sample rate {_sound.SampleRate}");
            Console.WriteLine($"channels {_sound.Channels.Count}:");

            foreach (var c in _sound.Channels)
            {
                Console.WriteLine($"length {c.Length}:");
                Console.WriteLine($"samples count {c.Samples.Length}:");
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

            // foreach (var i in _sound.Channels.First().Samples)
            // {
            //     Console.WriteLine(i);
            // }
        }


        public ImageType ImageType => ImageType.Vector;

        public bool Streaming => false;



        public ImageMaxValues MaxValues { get; }



        public IEnumerable<ImageFrame> GetFrames()
        {

            var frameCount = _sound.Length / (int)(_sound.SampleRate / 100);

            if (frameCount == 0)
                frameCount = 1;

            foreach (var f in Enumerable.Range(0, frameCount))
            {

                yield return new ImageFrame()
                {
                    Duration = -1,
                    Number = f,
                    Points = GetPoints(f * ((int)_sound.SampleRate / 100), (int)_sound.SampleRate / 100) // 3600
                };

            }

            yield break;

        }

        IEnumerable<ImagePoint> GetPoints(int start, int count)
        {

            int stop = start + count;

            if (stop > _sound.Length)
                stop = _sound.Length;

            count = stop - start;

            var points = new ImagePoint[count];

            foreach (var i in Enumerable.Range(start, count))
            {

                points[i - start] = new ImagePoint()
                {
                    X = _sound.Channels[_channelX].Samples[i],
                    Y = _sound.Channels[_channelY].Samples[i],
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
