using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FT2232ImageOutput.Utils;

public static class MathUtils
{

    // https://stackoverflow.com/questions/4229662/convert-numbers-within-a-range-to-numbers-within-another-range
    public static int ConvertRange(
        int originalStart, int originalEnd, // original range
        int newStart, int newEnd, // desired range
        int value) // value to convert
    {
        float scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
        return (int)(newStart + ((value - originalStart) * scale));
    }  
    
    public static float ConvertRange(
        float originalStart, float originalEnd, // original range
        float newStart, float newEnd, // desired range
        float value) // value to convert
    {
        float scale = (float)(newEnd - newStart) / (originalEnd - originalStart);
        return (newStart + ((value - originalStart) * scale));
    }
    
    public static double ConvertRange(
        double originalStart, double originalEnd, // original range
        double newStart, double newEnd, // desired range
        double value) // value to convert
    {
        var scale = (newEnd - newStart) / (originalEnd - originalStart);
        return (newStart + ((value - originalStart) * scale));
    }


    public static float RangePercent(float originalStart, float originalEnd, float percent)
    {
        var diff = originalEnd - originalStart;
        return originalStart + (diff * percent);
    }

    public static int RangePercent(int originalStart, int originalEnd, float percent)
    {
        return (int)Math.Round(RangePercent((float)originalStart, (float)originalEnd, percent));
    }


    public static float Distance(float x1, float y1, float x2, float y2)
    {
        return (float)Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
    }

    public static (float x, float y) AspectRatio(float width, float height)
    {
        return width >= height
            ? (1.0f, height / width)
            : (width / height, 1.0f);
    }

    public static (double x, double y) AspectRatio(double width, double height)
    {
        return width >= height
            ? (1.0, height / width)
            : (width / height, 1.0);
    }

}
