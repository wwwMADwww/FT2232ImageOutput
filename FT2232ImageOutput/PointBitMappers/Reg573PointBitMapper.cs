using System;
using FT2232ImageOutput.Utils;

namespace FT2232ImageOutput.PointBitMappers;

public class Reg573PointBitMapper : IPointBitMapper
{
    private readonly bool _invertZ;
    private readonly ImageMaxValues _maxValues;

    public Reg573PointBitMapper(bool invertZ, ImageMaxValues maxValues)
    {
        _invertZ = invertZ;
        _maxValues = maxValues;
    }

    public int MaxBytesPerPoint => 8 * 3;

    public byte[] Map(ImagePoint point)
    {
        // buf0: [xxxxxxxx] X bits 2-9
        // buf1: [yyyyyyyy] Y bits 2-9
        // buf2: [xxyyzzzz] X bits 0-1, Y bits 0-1, Z bits 0-3

        var z = GetZValue(point.Blanking, point.Z);

        var bytes = new byte[3];

        bytes[0] = (byte)((point.X >> 2) & 0xFF);
        bytes[1] = (byte)((point.Y >> 2) & 0xFF);
        bytes[2] = (byte)(
            (point.X & 0b11) | 
            ((point.Y & 0b11) << 2) | 
            ((z & 0b1111) << 4)
        );

        return bytes;
    }

    private int GetZValue(bool blanking, int z)
    {
        return !_invertZ
            ? (blanking ? _maxValues.MinZ : z)
            : (blanking ? _maxValues.MaxZ : MathUtils.ConvertRange(_maxValues.MinZ, _maxValues.MaxZ, _maxValues.MaxZ, _maxValues.MinZ, z));
    }

}
