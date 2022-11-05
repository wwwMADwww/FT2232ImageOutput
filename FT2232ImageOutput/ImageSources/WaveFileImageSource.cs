using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace FT2232ImageOutput.ImageSources
{

    public class WaveFileImageSource : IImageSource
    {
        private readonly string _filePath;

        private List<ImageFrame> _frames;

        public WaveFileImageSource(string filePath, ImageMaxValues maxValues)
        {            
            _filePath = Path.GetFullPath(filePath);

            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"File '{_filePath}' does not exists.");

            if (Directory.Exists(_filePath))
                throw new ArgumentException($"Path '{_filePath}' is a directory.");

            MaxValues = maxValues;
        }


        public ImageType ImageType => ImageType.Vector;

        public bool Streaming => false;



        public ImageMaxValues MaxValues { get; set; }



        public IEnumerable<ImageFrame> GetFrames()
        {
            if (_frames != null)
            {
                return _frames;
            }

            _frames = new List<ImageFrame>();

            using (var wavReader = new WaveFileReader(_filePath))
            {

                var sampleRate = wavReader.WaveFormat.SampleRate;
                var channels = wavReader.WaveFormat.Channels;

                // Console.WriteLine($"wave file '{_filePath}'");
                // // Console.WriteLine($"duration {wav.FileTimeLength}");
                // // Console.WriteLine($"length {wav.FullLength}");
                // Console.WriteLine($"sample rate {sampleRate}");
                // Console.WriteLine($"channels {channels}:");
                
                var pointsPerFrame = (int)(sampleRate / 100);

                var framePoints = new List<ImagePoint>(pointsPerFrame);
                int frameNumber = 0;

                while (true)
                {
                    var sampleFrame = wavReader.ReadNextSampleFrame();

                    if (sampleFrame != null)
                    {
                        var point = new ImagePoint()
                        {
                            X = (int)((sampleFrame[0] + 1f) / 2f * MaxValues.MaxX),
                            Y = (int)((sampleFrame[1] + 1f) / 2f * MaxValues.MaxY),
                            Z = channels > 2
                                ? (int)((sampleFrame[2] + 1f) / 2f * MaxValues.MaxY)
                                : MaxValues.MaxZ,

                            R = MaxValues.MaxRGB,
                            G = MaxValues.MaxRGB,
                            B = MaxValues.MaxRGB,

                            Blanking = false
                        };

                        framePoints.Add(point);
                    }

                    if (framePoints.Count >= pointsPerFrame || (sampleFrame == null && framePoints.Count > 0))
                    {
                        frameNumber++;
                        _frames.Add(new ImageFrame() {
                            Duration = -1,
                            Number = frameNumber,
                            Points = framePoints.ToArray()
                        });
                        framePoints.Clear();
                    }

                    if (sampleFrame == null)
                        break;
                }

            }

            return _frames;

        }

    }


}
