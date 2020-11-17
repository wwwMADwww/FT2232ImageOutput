using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.PointBitMappers
{


    public class Mcp4921PointBitMapper : IPointBitMapper
    {
        private readonly bool _analogZ;
        private readonly bool _manualDataClock;

        const int _packetMask = 0b1111111111111111;
        const int _dataMask = 0b0000111111111111;
        const int _configuration = 0b0111000000000000;

        public Mcp4921PointBitMapper(bool analogZ, bool manualDataClock)
        {
            _analogZ = analogZ;
            _manualDataClock = manualDataClock;
        }


        public int MaxBytesPerPoint => (_manualDataClock ? 2 * 16 : 16) + 1;


        public byte[] Map(ImagePoint point)
        {
            // TODO: make configurable
            // see the schematic diagram
            byte pinDataX = 0; // data X (SDC)
            byte pinDataY = 1; // data Y (SDC)
            byte pinDataZ = 2; // data Z (SDC) or binary

            byte pinSelect = 5; // select clock (CS)
            byte pinShift  = 6; // shift clock (SCK)
            byte pinStore  = 7; // store clock (LDAC)

            var values = new int[3];

            values[pinDataX] = (_configuration | (point.X & _dataMask)) & _packetMask;
            values[pinDataY] = (_configuration | (point.Y & _dataMask)) & _packetMask;

            if (_analogZ)
                values[pinDataZ] = _configuration | (point.Blanking 
                    ? _dataMask 
                    : (point.Z & _dataMask));
            else
                values[pinDataZ] = point.Blanking ? _packetMask : 0;

            var bytes = GetDataAndClockBytes(values, 16, pinSelect, pinShift, pinStore, _manualDataClock, false);

            return bytes;
        }

        
        // TODO: Unify SPI/shift register mapping
        protected byte[] GetDataAndClockBytes(int[] values, int bitsCount, int pinSelect, int pinShift, int pinStore, bool manualDataClock, bool resetDataOnClock)
        {

            var buf = new byte[(manualDataClock ? 2 * bitsCount : bitsCount) + 1];
            int bufpos = 0;


            // cs 1, store 1
            // buf[bufpos] = (byte)((1 << pinSelect) | (1 << pinStore));
            buf[bufpos] = (byte)((1 << pinSelect));
            bufpos++;

            // bits 1 - 32
            // data D, cs 0, store 1
            for (int bit = bitsCount-1; bit >= 0; bit--)
            {
                byte data = 0;

                for (int pin = 0; pin < values.Length; pin++)
                {
                    data |= (byte) (GetBit(values[pin], bit, pin) | (1 << pinStore));
                }

                // write data to buffer
                buf[bufpos] = data;
                bufpos++;

                if (manualDataClock)
                {
                    // after each data bit transmission Shift pin goes HIGH
                    buf[bufpos] = (byte)((1 << pinShift) | (resetDataOnClock ? 0 : data) | (1 << pinStore));

                    //str += ConsoleOut(buf[bufpos + 1]);
                    bufpos++;
                }

            }

            // // cs 1, store 1
            // buf[bufpos] = (byte)((1 << pinSelect) | (1 << pinStore));
            // bufpos++;
            // 
            // // cs 1, store 0
            // buf[bufpos] = (byte)(1 << pinSelect);


            return buf;
        }


        byte GetBit(int value, int bit, int pin) => (byte)((((value >> bit) & 1) << pin) & (1 << pin));


    }
}
