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
        const byte pinDataX = 0; // data X (DS or SER)
        const byte pinDataY = 1; // data Y (DS or SER)
        const byte pinDataZ = 2; // data Z (DS or SER)

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

            byte[] bufA = new byte[bufsize];
            int bufApos = 0;

            do
            {

                foreach (var frame in frames)
                {
                    byte oldX = 0;
                    byte oldY = 0;
                    bool oldIsBlanking = true;

                    foreach (var record in frame.Records)
                    {

                        GetXYZFromRecord(frame.Header.Format, record, out var ildaX, out var ildaY, out var ildaBlanking);
                        
                        // ilda stores coordinates in 16 bit, but my DACs currently is only 8 bit
                        byte x = (byte)((ildaX / 256) + 128);
                        byte y = (byte)((ildaY / 256) + 128);


                        // do not need to not draw the same dot multiple times
                        if (x == oldX && y == oldY && ildaBlanking == oldIsBlanking)
                            continue;

                        oldX = x;
                        oldY = y;
                        oldIsBlanking = ildaBlanking;


                        // preparing data for shift registers and putting it to the buffer

                        byte blanking = (byte)(((ildaBlanking ? 1 : 0) << pinDataZ) & (1 << pinDataZ));

                        for (int bit = 7; bit >= 0; bit--)
                        {
                            byte data = 0;

                            // X to first shift register
                            data |= (byte)((((x >> bit) & 1) << pinDataX) & (1 << pinDataX));
                            // Y to second shift register
                            data |= (byte)((((y >> bit) & 1) << pinDataY) & (1 << pinDataY));
                            // blanking (Z) to third shift register (only on/off for now)
                            data |= blanking;
                            // Shift and Store clock pins goes LOW

                            // write data to buffer
                            bufA[bufApos] = data;
                            bufApos++;

                            // after each data bit transmission Shift pin goes HIGH
                            // after each 7th data bit transmission Store pin goes HIGH
                            bufA[bufApos] = (byte)((1 << pinShift) | (bit == 7 ? (1 << pinStore) : 0));
                            bufApos++;

                        }


                        // writing buffer to ft2232 channel

                        var writtenTotalA = 0;

                        if (bufApos == bufA.Length)
                        {

                            while (writtenTotalA < bufA.Length)
                            {
                                uint writtenA = 0;
                                status = channel.Write(bufA.Skip(writtenTotalA).ToArray(), bufA.Length - writtenTotalA, ref writtenA);
                                Debug.Assert(status == FTDI.FT_STATUS.FT_OK);
                                writtenTotalA += (int)writtenA;
                            }

                            bufApos = 0;
                        }



                    }

                }
            }
            while (infinite);

            status = channel.Close();
            Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

        }

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
