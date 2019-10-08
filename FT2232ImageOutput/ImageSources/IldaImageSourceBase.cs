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

            // TODO: add default palette from ILDA datasheet (or whatever the "ILDA test pattern" wants to use without predefining palette in file, i don't know)
            List<RecordColourPalette> palette = new List<RecordColourPalette>();

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
                        // TODO: proper index checking
                        var c = r.ColourIndex < palette.Count() ? palette[r.ColourIndex] : new RecordColourPalette(0, 0, 0);

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
                        // TODO: proper index checking
                        var c = r.ColourIndex < palette.Count() ? palette[r.ColourIndex] : new RecordColourPalette(0, 0, 0);

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

    }
}
