using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{
    public class MatrixImageSource : IImageSource
    {
        private readonly MatrixImageSourceConfig _config;
        private readonly List<Symbol> _symbols;


        List<Trailer> _trailers = new List<Trailer>();
        Random _random = new Random(DateTime.Now.Millisecond);


        TrailerChain[,] _matrix;

        public MatrixImageSource(
            MatrixImageSourceConfig config,
            IEnumerable<Symbol> symbols)
        {
            _config = config;
            _symbols = symbols.ToList();

            _matrix = new TrailerChain[_config.Rows, _config.Cols];
        }


        public bool Streaming => true;

        public ImageType ImageType => ImageType.Vector;

        public ImageMaxValues MaxValues => new ImageMaxValues()
        {
            MaxRGB = 255,
            MinX = 0, MaxX = _config.Width,
            MinY = 0, MaxY = _config.Height,
            MinZ = 0, MaxZ = 255
        };


        public IEnumerable<ImageFrame> GetFrames()
        {
            ProcessTrailers();

            UpdateMatrix();

            return new[] { GenerateFrame() };

        }


        ImageFrame GenerateFrame()
        {
            return new ImageFrame()
            {
                Duration = -1,
                Number = 0,
                Points = GetPoints()
            };
        }

        IEnumerable<ImagePoint> GetPoints()
        {
            List<ImagePoint> points = new List<ImagePoint>(_config.Rows * _config.Cols);

            for (int row = 0; row < _config.Rows ; row++)
            {
                for (int col = 0; col < _config.Cols; col++)
                {
                    var chain = _matrix[row, col];

                    if (chain == null)
                        continue;

                    var symbol = chain.CurrentSymbol;

                    var symbolWidth  = symbol.MaxValues.MaxX - symbol.MaxValues.MinX;
                    var symbolHeigth = symbol.MaxValues.MaxY - symbol.MaxValues.MinY;
                    
                    foreach (var point in symbol.Points)
                    {

                        var p = new ImagePoint()
                        {
                            X = (point.X - symbol.MaxValues.MinX) + (symbolWidth  * col),
                            Y = (point.Y - symbol.MaxValues.MinY) + (symbolHeigth * (_config.Rows - row)),
                            Z = point.Z,

                            R = chain.Head ? _config.MaxRGB : _config.MaxRGB / 3,
                            G = chain.Head ? _config.MaxRGB : _config.MaxRGB / 3,
                            B = chain.Head ? _config.MaxRGB : _config.MaxRGB / 3,

                            Blanking = point.Blanking
                        };

                        if (p.X < _config.Width && p.Y < _config.Height)
                            points.Add(p);
                    }
                }
            }

            return points;

        }

        void UpdateMatrix()
        {
            for (int row = 0; row < _config.Rows; row++)
            {
                for (int col = 0; col < _config.Cols; col++)
                {
                    _matrix[row, col] = null;
                }
            }

            foreach (var trailer in _trailers.OrderBy(t => t.HeadY))
            {
                int chainPos = 0;
                foreach (var chain in trailer.Chains)
                {
                    var y = trailer.HeadY + chainPos;
                    if (y >= _config.Rows)
                        continue;
                    _matrix[y, trailer.HeadX] = chain;
                    chainPos++;
                }
            }
        }


        void ProcessTrailers()
        {
            // delete off-screen trailers
            _trailers = _trailers.Where(t => t.HeadY - t.Length < _config.Rows).ToList();

            // create new trailers
            for (int i = 0; i < NewTrailersCount(); i++)
            {
                int headx = 0;
                while(true)
                {
                    headx = _random.Next(0, _config.Cols);
                    if (!_trailers.Where(t => t.HeadY == 0).Any(t => t.HeadX == headx))
                        break;
                }

                _trailers.Add(new Trailer()
                {
                    HeadX = _random.Next(0, _config.Cols),
                    HeadY = 0,
                    Length = _random.Next(1, _config.TrailLength)
                });
            }

            // process all trailers
            foreach (var trailer in _trailers)
            {
                if (trailer.Chains.Count() >= trailer.Length)
                {
                    trailer.Chains.Dequeue();
                    trailer.HeadY++;
                }

                if (trailer.Chains.Any())
                    trailer.Chains.Last().Head = false;


                trailer.Chains.Enqueue(new TrailerChain()
                {
                    ChangeRate = GenerateChangeRate(),
                    CurrentSymbol = GetRandomSymbol(),
                    Head = true
                });

                foreach(var chain in trailer.Chains)
                {
                    if (NeedToChange(chain.ChangeRate))
                        chain.CurrentSymbol = GetRandomSymbol();
                }
            }
            
        }

        Symbol GetRandomSymbol() => _symbols[_random.Next(_symbols.Count())];

        int GenerateChangeRate()
        {
            if (_random.Next(0, 100) < _config.TrailChangeIntencity)
                return _random.Next(_config.TrailChangeRateMin, _config.TrailChangeRateMax);
            else
                return 0;
        }

        bool NeedToChange(int changeRate)
        {
            return changeRate == 0 ? false : _random.Next(0, 100) < changeRate;
        }


        int NewTrailersCount()
        {
            var k = (_config.Cols * _config.TrailGenerationRate) / 100;
            return _random.Next(0, k);
        }



    }

    public class Symbol
    {
        public string Name { get; set; }
        public IEnumerable<ImagePoint> Points { get; set; }
        public ImageMaxValues MaxValues { get; set; }
    }

    class TrailerChain
    { 
        public int ChangeRate { get; set; }

        public Symbol CurrentSymbol { get; set; }

        public bool Head { get; set; }

    }

    class Trailer
    {
        public Queue<TrailerChain> Chains { get; set; } = new Queue<TrailerChain>();

        public int Length { get; set; }

        public int HeadX { get; set; }
        public int HeadY { get; set; }
    }


    public class MatrixImageSourceConfig
    { 
        public int Width { get; set; }
        public int Height { get; set; }
        public int MaxRGB { get; set; }

        public int Rows { get; set; }
        public int Cols { get; set; }

        public int TrailLength { get; set; }
        // public int TrailSpeed { get; set; }
        public int TrailGenerationRate { get; set; } // 0-100
        public int TrailChangeRateMin { get; set; } // 0-100
        public int TrailChangeRateMax { get; set; } // 0-100
        public int TrailChangeIntencity { get; set; } // 0-100

    }

}
