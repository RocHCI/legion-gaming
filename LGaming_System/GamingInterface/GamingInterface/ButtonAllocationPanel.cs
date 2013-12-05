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
using System.Text.RegularExpressions;

namespace GamingInterface
{
    class ButtonAllocationPanel : Panel
    {
        //string winDir = System.Environment.GetEnvironmentVariable("windir");
        MainInterface _myParent;
        Dictionary<string, Dictionary<string, int[]>> savedConfigs = new Dictionary<string, Dictionary<string, int[]>>();
        //List<List<int>[]> savedConfigs = new List<List<int>[]>();
        //List<string> savedConfigNames = new List<string>;
        string configFile;

        public ButtonAllocationPanel(MainInterface myParent, string myConfigFile)
        {
            _myParent = myParent;
            configFile = myConfigFile;
            Width = 500;
            Height = 500;

            readConfigFile();

            /*foreach (var pair in savedConfigs)
            {
                Console.WriteLine("----------------keyval: " + pair.Key + ":" + pair.Value);
            }*/

            writeConfigFile();
            //File.Delete(configFile);

            ButtonAllocationControllerPanel mySubPanel = new ButtonAllocationControllerPanel();
            Controls.Add(mySubPanel);
        }

        public void writeConfigFile()
        {
            Console.WriteLine("Deleting...");
            //File.Delete(configFile);

            StreamWriter writer = new StreamWriter(configFile);
            writer.WriteLine("blablabla");
            writer.WriteLine("// Set up each button for all players (1,2,...)");
            writer.WriteLine("// Format: button->num1,num2,num3,...");
            writer.WriteLine("// Example: L1->1,3");
            writer.WriteLine("// Button and player order does not matter");
            writer.WriteLine("// If a button is missing, Player 1 controls it");
            Console.WriteLine("Wrote intro stuff.");

            foreach (var pair in savedConfigs)
            {
                writer.WriteLine(pair.Key);
                foreach (var subpair in pair.Value)
                {
                    writer.Write(subpair.Key+"->");
                    writer.Write(Convert.ToInt32(subpair.Value[0]+1));
                    for (int i = 1; i < subpair.Value.Length; i++)
                    {
                        writer.Write("," + Convert.ToInt32(subpair.Value[i] + 1));
                    }
                    writer.WriteLine();
                }
                writer.WriteLine();
            }
            writer.Flush();
            Console.WriteLine("Finished write.");
        }

        public bool readConfigFile()
        {
            Console.WriteLine("reading...");
            StreamReader reader = new StreamReader(configFile);
            try
            {
                Console.WriteLine("trying...");
                // Skip the first 5 lines
                for (int i = 0; i < 5; i++)
                {
                    reader.ReadLine();
                }

                string line = reader.ReadLine();
                string curConfigName = "";
                while(line != null)
                {
                    curConfigName = line;
                    //savedConfigNames.Add(reader.ReadLine());
                    //int[][] tmpSavedConfig = new int[numButtons][];
                    Dictionary<string, int[]> curSavedConfig = new Dictionary<string, int[]>();
                    line = reader.ReadLine();
                    while (line != null && line != "")
                    {
                        Console.WriteLine("allinfo: " + line);
                        string[] allInfo = Regex.Split(line, "->");
                        // I would like to convet these strings to variables directly, but alas...
                        string[] playersStr = allInfo[1].Split(',');
                        int[] players = new int[playersStr.Length];
                        for (int i = 0; i < players.Length; i++)
                        {
                            // Because normal people start counting at 1, not 0
                            players[i] = Convert.ToInt32(playersStr[i]) - 1;
                        }
                        curSavedConfig.Add(allInfo[0], players);
                        line = reader.ReadLine();
                    }
                    savedConfigs.Add(curConfigName, curSavedConfig);
                    //savedConfigs.Add(curConfigName, tmpSavedConfig);
                    //savedConfigs.Add(tmpSavedConfig);

                    // Ignore whitespace between and possibly at end
                    while (line == "")
                    {
                        line = reader.ReadLine();
                    }
                }
                //writeConfigFile();
                return true;
            }
            catch
            {
                Console.WriteLine("No config file found.");
                return false;
            }

            finally
            {
                reader.Close();
            }
        }
    }
}

