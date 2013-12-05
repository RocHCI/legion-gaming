using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Timers;
using System.Net;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Net.Sockets;
using System.Collections;
using System.Media;
using System.IO;
using System.Reflection;

namespace GamingInterface
{
    public partial class MainInterface : Form
    {
        // LEADER shows starts, PM, and CM
        public const int LEADER = 0;
        // MOB doesn't show any of these
        public const int MOB = 3;

        public const int STARH = 50;
        public const int STARSH = 20;

        //-----------------///////////// CHANGE PER BLOCK /////////////-----------------//
        // Canvas size: 3500,1085
        // Should be 1 for solo
        public const int numPlayers = 4;
        public int mode = MOB;
        public bool NOOP = false;
        
        //////////////////////----------------------------------//////////////////////////

        public Player[] players = new Player[numPlayers];

        public const int numButtons = 17;
        public List<int>[] buttonOwners = new List<int>[numButtons];

        public SoundPlayer pingSound = new SoundPlayer(@"d:\legion-gaming\cmping.wav");

        public const int L1 = 7;
        public const int L2 = 6;
        public const int L3 = 5;
        public const int R1 = 4;
        public const int R2 = 3;
        public const int R3 = 2;
        public const int UP = 1;
        public const int DOWN = 0;
        public const int LEFT = 15;
        public const int RIGHT = 14;
        public const int SQR = 13;
        public const int TRI = 12;
        public const int CIR = 11;
        public const int X = 10;
        public const int START = 9;
        public const int SELECT = 8;
        public const int PS = 16;

        /***** Legion *****/
        public int leader = 0;
        private int leaderAlpha = 0;
        public double leaderPer = 0;
        public int[] starLoc = new int[] { 550, 1000 };

        public bool drawCMStar = false;

        public Point CMUPPOS = new Point(900,100);
        public Point CMDOWNPOS = new Point(900,750);
        public Point CMLEFTPOS = new Point(100,500);
        public Point CMRIGHTPOS = new Point(1625, 500);
        public const int CMARROWMULTI = 25;
        public const int CMARROWSIZE = 100;

        public const int CMSMALLER = SQR;
        public const int CMBIGGER = CIR;
        public const int CMMARK = X;
        public const int CMINITSIZE = 50;
        public const int CMSIZESPEED = 5;
        public const int CMWAVESIZEMULTI = 3;
        public const int CMWAVESPEED = 10;
        public const double CMMOVESPEED = 0.25;
        public const int CMMINX = 0;
        public const int CMMAXX = 1920;
        public const int CMMINY = 0;
        public const int CMMAXY = 1085;
        public const int CMXOFFSET = 550;
        public const int CMXMULTISPACE = 250;
        public const int CMYOFFSET = 500;
        public const int CMMIND = 1;
        public const int CMMAXD = 100;
        // Account for fluctuations
        public const int CMSENSITIVITY = 10;

        public const int PMTOGGLE = SELECT;

        // Change this depending on if using timer for leader
        public const bool timed = true;

        public const int leaderVoteOffsetX = 1160;
        public const int leaderVoteOffsetY = 0;

        public const int leaderNameOffsetY = 0;
        public const int leaderNameLength = 290;

        public static PlayerInputServer[] servers = new PlayerInputServer[numPlayers];
        public static MMServer mmServer;
        public static MMClient mmClient;
        public Object bufferListLock = new Object();
        public List<Byte[]> bufferList = new List<Byte[]>();

        public const int playerOffsetXmult = 250;
        public const int playerInitOffset = 2300;

        // Milliseconds
        public const int colorChangeTime = 50;
        public const int colorChangeSpeed = 10;

        private int lastLeader = -1;

        public MainInterface()
        {
            System.Drawing.Color[] playerColors = new System.Drawing.Color[] {System.Drawing.Color.Blue,
                                                                                        System.Drawing.Color.Red,
                                                                                        System.Drawing.Color.Green,
                                                                                        System.Drawing.Color.Magenta};

            System.Drawing.Color[] playerColorsDark = new System.Drawing.Color[] {System.Drawing.Color.DarkBlue,
                                                                                            System.Drawing.Color.DarkRed,
                                                                                            System.Drawing.Color.DarkGreen,
                                                                                            System.Drawing.Color.DarkMagenta};
            int offsetY = 500;
            System.Drawing.Color myPressedColor = System.Drawing.Color.Yellow;

            Bitmap[] playerBacksImage = new Bitmap[] { Properties.Resources.controllerCenterBlue, 
                                                        Properties.Resources.controllerCenterRed,
                                                        Properties.Resources.controllerCenterGreen, 
                                                        Properties.Resources.controllerCenterPurple };
            Bitmap[] playerBackTopsLImage = new Bitmap[] { Properties.Resources.controllerTopLBlue, 
                                                            Properties.Resources.controllerTopLRed,
                                                            Properties.Resources.controllerTopLGreen, 
                                                            Properties.Resources.controllerTopLPurple };
            Bitmap[] playerBackTopsRImage = new Bitmap[] { Properties.Resources.controllerTopRBlue, 
                                                            Properties.Resources.controllerTopRRed,
                                                            Properties.Resources.controllerTopRGreen, 
                                                            Properties.Resources.controllerTopRPurple };

            for (int i = 0; i < numPlayers; i++)
            {
                // Each player needs her own button array because player-button pressedAlpha values are unique
                Button[] buttons = new Button[numButtons];
                for (int j = 0; j < numButtons; j++)
                {
                    buttons[j] = new Button(Color.White);
                    buttons[j].pressedColor = myPressedColor;
                }
                buttons[L1].loc = new Point(2, 30);
                buttons[L2].loc = new Point(5, 5);
                buttons[L3].loc = new Point(55, 66);
                buttons[R1].loc = new Point(2, 30);
                buttons[R2].loc = new Point(5, 5);
                buttons[R3].loc = new Point(119, 66);
                buttons[UP].loc = new Point(34, 37);
                buttons[DOWN].loc = new Point(34, 55);
                buttons[LEFT].loc = new Point(26, 46);
                buttons[RIGHT].loc = new Point(44, 46);
                buttons[SQR].loc = new Point(141, 44);
                buttons[TRI].loc = new Point(159, 27);
                buttons[CIR].loc = new Point(177, 44);
                buttons[X].loc = new Point(159, 60);
                buttons[START].loc = new Point(118, 48);
                buttons[SELECT].loc = new Point(81, 49);
                buttons[PS].loc = new Point(97, 57);

                players[i] = new Player(i, "PLAYER " + i, playerColors[i], playerColorsDark[i], new Point(playerInitOffset + playerOffsetXmult * i,offsetY), playerBacksImage[i], playerBackTopsLImage[i], playerBackTopsRImage[i], buttons, i, new Point(CMXOFFSET + CMXMULTISPACE * i, CMYOFFSET), CMINITSIZE);

                // If rectangle or ellipse, second point denotes sizes
                // Otherwise, a series of points that define the polygon
                players[i].buttons[L1].points = new Point[] {new Point(players[i].buttons[L1].loc.X, players[i].buttons[L1].loc.Y),
                                                             new Point(30, 14)};
                players[i].buttons[L2].points = new Point[] {new Point(players[i].buttons[L2].loc.X, players[i].buttons[L2].loc.Y),
                                                             new Point(28, 14)};
                players[i].buttons[L3].points = new Point[] {new Point(players[i].buttons[L3].loc.X, players[i].buttons[L3].loc.Y),
                                                             new Point(35, 35)};
                players[i].buttons[R1].points = new Point[] {new Point(players[i].buttons[R1].loc.X, players[i].buttons[R1].loc.Y),
                                                             new Point(30, 14)};
                players[i].buttons[R2].points = new Point[] {new Point(players[i].buttons[R2].loc.X, players[i].buttons[R2].loc.Y),
                                                             new Point(28, 14)};
                players[i].buttons[R3].points = new Point[] {new Point(players[i].buttons[R3].loc.X, players[i].buttons[R3].loc.Y),
                                                             new Point(35, 35)};

                players[i].buttons[UP].points = new Point[] { new Point(players[i].buttons[UP].loc.X + 7, players[i].buttons[UP].loc.Y + 14), 
                                                              new Point(players[i].buttons[UP].loc.X, players[i].buttons[UP].loc.Y), 
                                                              new Point(players[i].buttons[UP].loc.X + 14, players[i].buttons[UP].loc.Y)};
                players[i].buttons[DOWN].points = new Point[] { new Point(players[i].buttons[DOWN].loc.X + 7, players[i].buttons[DOWN].loc.Y), 
                                                                new Point(players[i].buttons[DOWN].loc.X, players[i].buttons[DOWN].loc.Y + 14), 
                                                                new Point(players[i].buttons[DOWN].loc.X + 14, players[i].buttons[DOWN].loc.Y + 14)};
                players[i].buttons[LEFT].points = new Point[] { new Point(players[i].buttons[LEFT].loc.X + 14, players[i].buttons[LEFT].loc.Y + 7), 
                                                                new Point(players[i].buttons[LEFT].loc.X, players[i].buttons[LEFT].loc.Y), 
                                                                new Point(players[i].buttons[LEFT].loc.X, players[i].buttons[LEFT].loc.Y + 14)};
                players[i].buttons[RIGHT].points = new Point[] { new Point(players[i].buttons[RIGHT].loc.X, players[i].buttons[RIGHT].loc.Y + 7), 
                                                                 new Point(players[i].buttons[RIGHT].loc.X + 14, players[i].buttons[RIGHT].loc.Y), 
                                                                 new Point(players[i].buttons[RIGHT].loc.X + 14, players[i].buttons[RIGHT].loc.Y + 14)};
                players[i].buttons[START].points = new Point[] { new Point(players[i].buttons[START].loc.X, players[i].buttons[START].loc.Y), 
                                                                 new Point(players[i].buttons[START].loc.X + 15, players[i].buttons[START].loc.Y + 5), 
                                                                 new Point(players[i].buttons[START].loc.X, players[i].buttons[START].loc.Y + 10)};

                players[i].buttons[SQR].points = new Point[] {new Point(players[i].buttons[SQR].loc.X, players[i].buttons[SQR].loc.Y),
                                                             new Point(18, 18)};
                players[i].buttons[TRI].points = new Point[] {new Point(players[i].buttons[TRI].loc.X, players[i].buttons[TRI].loc.Y),
                                                             new Point(18, 18)};
                players[i].buttons[CIR].points = new Point[] {new Point(players[i].buttons[CIR].loc.X, players[i].buttons[CIR].loc.Y),
                                                             new Point(18, 18)};
                players[i].buttons[X].points = new Point[] {new Point(players[i].buttons[X].loc.X, players[i].buttons[X].loc.Y),
                                                             new Point(18, 18)};

                players[i].buttons[SELECT].points = new Point[] {new Point(players[i].buttons[SELECT].loc.X, players[i].buttons[SELECT].loc.Y),
                                                             new Point(10, 7)};

                players[i].buttons[PS].points = new Point[] {new Point(players[i].buttons[PS].loc.X, players[i].buttons[PS].loc.Y),
                                                             new Point(15, 15)};
            }

            InitializeComponent();


            /***** Socket ******/
            int numConnections = 1;
            int receiveSize = 12;

            IPEndPoint[] localEndPoints = new IPEndPoint[] {new IPEndPoint(IPAddress.Any, 4141),
                                                            new IPEndPoint(IPAddress.Any, 4242),
                                                            new IPEndPoint(IPAddress.Any, 4343),
                                                            new IPEndPoint(IPAddress.Any, 4444)};
            Console.WriteLine("Let's set up servers.");
            for(int i = 0; i < numPlayers; i++) 
            {
                servers[i] = new PlayerInputServer(numConnections, receiveSize,this,i);
                servers[i].Init();
                servers[i].Start(localEndPoints[i]);
                // There's a race condition if we don't put a pause here of some sort
                // Might as well print something useful
                Console.WriteLine("SERVER " + i);
            }

            mmServer = new MMServer(numConnections, 50);
            mmServer.Init();
            mmServer.Start(new IPEndPoint(IPAddress.Any, 7777));

            Thread.Sleep(1000);

            mmClient = new MMClient(this);
        }

        private void MainInterface_Load(object sender, EventArgs e)
        {
            // Draw player boxes
            for (int i = 0; i < numPlayers; i++)
            {
                players[i].center = new PictureBox();
                players[i].center.Location = new Point(players[i].loc.X, players[i].loc.Y + 51);
                // Ratio is 1000/600
                players[i].center.Image = players[i].centerImage;
                players[i].center.Size = new Size(208, 125);
                players[i].center.SizeMode = PictureBoxSizeMode.StretchImage;
                players[i].center.Paint += new System.Windows.Forms.PaintEventHandler(playerCenter_Paint);
                Controls.Add(players[i].center);

                players[i].topL = new PictureBox();
                players[i].topL.Location = new Point(players[i].loc.X + 28, players[i].loc.Y);
                // Ratio is 400/117, 76/111
                players[i].topL.Image = players[i].topLImage;
                players[i].topL.Size = new Size(35, 51);
                players[i].topL.SizeMode = PictureBoxSizeMode.StretchImage;
                players[i].topL.Paint += new System.Windows.Forms.PaintEventHandler(playerTopL_Paint);
                Controls.Add(players[i].topL);

                players[i].topR = new PictureBox();
                players[i].topR.Location = new Point(players[i].loc.X + 150, players[i].loc.Y);
                // Ratio is 400/117, 76/111
                players[i].topR.Image = players[i].topRImage;
                players[i].topR.Size = new Size(35, 51);
                players[i].topR.SizeMode = PictureBoxSizeMode.StretchImage;
                players[i].topR.Paint += new System.Windows.Forms.PaintEventHandler(playerTopR_Paint);
                Controls.Add(players[i].topR);
            }

            System.Windows.Forms.Timer secondTimer = new System.Windows.Forms.Timer();
            secondTimer.Tick += new EventHandler(second_OnTimedEvent);
            secondTimer.Interval = 1000;
            secondTimer.Start();

            System.Windows.Forms.Timer fadeTimer = new System.Windows.Forms.Timer();
            fadeTimer.Tick += new EventHandler(Fade_OnTimedEvent);
            fadeTimer.Interval = colorChangeTime;
            fadeTimer.Start();
            /*
            Console.WriteLine(Environment.CurrentDirectory);
            ButtonAllocationPanel myAllocation = new ButtonAllocationPanel(this, Environment.CurrentDirectory + "/config/buttonConfig.ini");
            Controls.Add(myAllocation);
            */
        }

        private void second_OnTimedEvent(Object myObject, EventArgs myEventArgs)
        {
            leaderPer = mmServer.leaderTime;
            leader = mmServer.leader;

            if (mode != MOB && leader != lastLeader)
            {
                leaderAlpha = 255;
            }
            else
            {
                leaderAlpha = 0;
            }
            lastLeader = leader;
        }

        private void Fade_OnTimedEvent(Object myObject, EventArgs myEventArgs)
        {
            for (int i = 0; i < numPlayers; i++)
            {
                if (players[i].cm)
                {
                    if (servers[i].pressed[CMSMALLER] && players[i].cmSize - CMSIZESPEED >= CMMIND)
                    {
                        players[i].cmSize -= CMSIZESPEED;
                    }
                    if (servers[i].pressed[CMBIGGER] && players[i].cmSize + CMSIZESPEED <= CMMAXD)
                    {
                        players[i].cmSize += CMSIZESPEED;
                    }
                    if (servers[i].pressed[CMMARK])
                    {
                        players[i].cmWaveSize = players[i].cmSize * CMWAVESIZEMULTI;
                        pingSound.Play();
                    }

                    if (players[i].cmWaveSize - CMWAVESPEED <= 0)
                    {
                        players[i].cmWaveSize = 0;
                    } else {
                        players[i].cmWaveSize -= CMWAVESPEED;
                    }

                    // Center is at 128
                    // If not just random fluctuations
                    if (servers[i].analogLoc[0][0] - 128 < -CMSENSITIVITY || CMSENSITIVITY < servers[i].analogLoc[0][0] - 128)
                    {
                        players[i].cmLoc.X += Convert.ToInt32((servers[i].analogLoc[0][0] - 128) * CMMOVESPEED);
                        players[i].cmLoc.Y += Convert.ToInt32((servers[i].analogLoc[0][1] - 128) * CMMOVESPEED);

                        if (players[i].cmLoc.X <= CMMINX)
                        {
                            players[i].cmLoc.X = CMMINX;
                        }
                        else if (players[i].cmLoc.X + players[i].cmSize / 2 >= CMMAXX)
                        {
                            players[i].cmLoc.X = CMMAXX - players[i].cmSize / 2;
                        }

                        if (players[i].cmLoc.Y <= CMMINX)
                        {
                            players[i].cmLoc.Y = CMMINX;
                        }
                        else if (players[i].cmLoc.Y + players[i].cmSize / 2 >= CMMAXY)
                        {
                            players[i].cmLoc.Y = CMMAXY - players[i].cmSize / 2;
                        }

                    }
                }


                for (int j = 0; j < numButtons; j++)
                {
                    if (servers[i].pressed[j])
                    {
                        players[i].buttons[j].pressedAlpha = 100;
                        servers[i].pressed[j] = false;
                    }
                    // Note that this is an "else if" so we get pressedAlpha at 100 at least once
                    else if (players[i].buttons[j].pressedAlpha - colorChangeSpeed <= 0)
                    {
                        players[i].buttons[j].pressedAlpha = 0;
                    }
                    else
                    {
                        players[i].buttons[j].pressedAlpha -= colorChangeSpeed;
                    }
                }
            }

            Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (mode != MOB && mmServer.leader >= 0)
            {
                leader = mmServer.leader;
                leaderPer = mmServer.leaderTime;
            }
            else
            {
                leader = 0;
            }

            Graphics g = e.Graphics;

            // For testing
            //Pen TestPen = new Pen(System.Drawing.Color.Black, 2);
            //g.DrawLine(TestPen, new Point(CMMINX,CMMINY), new Point(CMMINX,CMMAXY));
            //TestPen.Dispose();

            // Draw leader name if new leader
            if (leaderAlpha > 0)
            {
                System.Drawing.SolidBrush leaderBGBrush = new System.Drawing.SolidBrush(Color.White);
                g.FillRectangle(leaderBGBrush, new Rectangle(players[leader].center.Location.X + (players[leader].center.Width / 2) - (leaderNameLength / 2), leaderNameOffsetY, leaderNameLength, 75));
                leaderBGBrush.Dispose();

                System.Drawing.Font leaderFont = new System.Drawing.Font("Sylfaen", 46);
                System.Drawing.SolidBrush leaderTextBrush = new
                System.Drawing.SolidBrush(players[leader].pColor);
                g.DrawString(players[leader].name, leaderFont, leaderTextBrush, players[leader].center.Location.X + (players[leader].center.Width / 2) - (leaderNameLength / 2), leaderNameOffsetY);
                leaderFont.Dispose();
                leaderTextBrush.Dispose();
            }

            if (!NOOP)
            {
                // Draw star
                // Taken from Andrey Butov's C# Star Rating Control (The Code Project)
                //STARSTAR
                for (int i = 0; i < numPlayers; i++)
                {
                    if (mode != MOB)
                    {
                        if (i == leader)
                        {
                            Rectangle rect = new Rectangle(starLoc[0] + i * playerOffsetXmult, starLoc[1], STARH, STARH);
                            Brush fillBrush = new LinearGradientBrush(rect, System.Drawing.Color.White, System.Drawing.Color.Orange, LinearGradientMode.ForwardDiagonal);
                            Pen outlinePen = new Pen(System.Drawing.Color.DarkOrange, 2);
                            drawStar(g, fillBrush, outlinePen, rect);
                            fillBrush.Dispose();

                            if (timed)
                            {
                                // Make star dissapear as time runs out
                                //Transparent
                                System.Drawing.SolidBrush myBrushTransparent = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, 192, 255));
                                g.FillRectangle(myBrushTransparent, new Rectangle(starLoc[0] + leader * playerOffsetXmult, starLoc[1] - 2 - Convert.ToInt32((STARH + 2) * leaderPer), STARH + 4, STARH + 4));
                                myBrushTransparent.Dispose();
                            }
                        }

                        else
                        {
                            // Make gray stars
                            Rectangle rect = new Rectangle(starLoc[0] + i * playerOffsetXmult, starLoc[1], STARH, STARH);
                            Brush fillBrush = new LinearGradientBrush(rect, System.Drawing.Color.White, System.Drawing.Color.Black, LinearGradientMode.ForwardDiagonal);
                            Pen outlinePen = new Pen(System.Drawing.Color.Gray, 2);
                            drawStar(g, fillBrush, outlinePen, rect);
                            fillBrush.Dispose();
                        }


                    }

                    if (players[i].pm)
                    {
                        // Make black stars
                        Rectangle rect = new Rectangle(starLoc[0] + i * playerOffsetXmult, starLoc[1], STARH, STARH);
                        Brush fillBrush = new LinearGradientBrush(rect, System.Drawing.Color.Black, System.Drawing.Color.Black, LinearGradientMode.ForwardDiagonal);
                        Pen outlinePen = new Pen(System.Drawing.Color.Gray, 2);
                        drawStar(g, fillBrush, outlinePen, rect);
                        fillBrush.Dispose();
                    }

                    drawCMStar = false;

                    if (players[i].cm)
                    {
                        drawCMStar = true;

                        // Draw marker
                        Rectangle markerRec = new Rectangle(players[i].cmLoc.X, players[i].cmLoc.Y, players[i].cmSize, players[i].cmSize);
                        if (players[i].cmHole)
                        {
                            Pen CMMOutlinePen = new Pen(Color.White, 7);
                            g.DrawEllipse(CMMOutlinePen, markerRec);
                            CMMOutlinePen.Dispose();

                            Pen CMMPen = new Pen(players[i].pColor, 3);
                            g.DrawEllipse(CMMPen, markerRec);
                            CMMPen.Dispose();
                        }
                        else
                        {
                            System.Drawing.SolidBrush CMMBrush = new System.Drawing.SolidBrush(players[i].pColorDark);
                            g.FillEllipse(CMMBrush, markerRec);
                            CMMBrush.Dispose();

                            Pen CMMOutlinePen = new Pen(Color.White, 5);
                            g.DrawEllipse(CMMOutlinePen, markerRec);
                            CMMOutlinePen.Dispose();
                        }

                        // Draw waves
                        Pen CMWPen = new Pen(players[i].pColor, 5);
                        g.DrawEllipse(CMWPen, new Rectangle(players[i].cmLoc.X + players[i].cmSize / 2 - (players[i].cmWaveSize / 2), players[i].cmLoc.Y + players[i].cmSize / 2 - (players[i].cmWaveSize / 2), players[i].cmWaveSize, players[i].cmWaveSize));
                        CMWPen.Dispose();

                        // Draw arrows
                        if (players[i].cmLeft)
                        {
                            drawCMStar = true;

                            Point[] cmLeftPts = new Point[] { new Point(CMLEFTPOS.X + CMARROWMULTI * i, CMLEFTPOS.Y + CMARROWSIZE / 2), new Point(CMLEFTPOS.X + CMARROWSIZE + CMARROWMULTI * i, CMLEFTPOS.Y), new Point(CMLEFTPOS.X + CMARROWSIZE + CMARROWMULTI * i, CMLEFTPOS.Y + CMARROWSIZE) };

                            SolidBrush CMLeftBrush = new System.Drawing.SolidBrush(players[i].pColorDark);
                            g.FillPolygon(CMLeftBrush, cmLeftPts);
                            CMLeftBrush.Dispose();

                            Pen CMLeftPen = new Pen(Color.White, 10);
                            g.DrawPolygon(CMLeftPen, cmLeftPts);
                            CMLeftPen.Dispose();
                        }

                        if (players[i].cmRight)
                        {
                            drawCMStar = true;

                            Point[] cmRightPts = new Point[] { new Point(CMRIGHTPOS.X + CMARROWMULTI * (numPlayers - i), CMRIGHTPOS.Y), new Point(CMRIGHTPOS.X + CMARROWMULTI * (numPlayers - i), CMRIGHTPOS.Y + CMARROWSIZE), new Point(CMRIGHTPOS.X + CMARROWSIZE + CMARROWMULTI * (numPlayers - i), CMRIGHTPOS.Y + CMARROWSIZE / 2) };

                            SolidBrush CMRightBrush = new System.Drawing.SolidBrush(players[i].pColorDark);
                            g.FillPolygon(CMRightBrush, cmRightPts);
                            CMRightBrush.Dispose();

                            Pen CMRightPen = new Pen(Color.White, 10);
                            g.DrawPolygon(CMRightPen, cmRightPts);
                            CMRightPen.Dispose();
                        }

                        if (players[i].cmUp)
                        {
                            drawCMStar = true;

                            Point[] cmUpPts = new Point[] { new Point(CMUPPOS.X + CMARROWSIZE / 2, CMUPPOS.Y + CMARROWMULTI * i), new Point(CMUPPOS.X, CMUPPOS.Y + CMARROWSIZE + CMARROWMULTI * i), new Point(CMUPPOS.X + CMARROWSIZE, CMUPPOS.Y + CMARROWSIZE + CMARROWMULTI * i) };

                            SolidBrush CMUpBrush = new System.Drawing.SolidBrush(players[i].pColorDark);
                            g.FillPolygon(CMUpBrush, cmUpPts);
                            CMUpBrush.Dispose();

                            Pen CMUpPen = new Pen(Color.White, 10);
                            g.DrawPolygon(CMUpPen, cmUpPts);
                            CMUpPen.Dispose();
                        }

                        if (players[i].cmDown)
                        {
                            drawCMStar = true;

                            Point[] cmDownPts = new Point[] { new Point(CMDOWNPOS.X + CMARROWSIZE / 2, CMDOWNPOS.Y + CMARROWSIZE + CMARROWMULTI * (numPlayers - i)), new Point(CMDOWNPOS.X, CMDOWNPOS.Y + CMARROWMULTI * (numPlayers - i)), new Point(CMDOWNPOS.X + CMARROWSIZE, CMDOWNPOS.Y + CMARROWMULTI * (numPlayers - i)) };

                            SolidBrush CMDownBrush = new System.Drawing.SolidBrush(players[i].pColorDark);
                            g.FillPolygon(CMDownBrush, cmDownPts);
                            CMDownBrush.Dispose();

                            Pen CMDownPen = new Pen(Color.White, 10);
                            g.DrawPolygon(CMDownPen, cmDownPts);
                            CMDownPen.Dispose();
                        }

                        if (drawCMStar)
                        {
                            // Draw star
                            Rectangle rectCM = new Rectangle(starLoc[0] + i * playerOffsetXmult + STARH / 4, starLoc[1] + STARH / 4, STARH / 2, STARH / 2);
                            Brush fillBrushCM;
                            fillBrushCM = new LinearGradientBrush(rectCM, System.Drawing.Color.White, players[i].pColor, LinearGradientMode.ForwardDiagonal);
                            Pen outlinePen = new Pen(players[i].pColorDark, 2);
                            drawStar(g, fillBrushCM, outlinePen, rectCM);
                            fillBrushCM.Dispose();
                        }
                    }

                    

                }
            }
            base.OnPaint(e);
        }

        private void drawRect(Graphics g, int player, int button)
        {
            SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(50, players[player].buttons[button].bColor.R, players[player].buttons[button].bColor.G, players[player].buttons[button].bColor.B));
            g.FillRectangle(myBrush, new Rectangle(players[player].buttons[button].loc.X, players[player].buttons[button].loc.Y, players[player].buttons[button].points[1].X, players[player].buttons[button].points[1].Y));
            myBrush.Dispose();
            if (players[player].buttons[button].pressedAlpha > 0)
            {
                System.Drawing.SolidBrush myBrushP = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(players[player].buttons[button].pressedAlpha, players[player].buttons[button].pressedColor.R, players[player].buttons[button].pressedColor.G, players[player].buttons[button].pressedColor.B));
                g.FillRectangle(myBrushP, new Rectangle(players[player].buttons[button].loc.X, players[player].buttons[button].loc.Y, players[player].buttons[button].points[1].X, players[player].buttons[button].points[1].Y));
                myBrushP.Dispose();
            }
        }

        private void drawCirc(Graphics g, int player, int button)
        {
            SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(50, players[player].buttons[button].bColor.R, players[player].buttons[button].bColor.G, players[player].buttons[button].bColor.B));
            g.FillEllipse(myBrush, new Rectangle(players[player].buttons[button].loc.X, players[player].buttons[button].loc.Y, players[player].buttons[button].points[1].X, players[player].buttons[button].points[1].Y));
            myBrush.Dispose();
            if (players[player].buttons[button].pressedAlpha > 0)
            {
                System.Drawing.SolidBrush myBrushP = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(players[player].buttons[button].pressedAlpha, players[player].buttons[button].pressedColor.R, players[player].buttons[button].pressedColor.G, players[player].buttons[button].pressedColor.B));
                g.FillEllipse(myBrushP, new Rectangle(players[player].buttons[button].loc.X, players[player].buttons[button].loc.Y, players[player].buttons[button].points[1].X, players[player].buttons[button].points[1].Y));
                myBrushP.Dispose();
            }
        }

        private void drawPoly(Graphics g, int player, int button)
        {
            SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(50, players[player].buttons[button].bColor.R, players[player].buttons[button].bColor.G, players[player].buttons[button].bColor.B));
            g.FillPolygon(myBrush, players[player].buttons[button].points);
            if (players[player].buttons[button].pressedAlpha > 0)
            {
                System.Drawing.SolidBrush myBrushP = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(players[player].buttons[button].pressedAlpha, players[player].buttons[button].pressedColor.R, players[player].buttons[button].pressedColor.G, players[player].buttons[button].pressedColor.B));
                g.FillPolygon(myBrushP, players[player].buttons[button].points);
                myBrushP.Dispose();
            }
        }

        private void button_Paint(Graphics g, int player, int button)
        {
            // Rectangle
            if(button == L1 || button == L2 || button == R1 || button == R2 || button == SELECT) {
                drawRect(g, player, button);
            }
            // Ellipse
            else if (button == L3 || button == R3 || button == SQR || button == TRI || button ==  CIR || button == X || button == PS)
            {
                drawCirc(g, player, button);
            }
            // Polygon
            else if (button == UP || button == DOWN || button == LEFT || button == RIGHT || button == START)
            {
                drawPoly(g, player, button);
            }
        }

        private void playerTopL_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Hack b/c can't pass arguments
            int player = (((PictureBox)sender).Location.X - playerInitOffset) / playerOffsetXmult;

            button_Paint(e.Graphics, player, L1);
            button_Paint(e.Graphics, player, L2);
        }

        private void playerTopR_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Hack b/c can't pass arguments
            int player = (((PictureBox)sender).Location.X - playerInitOffset) / playerOffsetXmult;

            button_Paint(e.Graphics, player, R1);
            button_Paint(e.Graphics, player, R2);
        }

        private void playerCenter_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Hack b/c can't pass arguments
            int player = (((PictureBox)sender).Location.X - playerInitOffset) / playerOffsetXmult;
            Graphics g = e.Graphics;

            button_Paint(g, player, L3);
            button_Paint(g, player, R3);
            button_Paint(g, player, UP);
            button_Paint(g, player, DOWN);
            button_Paint(g, player, LEFT);
            button_Paint(g, player, RIGHT);
            button_Paint(g, player, SQR);
            button_Paint(g, player, TRI);
            button_Paint(g, player, CIR);
            button_Paint(g, player, X);
            button_Paint(g, player, START);
            button_Paint(g, player, SELECT);
            button_Paint(g, player, PS);

            // Diameter of analog stick image / Max possible value of position
            double multi = 35.0 / 255.0;
            // For each analog stick, draw line from middle of analog stick to offset defined by controller analog position
            Pen myPenAnalog = new Pen(System.Drawing.Color.White, 5);
            // L3
            g.DrawLine(myPenAnalog, new Point(players[player].buttons[L3].loc.X + 35 / 2, players[player].buttons[L3].loc.Y + 35 / 2), new Point(Convert.ToInt32(players[player].buttons[L3].loc.X + multi * servers[player].analogLoc[0][0]), Convert.ToInt32(players[player].buttons[L3].loc.Y + multi * servers[player].analogLoc[0][1])));
            // R3
            g.DrawLine(myPenAnalog, new Point(players[player].buttons[R3].loc.X + 35 / 2, players[player].buttons[R3].loc.Y + 35 / 2), new Point(Convert.ToInt32(players[player].buttons[R3].loc.X + multi * servers[player].analogLoc[1][0]), Convert.ToInt32(players[player].buttons[R3].loc.Y + multi * servers[player].analogLoc[1][1])));
            myPenAnalog.Dispose();
        }

        private void drawStar(Graphics g, Brush fillBrush, Pen outlinePen, Rectangle rect) {
            PointF[] p = new PointF[10];
            p[0].X = rect.X + (rect.Width / 2);
            p[0].Y = rect.Y;
            p[1].X = rect.X + (42 * rect.Width / 64);
            p[1].Y = rect.Y + (19 * rect.Height / 64);
            p[2].X = rect.X + rect.Width;
            p[2].Y = rect.Y + (22 * rect.Height / 64);
            p[3].X = rect.X + (48 * rect.Width / 64);
            p[3].Y = rect.Y + (38 * rect.Height / 64);
            p[4].X = rect.X + (52 * rect.Width / 64);
            p[4].Y = rect.Y + rect.Height;
            p[5].X = rect.X + (rect.Width / 2);
            p[5].Y = rect.Y + (52 * rect.Height / 64);
            p[6].X = rect.X + (12 * rect.Width / 64);
            p[6].Y = rect.Y + rect.Height;
            p[7].X = rect.X + rect.Width / 4;
            p[7].Y = rect.Y + (38 * rect.Height / 64);
            p[8].X = rect.X;
            p[8].Y = rect.Y + (22 * rect.Height / 64);
            p[9].X = rect.X + (22 * rect.Width / 64);
            p[9].Y = rect.Y + (19 * rect.Height / 64);

            g.FillPolygon(fillBrush, p);
            g.DrawPolygon(outlinePen, p);
        }

        /***** Make MainInterface Movable *****/
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void MainInterface_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        /***** Close MainInterface via button *****/

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
