using FT2232ImageOutput.Configs;

namespace FT2232ImageOutput;

class Program
{
    static void Main(string[] args)
    {
        // i'm too lazy to provide a convinient way to configure the app
        // so i will configure everything in code, placed in "configs" dir
        // in different .cs files for each image and/or hardware setup

        // ConfigExampleSvgShiftRegs.Run();
        // ConfigExampleWavMcp4921.Run();
        // ConfigExampleSvgTlc5615.Run();
        // ConfigExampleWavReg573.Run();
        ConfigExampleSvgReg573.Run();
        // WolframFourierArtReg573.Run();
        // ConfigMatrix.Run();
    }
}
