using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GamingInterface
{
    public class Player
    {
        // ID
        public int id;
        public String name;
        public Color pColor;
        public Color pColorDark;

        // Position
        public Point loc;

        // Visual Parts
        public PictureBox center;
        public Bitmap centerImage;
        public PictureBox topL;
        public Bitmap topLImage;
        public PictureBox topR;
        public Bitmap topRImage;

        public Button[] buttons;

        // Leader info
        public int leaderVote;


        // Modes //

        // Command Mode
        public bool cm = false;
        public bool cmUp = false;
        public bool cmDown = false;
        public bool cmLeft = false;
        public bool cmRight = false;
        public bool cmHole = false;
        public Point cmLoc;
        public int cmSize;
        public int cmWaveSize = 0;
        // Pause Mode
        public bool pm = false;

        public Player(int myID, String myName, Color myColor, Color myColorDark, Point myLoc, Bitmap myCenterImage, Bitmap myTopLImage, Bitmap myTopRImage, Button[] myButtons, int myLeaderVote, Point myCMLoc, int myCMSize)
        {
            id = myID;
            name = myName;
            pColor = myColor;
            pColorDark = myColorDark;
            loc = myLoc;
            centerImage = myCenterImage;
            topLImage = myTopLImage;
            topRImage = myTopRImage;
            buttons = myButtons;
            leaderVote = myLeaderVote;
            cmLoc = myCMLoc;
            cmSize = myCMSize;
        }
    }
}
