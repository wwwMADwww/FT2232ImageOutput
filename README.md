# FT2232ImageOutput
Drawing images on oscilloscope using ft2232hc chip breakout board, 74hc595 shift registers and 8-bit R2R DACs.

Executable running example:
```
FT2232ImageOutput.exe -f ..\..\samplefiles\lain.ild
```

Example with baudrate setting to increase or decrease speed if needed:
```
FT2232ImageOutput.exe -f ..\..\samplefiles\lain.ild -b 5000000
```

Schematic diagram for the code (from `FT2232ImageOutput/diagrams/`):
![Schematic](/FT2232ImageOutput/diagrams/schematic.png?raw=true "Schematic")

Result photos on my setup (from `FT2232ImageOutput/samplefiles/`):
![pikachu](/FT2232ImageOutput/samplefiles/pikachu.jpg?raw=true "pikachu")
![lain](/FT2232ImageOutput/samplefiles/lain.jpg?raw=true "lain")

## Plans
 - Add realtime streaming feature
 - Add different image sources
 - Add different output targets and maybe devices
 - ~~Increase DAC resolution to 10-12 bits~~
 - ~~Add DAC to Z channel to using it as blanking and~~ b/w color output
 - Migrating to net.core
