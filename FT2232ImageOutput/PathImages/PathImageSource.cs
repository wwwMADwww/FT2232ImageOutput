using FT2232ImageOutput.Utils;
using ManuPath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace FT2232ImageOutput.PathImages
{

    public enum ColorChannel { None, Red, Green, Blue, Alpha, Grayscale }

    public class PathImageSource : IImageSource
    {
        private readonly IPathSource _pathSource;
        private readonly IPrimitiveConverter _strokeConverter;
        private readonly IPrimitiveConverter _fillConverter;
        private readonly Func<Path, IPrimitiveFillGenerator> _fillGeneratorFactory;
        private readonly ColorChannel _strokeBrightness;
        private readonly ColorChannel _fillIntensity;
        private readonly ColorChannel _fillBrightness;

        public PathImageSource(
            IPathSource pathSource,
            IPrimitiveConverter strokeConverter,
            ColorChannel strokeBrightness, 
            IPrimitiveConverter fillConverter, Func<Path, IPrimitiveFillGenerator> fillGeneratorFactory,
            ColorChannel fillIntensity, ColorChannel fillBrightness,
            ImageMaxValues maxValues,
            bool forceNotStreaming)
        {
            _pathSource = pathSource;
            _strokeConverter = strokeConverter;
            _fillConverter = fillConverter;
            _fillGeneratorFactory = fillGeneratorFactory;
            _strokeBrightness = strokeBrightness;
            _fillIntensity = fillIntensity;
            _fillBrightness = fillBrightness;
            MaxValues = maxValues;
            Streaming = forceNotStreaming ? false : _pathSource.Streaming;
        }

        public ImageType ImageType => ImageType.Vector;


        public ImageMaxValues MaxValues { get; protected set; }


        public bool Streaming { get; private set; }


        public IEnumerable<ImageFrame> GetFrames()
        {

            var globalShift = new Vector2(-MaxValues.MinX, -MaxValues.MinY);

            float scaleKoeff = (_pathSource.Size.X > _pathSource.Size.Y)
                ? (MaxValues.MaxX - MaxValues.MinX) / _pathSource.Size.X
                : (MaxValues.MaxY - MaxValues.MinY) / _pathSource.Size.Y;

            var globalScale = new Vector2(scaleKoeff, scaleKoeff);

            // TODO: streaming
            var pathFrames = _pathSource.GetFrames();

            var res = new List<ImageFrame>();

            var points = new List<ImagePoint>();

            var sw = new Stopwatch();
            sw.Start();
            foreach (var pathFrame in pathFrames)
            {
                // TODO: cache scaled
                // TODO: cache stroke, fill update only

                if (_strokeConverter != null)
                {
                    foreach (var path in pathFrame.Where(p => p.StrokeColor.HasValue))
                    {
                        var strokePrimitives = PathMath.ScalePaths(path.Primitives, globalScale, globalShift).ToArray();

                        strokePrimitives = _strokeConverter.Convert(strokePrimitives).ToArray();

                        if (strokePrimitives.Any())
                        {

                            points.Add(ToImagePoint(strokePrimitives.First().FirstPoint, GetChannelValue(path.StrokeColor.Value, _strokeBrightness)));

                            foreach (var prim in strokePrimitives)
                            {
                                points.Add(ToImagePoint(prim.LastPoint, GetChannelValue(path.StrokeColor.Value, _strokeBrightness)));
                            }

                            var lastpoint = points.Last();//.Clone();
                            lastpoint.Blanking = true;
                            points.Add(lastpoint);
                        }
                    }
                }

                if (_fillGeneratorFactory != null)
                {
                    foreach (var path in pathFrame.Where(p => p.FillColor.HasValue))
                    {
                        var fillpolygon = PathMath.ScalePaths(path.Primitives, globalScale, globalShift).ToArray();

                        if (_fillConverter != null)
                            fillpolygon = _fillConverter.Convert(fillpolygon).ToArray();

                        var fillPath = (Path)path.Clone();
                        fillPath.Primitives = fillpolygon;
                        fillPath.FillColor = Color.FromArgb(GetChannelValue(fillPath.FillColor.Value, _fillIntensity), 255, 255, 255);

                        var fillPrimitives = _fillGeneratorFactory(fillPath).GenerateFill();


                        foreach (var prim in fillPrimitives.Primitives)
                        {
                            points.Add(ToImagePoint(prim.FirstPoint, GetChannelValue(path.FillColor.Value, _fillBrightness)));
                        }

                        var lastpoint = points.Last().Clone();
                        lastpoint.Blanking = true;
                        points.Add(lastpoint);

                    }
                }


            }
            sw.Stop();

            return new[] { new ImageFrame() {
                Duration = 0,
                Number = 1,
                Points = points.ToArray()
            } };



        }


        byte GetChannelValue(Color color, ColorChannel channel) => channel switch
            {
                ColorChannel.None => 0,
                ColorChannel.Red => color.R,
                ColorChannel.Green => color.G,
                ColorChannel.Blue => color.B,
                ColorChannel.Alpha => color.A,
                ColorChannel.Grayscale => (byte)(.21f * color.R + .71f * color.G + .071f * color.B),
                _ => throw new ArgumentException("channelZ")
            };


        ImagePoint ToImagePoint(Vector2 p, int z)
        {
            return new ImagePoint()
            {
                Blanking = false,
                X = (int)p.X,
                Y = (int)p.Y,
                Z = MathUtils.ConvertRange(0, 255, MaxValues.MinZ, MaxValues.MaxZ, z),
                R = 255,
                G = 255,
                B = 255
            };
        }


    }
}
