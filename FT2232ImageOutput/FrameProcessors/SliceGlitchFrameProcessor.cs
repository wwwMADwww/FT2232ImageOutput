using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FT2232ImageOutput.FrameProcessors
{

    public class SliceGlitchFrameProcessorSettings
    { 
        public TimeSpan UpdateIntervalMin { get; set; }
        public TimeSpan UpdateIntervalMax { get; set; }

        public int SliceCountMin { get; set; }
        public int SliceCountMax { get; set; }

        public int SliceHeightMin { get; set; }
        public int SliceHeightMax { get; set; }

        public int ShiftMin { get; set; }
        public int ShiftMax { get; set; }
    }



    public class SliceGlitchFrameProcessor : IFrameProcessor
    {
        private readonly ImageMaxValues _maxValues;
        // private readonly Timer _timer;
        private readonly SliceGlitchFrameProcessorSettings _settings;

        private readonly Task _updateTask;

        ReaderWriterLockSlim _slicesLock = new ReaderWriterLockSlim();
        private List<Slice> _slices = new List<Slice>(); 

        public SliceGlitchFrameProcessor(ImageMaxValues maxValues, SliceGlitchFrameProcessorSettings settings)
        {
            _maxValues = maxValues;
            _settings = settings;
            // _timer = new Timer(UpdateSlices, null, _settings.UpdateInterval, _settings.UpdateInterval); // _settings.UpdateInterval);
            _updateTask = Task.Factory.StartNew(UpdateSlices);
        }


        public ImageFrame Process(ImageFrame frame)
        {
            var res = new ImageFrame();
            res.Duration = frame.Duration;
            res.Number = frame.Number;

            if (!frame.Points.Any())
            {
                res.Points = new ImagePoint[0];
                return res;
            }

            res.Points = GetPoints(frame.Points);


            return res;

        }


        void UpdateSlices() //object x)
        {

            var random = new Random((int)DateTime.Now.Ticks);

            while (true)
            {
                var slices = new List<Slice>();
                
                var count = random.Next(_settings.SliceCountMin, _settings.SliceCountMax);

                for (int i = 0; i < count; i++)
                {
                    var slice = new Slice();

                    var retry = 0;

                    while (retry < 300)
                    {

                        slice.Height = random.Next(_settings.SliceHeightMin, _settings.SliceHeightMax);
                        slice.Shift = random.Next(_settings.ShiftMin, _settings.ShiftMax);

                        slice.Position = random.Next(_maxValues.MinY, _maxValues.MaxX + _settings.SliceHeightMax);

                        var collisions = slices.Where(s =>
                            s.Position + s.Height >= slice.Position && s.Position <= slice.Position + slice.Height
                            );

                        if (!collisions.Any())
                        {
                            slices.Add(slice);
                            break;
                        }

                        retry++;

                    }

                }

                _slicesLock.EnterWriteLock();

                _slices = slices;

                _slicesLock.ExitWriteLock();


                Thread.Sleep(random.Next(
                    (int)_settings.UpdateIntervalMin.TotalMilliseconds,
                    (int)_settings.UpdateIntervalMax.TotalMilliseconds)
                    );
            }

        }

        IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
        {
            _slicesLock.EnterReadLock();

            var slices = _slices.Select(s => (Slice) s.Clone()).ToArray();

            _slicesLock.ExitReadLock();

            Slice slice = null;

            foreach (var point in originalPoints)
            {

                if (!(slice?.IsInBound(point.Y) ?? false))
                {
                    slice = slices.SingleOrDefault(s => s.IsInBound(point.Y));
                }

                var newX = point.X + (slice?.Shift ?? 0);

                var blanking = point.Blanking;

                if (_maxValues.MinX > newX)
                {
                    newX = _maxValues.MinX;
                    blanking = true;
                }

                if (newX > _maxValues.MaxX)
                {
                    newX = _maxValues.MaxX;
                    blanking = true;
                }

                var newPoint = point.Clone();
                newPoint.X = newX;
                newPoint.Blanking = blanking;

                yield return newPoint;
                
            }
            
            yield break;

        }

        // https://stackoverflow.com/questions/4229662/convert-numbers-within-a-range-to-numbers-within-another-range
        int ConvertRange(
            int originalStart, int originalEnd, // original range
            int newStart, int newEnd, // desired range
            int value) // value to convert
        {
            double scale = (double)(newEnd - newStart) / (originalEnd - originalStart);
            return (int)(newStart + ((value - originalStart) * scale));
        }

    }

    class Slice: ICloneable
    { 
        public int Height { get; set; }
        public int Position { get; set; }
        public int Shift { get; set; }

        public bool IsInBound(int y) => (Position <= y) && (y <= Position + Height);

        public object Clone()
        {
            return new Slice()
            {
                Height = this.Height,
                Position = this.Position,
                Shift = this.Shift
            };
        }
    }


}