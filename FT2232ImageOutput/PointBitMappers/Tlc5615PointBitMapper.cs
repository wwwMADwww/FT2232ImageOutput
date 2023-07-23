using FT2232ImageOutput.Utils;
using System;

namespace FT2232ImageOutput.PointBitMappers
{

    public enum Tlc5615DataMode 
    {
        Parallel12bit,
        Parallel16bit,
        DaisyChain
    }

    public class Tlc5615PointBitMapper : IPointBitMapper
    {
        private readonly Tlc5615DataMode _dataMode;

        // TODO: TLC5615  have slow rise time, compensation is required
        // e.g. additional points and/or dedicated blanking pin

        private readonly ImageMaxValues _maxValues;

        private readonly bool _manualDataClock;
        private readonly bool _invertZ;

        private const int _dataMask = 0b1111111111;

        public Tlc5615PointBitMapper(
            ImageMaxValues maxValues,
            Tlc5615DataMode dataMode,
            bool manualDataClock,
            bool invertZ)
        {
            if (dataMode == Tlc5615DataMode.DaisyChain) 
            {
                throw new NotImplementedException($"Daisy chain data mode is not implemented");
            }

            _maxValues = maxValues;
            _dataMode = dataMode;
            _manualDataClock = manualDataClock;
            _invertZ = invertZ;
        }


        public int MaxBytesPerPoint
        {
            get
            {
                var dataLength = _dataMode switch
                {
                    Tlc5615DataMode.Parallel12bit => 12,
                    Tlc5615DataMode.Parallel16bit => 16,
                };
                return dataLength * (_manualDataClock ? 2 : 1) + 1;
            }
        }


        public byte[] Map(ImagePoint point)
        {
            // TODO: make configurable
            // see the schematic diagram
            byte pinDataX = 0; // data X (DIN)
            byte pinDataY = 1; // data Y (DIN)
            byte pinDataZ = 2; // data Z (DIN)

            byte pinShift  = 6; // shift clock (SCLK). for manual data clocking
            byte pinSelect = 7; // select clock (CS)

            var values = new int[3];

            values[pinDataX] = point.X & _dataMask;
            values[pinDataY] = point.Y & _dataMask;

            values[pinDataZ] = (!_invertZ
                ? (point.Blanking ? _maxValues.MinZ : point.Z)
                : (point.Blanking ? _maxValues.MaxZ : MathUtils.ConvertRange(_maxValues.MinZ, _maxValues.MaxZ, _maxValues.MaxZ, _maxValues.MinZ, point.Z))
            ) & _dataMask;

            var bytes = GetDataAndClockBytes(values, pinSelect, pinShift);

            return bytes;
        }

        
        // TODO: Unify SPI/shift register mapping
        protected byte[] GetDataAndClockBytes(int[] values, int pinSelect, int pinShift)
        {
            int bufpos = 0;
            int bitsCount = 10;
            byte[] buf = null;

            switch (_dataMode)
            {
                case Tlc5615DataMode.Parallel12bit:
                    buf = new byte[(_manualDataClock ? 2 * 12 : 12) + 1];

                    // CS pin goes HIGH
                    buf[bufpos] = (byte)(1 << pinSelect);
                    bufpos++;
                    break;

                case Tlc5615DataMode.Parallel16bit:
                    buf = new byte[(_manualDataClock ? 2 * 16 : 16) + 1];

                    // CS pin goes HIGH
                    buf[bufpos] = (byte)(1 << pinSelect);
                    bufpos++;

                    // 4 Upper Dummy Bits
                    for (int bit = 3; bit >= 0; bit--)
                    {
                        buf[bufpos] = 0;
                        bufpos++;

                        if (_manualDataClock)
                        {
                            // after each data bit transmission Shift pin goes HIGH
                            buf[bufpos] = (byte)(1 << pinShift);
                            bufpos++;
                        }
                    }
                    break;
            };

            // 10 Data Bits, MSB first
            for (int bit = bitsCount-1; bit >= 0; bit--)
            {
                byte data = 0;
                for (int pin = 0; pin < values.Length; pin++)
                {
                    data |= GetBit(values[pin], bit, pin);
                }

                // write data to buffer
                buf[bufpos] = data;
                bufpos++;

                if (_manualDataClock)
                {
                    // after each data bit transmission Shift pin goes HIGH
                    buf[bufpos] = (byte)(data | (1 << pinShift));
                    bufpos++;
                }
            }

            // 2 Extra (Sub-LSB) Bits
            for (int bit = 1; bit >= 0; bit--)
            {
                buf[bufpos] = 0;
                bufpos++;

                if (_manualDataClock)
                {
                    // after each data bit transmission Shift pin goes HIGH
                    buf[bufpos] = (byte)(1 << pinShift);
                    bufpos++;
                }
            }

            return buf;
        }


        byte GetBit(int value, int bit, int pin) => (byte)((((value >> bit) & 1) << pin) & (1 << pin));


    }
}
