using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using FT2232ImageOutput.Configs;

namespace FT2232ImageOutput
{
    class Program
    {
        static void Main(string[] args)
        {
            // i'm too lazy to provide a convinient way to configure the app
            // so i will configure everything in code, placed in "configs" dir
            // in different .cs files for each image and/or hardware setup

            ConfigExampleSvgShiftRegs.Run();
            // ConfigExampleWavMcp4921.Run();
            // ConfigMatrix.Run();
        }
    }
}
