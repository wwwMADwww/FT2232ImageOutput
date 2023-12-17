using System.Collections.Generic;
using System.Threading;
using FT2232ImageOutput.FrameProcessors;
using FT2232ImageOutput.HardwareOutput;
using FT2232ImageOutput.ImageSources;
using FT2232ImageOutput.MainProcessors;
using FT2232ImageOutput.PointBitMappers;

namespace FT2232ImageOutput.Configs;

static class ConfigExampleWavReg574
{
    public static void Run()
    {
        // uint baudrate = 12_000_000; // = 20 MBytes/s = 6.(6) MPoints/s

        uint baudrate = 12_000_000;

        var filepath = @"..\..\..\samplefiles\wav\testcircle500hz_44100d.wav";

        var targetMaxValuesXY10Z4 = new ImageMaxValues() // XY 10 bit Z 4 bit
        {
            MaxRGB = 31,
            MinX = 0, MaxX = 1023,
            MinY = 0, MaxY = 1023,
            MinZ = 0, MaxZ = 31,
        };
        var targetMaxValues = targetMaxValuesXY10Z4;


        var imageSource = new WaveFileImageSource(filepath, targetMaxValues, true);

        var frameProcessors = new List<IFrameProcessor>() {
           new AddBlankingPointsFrameProcessor(4, 4, true)
        };

        var pointBitMapper = new Reg574PointBitMapper(
            invertZ: true,
            maxValues: targetMaxValues);

        var hardwareOutput = new FT2232HardwareOutput("A", baudrate, 100_000);

        var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput);

        mainProcess.Start();

        Thread.Sleep(Timeout.Infinite);
    }


}
