using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace FT2232ImageOutput.PathImages.FillGenerators
{
    public class EmptyFillGenerator : IPrimitiveFillGenerator
    {

        public EmptyFillGenerator()
        {
        }


        public ElementInfo GenerateFill(ElementInfo filledPoly)
        {
            return new ElementInfo()
            {
                Bounds = filledPoly.Bounds,
                HasStroke = true,
                StrokeColor = filledPoly.StrokeColor,
                Path = new Dot[0]
            };

        }

    }
}
