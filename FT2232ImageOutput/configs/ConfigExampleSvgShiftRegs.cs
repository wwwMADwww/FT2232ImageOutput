using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using FT2232ImageOutput.FrameProcessors;
using FT2232ImageOutput.HardwareOutput;
using FT2232ImageOutput.MainProcessors;
using FT2232ImageOutput.PathImages;
using FT2232ImageOutput.PathImages.PathSources;
using FT2232ImageOutput.PointBitMappers;
using ManuPath.FillGenerators;
using ManuPath.PrimitiveConverters;

namespace FT2232ImageOutput.Configs;

static class ConfigExampleSvgShiftRegs
{
    public static void Run()
    {
        uint baudrate = 500_000;

        var filepath = @"..\..\..\samplefiles\lain.svg";

        var targetMaxValues8 = new ImageMaxValues() // XYZ 8 bit
        {
            MaxRGB = 255,
            MinX = 0, MaxX = 255,
            MinY = 0, MaxY = 255,
            MinZ = 0, MaxZ = 255,
        };
        var targetMaxValues10 = new ImageMaxValues() // XY 10 bit, Z 4 bit
        {
            MaxRGB = 255,
            MinX = 0, MaxX = 1023,
            MinY = 0, MaxY = 1023,
            MinZ = 0, MaxZ = 15,
        };
        var targetMaxValues = targetMaxValues10;


        var oscilloscopeScreenSize = new Vector2(8 * 12, 8 * 12);

        var pixelSize = new Vector2(
            (targetMaxValues.MaxX - targetMaxValues.MinX) / oscilloscopeScreenSize.X,
            (targetMaxValues.MaxY - targetMaxValues.MinY) / oscilloscopeScreenSize.Y
            );
        var pixelMin = Math.Min(pixelSize.X, pixelSize.Y);

        var imageSource = new PathImageSource(
            pathSource: new SvgFilePathSource(filepath),
        
            strokeConverter: new PrimitiveToEvenSegmentsConverter(pixelMin * 0.3f, pixelMin * 0.3f + 0.3f, false),
            strokeBrightness: ColorChannel.Alpha,
        
            fillConverter: null,
            fillGeneratorFactory: path => new IntervalDotsFillGenerator(
                path,
                new Vector2(1.2f, 1.2f) * pixelSize,
                new Vector2(1.1f, 1.1f) * pixelSize
                ),
            fillIntensity : ColorChannel.Alpha,
            fillBrightness: ColorChannel.Green,
        
            maxValues: targetMaxValues,
            forceNotStreaming: true
        );


        var frameProcessors = new List<IFrameProcessor>() {
           new AddBlankingPointsFrameProcessor(4, 4, true)
        };
         
        var pointBitMapper = new ShiftRegisterPointBitMapper(
            // mode: ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY8_Z8, // XYZ 8 bit
            mode: ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4, // XY 10 bit, Z 4 bit
            invertZ: false, 
            maxValues: targetMaxValues
            );

        var hardwareOutput = new FT2232HardwareOutput("A", baudrate, 10240);

        var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput);

        mainProcess.Start();

        Thread.Sleep(Timeout.Infinite);
    }




}
