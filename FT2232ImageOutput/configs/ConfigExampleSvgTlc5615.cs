using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using FT2232ImageOutput.FrameProcessors;
using FT2232ImageOutput.HardwareOutput;
using FT2232ImageOutput.ImageSources;
using FT2232ImageOutput.MainProcessors;
using FT2232ImageOutput.PointBitMappers;
using ManuPath.DotGenerators.FillGenerators;
using ManuPath.DotGenerators.StrokeGenerators;

namespace FT2232ImageOutput.Configs;

static class ConfigExampleSvgTlc5615
{
    public static void Run()
    {
        uint baudrate = 240_000;

        var filepath = @"..\..\..\samplefiles\lain.svg";

        var targetMaxValues = new ImageMaxValues()
        {
            MaxRGB = 255,
            MinX = 0, MaxX = 1023,
            MinY = 0, MaxY = 1023,
            MinZ = 0, MaxZ = 1023,
        };

        var oscilloscopeScreenSize = new Vector2(8 * 12, 8 * 12);

        var pixelSize = new Vector2(
            targetMaxValues.Width  / oscilloscopeScreenSize.X,
            targetMaxValues.Height / oscilloscopeScreenSize.Y);

        var pixelMin = Math.Min(pixelSize.X, pixelSize.Y);

        var imageSource = new SvgImageSource(
            filepath: filepath,

            strokeGeneratorFactory: figure => new EqualDistanceStrokeDotGenerator(
                figure,
                transform: false,
                pointDistanceMin: 5,
                pointDistanceMax: 6
                ),

            fillGeneratorFactory: figure => new IntervalFillDotGenerator(
                figure,
                transform: false,
                intervalMin: pixelSize * 0,
                intervalMax: pixelSize * 20
                ),

            strokeIntensityChannel: ColorChannel.Alpha,
            strokeBrightnessChannel: ColorChannel.Green,

            fillIntensityChannel: ColorChannel.Alpha,
            fillBrightnessChannel: ColorChannel.Green,

            maxValues: targetMaxValues
        );

        imageSource.Init();

        var frameProcessors = new List<IFrameProcessor>() {
           new AddBlankingPointsFrameProcessor(4, 4, true)
        };
         
        var pointBitMapper = new Tlc5615PointBitMapper(
            maxValues: targetMaxValues,
            dataMode: Tlc5615DataMode.Parallel12bit,
            manualDataClock: false,
            invertZ: false
            );

        
        var hardwareOutput = new FT2232HardwareOutput(baudrate, bufferSize: 102400);

        var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput);

        mainProcess.Start();

        Thread.Sleep(Timeout.Infinite);
    }




}
