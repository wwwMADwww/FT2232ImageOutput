# FT2232ImageOutput
Drawing images on oscilloscope using [FT2232HC](https://www.ftdichip.com/Products/ICs/FT2232H.html) chip breakout board.

## Run
Executable running example:
```
cd FT2232ImageOutput
dotnet run -p FT2232ImageOutput.csproj -- -f samplefiles\lain.ild
```

Example with baudrate setting to increase or decrease speed if needed:
```
cd FT2232ImageOutput
dotnet run -p FT2232ImageOutput.csproj -- -f samplefiles\lain.ild -b 800000
```

Tips for setup and running on Linux: [Running on Linux](RunningOnLinux.md)

## Schematics
Schematic diagrams for all modes available at [FT2232ImageOutput/diagrams](FT2232ImageOutput/diagrams/)

Schematic diagrams for fully assembled device variants - [hardware](hardware)


## Plans
 - Add realtime streaming feature
 - Add different image sources (SVG, ~~WAV~~)
 - ~~Add different output targets and maybe devices~~
 - ~~Increase DAC resolution to 10-12 bits~~
 - ~~Add DAC to Z channel to using it as blanking and b/w color output~~
 - ~~Migrating to net.core~~
 - Add GUI

Result on my setup for `lain.ild` file (sample files available at [FT2232ImageOutput/samplefiles/](FT2232ImageOutput/samplefiles/)):

![lain](/FT2232ImageOutput/samplefiles/lain.png?raw=true "lain")

More photos, videos and russian blog on VK - [ebajous_curves](https://vk.com/ebajous_curves)