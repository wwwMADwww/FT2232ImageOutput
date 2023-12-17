using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FT2232ImageOutput.Utils;

namespace FT2232ImageOutput.FrameProcessors;


public class SliceGlitchFrameProcessorSettings
{
    // carry points to the opposite side if they moved out of frame
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


public class SliceGlitchFrameProcessor : IFrameProcessor
{
    private readonly ImageMaxValues _maxValues;
    private readonly SliceGlitchFrameProcessorSettings _settings;
    private readonly IEnumerable<SliceArea> _areas;

    private readonly Random _random = new Random((int)DateTime.Now.Ticks);

    private readonly Task _updateTask;

    private readonly bool _randomSlices;
    private readonly ReaderWriterLockSlim _slicesLock = new ReaderWriterLockSlim();
    private List<Slice> _slices = new List<Slice>();

    public SliceGlitchFrameProcessor(
        ImageMaxValues maxValues, 
        SliceGlitchFrameProcessorSettings settings,
        IEnumerable<SliceArea> areas = null,
        IEnumerable<Slice> slices = null)
    {
        _maxValues = maxValues;
        _settings = settings;

        _areas = areas ?? new[] { new SliceArea(_maxValues.MinY, _maxValues.MaxY) };
        if (slices == null)
            _randomSlices = true;
        else
            _slices = slices.ToList();

        if (_randomSlices)
            _updateTask = Task.Factory.StartNew(UpdateSlices);
    }


    public ImageFrame Process(ImageFrame frame)
    {
        var res = new ImageFrame();
        res.Duration = frame.Duration;
        res.Number = frame.Number;

        res.Points = GetPoints(frame.Points);

        return res;
    }


    void UpdateSlices()
    {

        while (true)
        {
            var slices = new List<Slice>();
            
            var count = _random.Next(_settings.SliceCountMin, _settings.SliceCountMax);

            var sliceMode = _random.NextDouble() <= _settings.DriftProbability
                ? SliceMode.Drift
                : SliceMode.Shift;

            for (int i = 0; i < count; i++)
            {
                var slice = new Slice();
                slice.Mode = sliceMode;

                var retry = 0;

                while (retry < 300)
                {

                    slice.Area.Start = _random.Next(_maxValues.MinY, _maxValues.MaxX + _settings.SliceHeightMax);

                    slice.Area.End = slice.Area.Start + _random.Next(_settings.SliceHeightMin, _settings.SliceHeightMax);

                    slice.Drift = _random.Next(_settings.DriftMin, _settings.DriftMax);
                    slice.Shift = _random.Next(_settings.ShiftMin, _settings.ShiftMax);

                    var collisions = slices.Where(s => !s.Area.IsOverlap(slice.Area));

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


            Thread.Sleep(_random.Next(
                (int)_settings.UpdateIntervalMin.TotalMilliseconds,
                (int)_settings.UpdateIntervalMax.TotalMilliseconds)
                );
        }

    }

    IEnumerable<ImagePoint> GetPoints(IEnumerable<ImagePoint> originalPoints)
    {
        IEnumerable<Slice> slices;

        if (_randomSlices)
        {
            _slicesLock.EnterReadLock();

            slices = _slices.Select(s => (Slice)s.Clone()).ToArray();

            _slicesLock.ExitReadLock();
        }
        else
        {
            slices = _slices;
        }

        Slice slice = null;
        SliceArea area = null;


        foreach (var point in originalPoints)
        {
            if (!(area?.Contains(point.Y) ?? false))
            {
                area = _areas.FirstOrDefault(a => a.Contains(point.Y));
                if (area == null)
                {
                    yield return point;
                    continue;
                }
            }

            if (!(slice?.Area.Contains(point.Y) ?? false))
            {
                slice = slices.FirstOrDefault(s => s.Area.Contains(point.Y));
                if (slice == null)
                {
                    yield return point;
                    continue;
                }
            }

            var newPoint = point.Clone();

            if (slice != null)
            {
                switch (slice.Mode)
                {
                    case SliceMode.Drift:
                        var vertPosInSlice = point.Y - slice.Area.Start;
                        var ypercent = (float) vertPosInSlice / slice.Area.Height;
                        var shift = (int)Math.Round((ypercent * slice.Drift) - (slice.Drift / 2.0));
                        newPoint.X += shift;

                        newPoint.X += slice.Shift;
                        break;

                    case SliceMode.Shift:
                        newPoint.X += slice.Shift;
                        break;
                }
            }



            if (_settings.CarryPoints)
            {
                // TODO: add blanking before carrying?

                if (_maxValues.MinX > newPoint.X)
                {
                    newPoint.X = _maxValues.MaxX - (_maxValues.MinX - newPoint.X);
                }

                if (newPoint.X > _maxValues.MaxX)
                {
                    newPoint.X = _maxValues.MinX + (newPoint.X - _maxValues.MaxX);
                }
            }
            else
            {
                if (_maxValues.MinX > newPoint.X)
                {
                    newPoint.X = _maxValues.MinX;
                    newPoint.Blanking = true;
                }

                if (newPoint.X > _maxValues.MaxX)
                {
                    newPoint.X = _maxValues.MaxX;
                    newPoint.Blanking = true;
                }

            }

            yield return newPoint;
            
        } // /foreach originalPoints
        
        yield break;

    }


}

public class Slice: ICloneable
{
    public SliceMode Mode { get; set; }
    public SliceArea Area { get; set; } = new SliceArea(0, 0);
    public int Shift { get; set; }
    public int Drift { get; set; }

    public object Clone()
    {
        return new Slice()
        {
            Area = this.Area,
            Shift = this.Shift,
            Drift = this.Drift,
            Mode = this.Mode
        };
    }
}


public class SliceArea
{
    public SliceArea(int start, int end)
    {
        Start = start;
        End = end;
    }

    public int Start { get; set; }
    public int End { get; set; }
    public int Height => End - Start;


    public bool Contains(int p) => Start < p && p < End;

    public bool IsOverlap(SliceArea area) => Contains(area.Start) || Contains(area.End);
}


public enum SliceMode { Shift, Drift };