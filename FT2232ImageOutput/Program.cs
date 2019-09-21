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

            // TODO: config file for building and configure everything

            var targetMaxValues = new ImageMaxValues() 
            {
                MaxRGB = 255,
                MinX = 0, MaxX = 1023,
                MinY = 0, MaxY = 1023,
                MinZ = 0, MaxZ = 15,
            };
            
            var generateMaxValues = new ImageMaxValues() 
            {
                MaxRGB = 255,
                MinX = 0, MaxX = 63,
                MinY = 0, MaxY = 63,
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

            // List<string> ildafiles = new List<string>();

            var imageSource = new IldaImageSource(filepath);
            // var imageSource = new IldaMultipleImageSource(ildafiles);
            // var imageSource = new LineImageSource(generateMaxValues);
            // var imageSource = new SolidRectangleImageSource(generateMaxValues);
            // var imageSource = new RandomImageSource(generateMaxValues);
            

            var frameProcessors = new List<IFrameProcessor>() {
                                
                new ScaleMaxValuesFrameProcessor(imageSource.MaxValues, targetMaxValues),

                new MonochromeFrameProcessor(MonochromeFrameProcessorSourceColor.Green),

                new GrayscaleFrameProcessor(targetMaxValues, GrayscaleFrameProcessorMapMode.CoordZ),
                
                new DuplicateReduceFrameProcessor(DuplicateReduceFrameProcessorFlags.All),

                new SineWaveFrameProcessor(TimeSpan.FromMilliseconds(10), 150, 3.5f, .5f, targetMaxValues),
                
                new AddBlankingPointsFrameProcessor()

            };

            var pointBitMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY10_Z4);
            // var pointBitMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY8_Z8);

            var hardwareOutput = new FT2232HardwareOutput("A", options.Baudrate, pointBitMapper);


            var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput, 60, 1000);

            mainProcess.Start();

            Thread.Sleep(Timeout.Infinite);

            return 1;
            
        }




    }
}
