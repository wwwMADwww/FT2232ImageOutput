using System;
using System.Collections.Generic;
using System.Linq;
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
         
        var pointBitMapper = new ShiftRegisterPointBitMapper(
            // mode: ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY8_Z8, // XYZ 8 bit
            mode: ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4, // XY 10 bit, Z 4 bit
            invertZ: false, 
            maxValues: targetMaxValues
            );

        // see console output for locationId
        var hardwareOutput = new FT2232HardwareOutput(locationId: 0, baudrate, bufferSize: 10240);

        var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput);

        mainProcess.Start();

        Thread.Sleep(Timeout.Infinite);
    }




}
