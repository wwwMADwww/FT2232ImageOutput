using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FT2232ImageOutput.FrameProcessors;
using FT2232ImageOutput.HardwareOutput;
using FT2232ImageOutput.ImageSources;
using FT2232ImageOutput.MainProcessors;
using FT2232ImageOutput.PointBitMappers;

namespace FT2232ImageOutput.Configs;

static class ConfigMatrix
{
    public static void Run()
    {
        uint baudrate = 2_000_000;

        var matrixSymbolsPath = @"..\..\..\samplefiles\matrixSymbols\";

        var targetMaxValues = new ImageMaxValues() 
        {
            MaxRGB = 255,
            MinX = 0, MaxX = 1023,
            MinY = 0, MaxY = 1023,
            MinZ = 0, MaxZ = 15,
        };

        var imageSource = CrateMatrix(
            matrixSymbolsPath,
            new MatrixImageSourceConfig()
            {
                Width = 1024,
                Height = 1024,
                MaxRGB = 255,
                Cols = 28,
                Rows = 19,
                TrailLength = 25,
                TrailGenerationRate = 20,
                TrailChangeRateMin = 15,
                TrailChangeRateMax = 50,
                TrailChangeIntencity = 50
            },
            targetMaxValues
        ); ;

        var pointBitMapper = new ShiftRegisterPointBitMapper(
            mode: ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4,
            invertZ: false,
            maxValues: targetMaxValues
            );

        var frameProcessors = new List<IFrameProcessor>() { };

        var hardwareOutput = new FT2232HardwareOutput("A", baudrate, 10240);
        
        var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput);

        mainProcess.Start();

        Thread.Sleep(Timeout.Infinite);
    }

    static IImageSource CrateMatrix(string dir, MatrixImageSourceConfig config, ImageMaxValues maxValues)
    {

        List<Symbol> symbols = new List<Symbol>();


        var blanker = new AddBlankingPointsFrameProcessor(10, 10, true);

        foreach (var file in Directory.EnumerateFiles(dir, "*.ild"))
        {

            var imagesource = new IldaImageSource(Path.Combine(dir, file));

            var scaler = new ScaleMaxValuesFrameProcessor(imagesource.MaxValues, maxValues);

            var image = imagesource.GetFrames()
                .Where(f => f.Points.Any())
                .Select(f => scaler.Process(f))
                .Select(f => blanker.Process(f));

            foreach (var frame in image)
            {
                var s = new Symbol()
                {
                    MaxValues = maxValues,
                    Name = Path.GetFileNameWithoutExtension(file),
                    Points = frame.Points.ToArray()
                };
                symbols.Add(s);
            }

        }

        var imageSource = new MatrixImageSource(config, symbols);

        return imageSource;
    }


}
