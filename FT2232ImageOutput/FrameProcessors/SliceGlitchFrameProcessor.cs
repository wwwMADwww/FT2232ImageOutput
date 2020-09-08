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
        public bool LoopFrame { get; set; }

        public float DriftProbability { get; set; }

        public float DriftMin { get; set; }
        public float DriftMax { get; set; }

        public TimeSpan UpdateIntervalMin { get; set; }
        public TimeSpan UpdateIntervalMax { get; set; }

        public int SliceCountMin { get; set; }
        public int SliceCountMax { get; set; }

        public float SliceHeightMin { get; set; }
        public float SliceHeightMax { get; set; }

        public float ShiftMin { get; set; }
        public float ShiftMax { get; set; }
    }





    public class SliceGlitchFrameProcessor : IFrameProcessor
    {
        private readonly ImageMaxValues _maxValues;
        // private readonly Timer _timer;
        private readonly SliceGlitchFrameProcessorSettings _settings;
        private readonly SliceGlitchFrameProcessorAbsoluteValues _abs;

        private readonly Task _updateTask;

        ReaderWriterLockSlim _slicesLock = new ReaderWriterLockSlim();
        private List<Slice> _slices = new List<Slice>(); 

        public SliceGlitchFrameProcessor(ImageMaxValues maxValues, SliceGlitchFrameProcessorSettings settings)
        {
            _maxValues = maxValues;
            _settings = settings;

            _abs = new SliceGlitchFrameProcessorAbsoluteValues()
            {
                CarryPoints = settings.LoopFrame,

                DriftMin = GetAbsoluteValuesFromPercent(maxValues.MinX, maxValues.MaxX, settings.DriftMin),
                DriftMax = GetAbsoluteValuesFromPercent(maxValues.MinX, maxValues.MaxX, settings.DriftMax),

                DriftProbability = settings.DriftProbability,

                ShiftMin = GetAbsoluteValuesFromPercent(maxValues.MinX, maxValues.MaxX, settings.ShiftMin),
                ShiftMax = GetAbsoluteValuesFromPercent(maxValues.MinX, maxValues.MaxX, settings.ShiftMax),

                SliceCountMin = settings.SliceCountMin,
                SliceCountMax = settings.SliceCountMax,

                SliceHeightMin = GetAbsoluteValuesFromPercent(maxValues.MinY, maxValues.MaxY, settings.SliceHeightMin),
                SliceHeightMax = GetAbsoluteValuesFromPercent(maxValues.MinY, maxValues.MaxY, settings.SliceHeightMax),

                UpdateIntervalMin = settings.UpdateIntervalMin,
                UpdateIntervalMax = settings.UpdateIntervalMax
            };



            // _timer = new Timer(UpdateSlices, null, _settings.UpdateInterval, _settings.UpdateInterval); // _settings.UpdateInterval);
            _updateTask = Task.Factory.StartNew(UpdateSlices);
        }


        public ImageFrame Process(ImageFrame frame)
        {
            var res = new ImageFrame();
            res.Duration = frame.Duration;
            res.Number = frame.Number;

            // if (!frame.Points.Any())
            // {
            //     res.Points = new ImagePoint[0];
            //     return res;
            // }

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

                var sliceMode = random.NextDouble() <= _abs.DriftProbability
                    ? SliceMode.Drift
                    : SliceMode.Shift;

                for (int i = 0; i < count; i++)
                {
                    var slice = new Slice();
                    slice.Mode = sliceMode;

                    var retry = 0;

                    while (retry < 300)
                    {

                        slice.Height = random.Next(_abs.SliceHeightMin, _abs.SliceHeightMax);

                        //if (sliceMode == SliceMode.Drift)
                        {
                            slice.Drift = random.Next(_abs.DriftMin, _abs.DriftMax);
                        }
                        //else
                        {
                            slice.Shift = random.Next(_abs.ShiftMin, _abs.ShiftMax);
                        }

                        slice.Position = random.Next(_maxValues.MinY, _maxValues.MaxX + _abs.SliceHeightMax);

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

                int newX = point.X;

                if (slice != null)
                {
                    switch (slice.Mode)
                    {
                        case SliceMode.Drift:
                            var vertPosInSlice = point.Y - slice.Position;
                            var ypercent = (float) vertPosInSlice / (float) slice.Height;
                            var shift = (int)Math.Round((ypercent * slice.Drift) - (slice.Drift / 2.0));
                            newX += shift;

                            newX = newX + slice.Shift;
                            break;

                        case SliceMode.Shift:
                            newX = newX + slice.Shift;
                            break;
                    }
                }

                var blanking = point.Blanking;

                if (_abs.CarryPoints)
                {

                    // TODO: add blanking before carrying?

                    if (_maxValues.MinX > newX)
                    {
                        newX = _maxValues.MaxX - (_maxValues.MinX - newX);
                        //blanking = true;
                    }

                    if (newX > _maxValues.MaxX)
                    {
                        newX = _maxValues.MinX + (newX - _maxValues.MaxX);
                        // blanking = true;
                    }

                    var newPoint = point.Clone();
                    newPoint.X = newX;
                    newPoint.Blanking = blanking;

                    yield return newPoint;

                }
                else
                {
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
        
        int GetAbsoluteValuesFromPercent(int originalStart, int originalEnd, float percent)
        {
            var diff = originalEnd - originalStart;
            return (int) Math.Round(originalStart + (diff * percent));
        }

    }

    class Slice: ICloneable
    {
        public SliceMode Mode { get; set; }
        public int Height { get; set; }
        public int Position { get; set; }
        public int Shift { get; set; }
        public int Drift { get; set; }

        public bool IsInBound(int y) => (Position <= y) && (y <= Position + Height);

        public object Clone()
        {
            return new Slice()
            {
                Height = this.Height,
                Position = this.Position,
                Shift = this.Shift,
                Drift = this.Drift,
                Mode = this.Mode
            };
        }
    }

    enum SliceMode { Shift, Drift };

    public class SliceGlitchFrameProcessorAbsoluteValues
    {
        public bool CarryPoints { get; set; }

        public float DriftProbability { get; set; }

        public int DriftMin { get; set; }
        public int DriftMax { get; set; }

        public TimeSpan UpdateIntervalMin { get; set; }
        public TimeSpan UpdateIntervalMax { get; set; }

        public int SliceCountMin { get; set; }
        public int SliceCountMax { get; set; }

        public int SliceHeightMin { get; set; }
        public int SliceHeightMax { get; set; }

        public int ShiftMin { get; set; }
        public int ShiftMax { get; set; }
    }


}