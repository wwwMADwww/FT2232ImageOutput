﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{
    public interface IHardwareOutput
    {
        int MaxBytes { get; }

        void Output(byte[] bytes, bool flush);
    }
}
