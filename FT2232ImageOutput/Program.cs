using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FT2232ImageOutput.FrameProcessors;
using FT2232ImageOutput.HardwareOutput;
using FT2232ImageOutput.ImageSources;
using FT2232ImageOutput.MainProcessors;
using FT2232ImageOutput.PathImages;
using FT2232ImageOutput.PathImages.PathSources;
using FT2232ImageOutput.PointBitMappers;
using ManuPath.FillGenerators;
using ManuPath.PrimitiveConverters;

namespace FT2232ImageOutput
{
    class Program
    {
        static int Main(string[] args)
        {
            // uint baudrate = 3_000_000; // MCP4921 absolute max data speed
            // uint baudrate = 3_000_000; // MCP4921 0-255 (8 bit). square wave 440khz (880k points/s). wave shape is heavy filtered triangle
            uint baudrate = 2_000_000; // MCP4921 0-1023 (10 bit). square wave 320khz (640k points/s). wave shape has triangle top, flat bottom
            // uint baudrate = 1_000_000; // most stable, minimum data stream tearing
            // uint baudrate =   610_000; // MCP4921 0-4095 (12 bit). square wave 90khz (180k points/s). wave shape is triangle


            var filepath = @"..\..\..\samplefiles\svg\circle.svg";
            //var filepath = @"..\..\..\samplefiles\svg\strokeshades.svg";
            //var filepath = @"..\..\..\samplefiles\svg\wikipedia_fillrules.svg";



            Console.WriteLine($"Filename is {filepath}");
            Console.WriteLine($"Baudrate is {baudrate}");

            // TODO: config file for building and configure everything

             //var targetMaxValues = new ImageMaxValues() 
             //{
             //    MaxRGB = 255,
             //    MinX = 0, MaxX = 255,
             //    MinY = 0, MaxY = 255,
             //    MinZ = 0, MaxZ = 255,
             //};

            
            //var targetMaxValues = new ImageMaxValues() 
            //{
            //    MaxRGB = 255,
            //    MinX = 0, MaxX = 1023,
            //    MinY = 0, MaxY = 1023,
            //    MinZ = 0, MaxZ = 1023,
            //};

            var targetMaxValues = new ImageMaxValues() 
            {
                MaxRGB = 255,
                MinX = 0, MaxX = 4095,
                MinY = 0, MaxY = 4095,
                MinZ = 0, MaxZ = 4095,
            };

            int genOffsetX = 0; // 32 * 0 - 1;
            int genOffsetY = 0;

            // var generateMaxValues = new ImageMaxValues() 
            // {
            //     MaxRGB = 255,
            //     MinX = genOffsetX, MaxX = 1023 + genOffsetX,
            //     MinY = genOffsetY, MaxY = 1023 + genOffsetY,
            //     MinZ = 0, MaxZ = 15,
            // };

            var generateMaxValues = new ImageMaxValues() 
            {
                MaxRGB = 255,
                MinX = genOffsetX, MaxX = 4095 + genOffsetX,
                MinY = genOffsetY, MaxY = 4095 + genOffsetY,
                MinZ = 0, MaxZ = 4095,
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



            var screenSize = new Vector2(8 * 12, 8 * 12);

            var pixelSize = new Vector2(
                (targetMaxValues.MaxX - targetMaxValues.MinX) / screenSize.X,
                (targetMaxValues.MaxY - targetMaxValues.MinY) / screenSize.Y
                );
            var pixelMin = Math.Min(pixelSize.X, pixelSize.Y);


            // List<string> ildafiles = new List<string>();

            // sample rate | baudrate
            //      192000     307200
            //       96000     153600
            //       48000      76800
            //       44100      70550
            // var imageSource = new WaveFileImageSource(filepath);

            // var imageSource = new IldaImageSource(filepath);
            // var imageSource = new IldaMultipleImageSource(ildafiles);
            // var imageSource = new TestLineImageSource(targetMaxValues, generateMaxValues, true);
            // var imageSource = new SolidRectangleImageSource(generateMaxValues);
            // var imageSource = new RandomImageSource(generateMaxValues);
            // var imageSource = new SpruceImageSource(targetMaxValues);
            // var imageSource = new MeandreImageSource(targetMaxValues);
            // var imageSource = new PointsImageSource(targetMaxValues, 10240);


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


            // var pathImageSourceParams = new PathToPointImageSourceParams()
            // {
            // 
            //     DotImageDistanceMin = 20f,
            //     DotImageDistanceMax = 21f,
            // 
            //     FillDistanceMin = 80f,
            //     FillDistanceMax = 85f
            // };


            var imageSource = new PathImageSource(
                pathSource: new SvgFilePathSource(filepath),

                strokeConverter: new PrimitiveToEvenSegmentsConverter(pixelMin * 0.2f, pixelMin * 0.2f + 1, false),
                strokeBrightness: ColorChannel.Alpha,

                fillConverter: null,
                fillGeneratorFactory: p => new IntervalDotsFillGenerator(p,
                    new Vector2(0.2f, 0.2f) * pixelSize,
                    new Vector2(2f, 2f) * pixelSize
                    , new Vector2(2f, 2f) * pixelSize
                    ),
                fillIntensity: ColorChannel.Green, 
                fillBrightness: ColorChannel.Alpha,

                targetMaxValues
            );


            var frameProcessors = new List<IFrameProcessor>() {

                //new ScaleMaxValuesFrameProcessor(imageSource.MaxValues, targetMaxValues),

                //new MonochromeFrameProcessor(MonochromeFrameProcessorSourceColor.Green),

                //new GrayscaleFrameProcessor(targetMaxValues, GrayscaleFrameProcessorMapMode.CoordZ, true),

                // new RotateFrameProcessor(targetMaxValues, 0, null, 0.1f, 25),
                
                // new RotateFrameProcessor(targetMaxValues, 90, null, 0.1f, 0),

                //new DuplicateReduceFrameProcessor(DuplicateReduceFrameProcessorFlags.All),

                // new DuplicatePointsFrameProcessor(10, 10f),

                // new AddBlankingPointsFrameProcessor(10, 4, true),


                // new SliceGlitchFrameProcessor(targetMaxValues, new SliceGlitchFrameProcessorSettings(){
                //     LoopFrame = true,
                //     DriftProbability = 0.2f,
                //     DriftMin = 0.2f,
                //     DriftMax = 0.4f,
                //     ShiftMin = -0.1f,
                //     ShiftMax = 0.1f,
                //     SliceCountMin = 1,
                //     SliceCountMax = 10,
                //     SliceHeightMin = 0.05f,
                //     SliceHeightMax = 0.40f,
                //     UpdateIntervalMin = TimeSpan.FromMilliseconds(30),
                //     UpdateIntervalMax = TimeSpan.FromMilliseconds(100)
                // })

            };

            // var pointBitMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4_3);
            // var pointBitMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4_2);
            // var pointBitMapper = new ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY8_Z8);
            // var pointBitMapper = new DirectPointBitMapper();

            var pointBitMapper = new Mcp4921PointBitMapper(true, false, true, targetMaxValues);

            var hardwareOutput = new FT2232HardwareOutput("A", baudrate,
                4096
                //8192
                //10240
                );
            // var hardwareOutput = new StubHardwareOutput(1024, 4096, 1);


            var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput, 0, true);

            mainProcess.Start();

            Thread.Sleep(Timeout.Infinite);

            return 1;
            
        }




    }
}
