using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FT2232ImageOutput.MainProcessors;

public class MainProcessor: IMainProcessor
{
    private readonly IImageSource _imageSource;
    private readonly IEnumerable<IFrameProcessor> _frameProcessors;
    private readonly IPointBitMapper _bitMapper;
    private readonly IHardwareOutput _hardwareOutput;

    private readonly ConcurrentQueue<byte[][]> _bufQueue = new ConcurrentQueue<byte[][]>();
    private readonly int _bufQueueMax = 2;

    
    public MainProcessor(
        IImageSource imageSource,
        IEnumerable<IFrameProcessor> frameProcessors,
        IPointBitMapper bitMapper,
        IHardwareOutput hardwareOutput
        )
    {
        _imageSource = imageSource;
        _frameProcessors = frameProcessors;
        _bitMapper = bitMapper;
        _hardwareOutput = hardwareOutput;
    }


    public void Start()
    {
        Task.Factory.StartNew(() => FrameReadAndProcess());
        Task.Factory.StartNew(() => FrameOutput());            
    }


    void FrameReadAndProcess()
    {
        try
        {
            var frames = _imageSource.GetFrames();
            var dataStream = new MemoryStream();

            while (true)
            {

                IEnumerable<ImageFrame> processedFrames = frames;

                foreach (var fp in _frameProcessors)
                {
                    processedFrames = processedFrames.Select(f => fp.Process(f));
                }

                // processedFrames = processedFrames.ToArray();

                foreach (var frame in processedFrames) //.Where(p => p.Points.Any()))
                {
                    List<byte[]> frameBytes = new List<byte[]>();

                    var bitsw = new Stopwatch();

                    var points = frame.Points.ToArray();

                    foreach (var point in points)
                    {
                        var bits = _bitMapper.Map(point);

                        dataStream.Write(bits, 0, bits.Length);
                    }


                    var blankedPoint = points.Last().Clone();
                    blankedPoint.Blanking = true;
                    var blankedBits = _bitMapper.Map(blankedPoint); 
                    dataStream.Write(blankedBits, 0, blankedBits.Length);

                    frameBytes.Add(dataStream.ToArray());
                    dataStream.SetLength(0);


                    while (true)
                    {
                        if (_bufQueue.Count() < _bufQueueMax)
                            break;

                        Thread.Yield();
                    }

                    _bufQueue.Enqueue(frameBytes.ToArray());

                }

                if (_imageSource.Streaming)
                    frames = _imageSource.GetFrames();

            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in FrameReadAndProcess:");
            Console.WriteLine(e.ToString());
            throw;
        }



    }


    void FrameOutput()
    {
        try
        {

            byte[][] frameBytes = null;

            while (true)
            {

                if (_bufQueue.TryDequeue(out var newFrameBytes))
                {
                    frameBytes = newFrameBytes;
                }

                if (frameBytes?.Any() ?? false)
                {
                    foreach (var fb in frameBytes.Where(b => b?.Any() ?? false))
                        _hardwareOutput.Output(fb);
                }
                else 
                {
                    Thread.Yield();
                }

            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception in FrameOutput:");
            Console.WriteLine(e.ToString());
            throw;
        }

    }

}
