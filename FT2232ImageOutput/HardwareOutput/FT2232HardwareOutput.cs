using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FT2232ImageOutput.HardwareOutput
{
    
    public class FT2232HardwareOutput : IHardwareOutput
    {
        protected readonly string _channelName;
        protected readonly uint _baudrate;
        protected readonly IPointBitMapper _pointBitMapper;
        protected int _bufferSize = 4096;
        protected TimeSpan _maxSequentialIoErrorsTime = TimeSpan.FromSeconds(3);

        protected FTDI _channel;

        protected DateTime _sequentialIoErrorsTimeStart = default;

        public FT2232HardwareOutput(string channelName, uint baudrate, IPointBitMapper pointBitMapper)
        {
            _channelName = channelName;
            _baudrate = baudrate;
            _pointBitMapper = pointBitMapper;

            _channel = OpenChannel(_channelName, _baudrate);
        }

        public void Output(IEnumerable<byte> bytes)
        {
            
            var buffer = bytes.ToArray();
            
            WriteToChannel(_channel, buffer);


        }



        protected void WriteToChannel(FTDI channel, byte[] dataBuf)
        {
            var writtenTotal = 0;

            byte[] sendbuf = dataBuf;

            while (writtenTotal < dataBuf.Length)
            {
                uint written = 0;
                FTDI.FT_STATUS status;

                while (true)
                {

                    status = channel.Write(sendbuf, dataBuf.Length - writtenTotal, ref written);

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
                        Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

                    _sequentialIoErrorsTimeStart = default;
                    break;
                }

                writtenTotal += (int)written;

                sendbuf = new byte[dataBuf.Length];
                Array.Copy(dataBuf, writtenTotal, sendbuf, 0, dataBuf.Length - writtenTotal);
            }
        }



        protected FTDI OpenChannel(string channelName, uint baudRate)
        {

            var res = new FTDI();

            FTDI.FT_STATUS status = FTDI.FT_STATUS.FT_OTHER_ERROR;

            FTDI.FT_DEVICE_INFO_NODE[] devicelist = new FTDI.FT_DEVICE_INFO_NODE[255];

            status = res.GetDeviceList(devicelist);
            
            // // ON LINUX RUN APP WITH SUDO OR CONFIGURE ACCESS TO USB FTDI DEVICES
            // Console.WriteLine($"getdevicelist status is {status}");
            // foreach(var device in devicelist.Where(x => x != null))
            // {
            //     Console.WriteLine($"Description is '{device.Description}'");
            //     Console.WriteLine($"SerialNumber is '{device.SerialNumber}'");
            //     Console.WriteLine($"ID is '{device.ID}'");
            //     Console.WriteLine($"LocId is '{device.LocId}'");
            //     Console.WriteLine($"Type is '{device.Type}'");
            //     Console.WriteLine($"------");
            // }

            status = res.OpenBySerialNumber(channelName);

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

            status = res.SetTimeouts(1, 1);
            Debug.Assert(status == FTDI.FT_STATUS.FT_OK);

            return res;

        }

    }


}
