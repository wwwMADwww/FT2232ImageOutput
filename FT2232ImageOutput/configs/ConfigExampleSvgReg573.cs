using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using FT2232ImageOutput.FrameProcessors;
using FT2232ImageOutput.HardwareOutput;
using FT2232ImageOutput.MainProcessors;
using FT2232ImageOutput.PathImages.PathSources;
using FT2232ImageOutput.PathImages;
using FT2232ImageOutput.PointBitMappers;
using ManuPath.PrimitiveConverters;
using ManuPath.FillGenerators;

namespace FT2232ImageOutput.Configs;

static class ConfigExampleSvgReg573
{
    public static void Run()
    {
        // uint baudrate = 12_000_000; // = 20 MBytes/s = 6.(6) MPoints/s

        uint baudrate = 1_000_000;

        var filepath = @"..\..\..\samplefiles\svg\circle_bezier.svg";

        var targetMaxValuesXY10Z4 = new ImageMaxValues() // XY 10 bit Z 4 bit
        {
            MaxRGB = 31,
            MinX = 0, MaxX = 1023,
            MinY = 0, MaxY = 1023,
            MinZ = 0, MaxZ = 31,
        };
        var targetMaxValues = targetMaxValuesXY10Z4;

        var oscilloscopeScreenSize = new Vector2(8 * 12, 8 * 12);

        var pixelSize = new Vector2(
            (targetMaxValues.MaxX - targetMaxValues.MinX) / oscilloscopeScreenSize.X,
            (targetMaxValues.MaxY - targetMaxValues.MinY) / oscilloscopeScreenSize.Y);

        var pixelMin = Math.Min(pixelSize.X, pixelSize.Y);

        var imageSource = new PathImageSource(
            pathSource: new SvgFilePathSource(filepath),

            strokeConverter: new PrimitiveToEvenSegmentsConverter(
                pointDistanceMin: pixelMin * 0.2f, 
                pointDistanceMax: pixelMin * 0.2f + 0.1f, 
                closePath: false),

            strokeBrightness: ColorChannel.Alpha,

            fillConverter: null,
            fillGeneratorFactory: path => new IntervalDotsFillGenerator(
                path,
                intervalMin: new Vector2(1.2f, 1.2f) * pixelSize,
                intervalMax: new Vector2(1.1f, 1.1f) * pixelSize
                ),

            fillIntensity: ColorChannel.Alpha,
            fillBrightness: ColorChannel.Green,

            maxValues: targetMaxValues,
            forceNotStreaming: true
        );

        var frameProcessors = new List<IFrameProcessor>() {
               new AddBlankingPointsFrameProcessor(4, 4, true),
               new ScaleMaxValuesFrameProcessor(imageSource.MaxValues, targetMaxValues)
            };

        var pointBitMapper = new Reg573PointBitMapper(
            invertZ: true,
            maxValues: targetMaxValues);

        var hardwareOutput = new FT2232HardwareOutput("A", baudrate, 100_000);

        var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput);

        mainProcess.Start();

        Thread.Sleep(Timeout.Infinite);
    }


}
