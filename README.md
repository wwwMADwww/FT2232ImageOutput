# FT2232ImageOutput
Drawing images on oscilloscope using [FT2232HC](https://www.ftdichip.com/Products/ICs/FT2232H.html) chip breakout board.

## Run
It stil haven't any configuration files or GUI, so to configure something you need to change the code, mostly in `Program.cs`.

But there is two run params you need to set:
```
 -f <path to file or directory>
 -b <baudrate>
```
Examples:
```
dotnet run -p FT2232ImageOutput.csproj -- -f samplefiles\lain.ild
dotnet run -p FT2232ImageOutput.csproj -- -f samplefiles\lain.ild -b 800000
```

Tips for setup and running on Linux: [Running on Linux](RunningOnLinux.md)

## Schematics
Schematic diagrams for all modes available at [FT2232ImageOutput/diagrams](FT2232ImageOutput/diagrams/)

Schematic diagrams for fully assembled device variants - [hardware](hardware)


## Plans
 - Add realtime streaming feature
 - ~~Add different image sources (SVG, WAV)~~
 - ~~Add different output targets and maybe devices~~
 - ~~Increase DAC resolution to 10-12 bits~~
 - ~~Add DAC to Z channel to using it as blanking and b/w color output~~
 - ~~Migrating to net.core~~
 - Add GUI

## Results

More photos, videos and russian blog on VK - [ebajous_curves](https://vk.com/ebajous_curves)

Result on my setup for `lain.ild` file (sample files available at [FT2232ImageOutput/samplefiles/](FT2232ImageOutput/samplefiles/)):

![lain](/FT2232ImageOutput/samplefiles/lain.png?raw=true "lain")
