using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace FT2232ImageOutput
{
    class Program
    {
        static int Main(string[] args)
        {

            var paramsParsed = CommandLineParamsProcessor.Process(args, out var options);

            if (!paramsParsed)
            {
                return 1;
            }

            var filepath = Path.GetFullPath(options.Filepath);

            Console.WriteLine($"Filename is {filepath}");
            Console.WriteLine($"Baudrate is {options.Baudrate}");

            var frames = new IldaFileReader().ReadFrames(filepath);

            new IldaOutput().DrawFrames(frames, true, options.Baudrate);

            return 0;

        }




    }
}
