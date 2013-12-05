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

                int counter = 0;
                while (true)
                {
                    Console.WriteLine("numControllers: " + controllers.Length);
                    for (int i = 0; i < controllers.Length; i++)
                    {
                        //// Connect ////
                        if (controllers[i] == null) break;

                        // setup this controller's socket
                        IPAddress ip = Dns.GetHostEntry("localhost").AddressList[1]; // won't always be list[1]
                        IPEndPoint ipe = new IPEndPoint(ip, ports[i]);
                        Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        s.Connect(ipe);

                        //Console.WriteLine("Socket " + i + " connected?: " + s.Connected);

                        //// Send ////
                        byte[] status_packet = new byte[49];
                        int len = 0;
                        UsbSetupPacket setup = new UsbSetupPacket(0xa1, 0x01, 0x0101, 0, 0x31); // magic values
                        controllers[i].ControlTransfer(ref setup, status_packet, 49, out len);

                        //Console.WriteLine("statuspacket: " + String.Join(" ",status_packet));

                        // send to UI
                        byte[] rearranged = rearrangeStatus(status_packet);
                        s.Send(rearranged, 12, 0);
                        //Console.WriteLine("Sent");

                        //// Close ////
                        s.Close();
                    }
                    counter++;
                    //Console.WriteLine("ctr: " + counter);
                    Thread.Sleep(50);
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

            /*int L1 = (s[20] > 127 ? 1 : 0) << 7;
            int L2 = (s[18] > 127 ? 1 : 0) << 6;
            int L3 = (s[2] == 0x02 ? 1 : 0) << 5;
            int R1 = (s[21] > 127 ? 1 : 0) << 4;
            int R2 = (s[19] > 127 ? 1 : 0) << 3;
            int R3 = (s[2] == 0x04 ? 1 : 0) << 2;
            int up = (s[2] == 0x10 ? 1 : 0) << 1;
            int down = (s[2] == 0x40 ? 1 : 0) << 0;
            */
            byte L1 = (byte) ((((s[3] & (1 << 2)) == 4) ? 1 : 0) << 7);
            byte L2 = (byte) ((((s[3] & (1 << 0)) == 1) ? 1 : 0) << 6);
            byte L3 = (byte) ((((s[2] & (1 << 1)) == 2) ? 1 : 0) << 5);
            byte R1 = (byte) ((((s[3] & (1 << 3)) == 8) ? 1 : 0) << 4);
            byte R2 = (byte) ((((s[3] & (1 << 1)) == 2) ? 1 : 0) << 3);
            byte R3 = (byte) ((((s[2] & (1 << 2)) == 4) ? 1 : 0) << 2);
            byte up = (byte) ((((s[2] & (1 << 4)) == 16) ? 1 : 0) << 1);
            byte down = (byte) ((((s[2] & (1 << 6)) == 64) ? 1 : 0) << 0);

            ns[8] = (byte) (L1 | L2 | L3 | R1 | R2 | R3 | up | down);

           /* byte left = (byte) ((s[2] == 0x80 ? 1 : 0) << 7);
            byte right = (byte) ((s[2] == 0x20 ? 1 : 0) << 6);

            int square = (byte) ((s[3] == 0x80 ? 1 : 0) << 5);
            int triangle = (byte) ((s[3] == 0x10 ? 1 : 0) << 4);
            int circle = (byte) ((s[3] == 0x20 ? 1 : 0) << 3);
            //int cross = (byte) ((s[3] == 0x40 ? 1 : 0) << 2);
            int cross = (byte)((((s[3] & (1 << 6)) == 64) ? 1 : 0) << 2);
            int start = (byte) ((s[2] == 0x08 ? 1 : 0) << 1);
            int select = (byte) ((s[2] == 0x01 ? 1 : 0) << 0);*/
            
            byte left = (byte) ((((s[2] & (1 << 7)) == 128) ? 1 : 0) << 7);
            byte right = (byte)((((s[2] & (1 << 5)) == 32) ? 1 : 0) << 6);
            byte square = (byte)((((s[3] & (1 << 7)) == 128) ? 1 : 0) << 5);
            byte triangle = (byte)((((s[3] & (1 << 4)) == 16) ? 1 : 0) << 4);
            byte circle = (byte)((((s[3] & (1 << 5)) == 32) ? 1 : 0) << 3);
            byte cross = (byte)((((s[3] & (1 << 6)) == 64) ? 1 : 0) << 2);
            byte start = (byte)((((s[2] & (1 << 3)) == 8) ? 1 : 0) << 1);
            byte select = (byte)((((s[2] & (1 << 0)) == 1) ? 1 : 0) << 0);

            ns[9] = (byte)(left | right | square | triangle | circle | cross | start | select);

            ns[10] = s[4]; // PS button
            ns[11] = 0;

            Console.WriteLine("L1F: " + (s[3] & (1 << 2)) + "    xF: " + (s[3] & (1 << 6)));
            Console.WriteLine("L1: " + (s[20] > 127 ? 1 : 0) + "    x: " + (s[3] == 0x40 ? 1 : 0));
            Console.WriteLine(String.Join(" ", ns));
            //Console.WriteLine(s[6] + ", " + s[7] + "," + s[8] + "," + s[9]);

            return ns;
        }
    }
}