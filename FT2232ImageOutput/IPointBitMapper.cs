﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput;

public interface IPointBitMapper
{
    int MaxBytesPerPoint { get; }

    byte[] Map(ImagePoint point);
}
