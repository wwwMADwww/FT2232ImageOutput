using FTD2XX_NET;
using System;
using System.Diagnostics;
using System.Linq;

namespace FT2232ImageOutput.HardwareOutput;


public class FT2232HardwareOutput : IHardwareOutput
{
    protected readonly uint? _locationId;
    protected readonly uint _baudrate;
    protected int _bufferSize;
    protected TimeSpan _maxSequentialIoErrorsTime = TimeSpan.FromSeconds(3);

    protected FTDI _channel;

    protected DateTime _sequentialIoErrorsTimeStart = default;

    public FT2232HardwareOutput(uint baudrate, int bufferSize, uint? locationId = null)
    {
        _locationId = locationId;
        _baudrate = baudrate;
        _bufferSize = bufferSize;
        _channel = OpenChannel(_locationId, _baudrate);
    }

    public int MaxBytes => _bufferSize;

    public void Output(byte[] bytes)
    {            
        WriteToChannel(_channel, bytes);
    }



    protected void WriteToChannel(FTDI channel, byte[] dataBuf)
    {
        var writtenTotal = 0;

        while (true)
        {
            uint written = 0;
            FTDI.FT_STATUS status;

            while (true)
            {
                var bufSpan = dataBuf.AsSpan((int)writtenTotal);

                status = channel.Write(bufSpan, ref written);

                // TODO: reconnect and retry when fails
                // Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

                if (status == FTDI.FT_STATUS.FT_IO_ERROR)
                {

                    if (_sequentialIoErrorsTimeStart == default)
                    {
                        _sequentialIoErrorsTimeStart = DateTime.Now;
                        continue;
                    }
                    else
                    {
                        if (DateTime.Now - _sequentialIoErrorsTimeStart >= _maxSequentialIoErrorsTime)
                            throw new Exception("Exceeded sequential IO errors time.");
                        else
                            continue;
                    }

                    // current FTDI drivers wont let you do this anyway
                    // status = _channel.Close();
                    // _channel = OpenChannel(_channelName, _baudrate);
                }
                else
                {
                    Debug.Assert(status == FTDI.FT_STATUS.FT_OK);
                }

                _sequentialIoErrorsTimeStart = default;
                break;
            }

            writtenTotal += (int)written;

            if (writtenTotal >= dataBuf.Length)
            {
                break;
            }
        }
    }



    protected FTDI OpenChannel(uint? locationId, uint baudRate)
    {

        var res = new FTDI();

        FTDI.FT_STATUS status = FTDI.FT_STATUS.FT_OTHER_ERROR;

        FTDI.FT_DEVICE_INFO_NODE[] devicelist = new FTDI.FT_DEVICE_INFO_NODE[255];

        status = res.GetDeviceList(devicelist);
        
        // ON LINUX RUN APP WITH SUDO OR CONFIGURE ACCESS TO USB FTDI DEVICES
        Console.WriteLine($"getdevicelist status is {status}");

        devicelist = devicelist.Where(x => x != null).ToArray();

        if (!devicelist.Any())
            throw new Exception("No FTDI devices found.");

        foreach(var device in devicelist)
        {
            Console.WriteLine($"Description is '{device.Description}'");
            Console.WriteLine($"SerialNumber is '{device.SerialNumber}'");
            Console.WriteLine($"ID is '{device.ID}'");
            Console.WriteLine($"LocId is '{device.LocId}'");
            Console.WriteLine($"Type is '{device.Type}'");
            Console.WriteLine($"------");
        }

        if (!locationId.HasValue)
        {
            Console.WriteLine($"LocationID is not set. Using first device from list.");
            locationId = devicelist.First().LocId;
        }
        else
        {
            if (!devicelist.Any(d => d.LocId == locationId))
            {
                throw new Exception($"FTDI device with LocationID {locationId} is not found");
            }
        }

        status = res.OpenByLocation(locationId.Value);
        Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

        res.ResetDevice();
        Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

        // for (int i = 0; i < 60; i++)
        // {
        //     status = res.OpenBySerialNumber(channelName);
        //     if (
        //         status != FTD2XX_NET.FTDI.FT_STATUS.FT_DEVICE_NOT_FOUND &&
        //         status != FTD2XX_NET.FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED
        //         )
        //         break;
        //     Thread.Sleep(1000);
        //     res = new FTDI();
        //     FTDI.FT_DEVICE_INFO_NODE[] list = new FTDI.FT_DEVICE_INFO_NODE[200];
        //     status = res.GetDeviceList(list);
        // }
        Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

        status = res.SetBaudRate(baudRate);
        Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

        status = res.SetLatency(0);
        Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

        // enable async bitbang mode for all 8 pins
        status = res.SetBitMode(0b11111111, FTDI.FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG);
        Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

        status = res.SetTimeouts(0, 0);
        Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

        return res;

    }

}
