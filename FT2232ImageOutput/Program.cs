﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using FT2232ImageOutput.FrameProcessors;
using FT2232ImageOutput.HardwareOutput;
using FT2232ImageOutput.ImageSources;
using FT2232ImageOutput.MainProcessors;
using FT2232ImageOutput.PointBitMappers;

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
                MinX = 0, MaxX = 255,
                MinY = 0, MaxY = 255,
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
            // var imageSource = new TestLineImageSource(generateMaxValues);
            // var imageSource = new SolidRectangleImageSource(generateMaxValues);
            // var imageSource = new RandomImageSource(generateMaxValues);
            // var imageSource = new SpruceImageSource(targetMaxValues);
            // var imageSource = new MeandreImageSource(targetMaxValues);
            // var imageSource = new TwoPointsImageSource(targetMaxValues, 1024);


            // var imageSource = new MatrixBootstrap().CrateMatrix(
            //     filepath,
            //     new MatrixImageSourceConfig()
            //     {
            //         Width = 1024,
            //         Height = 1024,
            //         MaxRGB = 255,
            //         Cols = 28,
            //         Rows = 19,
            //         TrailLength = 25,
            //         TrailGenerationRate = 20,
            //         TrailChangeRateMin = 15,
            //         TrailChangeRateMax = 50,
            //         TrailChangeIntencity = 50
            //     },
            //     new ImageMaxValues()
            //     {
            //         MaxRGB = 255,
            //         MinX = 0,
            //         MaxX = 35,
            //         MinY = 0,
            //         MaxY = 50,
            //         MinZ = 0,
            //         MaxZ = 15,
            //     }
            // ); ;

            var frameProcessors = new List<IFrameProcessor>() {

                new ScaleMaxValuesFrameProcessor(imageSource.MaxValues, targetMaxValues),

                new MonochromeFrameProcessor(MonochromeFrameProcessorSourceColor.Green),

                new GrayscaleFrameProcessor(targetMaxValues, GrayscaleFrameProcessorMapMode.CoordZ),

                // new RotateFrameProcessor(targetMaxValues, 0, null, 0.1f, 25),
                
                // new RotateFrameProcessor(targetMaxValues, 90, null, 0.1f, 0),

                new DuplicateReduceFrameProcessor(DuplicateReduceFrameProcessorFlags.All),

                // new DuplicatePointsFrameProcessor(10, 10f),
                
                new AddBlankingPointsFrameProcessor()

            };

            var pointBitMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY10_Z4);
            // var pointBitMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4);
            // var pointBitMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY8_Z8);

            var hardwareOutput = new FT2232HardwareOutput("A", options.Baudrate, pointBitMapper);


            var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput, 24, 1000);

            mainProcess.Start();

            Thread.Sleep(Timeout.Infinite);

            return 1;
            
        }




    }
}
