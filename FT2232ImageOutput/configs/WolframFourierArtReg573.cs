using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using FT2232ImageOutput.FrameProcessors;
using FT2232ImageOutput.HardwareOutput;
using FT2232ImageOutput.MainProcessors;
using FT2232ImageOutput.PointBitMappers;
using FT2232ImageOutput.ImageSources;

namespace FT2232ImageOutput.Configs;

static class WolframFourierArtReg573
{
    public static void Run()
    {
        // uint baudrate = 12_000_000; // = 20 MBytes/s = 6.(6) MPoints/s

        uint baudrate = 1_000_000;

        var filepath = @"..\..\..\samplefiles\etc\pooh formula.txt";

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
            targetMaxValues.Width / oscilloscopeScreenSize.X,
            targetMaxValues.Height / oscilloscopeScreenSize.Y);

        var pixelMin = Math.Min(pixelSize.X, pixelSize.Y);

        var imageSource = new WolframFourierArtImageSource(
            filepath, 
            argumentIncrement: 0.0001f,
            dotDistanceMin: pixelMin * 0.5f,
            dotDistanceMax: pixelMin * 0.6f,
            targetMaxValues);
        imageSource.Init();

        var frameProcessors = new List<IFrameProcessor>() {
               new AddBlankingPointsFrameProcessor(4, 4, true)
            };

        var pointBitMapper = new Reg573PointBitMapper(
            invertZ: true,
            maxValues: targetMaxValues);

        // see console output for locationId
        var hardwareOutput = new FT2232HardwareOutput(locationId: 37, baudrate, bufferSize: 100_000);

        var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput);

        mainProcess.Start();

        Thread.Sleep(Timeout.Infinite);
    }


}
