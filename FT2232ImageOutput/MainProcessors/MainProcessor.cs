using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FT2232ImageOutput.MainProcessors
{
    public class MainProcessor: IMainProcessor
    {
        private readonly IImageSource _imageSource;
        private readonly IEnumerable<IFrameProcessor> _frameProcessors;
        private readonly IPointBitMapper _bitMapper;
        private readonly IHardwareOutput _hardwareOutput;

        private readonly int _frameInterval;
        private readonly bool _overflowPreventing;
        private readonly ConcurrentQueue<byte[][]> _bufQueue = new ConcurrentQueue<byte[][]>();
        private readonly int _bufQueueMax = 2;

        
        public MainProcessor(
            IImageSource imageSource,
            IEnumerable<IFrameProcessor> frameProcessors,
            IPointBitMapper bitMapper,
            IHardwareOutput hardwareOutput,
            int frameInterval,
            bool overflowPreventing
            )
        {
            _imageSource = imageSource;
            _frameProcessors = frameProcessors;
            _bitMapper = bitMapper;
            _hardwareOutput = hardwareOutput;
            _frameInterval = frameInterval;
            _overflowPreventing = overflowPreventing;
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

                while (true)
                {

                    IEnumerable<ImageFrame> processedFrames = frames;

                    foreach (var fp in _frameProcessors)
                    {
                        processedFrames = processedFrames.Select(f => fp.Process(f));
                    }

                    // processedFrames = processedFrames.ToArray();


                    var sw = new Stopwatch();

                    using (var dataStream = new MemoryStream())
                    {


                        sw.Start();

                        int sleepOverhead = 0;

                        foreach (var frame in processedFrames) //.Where(p => p.Points.Any()))
                        {
                            sw.Reset();
                            sw.Start();

                            List<byte[]> frameBytes = new List<byte[]>();

                            foreach (var point in frame.Points.ToArray())
                            {
                                var bits = _bitMapper.Map(point);

                                dataStream.Write(bits, 0, bits.Length);

                                if (_overflowPreventing && 
                                    (dataStream.Length + _bitMapper.MaxBytesPerPoint * 2) > _hardwareOutput.MaxBytes)
                                {
                                    var blankedPoint = point.Clone();
                                    blankedPoint.Blanking = true;
                                    
                                    bits = _bitMapper.Map(blankedPoint);
                                    
                                    dataStream.Write(bits, 0, bits.Length);
                                
                                    // MemoryStream.ToArray() returns a copy of data
                                    frameBytes.Add(dataStream.ToArray());
                                
                                    dataStream.SetLength(0);
                                }

                            }


                            frameBytes.Add(dataStream.ToArray());
                            dataStream.SetLength(0);


                            while (true)
                            {
                                if (_bufQueue.Count() < _bufQueueMax)
                                    break;

                                Thread.Yield();
                            }

                            _bufQueue.Enqueue(frameBytes.ToArray());

                            dataStream.SetLength(0);

                            sw.Stop();

                            var restInterval = _frameInterval - sw.ElapsedMilliseconds - sleepOverhead;

                            if (restInterval > 0)
                            {
                                sw.Reset();
                                sw.Start();
                                Thread.Sleep((int)restInterval);
                                sw.Stop();

                                sleepOverhead = (int)(sw.ElapsedMilliseconds - restInterval);
                            }

                        }

                        sw.Reset();

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
                            _hardwareOutput.Output(fb, _overflowPreventing);
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
}
