using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.ImageSources
{
    public abstract class IldaImageSourceBase : IImageSource
    {


        public ImageType ImageType => ImageType.Vector;
        

        public ImageMaxValues MaxValues => new ImageMaxValues()
        {
            MaxX =  32767,
            MaxY =  32767,
            MaxZ =  32767,

            MinX = -32768,
            MinY = -32768,
            MinZ = -32768,

            MaxRGB = 255
        };

        public abstract bool Streaming { get; }

        public abstract IEnumerable<ImageFrame> GetFrames();

        protected IEnumerable<ImageFrame> ReadFile(string filepath)
        {

            var frames = new List<ImageFrame>();

            List<RecordColourPalette> palette = GetDefaultPalette().ToList();

            using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            {
                using (var br = new BinaryReader(fs))
                {

                    while (true)
                    {
                        var frameheader = new ILDAHeader(br);

                        var points = new List<ImagePoint>();

                        if (frameheader.Format == FormatCode.FormatColourPalette)
                            palette = new List<RecordColourPalette>();

                        for (int i = 0; i < frameheader.NumberOfRecords; i++)
                        {
                            var record = ReadIldaRecord(br, frameheader.Format);

                            if (frameheader.Format == FormatCode.FormatColourPalette)
                            {
                                palette.Add((RecordColourPalette) record);
                            }
                            else
                            {
                                var coordrec = (CoordinateRecord)record;

                                var point = RecordToPoint(coordrec, frameheader.Format, palette);

                                points.Add(point);
                                if (coordrec.LastPoint)
                                    break;
                            }
                        }

                        frames.Add(new ImageFrame()
                        {
                            Duration = -1,
                            Number = frameheader.EntityNumber,
                            Points = points
                        });

                        if (frameheader.NumberOfRecords == 0)
                            break;

                    }

                }
            }

            return frames;

        }



        protected ImagePoint RecordToPoint(CoordinateRecord ildaRecord, FormatCode formatCode, IList<RecordColourPalette> palette)
        {
            var res = new ImagePoint();

            switch (formatCode)
            {
                case FormatCode.Format2DIndexedColour:
                    {
                        var r = (Record2DIndexed)ildaRecord;

                        var c = palette[r.ColourIndex];

                        res.X = r.X;
                        res.Y = r.Y;
                        res.Blanking = r.Blanking;

                        res.R = c.Red;
                        res.G = c.Green;
                        res.B = c.Blue;

                        break;
                    }

                case FormatCode.Format2DTrueColour:
                    {
                        var r = (Record2DTrueColour)ildaRecord;

                        res.X = r.X;
                        res.Y = r.Y;
                        res.Blanking = r.Blanking;

                        res.R = r.Red;
                        res.G = r.Green;
                        res.B = r.Blue;

                        break;
                    }

                case FormatCode.Format3DIndexedColour:
                    {
                        var r = (Record3DIndexed)ildaRecord;

                        var c = palette[r.ColourIndex];

                        res.X = r.X;
                        res.Y = r.Y;
                        res.Z = r.Z;
                        res.Blanking = r.Blanking;

                        res.R = c.Red;
                        res.G = c.Green;
                        res.B = c.Blue;

                        break;
                    }

                case FormatCode.Format3DTrueColour:
                    {
                        var r = (Record3DTrueColour)ildaRecord;

                        res.X = r.X;
                        res.Y = r.Y;
                        res.Z = r.Z;
                        res.Blanking = r.Blanking;

                        res.R = r.Red;
                        res.G = r.Green;
                        res.B = r.Blue;

                        break;
                    }

                default: throw new ArgumentException($"Unknown FormatCode '{formatCode}'", nameof(formatCode));
            }


            return res;

        }



        protected DataRecord ReadIldaRecord(BinaryReader br, FormatCode formatCode)
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

        #region GetDefaultPalette

        protected IEnumerable<RecordColourPalette> GetDefaultPalette()
        {
            // ILDA_IDTF14_rev011.pdf 
            // Appendix
            // A.  Suggested Default Color Palette

            // RGB codes copied from:
            // https://github.com/awesomebytes/src/blob/master/LaserBoy_palette_set.cpp#L185

            return new RecordColourPalette[]
            {
                new RecordColourPalette(0xff, 0x00, 0x00),
                new RecordColourPalette(0xff, 0x10, 0x00),
                new RecordColourPalette(0xff, 0x20, 0x00),
                new RecordColourPalette(0xff, 0x30, 0x00),
                new RecordColourPalette(0xff, 0x40, 0x00),
                new RecordColourPalette(0xff, 0x50, 0x00),
                new RecordColourPalette(0xff, 0x60, 0x00),
                new RecordColourPalette(0xff, 0x70, 0x00),
                new RecordColourPalette(0xff, 0x80, 0x00),
                new RecordColourPalette(0xff, 0x90, 0x00),
                new RecordColourPalette(0xff, 0xa0, 0x00),
                new RecordColourPalette(0xff, 0xb0, 0x00),
                new RecordColourPalette(0xff, 0xc0, 0x00),
                new RecordColourPalette(0xff, 0xd0, 0x00),
                new RecordColourPalette(0xff, 0xe0, 0x00),
                new RecordColourPalette(0xff, 0xf0, 0x00),
                new RecordColourPalette(0xff, 0xff, 0x00),
                new RecordColourPalette(0xe0, 0xff, 0x00),
                new RecordColourPalette(0xc0, 0xff, 0x00),
                new RecordColourPalette(0xa0, 0xff, 0x00),
                new RecordColourPalette(0x80, 0xff, 0x00),
                new RecordColourPalette(0x60, 0xff, 0x00),
                new RecordColourPalette(0x40, 0xff, 0x00),
                new RecordColourPalette(0x20, 0xff, 0x00),
                new RecordColourPalette(0x00, 0xff, 0x00),
                new RecordColourPalette(0x00, 0xff, 0x20),
                new RecordColourPalette(0x00, 0xff, 0x40),
                new RecordColourPalette(0x00, 0xff, 0x60),
                new RecordColourPalette(0x00, 0xff, 0x80),
                new RecordColourPalette(0x00, 0xff, 0xa0),
                new RecordColourPalette(0x00, 0xff, 0xc0),
                new RecordColourPalette(0x00, 0xff, 0xe0),
                new RecordColourPalette(0x00, 0x82, 0xff),
                new RecordColourPalette(0x00, 0x72, 0xff),
                new RecordColourPalette(0x00, 0x68, 0xff),
                new RecordColourPalette(0x0a, 0x60, 0xff),
                new RecordColourPalette(0x00, 0x52, 0xff),
                new RecordColourPalette(0x00, 0x4a, 0xff),
                new RecordColourPalette(0x00, 0x40, 0xff),
                new RecordColourPalette(0x00, 0x20, 0xff),
                new RecordColourPalette(0x00, 0x00, 0xff),
                new RecordColourPalette(0x20, 0x00, 0xff),
                new RecordColourPalette(0x40, 0x00, 0xff),
                new RecordColourPalette(0x60, 0x00, 0xff),
                new RecordColourPalette(0x80, 0x00, 0xff),
                new RecordColourPalette(0xa0, 0x00, 0xff),
                new RecordColourPalette(0xe0, 0x00, 0xff),
                new RecordColourPalette(0xff, 0x00, 0xff),
                new RecordColourPalette(0xff, 0x20, 0xff),
                new RecordColourPalette(0xff, 0x40, 0xff),
                new RecordColourPalette(0xff, 0x60, 0xff),
                new RecordColourPalette(0xff, 0x80, 0xff),
                new RecordColourPalette(0xff, 0xa0, 0xff),
                new RecordColourPalette(0xff, 0xc0, 0xff),
                new RecordColourPalette(0xff, 0xe0, 0xff),
                new RecordColourPalette(0xff, 0xff, 0xff),
                new RecordColourPalette(0xff, 0xe0, 0xe0),
                new RecordColourPalette(0xff, 0xff, 0xff),
                new RecordColourPalette(0xff, 0xa0, 0xa0),
                new RecordColourPalette(0xff, 0x80, 0x80),
                new RecordColourPalette(0xff, 0x60, 0x60),
                new RecordColourPalette(0xff, 0x40, 0x40),
                new RecordColourPalette(0xff, 0x20, 0x20)
            };
        }

        #endregion GetDefaultPalette

    }
}
