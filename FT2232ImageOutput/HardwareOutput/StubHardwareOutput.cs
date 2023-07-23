using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FT2232ImageOutput.HardwareOutput
{
    
    public class StubHardwareOutput : IHardwareOutput
    {
        private readonly int _bufSize;
        private readonly int _interval;

        public StubHardwareOutput(int bufSize, int maxBytes, int interval)
        {
            _bufSize = bufSize;
            MaxBytes = maxBytes;
            _interval = interval;
        }

        public int MaxBytes { get; private set; }

        public void Output(byte[] bytes, bool flush)
        {
            if (!bytes.Any())
                return;

            int count = bytes.Count() > _bufSize
             ? bytes.Count() / _bufSize
             : 1;

            // Console.WriteLine($"{DateTime.Now.Ticks}.{DateTime.Now.Ticks} Incoming {bytes.Count()}");
            for (int i = 0; i < count; i++)
            {
                // Console.WriteLine($"{DateTime.Now.Ticks}.{DateTime.Now.Ticks} sending {i}/{count}");
                if (_interval > 0)
                    Thread.Sleep(_interval);
            }

            // Console.WriteLine($"{DateTime.Now.Ticks}.{DateTime.Now.Ticks} all sent");
            // Console.WriteLine("--------------");

        }


    }


}
