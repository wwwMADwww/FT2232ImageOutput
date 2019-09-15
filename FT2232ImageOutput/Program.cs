using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;

namespace FT2232ImageOutput
{
    class Program
    {
        static int Main(string[] args)
        {

            var paramsParsed = CommandLineParamsProcessor.Process(args, out var options);

            if (!paramsParsed)
            {
                return 1;
            }

            var filepath = Path.GetFullPath(options.Filepath);

            Console.WriteLine($"Filename is {filepath}");
            Console.WriteLine($"Baudrate is {options.Baudrate}");
            
            // TODO: dedicated domain class

            var targetMaxValues = new ImageMaxValues() 
            {
                MaxRGB = 255,
                MinX = 0, MaxX = 1023,
                MinY = 0, MaxY = 1023,
                MinZ = 0, MaxZ = 15,
            };

            // var generateMaxValues = new ImageMaxValues() 
            // {
            //     MaxRGB = 255,
            //     MinX = 0, MaxX = 1023,
            //     MinY = 0, MaxY = 1023,
            //     MinZ = 0, MaxZ = 1023,
            // };

            // var generateMaxValues = new ImageMaxValues()
            // {
            //     MaxRGB = 255,
            //     MinX = 0, MaxX = 255,
            //     MinY = 0, MaxY = 255,
            //     MinZ = 0, MaxZ = 255,
            // };

            
            var imageSource = new IldaImageSource(filepath);
            // var imageSource = new LineImageSource(generateMaxValues);


            // TODO: remove collection copying where possible
            // TODO: frame timings


            // TODO: config for building and configure pipeline

            var frameProcessors = new List<IFrameProcessor>() {

                new ScaleMaxValuesFrameProcessor(imageSource.MaxValues, targetMaxValues),
                
                new GrayscaleFrameProcessor(targetMaxValues, GrayscaleFrameProcessorMapMode.CoordZ),
                
                new DuplicateReduceFrameProcessor(DuplicateReduceFrameProcessorFlags.All),

                new AddBlankingPointsFrameProcessor()

            };

            var pointMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY10_Z4);
            // var pointMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY8_Z8);

            // TODO: decouple mapper and hw output and do buffering bits for sending those to hw repeatedly while next frame is processing in parallel
            var hardwareOutput = new FT2232HardwareOutput("A", options.Baudrate, pointMapper);

            var frames = imageSource.GetFrames();

            // TODO: parallel processing and output
            while (true)
            {
                IEnumerable<ImageFrame> processedFrames = frames;


                foreach (var fp in frameProcessors)
                {
                    processedFrames = processedFrames.Select(f => fp.Process(f));
                }

                processedFrames = processedFrames.ToArray();
                
                hardwareOutput.Output(processedFrames);
                
            }

            return 0;

        }




    }
}
