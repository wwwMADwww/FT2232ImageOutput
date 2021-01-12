using System;
using System.Collections.Generic;
using System.Text;

namespace FT2232ImageOutput.PathImages
{
    public interface IPrimitiveFillGenerator // <TFillData>
    {
        // TODO: prepare fill data by generator itself

        // TFillData CreateFillData(ElementInfo filledPath);

        // ElementInfo GenerateFill(TFillData fillData);

        ElementInfo GenerateFill(ElementInfo preparedFillPath);

    }
}
