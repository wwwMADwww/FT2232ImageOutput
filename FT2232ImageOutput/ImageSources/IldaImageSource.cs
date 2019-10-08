using DSS.ILDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.ImageSources
{
    public class IldaImageSource: IldaImageSourceBase
    {
        private readonly string _filename;


        public IldaImageSource(string filename)
        {
            _filename = filename;
        }

        public override bool Streaming => false;



        public override IEnumerable<ImageFrame> GetFrames()
        {

            return ReadFile(_filename);

        }


    }
}
