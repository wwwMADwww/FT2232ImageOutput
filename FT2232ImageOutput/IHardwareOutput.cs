namespace FT2232ImageOutput;

public interface IHardwareOutput
{
    int MaxBytes { get; }

    void Output(byte[] bytes);
}
