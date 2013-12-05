using System;
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
        // If true, buttons get pressed but not held
        public const bool BLINKMODE = false;

        public static int[] ports = { 4141, 4242, 4343, 4444 };
        public static Socket[] sockets = new Socket[4];

        public const int LX = 0;
        public const int LX2 = 1;
        public const int LY = 2;
        public const int LY2 = 3;
        public const int RX = 4;
        public const int RX2 = 5;
        public const int RY = 6;
        public const int RY2 = 7;
        public const int BUTTONS1 = 8;
        public const int BUTTONS2 = 9;
        public const int PS = 10;

        public static byte[][] profiles = new byte[4][];
        public static int numControllers;

        public static void readInput()
        {
            String[] newProfiles = Console.ReadLine().Split(new char[] {' '});
            
            lock (profiles)
            {
                int numChangedControllers = (newProfiles.Length) / 2;
                int player;
                string[] pressedButtons;

                int L1;
                int L2;
                int L3;
                int R1;
                int R2;
                int R3;
                int up;
                int down;

                int left;
                int right;
                int sqr;
                int circ;
                int tri;
                int cross;
                int start;
                int select;

                for(int i = 0; i < numChangedControllers; i++) {

                    player = Convert.ToInt32(newProfiles[2 * i]);
                    pressedButtons = newProfiles[2 * i + 1].Split(new char[] { ';' });

                    profiles[player] = new byte[12];
                    for (int j = 0; j < profiles[player].Length; j++)
                    {
                        profiles[player][j] = 0;
                    }
                    profiles[player][LX2] = 128;
                    profiles[player][LY2] = 128;
                    profiles[player][RX2] = 128;
                    profiles[player][RY2] = 128;

                    L1 = 0;
                    L2 = 0;
                    L3 = 0;
                    R1 = 0;
                    R2 = 0;
                    R3 = 0;
                    up = 0;
                    down = 0;

                    left = 0;
                    right = 0;
                    sqr = 0;
                    circ = 0;
                    tri = 0;
                    cross = 0;
                    start = 0;
                    select = 0;

                    for (int j = 0; j < pressedButtons.Length; j++)
                    {
                        if (pressedButtons[j].StartsWith("lx"))
                        {
                            profiles[player][LX2] = (byte)Convert.ToInt32(pressedButtons[j].Substring(2));
                        }
                        else if (pressedButtons[j].StartsWith("ly"))
                        {
                            profiles[player][LY2] = (byte)Convert.ToInt32(pressedButtons[j].Substring(2));
                        }
                        else if (pressedButtons[j].StartsWith("rx"))
                        {
                            profiles[player][RX2] = (byte)Convert.ToInt32(pressedButtons[j].Substring(2));
                        }
                        else if (pressedButtons[j].StartsWith("ry"))
                        {
                            profiles[player][RY2] = (byte)Convert.ToInt32(pressedButtons[j].Substring(2));
                        }
                        else
                        {

                            switch (pressedButtons[j])
                            {
                                case "l1":
                                    L1 = 1 << 7;
                                    break;
                                case "l2":
                                    L2 = 1 << 6;
                                    break;
                                case "l3":
                                    L3 = 1 << 5;
                                    break;
                                case "r1":
                                    R1 = 1 << 4;
                                    break;
                                case "r2":
                                    R2 = 1 << 3;
                                    break;
                                case "r3":
                                    R3 = 1 << 2;
                                    break;
                                case "up":
                                case "u":
                                    up = 1 << 1;
                                    break;
                                case "down":
                                case "d":
                                    down = 1;
                                    break;
                                case "left":
                                case "l":
                                    left = 1 << 7;
                                    break;
                                case "right":
                                case "r":
                                    right = 1 << 6;
                                    break;
                                case "square":
                                case "sqr":
                                    sqr = 1 << 5;
                                    break;
                                case "triangle":
                                case "tri":
                                    tri = 1 << 4;
                                    break;
                                case "circle":
                                case "circ":
                                    circ = 1 << 3;
                                    break;
                                case "x":
                                case "cross":
                                    cross = 1 << 2;
                                    break;
                                case "start":
                                case "st":
                                    start = 1 << 1;
                                    break;
                                case "select":
                                case "se":
                                    select = 1;
                                    break;
                                default:
                                    Console.Write("Not a properly formed input.");
                                    break;
                            }
                            profiles[player][BUTTONS1] = (byte)(L1 | L2 | L3 | R1 | R2 | R3 | up | down);
                            profiles[player][BUTTONS2] = (byte)(left | right | sqr | tri | circ | cross | start | select);
                        }
                    }
                }
            }
            readInput();
        }

        /**
         * Finds all controllers, connects them to their own sockets, and sends input over those sockets.
         */
        public static void Main(string[] args)
        {
            //Console.WriteLine("Got " + args[0]);
            numControllers = Convert.ToInt32(args[0]);

            for (int i = 0; i < numControllers; i++)
            {
                profiles[i] = new byte[12];
                for(int j = 0; j < profiles[i].Length; j++) {
                    profiles[i][j] = 0;
                }
                profiles[i][LX2] = 128;
                profiles[i][LY2] = 128;
                profiles[i][RX2] = 128;
                profiles[i][RY2] = 128;
            }

            string[] pressedButtons;
            int player;
            int L1;
            int L2;
            int L3;
            int R1;
            int R2;
            int R3;
            int up;
            int down;

            int left;
            int right;
            int sqr;
            int circ;
            int tri;
            int cross;
            int start;
            int select;

            Console.WriteLine("args[0]: " + args[0] + "    len: " + args.Length);
            int numChangedControllers = (args.Length - 1) / 2;
            for (int i = 0; i < numChangedControllers; i++)
            {
                L1 = 0;
                L2 = 0;
                L3 = 0;
                R1 = 0;
                R2 = 0;
                R3 = 0;
                up = 0;
                down = 0;

                left = 0;
                right = 0;
                sqr = 0;
                circ = 0;
                tri = 0;
                cross = 0;
                start = 0;
                select = 0;

                player = Convert.ToInt32(args[i+i+1]);
                pressedButtons = args[i+i+2].Split(new char[] { ';' });

                for (int j = 0; j < pressedButtons.Length; j++)
                {
                    if (pressedButtons[j].StartsWith("lx"))
                    {
                        profiles[player][LX2] = (byte)Convert.ToInt32(pressedButtons[j].Substring(2));
                    }
                    else if (pressedButtons[j].StartsWith("ly"))
                    {
                        profiles[player][LY2] = (byte)Convert.ToInt32(pressedButtons[j].Substring(2));
                    }
                    else if (pressedButtons[j].StartsWith("rx"))
                    {
                        profiles[player][RX2] = (byte)Convert.ToInt32(pressedButtons[j].Substring(2));
                    }
                    else if (pressedButtons[j].StartsWith("ry"))
                    {
                        profiles[player][RY2] = (byte)Convert.ToInt32(pressedButtons[j].Substring(2));
                    }

                    switch (pressedButtons[j])
                    {
                        case "l1":
                            L1 = 1 << 7;
                            break;
                        case "l2":
                            L2 = 1 << 6;
                            break;
                        case "l3":
                            L3 = 1 << 5;
                            break;
                        case "r1":
                            L1 = 1 << 4;
                            break;
                        case "r2":
                            L2 = 1 << 3;
                            break;
                        case "r3":
                            L3 = 1 << 2;
                            break;
                        case "up":
                        case "u":
                            Console.WriteLine("UP");
                            up = 1 << 1;
                            break;
                        case "down":
                        case "d":
                            down = 1;
                            break;
                        case "left":
                        case "l":
                            left = 1 << 7;
                            break;
                        case "right":
                        case "r":
                            right = 1 << 6;
                            break;
                        case "square":
                        case "sqr":
                            sqr = 1 << 5;
                            break;
                        case "triangle":
                        case "tri":
                            tri = 1 << 4;
                            break;
                        case "circle":
                        case "circ":
                            circ = 1 << 3;
                            break;
                        case "x":
                        case "cross":
                            cross = 1 << 2;
                            break;
                        case "start":
                        case "st":
                            start = 1 << 1;
                            break;
                        case "select":
                        case "se":
                            select = 1;
                            break;
                        default:
                            Console.Write("Not a properly formed input.");
                            break;
                    }

                    profiles[player][BUTTONS1] = (byte)(L1 | L2 | L3 | R1 | R2 | R3 | up | down);
                    profiles[player][BUTTONS2] = (byte)(left | right | sqr | tri | circ | cross | start | select);
                }
            }

            Thread listenForChange = new Thread(new ThreadStart(readInput));
            listenForChange.Start();
            while (!listenForChange.IsAlive);

            ErrorCode ec = ErrorCode.None;
            try
            {
                int counter = 0;
                while (true)
                {
                    for(int i = 0; i < numControllers; i++)
                    {
                        //// Connect ////

                        // setup this controller's socket
                        IPAddress ip = Dns.GetHostEntry("localhost").AddressList[1]; // won't always be list[1]
                        IPEndPoint ipe = new IPEndPoint(ip, ports[i]);
                        Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                        s.Connect(ipe);

                        //Console.WriteLine("Socket " + i + " connected?: " + s.Connected);

                        //// Send ////
                        byte[] status_packet = new byte[49];

                        // send to UI
                        s.Send(profiles[i], 12, 0);
                        //Console.WriteLine("Sent");

                        if (BLINKMODE)
                        {
                            // Reset the profiles
                            for (int j = 0; j < profiles[i].Length; j++)
                            {
                                if (j != LX2 && j != LY2 && j != RX2 && j != RY2)
                                {
                                    profiles[i][j] = 0;
                                }
                            }

                            // send to UI
                            s.Send(profiles[i], 12, 0);
                        }

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
                Console.ReadKey();
            }
        }

        /**
         * Rearranges the data from PS3 controller so that it fits our format.
         */
        /*public static byte[] rearrangeStatus(byte[] s)
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
        }*/
    }
}