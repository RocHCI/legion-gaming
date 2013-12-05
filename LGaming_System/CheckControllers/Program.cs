using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.Info;
using System.Diagnostics;
using System.Timers;

namespace AxisSocket
{
    internal class AxisSocket
    {
        public static UsbDevice[] devices; // ignore warning; this is definitely used
        public static UsbDevice[] controllers;
        public static int[] ports = { 4141, 4242, 4343, 4444 };
        public static Socket[] sockets = new Socket[4];

        /**
         * Finds all controllers, connects them to their own sockets, and sends input over those sockets.
         */
        public static void Main(string[] args)
        {
            ErrorCode ec = ErrorCode.None;
            controllers = getControllers();
            try
            {
                for (int i = 0; i < controllers.Length; i++)
                {
                    if (controllers[i] == null) break;
                    UsbDevice controller = controllers[i]; // already opened
                    IUsbDevice device = controller as IUsbDevice;
                    device.SetConfiguration(1);
                    device.ClaimInterface(0);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally // cleanup
            {
                for(int i = 0; i < controllers.Length; i++){
                    if (controllers[i] != null)
                    {
                        UsbDevice controller = controllers[i];
                        if (controller != null && controller.IsOpen)
                        {
                            IUsbDevice device = controller as IUsbDevice;
                            device.ReleaseInterface(0);
                            controller.Close();
                        }
                    }
                }
                UsbDevice.Exit();
                Console.ReadKey();
            }
        }

        /**
         * Finds all of the connected Playstation controllers.
         */
        public static UsbDevice[] getControllers()
        {
            List<UsbDevice> controllersL = new List<UsbDevice>();
            //UsbDevice[] controllers = new UsbDevice[4];
            UsbDevice[] devices = new UsbDevice[20];

            UsbRegDeviceList allDevices = UsbDevice.AllDevices;
            Console.WriteLine("numDevices: " + allDevices.Count);
            int i = 0;
            foreach (UsbRegistry usbRegistry in allDevices)
            {
                //Console.WriteLine("device"+i);
                if (usbRegistry.Open(out devices[i]))
                {
                    Console.WriteLine(devices[i].Info.ToString());
                    if (devices[i].Info.ToString().Contains("PLAYSTATION"))
                    {
                        /*int index = 0;
                        for (int j = 0; j < 4; j++)
                        {
                            if (controllers[j] == null)
                            {
                                index = j;
                                break;
                            }
                        }
                        controllers[index] = devices[i];*/
                        controllersL.Add(devices[i]);
                    }
                }
                i++;
            }

            return controllersL.ToArray();
        }

        /**
         * Rearranges the data from PS3 controller so that it fits our format.
         */
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

            ns[8] = (byte) (L1 | L2 | L3 | R1 | R2 | R3 | up | down);

            byte left = (byte) ((s[2] == 0x80 ? 1 : 0) << 7);
            byte right = (byte) ((s[2] == 0x20 ? 1 : 0) << 6);

            int square = (byte) ((s[3] == 0x80 ? 1 : 0) << 5);
            int triangle = (byte) ((s[3] == 0x10 ? 1 : 0) << 4);
            int circle = (byte) ((s[3] == 0x20 ? 1 : 0) << 3);
            int cross = (byte) ((s[3] == 0x40 ? 1 : 0) << 2);
            int start = (byte) ((s[2] == 0x08 ? 1 : 0) << 1);
            int select = (byte) ((s[2] == 0x01 ? 1 : 0) << 0);

            ns[9] = (byte)(left | right | square | triangle | circle | cross | start | select);

            ns[10] = s[4]; // PS button
            ns[11] = 0;

            Console.WriteLine(s[6] + ", " + s[7] + "," + s[8] + "," + s[9]);

            return ns;
        }
    }
}