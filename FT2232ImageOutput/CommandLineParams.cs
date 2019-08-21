using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace FT2232ImageOutput
{
    public class CommandLineParams
    {

        [Option('f', "file", Required = true, HelpText = "Path to ILDA file.")]
        public string Filepath { get; set; }

        [Option('b', "baudrate", Required = false, HelpText = "Data transmission speed. Default is 5000000.")]
        public uint Baudrate { get; set; } = 5000000;

    }

    public static class CommandLineParamsProcessor
    {

        public static bool Process(string[] args, out CommandLineParams options)
        {
            CommandLineParams optbuf = null;
            var result = CommandLine.Parser.Default.ParseArguments<CommandLineParams>(args);

            var res = result.MapResult
            (
                (CommandLineParams opt) =>
                {
                    optbuf = opt;
                    return true;
                },
                errors => false
            );
            options = optbuf;
            return res;
        }

    }

}
