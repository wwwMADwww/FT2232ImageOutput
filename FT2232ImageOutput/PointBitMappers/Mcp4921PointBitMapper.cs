﻿using System;
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

        const int _packetMask    = 0b1111111111111111;
        const int _dataMask      = 0b0000111111111111;
        const int _configuration = 0b0111000000000000;
        
        public Mcp4921PointBitMapper(bool analogZ)
        {
            _analogZ = analogZ;
        }

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

            var bytes = GetDataAndClockBytes(values, 16, pinSelect, pinShift, pinStore, false);

            return bytes;
        }

        
        // TODO: Unify SPI/shift register mapping
        protected byte[] GetDataAndClockBytes(int[] values, int bitsCount, int pinSelect, int pinShift, int pinStore, bool resetDataOnClock)
        {
            var buf = new byte[(2 * bitsCount) + 3];
            int bufpos = 0;


            // cs 1, store 1
            buf[0] = (byte)((1 << pinSelect) | (1 << pinStore));

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
                buf[bufpos + 1] = data;
                bufpos++;

                // after each data bit transmission Shift pin goes HIGH
                buf[bufpos + 1] = (byte)((1 << pinShift) | (resetDataOnClock ? 0 : data) | (1 << pinStore));

                //str += ConsoleOut(buf[bufpos + 1]);
                bufpos++;

            }

            // cs 1, store 1
            buf[33] = (byte)((1 << pinSelect) | (1 << pinStore));

            // cs 1, store 0
            buf[34] = (byte)(1 << pinSelect);


            return buf;
        }


        byte GetBit(int value, int bit, int pin) => (byte)((((value >> bit) & 1) << pin) & (1 << pin));


    }
}