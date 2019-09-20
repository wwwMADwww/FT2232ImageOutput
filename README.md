# FT2232ImageOutput
Drawing images on oscilloscope using ft2232hc chip breakout board, 74hc595 shift registers and R2R DACs.

## Run
Executable running example:
```
FT2232ImageOutput.exe -f ..\..\samplefiles\lain.ild
```

Example with baudrate setting to increase or decrease speed if needed:
```
FT2232ImageOutput.exe -f ..\..\samplefiles\lain.ild -b 5000000
```

## Schematics
Schematic diagrams for all modes available at [FT2232ImageOutput/diagrams/](FT2232ImageOutput/diagrams/)


## Plans
 - Add realtime streaming feature
 - Add different image sources
 - Add different output targets and maybe devices
 - ~~Increase DAC resolution to 10-12 bits~~
 - ~~Add DAC to Z channel to using it as blanking and b/w color output~~
 - Migrating to net.core


Result on my setup for `lain.ild` file (sample files available at [FT2232ImageOutput/samplefiles/](FT2232ImageOutput/samplefiles/)):

![lain](/FT2232ImageOutput/samplefiles/lain.png?raw=true "lain")