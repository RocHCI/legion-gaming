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

namespace GamingInterface
{
    class ButtonAllocationControllerPanel : Panel
    {
        public ButtonAllocationControllerPanel()
        {
            PictureBox controller = new PictureBox();
            controller.Location = new Point(0, 0);
            //controller.Image = Properties.Resources.controllerCenterBlack;
            controller.Size = new Size(208, 125);
            controller.SizeMode = PictureBoxSizeMode.StretchImage;
            controller.Paint += new System.Windows.Forms.PaintEventHandler(playerCenter_Paint);
            Controls.Add(controller);
        }

        private void playerCenter_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
        }
    }
}