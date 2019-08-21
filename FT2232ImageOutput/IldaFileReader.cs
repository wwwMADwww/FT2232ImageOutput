using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{
    public class IldaFileReader
    {


        public IEnumerable<Frame> ReadFrames(string filename)
        {

            var frames = new List<Frame>();

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {

                    while (true)
                    {
                        var frameheader = new ILDAHeader(br);

                        var records = new List<CoordinateRecord>();

                        for (int i = 0; i < frameheader.NumberOfRecords; i++)
                        {
                            var record = ReadIldaRecord(br, frameheader.Format);

                            // ignoring colors for now
                            if (frameheader.Format != FormatCode.FormatColourPalette)
                            {
                                records.Add((CoordinateRecord)record);
                                if (((CoordinateRecord)record).LastPoint)
                                    break;
                            }
                        }

                        frames.Add(new Frame(frameheader, records));

                        if (frameheader.NumberOfRecords == 0)
                            break;

                    }

                }
            }

            return frames;

        }

        DataRecord ReadIldaRecord(BinaryReader br, FormatCode formatCode)
        {
            switch (formatCode)
            {
                case FormatCode.Format2DIndexedColour: return new Record2DIndexed(br);
                case FormatCode.Format2DTrueColour: return new Record2DTrueColour(br);
                case FormatCode.Format3DIndexedColour: return new Record3DIndexed(br);
                case FormatCode.Format3DTrueColour: return new Record3DTrueColour(br);
                case FormatCode.FormatColourPalette: return new RecordColourPalette(br);
                default: throw new ArgumentException($"Unknown FormatCode '{formatCode}'", nameof(formatCode));
            }
        }

    }
}
