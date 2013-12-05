using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GamingInterface
{
    public class MMClient
    {
        static Socket clientSocket;
        private MainInterface _myParent;
        bool connected = false;
        byte[] sendBuffer = new byte[13];
        //byte[] sendBuffer = new byte[14];

        private int LclientCtr = 0;

        public MMClient(MainInterface myParent)
        {
            _myParent = myParent;
            try
            {
                IPAddress ip = Dns.GetHostEntry("localhost").AddressList[1]; // won't always be list[1]
                IPEndPoint ipe = new IPEndPoint(ip, 7698);
                clientSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(ipe);

                Console.WriteLine("Socket connected?: " + clientSocket.Connected);
                connected = clientSocket.Connected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("connection failed");
                Console.WriteLine(ex.Message);
            }
        }

        //public void sendInputInfo(byte[] buffer, byte playerID)
        public void sendInputInfo()
        {
            //Console.WriteLine("SendInfo");
            if (connected)
            {
                lock (_myParent.bufferListLock)
                {
                    if (_myParent.bufferList.Count > 0)
                    {
                        try
                        {
                            Array.Copy(_myParent.bufferList[0], sendBuffer, 13);
                            //Array.Copy(_myParent.bufferList[0], sendBuffer, 14);
                            clientSocket.Send(sendBuffer, 13, 0);
                            //clientSocket.Send(sendBuffer, 14, 0);
                            //LclientCtr++;
                            //Console.WriteLine("LclientCtr: " + LclientCtr);
                            _myParent.bufferList.RemoveAt(0);
                        }
                        catch (SocketException se)
                        {
                            System.Diagnostics.Trace.WriteLine("Socket exception: " + se);
                        }
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("Too fast");
                    }
                }
            }
            else
            {
                System.Diagnostics.Trace.WriteLine("Legion Token is null");
            }
        }
    }
}
