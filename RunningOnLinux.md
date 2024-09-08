# Tips for setup and running on Linux
This may be not 100% right but works good for me.

## Install .NET 8:

*// TODO:*

## Install FTDI driver:

 - download latest libs from https://www.ftdichip.com/Drivers/D2XX.htm

 - unpack binaries and copy to `/usr/local/lib/` as [official installation guide for Linux](https://www.ftdichip.com/Support/Documents/AppNotes/AN_220_FTDI_Drivers_Installation_Guide_for_Linux.pdf) suggest

```bash
tar -xf libftd2xx-x86_64-1.4.8.gz
cd release/build/
sudo cp * /usr/local/lib/
cd /usr/local/lib/
sudo ln -s libftd2xx.so.1.4.8 libftd2xx.so
sudo chmod 0755 libftd2xx.so.1.4.8
```
(change version `1.4.8` if needed)

## Run application:

 - Before run the application make sure you have unloaded `ftdi_sio` and `usbserial` kernel modules. You can unload them using commands:
```bash
sudo rmmod ftdi_sio
sudo rmmod usbserial
```

 - Accessing FTDI device requires root permissions so run application using `sudo` or configure access to FTDI device (see below)


## Access to FTDI device without root permissions:

 - Add new group (`ftdi` here, name can be any) and add your user to it (`username` here, change to your user name)
```bash
sudo groupadd ftdi
sudo usermod -aG ftdi username
```

 - Run `lsusb` to get Vendor id and Product id. Output would be like this:
```
Bus 003 Device 007: ID 0403:6010 Future Technology Devices International, Ltd FT2232C/D/H Dual UART/FIFO IC
```
In this case Vendor id = `0403`, Product id = `6010`

Or you can use `lsusb -v`. Output would be bulky but more obvious:
```
...
  idVendor           0x0403 Future Technology Devices International, Ltd
  idProduct          0x6010 FT2232C/D/H Dual UART/FIFO IC
...
```

 - Create file `/etc/udev/rules.d/99-ftdi.rules` with following content. `GROUP` is the group we created earlier, `idVendor` and `idProduct` are values from `lsusb`.
```
SUBSYSTEMS=="usb", ATTRS{idVendor}=="0403", ATTRS{idProduct}=="6010", GROUP="ftdi", MODE="0666"
```
 - Reload configuration
```bash
sudo udevadm control --reload-rules
```
or reboot machine. Now you can use FTDI device and run application without root permissions and `sudo`.
