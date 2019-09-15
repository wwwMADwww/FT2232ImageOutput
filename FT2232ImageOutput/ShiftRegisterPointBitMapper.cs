using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{


    public enum ShiftRegisterPointBitMapperMode
    {
        // 3 registers, 8 bit XYZ R2R DACs
        Mode_Sr8x3_XY8_Z8,

        // 3 registers, 10 bit XY R2R DACs, 4 bit Z R2R DAC
        Mode_Sr8x3_XY10_Z4,

        // 5 registers, 16 bit XY R2R DACs, 8 bit Z R2R DAC
        Mode_Sr8x5_XY16_Z8
    }

    public class ShiftRegisterPointBitMapper : IPointBitMapper
    {
        private readonly ShiftRegisterPointBitMapperMode _mode;

        public ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode mode)
        {
            _mode = mode;
        }

        public byte[] Map(ImagePoint point)
        {
            switch (_mode)
            {
                case ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY8_Z8 : return Mode_Sr8x3_XY8_Z8(point);
                case ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY10_Z4: return Mode_Sr8x3_XY10_Z4(point);
                case ShiftRegisterPointBitMapperMode.Mode_Sr8x5_XY16_Z8: return Mode_Sr8x5_XY16_Z8(point);
                default:
                    throw new ArgumentException($"Unknown mapping mode {_mode}", nameof(_mode));
            }
        }



        byte[] Mode_Sr8x3_XY8_Z8(ImagePoint point)
        {

            // TODO: make configurable
            // see the schematic diagram
            byte pinDataX = 0; // |xxxxxxxx|
            byte pinDataY = 1; // |yyyyyyyy|
            byte pinDataZ = 2; // |zzzzzzzz|

            byte pinShift = 6; // shift clock (SHCP or SRCLK)
            byte pinStore = 7; // store clock (STCP or RCLK)

            var buf = new byte[2 * 8];
            int bufpos = 0;

            byte x, y, z;

            x = (byte)(point.X & 0xFF);
            y = (byte)(point.Y & 0xFF);
            z = (byte)(point.Blanking ? 0xFF : ((point.Z ^ 0xFF) & 0xFF));


            for (int bit = 7; bit >= 0; bit--)
            {
                byte data = 0;

                data |= GetBit(x, bit, pinDataX);
                data |= GetBit(y, bit, pinDataY);
                data |= GetBit(z, bit, pinDataZ);
                // Shift and Store clock pins goes LOW

                // write data to buffer
                buf[bufpos] = data;
                bufpos++;

                // after each data bit transmission Shift pin goes HIGH
                buf[bufpos] = (byte)(1 << pinShift);
                bufpos++;

            }

            // after each 8th data bit transmission Store pin goes HIGH
            buf[0] |= (byte)(1 << pinStore);

            return buf;
        }



        byte[] Mode_Sr8x3_XY10_Z4(ImagePoint point)
        {

            // TODO: make configurable
            // see the schematic diagram
            byte pinDataX1    = 0; // |xxxxxxxx|
            byte pinDataY1    = 2; // |yyyyyyyy|
            byte pinDataX1Y2Z = 1; // |xxyyzzzz|

            byte pinShift = 6; // shift clock (SHCP or SRCLK)
            byte pinStore = 7; // store clock (STCP or RCLK)

            var buf = new byte[2 * 8];
            int bufpos = 0;

            byte x1, x2, y1, y2, z1;

            x1 = (byte)(point.X & 0xFF);
            x2 = (byte)((point.X >> 8) & 0b11);
            y1 = (byte)(point.Y & 0xFF);
            y2 = (byte)((point.Y >> 8) & 0b11);
            z1 = (byte)(point.Blanking ? 0b1111 : ((point.Z ^ 0b1111) & 0b1111));


            var regX1 = x1;
            var regY1 = y1;
            var regX2Y2Z = (byte) (x2 | (y2 << 2) | (z1 << 4));

            for (int bit = 7; bit >= 0; bit--)
            {
                byte data = 0;

                data |= GetBit(regX1, bit, pinDataX1);
                data |= GetBit(regY1, bit, pinDataY1);
                data |= GetBit(regX2Y2Z, bit, pinDataX1Y2Z);
                // Shift and Store clock pins goes LOW

                // write data to buffer
                buf[bufpos] = data;
                bufpos++;

                // after each data bit transmission Shift pin goes HIGH
                buf[bufpos] = (byte)(buf[bufpos-1] | (1 << pinShift));
                bufpos++;

            }

            // after each 8th data bit transmission Store pin goes HIGH
            buf[0] |= (byte) (1 << pinStore);

            return buf;
        }



        byte[] Mode_Sr8x5_XY16_Z8(ImagePoint point)
        {

            // TODO: make configurable
            // see the schematic diagram
            byte pinDataX1 = 0; // data Xlo (DS or SER)
            byte pinDataX2 = 1; // data Xhi (DS or SER)
            byte pinDataY1 = 2; // data Ylo (DS or SER)
            byte pinDataY2 = 3; // data Yhi (DS or SER)
            byte pinDataZ  = 4; // data Z   (DS or SER)

            byte pinShift = 6; // shift clock (SHCP or SRCLK)
            byte pinStore = 7; // store clock (STCP or RCLK)

            var buf = new byte[2 * 8];
            int bufpos = 0;

            byte x1, x2, y1, y2, z1;

            x1 = (byte)(point.X & 0xFF);
            x2 = (byte)((point.X >> 8) & 0xFF);
            y1 = (byte)(point.Y & 0xFF);
            y2 = (byte)((point.Y >> 8) & 0xFF);
            z1 = (byte)(point.Blanking ? 255 : 0);

            for (int bit = 7; bit >= 0; bit--)
            {
                byte data = 0;

                data |= GetBit(x1, bit, pinDataX1);
                data |= GetBit(x2, bit, pinDataX2);
                data |= GetBit(y1, bit, pinDataY1);
                data |= GetBit(y2, bit, pinDataY2);
                data |= GetBit(z1, bit, pinDataZ);
                // Shift and Store clock pins goes LOW

                // write data to buffer
                buf[bufpos] = data;
                bufpos++;

                // after each data bit transmission Shift pin goes HIGH
                buf[bufpos] = (byte)(1 << pinShift);
                bufpos++;

            }

            // after each 8th data bit transmission Store pin goes HIGH
            buf[0] |= (byte)(1 << pinStore);

            return buf;

        }



        byte GetBit(byte value, int bit, int pin) => (byte)((((value >> bit) & 1) << pin) & (1 << pin));

    }
}
