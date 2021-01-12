using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FT2232ImageOutput.PathImages
{

    public class PathToPointImageSourceParams
    {
        public float DotImageDistanceMin { get; set; }
        public float DotImageDistanceMax { get; set; }
        public float FillDistanceMin { get; set; }
        public float FillDistanceMax { get; set; }
    }
        


    public class PathToPointImageSource : IImageSource
    {
        private readonly List<PathImageInfo> _pathImages;
        private readonly IPrimitiveFillGenerator _fillGenerator;
        private readonly PathToPointImageSourceParams _parameters;
        int _currentFrame = 0;

        public PathToPointImageSource(
            IEnumerable<PathImageInfo> pathImages, IPrimitiveFillGenerator fillGenerator, 
            ImageMaxValues maxValues, PathToPointImageSourceParams parameters)
        {
            _pathImages = pathImages.ToList();
            _fillGenerator = fillGenerator;
            MaxValues = maxValues;
            _parameters = parameters;
        }

        public ImageType ImageType => ImageType.Vector;


        public ImageMaxValues MaxValues { get; protected set; }


        public bool Streaming => true;


        public IEnumerable<ImageFrame> GetFrames()
        {

            var globalShift = new Vector2(-MaxValues.MinX, -MaxValues.MinY);

            // TODO: streaming
            var pathImage = _pathImages[_currentFrame];
            _currentFrame = (_currentFrame + 1) % _pathImages.Count;


            var imageWidth = pathImage.Width;
            var imageHeight = pathImage.Height;

            float scaleKoeff = (imageWidth > imageHeight)
                ? (MaxValues.MaxX - MaxValues.MinX) / imageWidth
                : (MaxValues.MaxY - MaxValues.MinY) / imageHeight;

            var globalScale = new Vector2(scaleKoeff, scaleKoeff);


            var scaledImage = PathUtils.ScalePathImage(pathImage, globalScale, globalShift);

            // TODO: cache dotImage, fill update only

            var dotImage = scaledImage.Elements
                .Where(e => e.HasStroke)
                .Select(pathInfo =>
                {
                    var pi = (ElementInfo)pathInfo.Clone();
                    pi.Path = PathUtils.PathToDots(pathInfo, globalScale, globalShift, _parameters.DotImageDistanceMin, _parameters.DotImageDistanceMax);
                    return pi;
                });

            var segFillImage = scaledImage.Elements
                .Where(e => e.HasFill)
                .Select(pathInfo =>
                {
                    var pi = (ElementInfo)pathInfo.Clone();
                    pi.Path = PathUtils.PathToSegments(pathInfo, globalScale, globalShift, _parameters.FillDistanceMin, _parameters.FillDistanceMax);
                    return pi;
                });



            var points = new List<ImagePoint>();

            foreach (var ei in dotImage)
            {
                points.AddRange(ei.Path.OfType<Dot>().Select(p => new ImagePoint() { 
                    Blanking = false,
                    X = (int) p.Pos.X,
                    Y = (int) p.Pos.Y,
                    Z = ei.StrokeColor.A,
                    R = ei.StrokeColor.R,
                    G = ei.StrokeColor.G,
                    B = ei.StrokeColor.B
                }));
            }

            var lastpoint = points.Last().Clone();
            lastpoint.Blanking = true;
            points.Add(lastpoint);

            foreach (var ei in segFillImage)
            {

                // var rMax = 20;
                // var rMin = 3;

                // var filldots = CreateIntervalFillDots(seg.segs, seg.ei.Bounds, r, new Vector2(coordShift % r, coordShift % r), false, seg.ei.FillRule)
                // var filldots = CreateIntervalFillDots(poly.poly, 15, random.Next(0, 15), random.Next(0, 15), poly.rule)
                //var filldots = CreateIntervalFillDots(seg.segs, seg.ei.Bounds, r, new Vector2(r, r), true, seg.ei.FillRule)
                // var filldots = CreateRandomFillDots(seg.segs, seg.ei.Bounds, ((rMax - r) * 200), false, true, seg.ei.FillRule);
                //      .ForEach(v => fillPoints.Append(new Vertex(new Vector2f(v.X, v.Y), Color.Green)));
                ;


                var fillPoints = _fillGenerator.GenerateFill(ei);

                points.AddRange(fillPoints.Path.OfType<Dot>().Select(p => new ImagePoint()
                {
                    Blanking = false,
                    X = (int)p.Pos.X,
                    Y = (int)p.Pos.Y,
                    Z = ei.StrokeColor.A,
                    R = ei.StrokeColor.R,
                    G = ei.StrokeColor.G,
                    B = ei.StrokeColor.B
                }));

                lastpoint = points.Last().Clone();
                lastpoint.Blanking = true;
                points.Add(lastpoint);

            }



            return new[] { new ImageFrame() {
                Duration = 0,
                Number = 1,
                Points = points.ToArray()
            } };



        }




    }
}
