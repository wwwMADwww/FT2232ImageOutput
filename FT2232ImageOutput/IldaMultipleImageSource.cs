using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput
{
    public class IldaMultipleImageSource : IldaImageSourceBase
    {
        private readonly IEnumerable<string> _filepaths;

        public IldaMultipleImageSource(IEnumerable<string> filepaths)
        {
            _filepaths = filepaths;
            
        }


        public override bool Streaming => false;

        public override IEnumerable<ImageFrame> GetFrames()
        {

            var frames = new List<ImageFrame>();


            foreach(var path in _filepaths)
            {
                var fullpath = Path.GetFullPath(path);

                var image = ReadFile(fullpath);

                frames.AddRange(image);

            }

            return frames;

        }



    }
}
