﻿using FT2232ImageOutput.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.PointBitMappers;



public enum ShiftRegisterPointBitMapperMode
{
    // 3 registers, 8 bit XYZ R2R DACs
    Mode_Sr8x3_XY8_Z8,

    // 3 registers, 10 bit XY R2R DACs, 4 bit Z R2R DAC
    Mode_Sr8x3_XY10_Z4,
    Mode_Sr8x3_XY10_Z4_2,

    // 6 registers, 10 bit XY R2R DACs, 4 bit Z R2R DAC
    Mode_Sr8x6_XY10_Z4,
    Mode_Sr8x6_XY10_Z4_2,
    Mode_Sr8x6_XY10_Z4_3,
    Mode_Sr8x6_XY10_Z4_4,

    // 5 registers, 16 bit XY R2R DACs, 8 bit Z R2R DAC
    Mode_Sr8x5_XY16_Z8
}

public class ShiftRegisterPointBitMapper : IPointBitMapper
{
    private readonly ShiftRegisterPointBitMapperMode _mode;
    private readonly bool _invertZ;
    private readonly ImageMaxValues _maxValues;
    const byte _allBits8 = 0xFF;
    const byte _allBits4 = 0b1111;

    public ShiftRegisterPointBitMapper(ShiftRegisterPointBitMapperMode mode, bool invertZ, ImageMaxValues maxValues)
    {
        _mode = mode;
        _invertZ = invertZ;
        _maxValues = maxValues;
    }

    public byte[] Map(ImagePoint point)
    {
        switch (_mode)
        {
            case ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY8_Z8 : return Mode_Sr8x3_XY8_Z8(point);
            case ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY10_Z4: return Mode_Sr8x3_XY10_Z4(point);
            case ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY10_Z4_2: return Mode_Sr8x3_XY10_Z4_2(point);
            case ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4: return Mode_Sr8x6_XY10_Z4(point);
            case ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4_2: return Mode_Sr8x6_XY10_Z4_2(point);
            case ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4_3: return Mode_Sr8x6_XY10_Z4_3(point);
            case ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4_4: return Mode_Sr8x6_XY10_Z4_4(point);
            case ShiftRegisterPointBitMapperMode.Mode_Sr8x5_XY16_Z8: return Mode_Sr8x5_XY16_Z8(point);
            default:
                throw new ArgumentException($"Unknown mapping mode {_mode}", nameof(_mode));
        }
    }

    public int MaxBytesPerPoint { get 
        {
            switch (_mode)
            {
                case ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY8_Z8:
                case ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY10_Z4: 
                case ShiftRegisterPointBitMapperMode.Mode_Sr8x3_XY10_Z4_2: return 16;

                case ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4: 
                case ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4_2: 
                case ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4_3: 
                case ShiftRegisterPointBitMapperMode.Mode_Sr8x6_XY10_Z4_4: return 8;

                case ShiftRegisterPointBitMapperMode.Mode_Sr8x5_XY16_Z8: return 16;

                default:
                    throw new ArgumentException($"Unknown mapping mode {_mode}", nameof(_mode));
            }
        }
    }

    int GetZValue(bool blanking, int z)
    {
        return !_invertZ
            ? (blanking ? _maxValues.MinZ : z)
            : (blanking ? _maxValues.MaxZ : MathUtils.ConvertRange(_maxValues.MinZ, _maxValues.MaxZ, _maxValues.MaxZ, _maxValues.MinZ, z));
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
                   
        var values = new byte[3];

        values[pinDataX] = (byte)(point.X & 0xFF);
        values[pinDataY] = (byte)(point.Y & 0xFF);
        values[pinDataZ] = (byte)(GetZValue(point.Blanking, point.Z) & _allBits8);


        var bytes = GetDataAndClockBytes(values, 8, pinShift, pinStore, false);

        return bytes;
    }



    byte[] Mode_Sr8x3_XY10_Z4(ImagePoint point)
    {

        // TODO: make configurable
        // see the schematic diagram
        byte pinDataX1    = 0; // |xxxxxxxx|
        byte pinDataY1    = 1; // |yyyyyyyy|
        byte pinDataX1Y2Z = 2; // |xxyyzzzz|

        byte pinShift = 6; // shift clock (SHCP or SRCLK)
        byte pinStore = 7; // store clock (STCP or RCLK)
        
        var values = new byte[3];

        values[pinDataX1] = (byte)(point.X & 0xFF);
        values[pinDataY1] = (byte)(point.Y & 0xFF);
        values[pinDataX1Y2Z] = (byte)(
            ((point.X >> 8) & 0b11) |
            (((point.Y >> 8) & 0b11) << 2) |
            ((GetZValue(point.Blanking, point.Z) & _allBits4) << 4)
            );


        var bytes = GetDataAndClockBytes(values, 8, pinShift, pinStore, false);

        return bytes;
    }
    
    
            
    byte[] Mode_Sr8x3_XY10_Z4_2(ImagePoint point)
    {

        // TODO: make configurable
        // see the schematic diagram
        byte pinDataX1    = 0; // |xxxxxxxx|
        byte pinDataY1    = 1; // |yyyyyyyy|
        byte pinDataX1Y2Z = 2; // |xxyyzzzz|

        byte pinShift = 6; // shift clock (SHCP or SRCLK)
        byte pinStore = 7; // store clock (STCP or RCLK)

        var values = new byte[3];

        values[pinDataX1] = (byte)((point.X >> 2) & 0xFF);
        values[pinDataY1] = (byte)((point.Y >> 2) & 0xFF);
        values[pinDataX1Y2Z] = (byte) (
            (point.X & 0b11) | 
            ((point.Y & 0b11) << 2) | 
            ((GetZValue(point.Blanking, point.Z) & _allBits4) << 4)
            );


        var bytes = GetDataAndClockBytes(values, 8, pinShift, pinStore, false);

        return bytes;
    }


    byte[] Mode_Sr8x6_XY10_Z4(ImagePoint point)
    {

        // TODO: make configurable
        // see the schematic diagram
        byte pinDataX1   = 0; // |xxxx----|
        byte pinDataX2   = 1; // |xxxx----|
        byte pinDataY1   = 2; // |yyyy----|
        byte pinDataY2   = 3; // |yyyy----|
        byte pinDataX3Y3 = 4; // |xxyy----|
        byte pinDataZ    = 5; // |zzzz----|

        byte pinShift = 6; // shift clock (SHCP or SRCLK)
        byte pinStore = 7; // store clock (STCP or RCLK)
        
        var values = new byte[6];

        values[pinDataX1] = (byte)(point.X & 0b1111);
        values[pinDataX2] = (byte)((point.X >> 4) & 0b1111);
        values[pinDataY1] = (byte)(point.Y & 0b1111);
        values[pinDataY2] = (byte)((point.Y >> 4) & 0b1111);
        values[pinDataX3Y3] = (byte)(((point.X >> 8) & 0b11) | (((point.Y >> 8) & 0b11) << 2));
        values[pinDataZ] = (byte)(GetZValue(point.Blanking, point.Z) & _allBits4);

        var bytes = GetDataAndClockBytes(values, 4, pinShift, pinStore, false);

        return bytes;
    }


    byte[] Mode_Sr8x6_XY10_Z4_2(ImagePoint point)
    {
        // TODO: make configurable
        // see the schematic diagram
        byte pinDataX1Y1 = 0; // |yyxx----|
        byte pinDataX2 = 1; // |xxxx----|
        byte pinDataX3 = 2; // |xxxx----|
        byte pinDataZ  = 3; // |zzzz----|
        byte pinDataY2 = 4; // |yyyy----|
        byte pinDataY3 = 5; // |yyyy----|

        byte pinShift = 6; // shift clock (SHCP or SRCLK)
        byte pinStore = 7; // store clock (STCP or RCLK)

        var values = new byte[6];

        values[pinDataX1Y1] = (byte) (((point.X & 0b11) << 2) | (point.Y & 0b11));
        values[pinDataX2] = (byte)((point.X >> 2) & 0b1111);
        values[pinDataX3] = (byte)((point.X >> 6) & 0b1111);
        values[pinDataY2] = (byte)((point.Y >> 2) & 0b1111);
        values[pinDataY3] = (byte)((point.Y >> 6) & 0b1111);
        values[pinDataZ] = (byte)(GetZValue(point.Blanking, point.Z) & _allBits4);

        var bytes = GetDataAndClockBytes(values, 4, pinShift, pinStore, false);

        return bytes;
    }


    byte[] Mode_Sr8x6_XY10_Z4_3(ImagePoint point)
    {
        // TODO: make configurable
        // see the schematic diagram
        byte pinDataX2   = 0; // |xxxx----|
        byte pinDataX3   = 1; // |xxxx----|
        byte pinDataX1Y1 = 2; // |xxyy----|
        byte pinDataY2   = 3; // |yyyy----|
        byte pinDataY3   = 4; // |yyyy----|
        byte pinDataZ    = 5; // |zzzz----|

        byte pinShift = 6; // shift clock (SHCP or SRCLK)
        byte pinStore = 7; // store clock (STCP or RCLK)

        var values = new byte[6];

        values[pinDataX2] = (byte)((point.X >> 2) & 0b1111);
        values[pinDataX3] = (byte)((point.X >> 6) & 0b1111);
        values[pinDataX1Y1] = (byte)(((point.Y & 0b11) << 2) | (point.X & 0b11));
        values[pinDataY2] = (byte)((point.Y >> 2) & 0b1111);
        values[pinDataY3] = (byte)((point.Y >> 6) & 0b1111);
        values[pinDataZ] = (byte)(GetZValue(point.Blanking, point.Z) & _allBits4);

        var bytes = GetDataAndClockBytes(values, 4, pinShift, pinStore, false);

        return bytes;
    }
    
    byte[] Mode_Sr8x6_XY10_Z4_4(ImagePoint point)
    {
        // TODO: make configurable
        // see the schematic diagram
        byte pinDataX2   = 0; // |xxxx----|
        byte pinDataX3   = 1; // |xxxx----|
        byte pinDataY2   = 2; // |yyyy----|
        byte pinDataY3   = 3; // |yyyy----|
        byte pinDataZ    = 4; // |zzzz----|
        byte pinDataX1Y1 = 5; // |xxyy----|

        byte pinShift = 6; // shift clock (SHCP or SRCLK)
        byte pinStore = 7; // store clock (STCP or RCLK)

        var values = new byte[6];

        values[pinDataX2] = (byte)((point.X >> 2) & 0b1111);
        values[pinDataX3] = (byte)((point.X >> 6) & 0b1111);
        values[pinDataY2] = (byte)((point.Y >> 2) & 0b1111);
        values[pinDataY3] = (byte)((point.Y >> 6) & 0b1111);
        values[pinDataZ] = (byte)(GetZValue(point.Blanking, point.Z) & _allBits4);
        values[pinDataX1Y1] = (byte)(((point.Y & 0b11) << 2) | (point.X & 0b11));

        var bytes = GetDataAndClockBytes(values, 4, pinShift, pinStore, false);

        return bytes;
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

        var values = new byte[5];
        
        values[pinDataX1] = (byte)(point.X & 0xFF);
        values[pinDataX2] = (byte)((point.X >> 8) & 0xFF);
        values[pinDataY1] = (byte)(point.Y & 0xFF);
        values[pinDataY2] = (byte)((point.Y >> 8) & 0xFF);
        values[pinDataZ ] = (byte)(GetZValue(point.Blanking, point.Z) & _allBits8);

        var bytes = GetDataAndClockBytes(values, 8, pinShift, pinStore, false);

        return bytes;

    }


    // TODO: Unify SPI/shift register mapping
    protected byte[] GetDataAndClockBytes(byte[] values, int bitsCount, int pinShift, int pinStore, bool resetDataOnClock)
    {
        var buf = new byte[2 * bitsCount];
        int bufpos = 0;
        
        for (int bit = bitsCount-1; bit >= 0; bit--)
        {
            byte data = 0;

            for (int pin = 0; pin < values.Length; pin++)
            {
                data |= GetBit(values[pin], bit, pin);
                // Shift and Store clock pins goes LOW
            }

            // write data to buffer
            buf[bufpos] = data;
            bufpos++;

            // after each data bit transmission Shift pin goes HIGH
            buf[bufpos] = (byte)((1 << pinShift) | (resetDataOnClock ? 0 : data));
            bufpos++;

        }

        // after each 8th data bit transmission Store pin goes HIGH
        buf[0] |= (byte)(1 << pinStore);

        return buf;
    }


    byte GetBit(byte value, int bit, int pin) => (byte)((((value >> bit) & 1) << pin) & (1 << pin));

}
