using System.Collections.Generic;
using System.Threading;
using FT2232ImageOutput.FrameProcessors;
using FT2232ImageOutput.HardwareOutput;
using FT2232ImageOutput.ImageSources;
using FT2232ImageOutput.MainProcessors;
using FT2232ImageOutput.PointBitMappers;

namespace FT2232ImageOutput.Configs;

static class ConfigExampleWavMcp4921
{
    public static void Run()
    {
        // Notes for driving MPC4921
        // uint baudrate = 3_000_000; // MCP4921 absolute max data speed
        // uint baudrate = 3_000_000; // MCP4921 0- 255 ( 8 bit). square wave 440khz (880k points/s). wave shape is heavy filtered triangle
        // uint baudrate = 2_000_000; // MCP4921 0-1023 (10 bit). square wave 320khz (640k points/s). wave shape has triangle top, flat bottom
        // uint baudrate = 1_000_000; // most stable, minimum data stream tearing
        // uint baudrate =   610_000; // MCP4921 0-4095 (12 bit). square wave  90khz (180k points/s). wave shape is triangle

        // if you want to play auido, actual frequencies will be about 10Hz off
        // these baudrate values obtained experimentally
        //               baudrate| wav sample rate|
        // uint baudrate = 307200;          192000
        // uint baudrate = 153600;           96000
        // uint baudrate =  76800;           48000
        // uint baudrate =  70550;           44100

        uint baudrate = 500_000;

        var filepath = @"..\..\..\samplefiles\svg\testcircle500hz_44100d.wav";


        var targetMaxValues8 = new ImageMaxValues() // XYZ 8 bit
        {
            MaxRGB = 255,
            MinX = 0, MaxX = 255,
            MinY = 0, MaxY = 255,
            MinZ = 0, MaxZ = 255,
        };
        var targetMaxValues10 = new ImageMaxValues() // XYZ 10 bit
        {
            MaxRGB = 255,
            MinX = 0, MaxX = 1023,
            MinY = 0, MaxY = 1023,
            MinZ = 0, MaxZ = 1023,
        };
        var targetMaxValues12 = new ImageMaxValues() // XYZ 12 bit
        {
            MaxRGB = 255,
            MinX = 0, MaxX = 4095,
            MinY = 0, MaxY = 4095,
            MinZ = 0, MaxZ = 4095,
        };
        var targetMaxValues = targetMaxValues12;


        var imageSource = new WaveFileImageSource(filepath, targetMaxValues);

        var frameProcessors = new List<IFrameProcessor>() {
           new AddBlankingPointsFrameProcessor(4, 4, true)
        };

        var pointBitMapper = new Mcp4921PointBitMapper(
            analogZ: true,
            manualDataClock: false,
            invertZ: false,
            maxValues: targetMaxValues);

        
        var hardwareOutput = new FT2232HardwareOutput(baudrate, bufferSize: 10240);

        var mainProcess = new MainProcessor(imageSource, frameProcessors, pointBitMapper, hardwareOutput);

        mainProcess.Start();

        Thread.Sleep(Timeout.Infinite);
    }




}
