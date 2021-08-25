using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using ManuPath;

namespace FT2232ImageOutput.PathImages
{

    public interface IPathSource
    {

        IEnumerable<IEnumerable<Path>> GetFrames();

        Vector2 Size { get; }

        bool Streaming { get; }

    }


}
