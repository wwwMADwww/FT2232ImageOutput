using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.PointBitMappers
{



    public class DirectPointBitMapper : IPointBitMapper
    {
        public DirectPointBitMapper()
        {
        }

        public int MaxBytesPerPoint => 1;

        public byte[] Map(ImagePoint point)
        {
            return new byte[] { (byte) (point.X & (byte) 0xFF) };
        }


    }
}
