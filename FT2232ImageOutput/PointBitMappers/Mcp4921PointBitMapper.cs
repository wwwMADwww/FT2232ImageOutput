﻿using FT2232ImageOutput.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.PointBitMappers;



public class Mcp4921PointBitMapper : IPointBitMapper
{
    // TODO: MCP4921 have slow rise time, compensation is required
    // e.g. additional points and/or dedicated blanking pin

    private readonly bool _analogZ;
    private readonly bool _manualDataClock;
    private readonly bool _invertZ;
    private readonly ImageMaxValues _maxValues;

    const int configDacSelect = 1 << 15; // !A/B
    const int configVrefBuf   = 1 << 14; // BUF
    const int configOutGain   = 1 << 13; // !GA
    const int configOutBuf    = 1 << 12; // !SHDN

    const int _packetMask = 0b1111111111111111;
    const int _dataMask   = 0b0000111111111111;
    const int _configuration = configVrefBuf | configOutGain | configOutBuf;

    public Mcp4921PointBitMapper(bool analogZ, bool manualDataClock, bool invertZ, ImageMaxValues maxValues)
    {
        _analogZ = analogZ;
        _manualDataClock = manualDataClock;
        _invertZ = invertZ;
        _maxValues = maxValues;
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
            values[pinDataZ] = _configuration | (!_invertZ
                ? (point.Blanking ? _maxValues.MinZ : point.Z)
                : (point.Blanking ? _maxValues.MaxZ : MathUtils.ConvertRange(_maxValues.MinZ, _maxValues.MaxZ, _maxValues.MaxZ, _maxValues.MinZ, point.Z))
            ) & _dataMask;
        else
            values[pinDataZ] = !_invertZ
                ? (point.Blanking ? 0 : _dataMask)
                : (point.Blanking ? _dataMask : 0);

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
