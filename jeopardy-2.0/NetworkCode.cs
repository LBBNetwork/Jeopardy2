using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.IO;


/// <summary>
/// Todo: Add error handling for everything here. Otherwise this should be good to go.
/// </summary>

namespace jeopardy_2._0
{
    class NetworkCode
    {
        string FilePath = @"G:\jeopardy\";

        //DispatcherTimer NetworkTimer = new DispatcherTimer();
        public int InitNetwork()
        {
            try
            {
                File.Create(@"G:\jeopardy\init");

                //File.Create(FilePath + @"score\1");
                //File.Create(FilePath + @"score\2");
                //File.Create(FilePath + @"score\3");

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        /*public void StartTimer()
        {
            NetworkTimer.Interval = TimeSpan.FromMilliseconds(250);
            //NetworkTimer.Tick += NCReadCurrentClue;


        }*/

        public int NCWriteCloseClueDisplay(int silent)
        {
            try
            {
                FileStream CloseCD;
                
                if(silent != 1)
                {
                    CloseCD = File.Create(FilePath + @"misc\closeclue");
                }
                else
                {
                    CloseCD = File.Create(FilePath + @"misc\closecluesilent");
                }

                CloseCD.Close();

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        public int NCWriteCurrentClue(int Clue)
        {
            try
            {
                FileStream WriteClue;

                switch (Clue)
                {
                    case 1: // Category 1, Clue 1
                        WriteClue = File.Create(FilePath + @"clues\1");
                        WriteClue.Close();                        
                        break;
                    case 2:
                        WriteClue = File.Create(FilePath + @"clues\2");
                        WriteClue.Close();
                        break;
                    case 3:
                        WriteClue = File.Create(FilePath + @"clues\3");
                        WriteClue.Close();
                        break;
                    case 4:
                        WriteClue = File.Create(FilePath + @"clues\4");
                        WriteClue.Close();
                        break;
                    case 5:
                        WriteClue = File.Create(FilePath + @"clues\5");
                        WriteClue.Close();
                        break;
                    case 6: // Category 2, Clue 1
                        WriteClue = File.Create(FilePath + @"clues\6");
                        WriteClue.Close();
                        break;
                    case 7:
                        WriteClue = File.Create(FilePath + @"clues\7");
                        WriteClue.Close();
                        break;
                    case 8:
                        WriteClue = File.Create(FilePath + @"clues\8");
                        WriteClue.Close();
                        break;
                    case 9:
                        WriteClue = File.Create(FilePath + @"clues\9");
                        WriteClue.Close();
                        break;
                    case 10:
                        WriteClue = File.Create(FilePath + @"clues\10");
                        WriteClue.Close();
                        break;
                    case 11: // Category 3 Clue 1
                        WriteClue = File.Create(FilePath + @"clues\11");
                        WriteClue.Close();
                        break;
                    case 12:
                        WriteClue = File.Create(FilePath + @"clues\12");
                        WriteClue.Close();
                        break;
                    case 13:
                        WriteClue = File.Create(FilePath + @"clues\13");
                        WriteClue.Close();
                        break;
                    case 14:
                        WriteClue = File.Create(FilePath + @"clues\14");
                        WriteClue.Close();
                        break;
                    case 15:
                        WriteClue = File.Create(FilePath + @"clues\15");
                        WriteClue.Close();
                        break;
                    case 16: // Category 4 Clue 1
                        WriteClue = File.Create(FilePath + @"clues\16");
                        WriteClue.Close();
                        break;
                    case 17:
                        WriteClue = File.Create(FilePath + @"clues\17");
                        WriteClue.Close();
                        break;
                    case 18:
                        WriteClue = File.Create(FilePath + @"clues\18");
                        WriteClue.Close();
                        break;
                    case 19:
                        WriteClue = File.Create(FilePath + @"clues\19");
                        WriteClue.Close();
                        break;
                    case 20:
                        WriteClue = File.Create(FilePath + @"clues\20");
                        WriteClue.Close();
                        break;
                    case 21: //Category 5 Clue 1
                        WriteClue = File.Create(FilePath + @"clues\21");
                        WriteClue.Close();
                        break;
                    case 22:
                        WriteClue = File.Create(FilePath + @"clues\22");
                        WriteClue.Close();
                        break;
                    case 23:
                        WriteClue = File.Create(FilePath + @"clues\23");
                        WriteClue.Close();
                        break;
                    case 24:
                        WriteClue = File.Create(FilePath + @"clues\24");
                        WriteClue.Close();
                        break;
                    case 25:
                        WriteClue = File.Create(FilePath + @"clues\25");
                        WriteClue.Close();
                        break;
                    case 26: // Category 6 Clue 1
                        WriteClue = File.Create(FilePath + @"clues\26");
                        WriteClue.Close();
                        break;
                    case 27:
                        WriteClue = File.Create(FilePath + @"clues\27");
                        WriteClue.Close();
                        break;
                    case 28:
                        WriteClue = File.Create(FilePath + @"clues\28");
                        WriteClue.Close();
                        break;
                    case 29:
                        WriteClue = File.Create(FilePath + @"clues\29");
                        WriteClue.Close();
                        break;
                    case 30:
                        WriteClue = File.Create(FilePath + @"clues\30");
                        WriteClue.Close();
                        break;

                    case 91: //Category 1
                        WriteClue = File.Create(FilePath + @"clues\91");
                        WriteClue.Close();
                        break;
                    case 92:
                        WriteClue = File.Create(FilePath + @"clues\92");
                        WriteClue.Close();
                        break;
                    case 93:
                        WriteClue = File.Create(FilePath + @"clues\93");
                        WriteClue.Close();
                        break;
                    case 94:
                        WriteClue = File.Create(FilePath + @"clues\94");
                        WriteClue.Close();
                        break;
                    case 95:
                        WriteClue = File.Create(FilePath + @"clues\95");
                        WriteClue.Close();
                        break;
                    case 96:
                        WriteClue = File.Create(FilePath + @"clues\96");
                        WriteClue.Close();
                        break;



                    case 100: // Daily Double Reveal 1
                        WriteClue = File.Create(FilePath + @"clues\100");
                        WriteClue.Close();
                        break;
                    case 101: // Daily Double Reveal 2
                        WriteClue = File.Create(FilePath + @"clues\101");
                        WriteClue.Close();
                        break;
                    case 105: // Reveal FJ Category
                        WriteClue = File.Create(FilePath + @"clues\105");
                        WriteClue.Close();
                        break;
                    case 106: // Reveal FJ Clue
                        WriteClue = File.Create(FilePath + @"clues\106");
                        WriteClue.Close();
                        break;

                    case 200: // Begin Jeopardy Round
                        WriteClue = File.Create(FilePath + @"clues\200");
                        WriteClue.Close();
                        break;
                    case 201: // Begin Double Jeopardy Round
                        WriteClue = File.Create(FilePath + @"clues\201");
                        WriteClue.Close();
                        break;
                    case 202:
                        WriteClue = File.Create(FilePath + @"clues\202");
                        WriteClue.Close();
                        break;

                    case 300: // close clue
                        WriteClue = File.Create(FilePath + @"clues\300");
                        WriteClue.Close();
                        break;

                    default:
                        return 0;
                        break;
                }

                
            }
            catch
            {

            }
            return 1;
        }

        /*public int NCWriteCurrentCategory(int Category)
        {
            try
            {
                if (Category != 1)
                {
                    File.Delete(FilePath + @"category\" + (Category - 1).ToString());
                }

                File.Create(FilePath + @"category\" + Category.ToString());
                
                return 0;
            }
            catch
            {
                return 1;
            }
        }*/

        /*public int NCWriteCurrentRound(int Round)
        {
            File.Create(FilePath + @"round\" + Round.ToString());
            return 0;
        }*/

        public int NCWritePlayerScores(int Player, double Score)
        {
            File.WriteAllText(FilePath + @"score\" + Player.ToString(), Score.ToString());
            
            return 0;
        }

        public int NCWritePlayerName(int Player, string Name)
        {
            File.WriteAllText(FilePath + @"misc\" + Player.ToString(), Name);

            return 0;
        }

        /*public string NCReadCurrentCategory()
        {
            string[] CurrentCat = Directory.GetFiles(FilePath + "category");

            File.Delete(FilePath + @"clues\" + CurrentCat[0]);

            return CurrentCat[0];
        }

        public string NCReadCurrentRound()
        {
            string[] CurrentRound = Directory.GetFiles(FilePath + "round");

            return CurrentRound[0];
        }*/
        public string NCReadPlayerName(int Player)
        {
            string Name = "0";

            switch (Player)
            {
                case 1:
                    Name = File.ReadAllText(FilePath + @"misc\1");
                    break;
                case 2:
                    Name = File.ReadAllText(FilePath + @"misc\2");
                    break;
                case 3:
                    Name = File.ReadAllText(FilePath + @"misc\3");
                    break;
                default:
                    break;
            }

            return Name;
        }

        public int NCReadPlayerScores(int Player)
        {
            int Score = 0;

            switch(Player)
            {
                case 1:
                    Score = Convert.ToInt16(File.ReadAllText(FilePath + @"score\1"));
                    break;
                case 2:
                    Score = Convert.ToInt16(File.ReadAllText(FilePath + @"score\2"));
                    break;
                case 3:
                    Score = Convert.ToInt16(File.ReadAllText(FilePath + @"score\3"));
                    break;
                default:
                    break;
            }

            return Score;
        }

        public string NCReadPlayerRingin()
        {
            string[] CurrentRI = Directory.GetFiles(FilePath + "ri");

            try
            {
                return CurrentRI[0];
            }
            catch
            {
                return "";
            }
            
        }

        public int NCReadCurrentClue()
        {
            string[] CurrentClue = Directory.GetFiles(FilePath + "clues");

            try
            {
                File.Delete(CurrentClue[0]);

                switch (CurrentClue[0])
                {
                    case @"G:\jeopardy\clues\1":
                        return 1;
                        break;
                    case @"G:\jeopardy\clues\2":
                        return 2;
                        break;
                    case @"G:\jeopardy\clues\3":
                        return 3;
                        break;
                    case @"G:\jeopardy\clues\4":
                        return 4;
                        break;
                    case @"G:\jeopardy\clues\5":
                        return 5;
                        break;
                    case @"G:\jeopardy\clues\6":
                        return 6;
                        break;
                    case @"G:\jeopardy\clues\7":
                        return 7;
                        break;
                    case @"G:\jeopardy\clues\8":
                        return 8;
                        break;
                    case @"G:\jeopardy\clues\9":
                        return 9;
                        break;
                    case @"G:\jeopardy\clues\10":
                        return 10;
                        break;
                    case @"G:\jeopardy\clues\11":
                        return 11;
                        break;
                    case @"G:\jeopardy\clues\12":
                        return 12;
                        break;
                    case @"G:\jeopardy\clues\13":
                        return 13;
                        break;
                    case @"G:\jeopardy\clues\14":
                        return 14;
                        break;
                    case @"G:\jeopardy\clues\15":
                        return 15;
                        break;
                    case @"G:\jeopardy\clues\16":
                        return 16;
                        break;
                    case @"G:\jeopardy\clues\17":
                        return 17;
                        break;
                    case @"G:\jeopardy\clues\18":
                        return 18;
                        break;
                    case @"G:\jeopardy\clues\19":
                        return 19;
                        break;
                    case @"G:\jeopardy\clues\20":
                        return 20;
                        break;
                    case @"G:\jeopardy\clues\21":
                        return 21;
                        break;
                    case @"G:\jeopardy\clues\22":
                        return 22;
                        break;
                    case @"G:\jeopardy\clues\23":
                        return 23;
                        break;
                    case @"G:\jeopardy\clues\24":
                        return 24;
                        break;
                    case @"G:\jeopardy\clues\25":
                        return 25;
                        break;
                    case @"G:\jeopardy\clues\26":
                        return 26;
                        break;
                    case @"G:\jeopardy\clues\27":
                        return 27;
                        break;
                    case @"G:\jeopardy\clues\28":
                        return 28;
                        break;
                    case @"G:\jeopardy\clues\29":
                        return 29;
                        break;
                    case @"G:\jeopardy\clues\30":
                        return 30;
                        break;


                    case @"G:\jeopardy\clues\91":
                        return 91;
                    case @"G:\jeopardy\clues\92":
                        return 92;
                    case @"G:\jeopardy\clues\93":
                        return 93;
                    case @"G:\jeopardy\clues\94":
                        return 94;
                    case @"G:\jeopardy\clues\95":
                        return 95;
                    case @"G:\jeopardy\clues\96":
                        return 96;

                    case @"G:\jeopardy\clues\100": // Daily Double Reveal 1
                        return 100;
                        break;
                    case @"G:\jeopardy\clues\101": // Daily Double Reveal 2
                        return 101;
                        break;
                    case @"G:\jeopardy\clues\105": // Reveal FJ Category
                        return 105;
                        break;
                    case @"G:\jeopardy\clues\106": // Reveal FJ Clue
                        return 106;
                        break;

                    case @"G:\jeopardy\clues\200": // Begin Jeopardy Round
                        return 200;
                        break;
                    case @"G:\jeopardy\clues\201": // Begin Double Jeopardy Round
                        return 201;
                        break;
                    case @"G:\jeopardy\clues\202":
                        return 202;

                    case @"G:\jeopardy\clues\300":
                        return 300;

                    default:
                        return 0;
                        break;
                }

                //return CurrentClue[0];
            }
            catch
            {
                //return "";

                return -1;
            }
        }

        /*public string NCReadCloseClueDisplay(int silent)
        {
            string[] Close = Directory.GetFiles(FilePath + "misc");

            if(silent != 1)
            {
                File.Delete(FilePath + @"misc\closeclue");
            }
            else
            {
                File.Delete(FilePath + @"misc\closecluesilent");
            }
            

            try
            {
                return Close[0];
            }
            catch
            {
                return "";
            }

        }*/

        //-------------------------

        int CreatePlayerFiles()
        {


            return 0;
        }
    }
}
