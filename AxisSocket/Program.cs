using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace AxisSocket
{
    internal class AxisSocket
    {
        public static UsbDevice MyUsbDevice;

        #region SET YOUR USB Vendor and Product ID!

        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(0x054c, 0x0268);

        #endregion

        public static void Main(string[] args)
        {
            ErrorCode ec = ErrorCode.None;

            try
            {
                // Find and open the usb device.
                MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);

                // If the device is open and ready
                if (MyUsbDevice == null) throw new Exception("Device Not Found.");

                // If this is a "whole" usb device (libusb-win32, linux libusb)
                // it will have an IUsbDevice interface. If not (WinUSB) the 
                // variable will be null indicating this is an interface of a 
                // device.
                IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    // This is a "whole" USB device. Before it can be used, 
                    // the desired configuration and interface must be selected.

                    // Select config #1
                    wholeUsbDevice.SetConfiguration(1);

                    // Claim interface #0.
                    wholeUsbDevice.ClaimInterface(0);
                }

                IPAddress ip = Dns.GetHostEntry("localhost").AddressList[1]; // won't always be list[1]
                IPEndPoint ipe = new IPEndPoint(ip, 4242);
                Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                s.Connect(ipe);
                Console.WriteLine("Socket connected?: " + s.Connected);

                while (true)
                {
                    byte[] status_packet = new byte[49];
                    int len = 0;
                    UsbSetupPacket setup = new UsbSetupPacket(0xa1, 0x01, 0x0101, 0, 0x31); // magic values
                    MyUsbDevice.ControlTransfer(ref setup, status_packet, 49, out len);
                    for (int i = 0; i < 49; i++)
                    {
                        Console.Write(status_packet[i] + " ");
                    }
                    Console.WriteLine();

                    // send to UI
                    byte[] rearranged = rearrangeStatus(status_packet);
                    s.Send(rearranged, 12, 0);

                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        MyUsbDevice.Close();
                    }
                    MyUsbDevice = null;

                    // Free usb resources
                    UsbDevice.Exit();

                }

                // Wait for user input..
                Console.ReadKey();
            }
        }

        public static byte[] rearrangeStatus(byte[] s)
        {
            byte[] ns = new byte[12];

            // big endian... these values are really only 1 byte apiece rather than the 2 apiece we specified
            ns[0] = 0; // left joystick X
            ns[1] = s[6]; // left joystick X
            ns[2] = 0; // left joystick Y
            ns[3] = s[7]; // left joystick Y
            ns[4] = 0; // right joystick X
            ns[5] = s[8]; // right joystick X
            ns[6] = 0; // right joystick Y
            ns[7] = s[9]; // right joystick Y

            int L1 = (s[20] > 127 ? 1 : 0) << 7;
            int L2 = (s[18] > 127 ? 1 : 0) << 6;
            int L3 = (s[2] == 0x02 ? 1 : 0) << 5;
            int R1 = (s[21] > 127 ? 1 : 0) << 4;
            int R2 = (s[19] > 127 ? 1 : 0) << 3;
            int R3 = (s[2] == 0x04 ? 1 : 0) << 2;
            int up = (s[2] == 0x10 ? 1 : 0) << 1;
            int down = (s[2] == 0x40 ? 1 : 0) << 0;

            ns[8] = (byte) (L1 & L2 & L3 & R1 & R2 & R3 & up & down);

            byte left = (byte) ((s[2] == 0x80 ? 1 : 0) << 7);
            byte right = (byte) ((s[2] == 0x20 ? 1 : 0) << 6);

            int square = (byte) ((s[3] == 0x80 ? 1 : 0) << 5);
            int triangle = (byte) ((s[3] == 0x10 ? 1 : 0) << 4);
            int circle = (byte) ((s[3] == 0x20 ? 1 : 0) << 3);
            int cross = (byte) ((s[3] == 0x40 ? 1 : 0) << 2);
            int start = (byte) ((s[2] == 0x08 ? 1 : 0) << 1);
            int select = (byte) ((s[2] == 0x01 ? 1 : 0) << 0);

            ns[9] = (byte)(left & right & square & triangle & circle & cross & start & select);

            ns[10] = s[4]; // PS button
            ns[11] = 0;

            return ns;
        }
    }
}