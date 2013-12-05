using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Collections;
using System.Linq;
using System.IO;


namespace GamingInterface
{
    /// <summary>
    /// Implements the connection logic for the socket server.  After accepting a connection, all data read
    /// from the client is sent back to the client.  The read and echo back to the client pattern is continued 
    /// until the client disconnects.
    /// </summary>
    public class PlayerInputServer
    {
        public int mode;

        private int m_numConnections;   // the maximum number of connections the sample is designed to handle simultaneously 
        private int m_receiveBufferSize;// buffer size to use for each socket I/O operation 
        BufferManager m_bufferManager;  // represents a large reusable set of buffers for all socket operations
        const int opsToPreAlloc = 2;    // read, write (don't alloc buffer space for accepts)
        Socket listenSocket;            // the socket used to listen for incoming connection requests
        // pool of reusable SocketAsyncEventArgs objects for write, read and accept socket operations
        SocketAsyncEventArgsPool m_readWritePool;
        int m_totalBytesRead;           // counter of the total # bytes received by the server
        int m_numConnectedSockets;      // the total number of clients connected to the server 
        Semaphore m_maxNumberAcceptedClients;

        private const int NUMBUTTONS = 17;
        public bool[] pressed = new bool[NUMBUTTONS];

        private const int PMBIT = 72;                   // Select
        private const int CMBIT = 76;                   // Tri
        private const int CMUPBIT = 65;                 // Up
        private const int CMDOWNBIT = 64;               // Down
        private const int CMLEFTBIT = 79;               // Left
        private const int CMRIGHTBIT = 78;              // Right
        private const int CMHOLEBIT = 68;               // R1
        
        public int numSelects = 0;
        
        public int[][] analogLoc = new int[2][];
        public const int ANALOGMAX = 255;

        public int leader = 0;
        public int myport = -1;

        public bool wasPressedPM = false;
        public bool wasPressedCM = false;
        public bool wasPressedCMUP = false;
        public bool wasPressedCMDOWN = false;
        public bool wasPressedCMLEFT = false;
        public bool wasPressedCMRIGHT = false;
        public bool wasPressedCMHOLE = false;

        public Byte playerID;

        private MainInterface _myParent;

        String logFilePath;

        /// <summary>
        /// Create an uninitialized server instance.  To start the server listening for connection requests
        /// call the Init method followed by Start method 
        /// </summary>
        /// <param name="numConnections">the maximum number of connections the sample is designed to handle simultaneously</param>
        /// <param name="receiveBufferSize">buffer size to use for each socket I/O operation</param>
        public PlayerInputServer(int numConnections, int receiveBufferSize, MainInterface myParent, int myPlayerID)
        {
            _myParent = myParent;
            mode = _myParent.mode;
            playerID = BitConverter.GetBytes(myPlayerID)[0];
            logFilePath = "player_" + playerID + ".log";
            m_totalBytesRead = 0;
            m_numConnectedSockets = 0;
            m_numConnections = numConnections;
            m_receiveBufferSize = receiveBufferSize;
            // allocate buffers such that the maximum number of sockets can have one outstanding read and 
            //write posted to the socket simultaneously  
            m_bufferManager = new BufferManager(receiveBufferSize * numConnections * opsToPreAlloc,
                receiveBufferSize);

            m_readWritePool = new SocketAsyncEventArgsPool(numConnections);
            m_maxNumberAcceptedClients = new Semaphore(numConnections, numConnections);

            analogLoc[0] = new int[] { 0, 0 };
            analogLoc[1] = new int[] { 0, 0 };
        }

        /// <summary>
        /// Initializes the server by preallocating reusable buffers and context objects.  These objects do not 
        /// need to be preallocated or reused, by is done this way to illustrate how the API can easily be used
        /// to create reusable objects to increase server performance.
        /// </summary>
        public void Init()
        {
            // Allocates one large byte buffer which all I/O operations use a piece of.  This gaurds 
            // against memory fragmentation
            m_bufferManager.InitBuffer();

            // preallocate pool of SocketAsyncEventArgs objects
            SocketAsyncEventArgs readWriteEventArg;

            for (int i = 0; i < m_numConnections; i++)
            {
                //Pre-allocate a set of reusable SocketAsyncEventArgs
                readWriteEventArg = new SocketAsyncEventArgs();
                readWriteEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(IO_Completed);
                readWriteEventArg.UserToken = new AsyncUserToken();

                // assign a byte buffer from the buffer pool to the SocketAsyncEventArg object
                m_bufferManager.SetBuffer(readWriteEventArg);

                // add SocketAsyncEventArg to the pool
                m_readWritePool.Push(readWriteEventArg);
            }

        }

        /// <summary>
        /// Starts the server such that it is listening for incoming connection requests.    
        /// </summary>
        /// <param name="localEndPoint">The endpoint which the server will listening for conenction requests on</param>
        public void Start(IPEndPoint localEndPoint)
        {
            myport = localEndPoint.Port;
            // create the socket which listens for incoming connections
            listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listenSocket.Bind(localEndPoint);
            // start the server with a listen backlog of 100 connections
            listenSocket.Listen(100);

            // post accepts on the listening socket
            StartAccept(null);
        }


        /// <summary>
        /// Begins an operation to accept a connection request from the client 
        /// </summary>
        /// <param name="acceptEventArg">The context object to use when issuing the accept operation on the 
        /// server's listening socket</param>
        public void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArg_Completed);
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptEventArg.AcceptSocket = null;
            }

            m_maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
            }
        }

        /// <summary>
        /// This method is the callback method associated with Socket.AcceptAsync operations and is invoked
        /// when an accept operation is complete
        /// </summary>
        void AcceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            Interlocked.Increment(ref m_numConnectedSockets);

            // Get the socket for the accepted client connection and put it into the 
            //ReadEventArg object user token
            SocketAsyncEventArgs readEventArgs = m_readWritePool.Pop();
            if (readEventArgs == null)
            {
                return;
            }
            ((AsyncUserToken)readEventArgs.UserToken).Socket = e.AcceptSocket;

            // As soon as the client is connected, post a receive to the connection
            bool willRaiseEvent = e.AcceptSocket.ReceiveAsync(readEventArgs);
            if (!willRaiseEvent)
            {
                ProcessReceive(readEventArgs);
            }

            // Accept the next connection request
            StartAccept(e);
        }

        /// <summary>
        /// This method is called whenever a receive or send opreation is completed on a socket 
        /// </summary> 
        /// <param name="e">SocketAsyncEventArg associated with the completed receive operation</param>
        void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            // determine which type of operation just completed and call the associated handler
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive or send");
            }

        }

        IEnumerable<bool> GetBits(byte b)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return (b & 0x80) != 0;
                b *= 2;
            }
        }

        /// <summary>
        /// This method is invoked when an asycnhronous receive operation completes. If the 
        /// remote host closed the connection, then the socket is closed.  If data was received then
        /// the data is echoed back to the client.
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            // check if the remote host closed the connection
            AsyncUserToken token = (AsyncUserToken)e.UserToken;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                //increment the count of the total bytes receive by the server
                Interlocked.Add(ref m_totalBytesRead, e.BytesTransferred);

                //echo the data received back to the client
                BitArray tmpbits = new BitArray(e.Buffer);
                Byte[] sendBytes = new Byte[m_receiveBufferSize + 1];
                for (int i = 0; i < m_receiveBufferSize; i++)
                {
                    sendBytes[i] = e.Buffer[i];
                }
                sendBytes[m_receiveBufferSize] = playerID;

                byte[] sendBuffer = new byte[14];
                Array.Copy(sendBytes, 0, sendBuffer, 0, 12);
                sendBuffer[12] = playerID;

                // 1 = cmd mode, 2 = pause mode, 3 = both
                if (_myParent.players[playerID].cm && !_myParent.players[playerID].pm)
                {
                    sendBuffer[13] = (byte)2;
                }
                else if (!_myParent.players[playerID].cm && _myParent.players[playerID].pm)
                {
                    sendBuffer[13] = (byte)1;
                }
                else if (_myParent.players[playerID].cm && _myParent.players[playerID].pm)
                {
                    sendBuffer[13] = (byte)3;
                }
                else
                {
                    sendBuffer[13] = (byte)0;
                }

                lock (_myParent.bufferListLock)
                {
                    _myParent.bufferList.Add(sendBuffer);
                }

                MainInterface.mmClient.sendInputInfo();

                using (StreamWriter logWriter = new StreamWriter(logFilePath, true)) {
                    //logWriter.Write(DateTime.Now + "    ");
                    logWriter.Write((Environment.TickCount & Int32.MaxValue) + "    " + DateTime.Now + "    ");
                    logWriter.WriteLine(String.Concat(sendBuffer.Select(b => " " + Convert.ToString(b, 2).PadLeft(8,'0'))));
                }

                // Get x an y LAnalog and RAnalog
                analogLoc[0][0] = BitConverter.ToInt32(new byte[] { e.Buffer[1], 0, 0, 0 }, 0);
                analogLoc[0][1] = BitConverter.ToInt32(new byte[] { e.Buffer[3], 0, 0, 0 }, 0);
                analogLoc[1][0] = BitConverter.ToInt32(new byte[] { e.Buffer[5], 0, 0, 0 }, 0);
                analogLoc[1][1] = BitConverter.ToInt32(new byte[] { e.Buffer[7], 0, 0, 0 }, 0);

                // 80 means we get 2 bytes of button data
                for (int i = 64; i < 80; i++)
                {
                    pressed[i - 64] = tmpbits[i];
                }

                pressed[16] = tmpbits[80];

                // 80 + 8
                pressed[16] = tmpbits[88];

                leader = _myParent.leader;

                // select means PM for multi
                if (mode == MainInterface.MOB)
                {
                    if (!wasPressedCM && tmpbits[PMBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cm = !_myParent.players[playerID].cm;
                        }
                        System.Diagnostics.Trace.WriteLine("+sel");
                    }
                    else if (wasPressedCM && !tmpbits[PMBIT])
                    {
                        System.Diagnostics.Trace.WriteLine("-sel");
                    }

                    wasPressedCM = tmpbits[PMBIT];

                    if (!wasPressedCMUP && tmpbits[CMUPBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cmUp = true;
                        }
                        System.Diagnostics.Trace.WriteLine("+up");
                    }
                    else if (wasPressedCMUP && !tmpbits[CMUPBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cmUp = false;
                        }
                        System.Diagnostics.Trace.WriteLine("-up");
                    }
                    wasPressedCMUP = tmpbits[CMUPBIT];

                    if (!wasPressedCMDOWN && tmpbits[CMDOWNBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cmDown = true;
                        }
                        System.Diagnostics.Trace.WriteLine("+down");
                    }
                    else if (wasPressedCMDOWN && !tmpbits[CMDOWNBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cmDown = false;
                        }
                        System.Diagnostics.Trace.WriteLine("-down");
                    }
                    wasPressedCMDOWN = tmpbits[CMDOWNBIT];

                    if (!wasPressedCMLEFT && tmpbits[CMLEFTBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cmLeft = true;
                        }
                        System.Diagnostics.Trace.WriteLine("+up");
                    }
                    else if (wasPressedCMLEFT && !tmpbits[CMLEFTBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cmLeft = false;
                        }
                        System.Diagnostics.Trace.WriteLine("-up");
                    }
                    wasPressedCMLEFT = tmpbits[CMLEFTBIT];

                    if (!wasPressedCMRIGHT && tmpbits[CMRIGHTBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cmRight = true;
                        }
                        System.Diagnostics.Trace.WriteLine("+up");
                    }
                    else if (wasPressedCMRIGHT && !tmpbits[CMRIGHTBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cmRight = false;
                        }
                        System.Diagnostics.Trace.WriteLine("-up");
                    }
                    wasPressedCMRIGHT = tmpbits[CMRIGHTBIT];

                    if (!wasPressedCMHOLE && tmpbits[CMHOLEBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cmHole = !_myParent.players[playerID].cmHole;
                        }

                        System.Diagnostics.Trace.WriteLine("+r1");
                    }
                    else if (wasPressedCMHOLE && !tmpbits[CMHOLEBIT])
                    {
                        System.Diagnostics.Trace.WriteLine("-r1");
                    }

                    wasPressedCMHOLE = tmpbits[CMHOLEBIT];

                // Check for selects
                } else if (mode == MainInterface.LEADER)
                {
                    if (!wasPressedPM && tmpbits[PMBIT])
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].pm = !_myParent.players[playerID].pm;
                        }

                        System.Diagnostics.Trace.WriteLine("+sel");
                        numSelects++;
                    }
                    else if (wasPressedPM && !tmpbits[PMBIT])
                    {
                        System.Diagnostics.Trace.WriteLine("-sel");
                    }

                    wasPressedPM = tmpbits[PMBIT];

                    if (leader == playerID)
                    {
                        lock (_myParent.players[playerID])
                        {
                            _myParent.players[playerID].cm = false;
                            _myParent.players[playerID].cmUp = false;
                            _myParent.players[playerID].cmDown = false;
                            _myParent.players[playerID].cmLeft = false;
                            _myParent.players[playerID].cmRight = false;
                        }
                    }
                    else
                    {
                        if (!wasPressedCM && tmpbits[CMBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cm = !_myParent.players[playerID].cm;
                            }

                            System.Diagnostics.Trace.WriteLine("+tri");
                        }
                        else if (wasPressedCM && !tmpbits[CMBIT])
                        {
                            System.Diagnostics.Trace.WriteLine("-tri");
                        }

                        wasPressedCM = tmpbits[CMBIT];

                        if (!wasPressedCMUP && tmpbits[CMUPBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cmUp = true;
                            }
                            System.Diagnostics.Trace.WriteLine("+up");
                        }
                        else if (wasPressedCMUP && !tmpbits[CMUPBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cmUp = false;
                            }
                            System.Diagnostics.Trace.WriteLine("-up");
                        }
                        wasPressedCMUP = tmpbits[CMUPBIT];

                        if (!wasPressedCMDOWN && tmpbits[CMDOWNBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cmDown = true;
                            }
                            System.Diagnostics.Trace.WriteLine("+down");
                        }
                        else if (wasPressedCMDOWN && !tmpbits[CMDOWNBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cmDown = false;
                            }
                            System.Diagnostics.Trace.WriteLine("-down");
                        }
                        wasPressedCMDOWN = tmpbits[CMDOWNBIT];

                        if (!wasPressedCMLEFT && tmpbits[CMLEFTBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cmLeft = true;
                            }
                            System.Diagnostics.Trace.WriteLine("+up");
                        }
                        else if (wasPressedCMLEFT && !tmpbits[CMLEFTBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cmLeft = false;
                            }
                            System.Diagnostics.Trace.WriteLine("-up");
                        }
                        wasPressedCMLEFT = tmpbits[CMLEFTBIT];

                        if (!wasPressedCMRIGHT && tmpbits[CMRIGHTBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cmRight = true;
                            }
                            System.Diagnostics.Trace.WriteLine("+up");
                        }
                        else if (wasPressedCMRIGHT && !tmpbits[CMRIGHTBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cmRight = false;
                            }
                            System.Diagnostics.Trace.WriteLine("-up");
                        }
                        wasPressedCMRIGHT = tmpbits[CMRIGHTBIT];

                        if (!wasPressedCMHOLE && tmpbits[CMHOLEBIT])
                        {
                            lock (_myParent.players[playerID])
                            {
                                _myParent.players[playerID].cmHole = !_myParent.players[playerID].cmHole;
                            }

                            System.Diagnostics.Trace.WriteLine("+r1");
                        }
                        else if (wasPressedCMHOLE && !tmpbits[CMHOLEBIT])
                        {
                            System.Diagnostics.Trace.WriteLine("-r1");
                        }

                        wasPressedCMHOLE = tmpbits[CMHOLEBIT];
                    }

                }
                CloseClientSocket(e);
            }
            else
            {
                Console.WriteLine("CloseRec: " + e.BytesTransferred + "    " + e.SocketError);
                CloseClientSocket(e);
            }
        }

        /// <summary>
        /// This method is invoked when an asynchronous send operation completes.  The method issues another receive
        /// on the socket to read any additional data sent from the client
        /// </summary>
        /// <param name="e"></param>
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                // done echoing data back to the client
                AsyncUserToken token = (AsyncUserToken)e.UserToken;
                // read the next block of data send from the client
                /*bool willRaiseEvent = token.Socket.ReceiveAsync(e);
                if (!willRaiseEvent)
                {
                    ProcessReceive(e);
                }*/
            }
            else
            {
                Console.WriteLine("CloseSend: " + e.SocketError);
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            AsyncUserToken token = e.UserToken as AsyncUserToken;

            // close the socket associated with the client
            try
            {
                //token.Socket.Shutdown(SocketShutdown.Send);
            }
            // throws if client process has already closed
            catch (Exception) { }
            token.Socket.Close();

            // decrement the counter keeping track of the total number of clients connected to the server
            Interlocked.Decrement(ref m_numConnectedSockets);
            m_maxNumberAcceptedClients.Release();

            // Free the SocketAsyncEventArg so they can be reused by another client
            m_readWritePool.Push(e);
        }

    }
}
