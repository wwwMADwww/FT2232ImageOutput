using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        private readonly int _framerate;
        private readonly int _waitTimeout;

        byte[] _frameBuf = new byte[0];
        SemaphoreSlim _frameDrawnSemaphore;
        SemaphoreSlim _frameBufUpdateSemaphore;

        
        public MainProcessor(
            IImageSource imageSource,
            IEnumerable<IFrameProcessor> frameProcessors,
            IPointBitMapper bitMapper,
            IHardwareOutput hardwareOutput,
            int framerate,
            int waitTimeout
            )
        {
            _imageSource = imageSource;
            _frameProcessors = frameProcessors;
            _bitMapper = bitMapper;
            _hardwareOutput = hardwareOutput;
            _framerate = framerate;
            _waitTimeout = waitTimeout;
            _frameDrawnSemaphore = new SemaphoreSlim(1, 1);
            _frameBufUpdateSemaphore = new SemaphoreSlim(1, 1);
        }


        public void Start()
        {
            Task.Factory.StartNew(async () => await FrameReadAndProcess());
            Task.Factory.StartNew(async () => await FrameOutput());            
        }


        async Task FrameReadAndProcess()
        {
            int frameInterval = (int) Math.Floor(1000.0 / _framerate);

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

                        foreach (var frame in processedFrames.Where(p => p.Points.Any()))
                        {
                            sw.Reset();
                            sw.Start();

                            byte[] frameBytes;

                            foreach (var point in frame.Points.ToArray())
                            {
                                var bits = _bitMapper.Map(point);

                                dataStream.Write(bits, 0, bits.Length);

                            }

                            frameBytes = dataStream.ToArray();


                            var b = await _frameDrawnSemaphore.WaitAsync(_waitTimeout);
                            Debug.Assert(b); // trying to catch a deadlock. maybe already fixed.
                            b = await _frameBufUpdateSemaphore.WaitAsync(_waitTimeout);
                            Debug.Assert(b);
                            _frameBuf = frameBytes;
                            _frameBufUpdateSemaphore.Release();

                            dataStream.SetLength(0);

                            sw.Stop();

                            var restInterval = frameInterval - sw.ElapsedMilliseconds;


                            if (restInterval > 0)
                                Thread.Sleep((int)restInterval);
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
            }



        }


        async Task FrameOutput()
        {
            try
            {
                while (true)
                {

                    var b = await _frameBufUpdateSemaphore.WaitAsync(_waitTimeout);
                    Debug.Assert(b);


                    var frameBytes = _frameBuf;
                    _hardwareOutput.Output(frameBytes);

                    _frameBufUpdateSemaphore.Release();

                    if (_frameDrawnSemaphore.CurrentCount == 0)
                        _frameDrawnSemaphore.Release();

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception in FrameOutput:");
                Console.WriteLine(e.ToString());
            }

        }

    }
}
