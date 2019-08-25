using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSS.ILDA;
using FTD2XX_NET;

namespace FT2232ImageOutput
{

    public class IldaOutput
    {
        // see the schematic diagram
        const byte pinDataX1 = 0; // data Xlo (DS or SER)
        const byte pinDataX2 = 1; // data Xhi (DS or SER)
        const byte pinDataY1 = 2; // data Ylo (DS or SER)
        const byte pinDataY2 = 3; // data Yhi (DS or SER)
        const byte pinDataZ  = 4; // data Z (DS or SER)

        const byte pinShift = 6; // shift clock (SHCP or SRCLK)
        const byte pinStore = 7; // store clock (STCP or RCLK)

        public void DrawFrames(IEnumerable<Frame> frames, bool infinite, uint baudRate)
        {

            FTDI.FT_STATUS status = FTDI.FT_STATUS.FT_OK;

            // ft2232 has 2 channels - A and B
            var channel = OpenChannel("A", baudRate);

            // (((ds | stcp up | shcp down) + (shcp up)) x 1) + (((ds | stcp down | shcp down) + (shcp up)) x 7)
            // must be a multiple of 16
            var bufsize = (2 * 8) * 256; // 4096

            byte[] dataBuf = new byte[bufsize];
            int bufApos = 0;

            do
            {

                foreach (var frame in frames)
                {
                    ushort oldX = 0;
                    ushort oldY = 0;
                    byte oldZ = 0;

                    foreach (var record in frame.Records)
                    {

                        GetXYZFromRecord(frame.Header.Format, record, out var ildaX, out var ildaY, out var ildaBlanking);

                        // do not need to not draw blanked dots
                        if (ildaBlanking)
                            continue;

                        ushort x = (ushort)(ildaX + 32768);
                        ushort y = (ushort)(ildaY + 32768);
                        byte z = (byte)(ildaBlanking ? 255 : 0);


                        // do not need to not draw the same dot multiple times
                        if (x == oldX && y == oldY && z == oldZ)
                            continue;

                        oldX = x;
                        oldY = y;
                        oldZ = z;

                        byte x1, x2, y1, y2, z1;

                        x1 = (byte)(x & 0xFF);
                        x2 = (byte)((x >> 8) & 0xFF);
                        y1 = (byte)(y & 0xFF);
                        y2 = (byte)((y >> 8) & 0xFF);
                        z1 = z;

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
                            dataBuf[bufApos] = data;
                            bufApos++;

                            // after each data bit transmission Shift pin goes HIGH
                            // after each 7th data bit transmission Store pin goes HIGH
                            dataBuf[bufApos] = (byte)((1 << pinShift) | (bit == 7 ? (1 << pinStore) : 0));
                            bufApos++;

                        }


                        // writing buffer to ft2232 channel

                        if (bufApos == dataBuf.Length)
                        {
                            WriteToChannel(channel, dataBuf);

                            bufApos = 0;
                        }

                    }

                }
            }
            while (infinite);

            status = channel.Close();
            Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

        }

        void WriteToChannel(FTDI channel, byte[] dataBuf)
        {
            var writtenTotal = 0;

            while (writtenTotal < dataBuf.Length)
            {
                uint written = 0;
                var status = channel.Write(dataBuf.Skip(writtenTotal).ToArray(), dataBuf.Length - writtenTotal, ref written);
                Debug.Assert(status == FTDI.FT_STATUS.FT_OK);
                writtenTotal += (int)written;
            }
        }

        byte GetBit(byte value, int bit, int pin) => (byte)((((value >> bit) & 1) << pin) & (1 << pin));

        void GetXYZFromRecord(FormatCode formatCode, CoordinateRecord record, out short x, out short y, out bool blanking)
        {
            switch (formatCode)
            {
                case FormatCode.Format2DIndexedColour:
                    {
                        var r = (Record2DIndexed)record;
                        x = r.X;
                        y = r.Y;
                        blanking = r.Blanking;
                        break;
                    }

                case FormatCode.Format2DTrueColour:
                    {
                        var r = (Record2DTrueColour)record;
                        x = r.X;
                        y = r.Y;
                        blanking = r.Blanking;
                        break;
                    }

                case FormatCode.Format3DIndexedColour:
                    {
                        var r = (Record3DIndexed)record;
                        x = r.X;
                        y = r.Y;
                        blanking = r.Blanking;
                        break;
                    }

                case FormatCode.Format3DTrueColour:
                    {
                        var r = (Record3DTrueColour)record;
                        x = r.X;
                        y = r.Y;
                        blanking = r.Blanking;
                        break;
                    }

                default: throw new ArgumentException($"Unknown FormatCode '{formatCode}'", nameof(formatCode));
            }
        }



        FTDI OpenChannel(string channelName, uint baudRate)
        {

            var res = new FTDI();

            var status = res.OpenBySerialNumber(channelName);
            Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

            status = res.SetBaudRate(baudRate);
            Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

            // byte latency = 0;

            // status = res.GetLatency(ref latency);
            // Debug.Assert(status == FTDI.FT_STATUS.FT_OK);
            // Console.WriteLine($"current latency channel {channelName} = {latency} ms");

            status = res.SetLatency(0);
            Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

            // status = res.GetLatency(ref latency);
            // Debug.Assert(status == FTDI.FT_STATUS.FT_OK);
            // Console.WriteLine($"new latency channel {channelName} = {latency} ms");

            // enable async bitbang mode for all 8 pins
            status = res.SetBitMode(0b11111111, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
            Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

            status = res.SetTimeouts(1, 1);
            Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

            return res;

        }

    }
}
