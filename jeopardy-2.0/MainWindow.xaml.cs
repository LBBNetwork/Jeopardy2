/* Filename: MainWindow.xaml.cs
   Author: neko2k (neko2k@beige-box.com)
   Website: http://www.beige-box.com
   Description: Code-behind for MainWindow.xaml



   The following source code is CONFIDENTIAL and PROPRIETARY PROPERTY
   of The Little Beige Box and MAY NOT BE RELEASED under PENALTY OF LAW.

   This file Copyright (c) 2019 The Little Beige Box.
*/

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;

namespace jeopardy_2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string GameFilePath;

        string Category6VideoIntro;

        string[] CategoryTitles = new string[6];
        string[] ClueText = new string[31];
        string[] ClueAnswer = new string[31];
        string[] CluePicture = new string[31];
        string[] ClueAudio = new string[31];
        string[] ClueVideo = new string[31];

        // Make it easier to do the MP2023 double point game by defining all the values in arrays rather than hardcoding them.
        // I should have done it this way to begin with. Dummy value in [0] so I can match up the numbering scheme for button labels.
        string[] ClueValuesStr = new string[11] { "imstupid", "$200", "$400", "$600", "$800", "$1000", "$400", "$800", "$1200", "$1600", "$2000" };
        int[] ClueValuesInt = new int[6] { 1337, 200, 400, 600, 800, 1000 };

        string[] ClueSpecialDD = new string[4] { "ClueTextHere", "nul", "nul", "nul" };

        int[] DailyDoubleLoc = new int[2];
        int[] DailyDoubleClueType = new int[2];

        double[] PlayerScores = new double[3];

        int AnimCount = 0;
        int[] AnimClues = new int[31];

        // Add +1 to each index; when an index = 5 then hide category
        // on main game board
        int[] CategoryHide = new int[6] { 0, 0, 0, 0, 0, 0 };

        // Each index is set to 1 when a RadioButton is checked
        // Used if the board op goes back to an earlier round to
        // warn them we've already played that round
        int[] RoundSwitchWarn = new int[3] { 0, 0, 0 };

        int ClueScore = 0;

        int ClueOpen = 0;

        private BuildString SetBuildStr = new BuildString();

        GameBoard gb = new GameBoard();

        ReadXML XMLRead = new ReadXML();

        SerialComm RSCOMPort = new SerialComm();

        System.Media.SoundPlayer GPAudio = new System.Media.SoundPlayer();

        DispatcherTimer Anim = new DispatcherTimer();
        DispatcherTimer VideoRuntime = new DispatcherTimer();

        int JeopardyRound = 0;

        // int RemoteDisplay = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LblDebug.Content = SetBuildStr.SetEvalNotice(3);

            // RSCOMPort.

            gb.Show();
            gb.Grid_ClueDisplay.Visibility = Visibility.Hidden;
            gb.GB_P1_Correct.Content = "";
            gb.GB_P2_Correct.Content = "";
            gb.GB_P3_Correct.Content = "";

            InitClueArrays();

            File.WriteAllText(@"data\local\score1", "0");
            File.WriteAllText(@"data\local\score2", "0");
            File.WriteAllText(@"data\local\score3", "0");

            try
            {
                if (File.Exists(@"data\local\clueopen"))
                {
                    File.Delete(@"data\local\clueopen");
                }
            }
            catch (Exception)
            {

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // todo: ask operator if they really want to quit Jeopardy

            Application.Current.Shutdown();
        }

        void RandomizeClueArray()
        {
                Array.Clear(AnimClues, 0, 31);

                int FilledLabels = 0;

                CategoryHide[0] = 0;
                CategoryHide[1] = 0;
                CategoryHide[2] = 0;
                CategoryHide[3] = 0;
                CategoryHide[4] = 0;
                CategoryHide[5] = 0;


                for (int i = 0; i < 31; i++)
                {
                ClueFillRnd:

                    Random rnd = new Random(Guid.NewGuid().GetHashCode());
                    int rn = rnd.Next(1, 30);

                    if (FilledLabels < 434)
                    {
                        if (AnimClues.Contains(rn))
                        {
                            goto ClueFillRnd;
                        }
                        else
                        {
                            AnimClues[i] = rn;
                            FilledLabels = FilledLabels + rn;
                        }
                    }

                    AnimClues[29] = 30;
                    AnimClues[30] = 30;

                    System.Diagnostics.Debug.WriteLine("AnimClues (" + i.ToString() + "): " + AnimClues[i].ToString());
                    System.Diagnostics.Debug.WriteLine("FilledLabels: " + FilledLabels.ToString());
                }
        }

        void InitClueArrays()
        {
            for(int i = 0; i < 6; i++)
            {
                CategoryTitles[i] = " ";
                System.Diagnostics.Debug.WriteLine("CategoryTitles: " + i.ToString());
            }
            
            for (int i = 0; i < 31; i++)
            {
                ClueText[i] = "0";
                ClueAnswer[i] = "0";

                System.Diagnostics.Debug.WriteLine("ClueText: " + i.ToString());
                System.Diagnostics.Debug.WriteLine("ClueAnswer: " + i.ToString());
            }
        }

        void LoadClueArrays(int JeopardyRound)
        {
            /* This function is used to initialize clue arrays
             * and to pull clue data from XML and store them in
             * RAM - RAM is faster than disk, so use RAM when
             * possible */

            for (int i = 0; i < 6; i++)
            {
                CategoryTitles[i] = XMLRead.XMLGetCategoryTitle(GameFilePath, i + 1, 0, JeopardyRound);
            }

            for (int i = 1; i < 31; i++)
            {
                if (i >= 0 && i <= 5) //Category 1
                {
                    ClueText[i] = XMLRead.XMLGetClueText(GameFilePath, 1, i, JeopardyRound);
                    ClueAnswer[i] = XMLRead.XMLGetClueAnswer(GameFilePath, 1, i, JeopardyRound);
                    CluePicture[i] = XMLRead.XMLGetCluePicture(GameFilePath, 1, i, JeopardyRound);
                    ClueAudio[i] = XMLRead.XMLGetClueAudio(GameFilePath, 1, i, JeopardyRound);
                }
                else if(i >= 6 && i <= 10) //Category 2
                {
                    ClueText[i] = XMLRead.XMLGetClueText(GameFilePath, 2, i, JeopardyRound);
                    ClueAnswer[i] = XMLRead.XMLGetClueAnswer(GameFilePath, 2, i, JeopardyRound);
                    CluePicture[i] = XMLRead.XMLGetCluePicture(GameFilePath, 2, i, JeopardyRound);
                    ClueAudio[i] = XMLRead.XMLGetClueAudio(GameFilePath, 2, i, JeopardyRound);
                }
                else if(i >= 11 && i <= 15) //Category 3
                {
                    ClueText[i] = XMLRead.XMLGetClueText(GameFilePath, 3, i, JeopardyRound);
                    ClueAnswer[i] = XMLRead.XMLGetClueAnswer(GameFilePath, 3, i, JeopardyRound);
                    CluePicture[i] = XMLRead.XMLGetCluePicture(GameFilePath, 3, i, JeopardyRound);
                    ClueAudio[i] = XMLRead.XMLGetClueAudio(GameFilePath, 3, i, JeopardyRound);
                }
                else if(i >= 16 && i <= 20) //Category 4
                {
                    ClueText[i] = XMLRead.XMLGetClueText(GameFilePath, 4, i, JeopardyRound);
                    ClueAnswer[i] = XMLRead.XMLGetClueAnswer(GameFilePath, 4, i, JeopardyRound);
                    CluePicture[i] = XMLRead.XMLGetCluePicture(GameFilePath, 4, i, JeopardyRound);
                    ClueAudio[i] = XMLRead.XMLGetClueAudio(GameFilePath, 4, i, JeopardyRound);
                }
                else if(i >= 21 && i <= 25) //Category 5
                {
                    ClueText[i] = XMLRead.XMLGetClueText(GameFilePath, 5, i, JeopardyRound);
                    ClueAnswer[i] = XMLRead.XMLGetClueAnswer(GameFilePath, 5, i, JeopardyRound);
                    CluePicture[i] = XMLRead.XMLGetCluePicture(GameFilePath, 5, i, JeopardyRound);
                    ClueAudio[i] = XMLRead.XMLGetClueAudio(GameFilePath, 5, i, JeopardyRound);
                }
                else if(i >= 26 && i <= 30)
                {
                    ClueText[i] = XMLRead.XMLGetClueText(GameFilePath, 6, i, JeopardyRound);
                    ClueAnswer[i] = XMLRead.XMLGetClueAnswer(GameFilePath, 6, i, JeopardyRound);
                    CluePicture[i] = XMLRead.XMLGetCluePicture(GameFilePath, 6, i, JeopardyRound);
                    ClueAudio[i] = XMLRead.XMLGetClueAudio(GameFilePath, 6, i, JeopardyRound);

                    ClueVideo[i] = XMLRead.XMLGetClueVideo(GameFilePath, 6, i, JeopardyRound);
                }

                Category6VideoIntro = XMLRead.XMLGetCategoryIntroVideo(GameFilePath, JeopardyRound);

                System.Diagnostics.Debug.WriteLine("ClueText (" + i.ToString() + "): " + ClueText[i]);
                System.Diagnostics.Debug.WriteLine("ClueAnswer (" + i.ToString() + "): " + ClueAnswer[i]);
                System.Diagnostics.Debug.WriteLine("CluePicture (" + i.ToString() + "): " + CluePicture[i]);
                System.Diagnostics.Debug.WriteLine("ClueAudio (" + i.ToString() + "): " + ClueAudio[i]);
                System.Diagnostics.Debug.WriteLine("Category6VideoIntro file path: " + Category6VideoIntro);
            }
        }   

        void DisplayClue(string ClueText, int DailyDouble, int AlwaysZero, string AudioFilePath, string PictureFilePath, string VideoFilePath)
        {
            // Set up the clue to be displayed on the screen. Also check for special options such as
            // an audio clue, video clue, picture clue, or if we're in a Daily Double situation.

            ClueOpen = 1;

            var ClueBG = new ImageBrush();
            //ClueBG.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\cluebg.png", UriKind.Absolute));

            if (DailyDouble == 1)
            {
                if (AudioFilePath != "nul")
                {
                    ClueBG.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\aud_dd.png", UriKind.Absolute));
                }
                else if (VideoFilePath != "nul")
                {
                    ClueBG.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\vid_dd.png", UriKind.Absolute));
                }
                else if (PictureFilePath != "nul")
                {
                    ClueBG.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\vid_dd.png", UriKind.Absolute));
                }
                else
                {
                    ClueBG.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\dailydouble.png", UriKind.Absolute));
                }

                    /*switch (DDType) //todo: change this int out for a string
                    {
                        case 1: //video clue - either still or motion
                            DDImg.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\vid_dd.png", UriKind.Absolute));
                            break;
                        case 2: //audio
                            DDImg.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\aud_dd.png", UriKind.Absolute));
                            break;
                        default: //if all else fails
                            DDImg.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\dailydouble.png", UriKind.Absolute));
                            break;
                    }*/


                gb.Grid_ClueDisplay.Background = ClueBG;

                gb.TB_BGText.Text = ClueText;
                gb.TB_FGText.Text = ClueText;

                gb.TB_BGText.Visibility = Visibility.Hidden;
                gb.TB_FGText.Visibility = Visibility.Hidden;

                gb.Grid_ClueDisplay.Visibility = Visibility.Visible;

                System.Media.SoundPlayer DD = new System.Media.SoundPlayer();
                DD.SoundLocation = @"data\sound\dailydouble.wav";
                DD.Load();
                DD.Play();
            }
            else
            {
                if (AudioFilePath != "nul")
                {
                    //MessageBox.Show(@AudioFilePath);

                    GPAudio.SoundLocation = @AudioFilePath;
                    GPAudio.Load();

                    Btn_PlayAudio.IsEnabled = true;
                }

                try
                {
                    if (PictureFilePath != "nul")
                    {
                        ClueBG.ImageSource = new BitmapImage(new Uri(PictureFilePath, UriKind.Absolute));

                        gb.TB_BGText.Visibility = Visibility.Hidden;
                        gb.TB_FGText.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        ClueBG.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\cluebg.png", UriKind.Absolute));

                        gb.TB_BGText.Visibility = Visibility.Visible;
                        gb.TB_FGText.Visibility = Visibility.Visible;

                        gb.TB_BGText.Text = ClueText;
                        gb.TB_FGText.Text = ClueText;

                        gb.Grid_ClueDisplay.Visibility = Visibility.Visible;
                    }
                }
                catch
                {
                    MessageBox.Show("Exception in MainWindow.xaml.cs:DisplayClue(), clue bg try block - setting to default bg/text\n\ncouldn't set " + PictureFilePath);

                    ClueBG.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\cluebg.png", UriKind.Absolute));
                    gb.TB_BGText.Visibility = Visibility.Visible;
                    gb.TB_FGText.Visibility = Visibility.Visible;
                }

                gb.Grid_ClueDisplay.Background = ClueBG;

                /*gb.TB_BGText.Text = ClueText;
                gb.TB_FGText.Text = ClueText;*/

                gb.Grid_ClueDisplay.Visibility = Visibility.Visible;
            }
        }

        void PlayVideoAndDispose(string VideoPath)
        {
            VideoRuntime.Tick += VideoRuntime_Tick;

            try
            {
                gb.GB_VideoPlayer.Source = new Uri(VideoPath, UriKind.Absolute);

                // with thanks to https://markheath.net/post/how-to-get-media-file-duration-in-c for this block here
                using (var shell = ShellObject.FromParsingName(VideoPath))
                {
                    IShellProperty prop = shell.Properties.System.Media.Duration;
                    var t = (ulong)prop.ValueAsObject;

                    VideoRuntime.Interval = TimeSpan.FromTicks((long)t);
                }
                //end thanks

                gb.GB_VideoClueDisplay.Visibility = Visibility.Visible;

                VideoRuntime.Start();

                gb.GB_VideoPlayer.Play();
            }
            catch
            {
                MessageBox.Show("WARNING!!! The specified video failed to play! Path to file:\n\n" + VideoPath);
            }
         
        }

        private void VideoRuntime_Tick(object sender, EventArgs e)
        {
            gb.GB_VideoPlayer.Stop();

            gb.GB_VideoClueDisplay.Visibility = Visibility.Hidden;

            VideoRuntime.Stop();
        }

        private void Btn_CA1_Click(object sender, RoutedEventArgs e)
        {
            Tb_CA1.Text = CategoryTitles[0];
            gb.GameBoard_Tb_Ca1.Text = CategoryTitles[0];
        }
        private void Btn_CA2_Click(object sender, RoutedEventArgs e)
        {
            Tb_CA2.Text = CategoryTitles[1];
            gb.GameBoard_Tb_Ca2.Text = CategoryTitles[1];
        }

        private void Btn_CA3_Click(object sender, RoutedEventArgs e)
        {
            Tb_CA3.Text = CategoryTitles[2];
            gb.GameBoard_Tb_Ca3.Text = CategoryTitles[2];
        }

        private void Btn_CA4_Click(object sender, RoutedEventArgs e)
        {
            Tb_CA4.Text = CategoryTitles[3];
            gb.GameBoard_Tb_Ca4.Text = CategoryTitles[3];
        }

        private void Btn_CA5_Click(object sender, RoutedEventArgs e)
        {
            Tb_CA5.Text = CategoryTitles[4];
            gb.GameBoard_Tb_Ca5.Text = CategoryTitles[4];
        }

        private void Btn_CA6_Click(object sender, RoutedEventArgs e)
        {
            Tb_CA6.Text = CategoryTitles[5];
            gb.GameBoard_Tb_Ca6.Text = CategoryTitles[5];

            if(Category6VideoIntro != "nul")
            {
                PlayVideoAndDispose(Category6VideoIntro);
            }

            Gr_ClueValues.IsEnabled = true;
        }

        private void Rb_Jeopardy_Click(object sender, RoutedEventArgs e)
        {
            if(RoundSwitchWarn[0] != 0)
            {
                MessageBox.Show("Warning: You have already played the Jeopardy round.\nAre you sure you want to go back to this round?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            }

            RoundSwitchWarn[0] = 1;

            RandomizeClueArray();

            /*Array.Clear(AnimClues, 0, 31);

            int FilledLabels = 0;

            CategoryHide[0] = 0;
            CategoryHide[1] = 0;
            CategoryHide[2] = 0;
            CategoryHide[3] = 0;
            CategoryHide[4] = 0;
            CategoryHide[5] = 0;


            for (int i = 0; i < 31; i++)
            {
            ClueFillRnd:

                Random rnd = new Random(Guid.NewGuid().GetHashCode());
                int rn = rnd.Next(1, 30);

                if(FilledLabels < 434)
                {
                    if(AnimClues.Contains(rn))
                    {
                        goto ClueFillRnd;
                    }
                    else
                    {
                        AnimClues[i] = rn;
                        FilledLabels = FilledLabels + rn;
                    }
                }

                AnimClues[29] = 30;
                AnimClues[30] = 30;

                System.Diagnostics.Debug.WriteLine("AnimClues (" + i.ToString() + "): " + AnimClues[i].ToString());
                System.Diagnostics.Debug.WriteLine("FilledLabels: " + FilledLabels.ToString());
            }*/

            System.Media.SoundPlayer Fill = new System.Media.SoundPlayer();

                      

            if (Cb_Fill1984.IsChecked == true)
            {
                // load the sfx for the 1984 fill mode
                Fill.SoundLocation = @"data\sound\boardfill84.wav";
                Anim.Interval = TimeSpan.FromMilliseconds(100);
                Anim.Tick += AnimateClueValues1984;
            }
            else if (Cb_FillDblPoints.IsChecked == true)
            {
                // todo: load sfx for the special double points mode at Megaplex 2023

                Fill.SoundLocation = @"data\sound\boardfilldbl.wav";
                Anim.Interval = TimeSpan.FromMilliseconds(293);
                Anim.Tick += AnimateClueValues;

            }    
            else
            {
                // use the standard 6 ding sfx
                Fill.SoundLocation = @"data\sound\boardfill.wav";
                Anim.Interval = TimeSpan.FromMilliseconds(293);
                Anim.Tick += AnimateClueValues;
            }
            
            Fill.Load();

            while(!Fill.IsLoadCompleted)
            {

            }

            Gr_CatBtns.IsEnabled = true;

            gb.Img_SplashScr.Visibility = Visibility.Hidden;

            Fill.Play();
            Anim.Start();

            // DailyDoubleLoc[0] = Convert.ToInt32(XMLRead.XMLGetDailyDouble(GameFilePath, 1, 0));
            //  DailyDoubleLoc[1] = Convert.ToInt32(XMLRead.XMLGetDailyDouble(GameFilePath, 2, 0));

            SetClueVals(0);

            //Set up DD display for Single Jeopardy
            DailyDoubleSetup(0);

            System.Diagnostics.Debug.WriteLine("DDLoc[0]: " + DailyDoubleLoc[0].ToString());
            System.Diagnostics.Debug.WriteLine("DDLoc[1]: " + DailyDoubleLoc[1].ToString());
        }

        private void AnimateClueValues(object sender, EventArgs e)
        {
            if (Cb_FillDblPoints.IsChecked == true)
            {
                // Use this animation only if the Double Points box is set to checked
                switch (AnimCount)
                {
                    case 0:
                        SwitchClueValuesClear(AnimClues[0]);
                        SwitchClueValuesClear(AnimClues[1]);
                        SwitchClueValuesClear(AnimClues[2]);
                        SwitchClueValuesClear(AnimClues[3]);
                        SwitchClueValuesClear(AnimClues[4]);
                        break;
                    case 1:
                        SwitchClueValuesClear(AnimClues[5]);
                        SwitchClueValuesClear(AnimClues[6]);
                        SwitchClueValuesClear(AnimClues[7]);
                        SwitchClueValuesClear(AnimClues[8]);
                        SwitchClueValuesClear(AnimClues[9]);
                        break;
                    case 2:
                        SwitchClueValuesClear(AnimClues[10]);
                        SwitchClueValuesClear(AnimClues[11]);
                        SwitchClueValuesClear(AnimClues[12]);
                        SwitchClueValuesClear(AnimClues[13]);
                        SwitchClueValuesClear(AnimClues[14]);
                        break;
                    case 3:
                        SwitchClueValuesClear(AnimClues[15]);
                        SwitchClueValuesClear(AnimClues[16]);
                        SwitchClueValuesClear(AnimClues[17]);
                        SwitchClueValuesClear(AnimClues[18]);
                        SwitchClueValuesClear(AnimClues[19]);
                        break;
                    case 4:
                        SwitchClueValuesClear(AnimClues[20]);
                        SwitchClueValuesClear(AnimClues[21]);
                        SwitchClueValuesClear(AnimClues[22]);
                        SwitchClueValuesClear(AnimClues[23]);
                        SwitchClueValuesClear(AnimClues[24]);
                        break;
                    case 5:
                        SwitchClueValuesClear(AnimClues[25]);
                        SwitchClueValuesClear(AnimClues[26]);
                        SwitchClueValuesClear(AnimClues[27]);
                        SwitchClueValuesClear(AnimClues[28]);
                        SwitchClueValuesClear(AnimClues[29]);


                        Cb_Fill6Ding.IsChecked = true;
                        AnimCount = -1; // set animcount to -1 at the end here so that when the +1 to animcount
                                        // after this runs, it sets animcount back to 0 and the animation plays correctly
                        

                        break;
                    default:

                        Cb_Fill6Ding.IsChecked = true;
                        AnimCount = -1;

                        break;
                }

                AnimCount = AnimCount + 1;
            }
            else
            {
                // Animate the clue value loadin according to the modern (2008+) six ding fill


                switch (AnimCount)
                {
                    case 0:
                        SwitchClueValues(AnimClues[0]);
                        SwitchClueValues(AnimClues[1]);
                        SwitchClueValues(AnimClues[2]);
                        SwitchClueValues(AnimClues[3]);
                        SwitchClueValues(AnimClues[4]);
                        break;
                    case 1:
                        SwitchClueValues(AnimClues[5]);
                        SwitchClueValues(AnimClues[6]);
                        SwitchClueValues(AnimClues[7]);
                        SwitchClueValues(AnimClues[8]);
                        SwitchClueValues(AnimClues[9]);
                        break;
                    case 2:
                        SwitchClueValues(AnimClues[10]);
                        SwitchClueValues(AnimClues[11]);
                        SwitchClueValues(AnimClues[12]);
                        SwitchClueValues(AnimClues[13]);
                        SwitchClueValues(AnimClues[14]);
                        break;
                    case 3:
                        SwitchClueValues(AnimClues[15]);
                        SwitchClueValues(AnimClues[16]);
                        SwitchClueValues(AnimClues[17]);
                        SwitchClueValues(AnimClues[18]);
                        SwitchClueValues(AnimClues[19]);
                        break;
                    case 4:
                        SwitchClueValues(AnimClues[20]);
                        SwitchClueValues(AnimClues[21]);
                        SwitchClueValues(AnimClues[22]);
                        SwitchClueValues(AnimClues[23]);
                        SwitchClueValues(AnimClues[24]);
                        break;
                    case 5:
                        SwitchClueValues(AnimClues[25]);
                        SwitchClueValues(AnimClues[26]);
                        SwitchClueValues(AnimClues[27]);
                        SwitchClueValues(AnimClues[28]);
                        SwitchClueValues(AnimClues[29]);

                        Anim.Stop();
                        break;
                    default:
                        Anim.Stop();
                        break;
                }

                AnimCount = AnimCount + 1;
            }


        }

        private void AnimateClueValues1984(object sender, EventArgs e)
        {
            // fill the board according to the original 1984 fill
            switch(AnimCount)
            {
                case 30:
                    Anim.Stop();
                    break;
                default:
                    SwitchClueValues(AnimClues[AnimCount]);
                    break;
            }

            AnimCount = AnimCount + 1;
        }

        void SwitchClueValues(int a)
        {
            // takes the clue value passed in parameter a (that was determined by RNG)
            // and fill the space on the main game board with the appropriate text for the clue value.

            switch (a)
            {
                case 1: // Category 1, Clue 1
                    gb.GB_Tb_Ca1C1.Text = ClueValuesStr[1];
                    break;
                case 2:
                    gb.GB_Tb_Ca1C2.Text = ClueValuesStr[2];
                    break;
                case 3:
                    gb.GB_Tb_Ca1C3.Text = ClueValuesStr[3];
                    break;
                case 4:
                    gb.GB_Tb_Ca1C4.Text = ClueValuesStr[4];
                    break;
                case 5:
                    gb.GB_Tb_Ca1C5.Text = ClueValuesStr[5];
                    break;
                case 6: // Category 2, Clue 1
                    gb.GB_Tb_Ca2C1.Text = ClueValuesStr[1];
                    break;
                case 7:
                    gb.GB_Tb_Ca2C2.Text = ClueValuesStr[2];
                    break;
                case 8:
                    gb.GB_Tb_Ca2C3.Text = ClueValuesStr[3];
                    break;
                case 9:
                    gb.GB_Tb_Ca2C4.Text = ClueValuesStr[4];
                    break;
                case 10:
                    gb.GB_Tb_Ca2C5.Text = ClueValuesStr[5];
                    break;
                case 11: // Category 3 Clue 1
                    gb.GB_Tb_Ca3C1.Text = ClueValuesStr[1];
                    break;
                case 12:
                    gb.GB_Tb_Ca3C2.Text = ClueValuesStr[2];
                    break;
                case 13:
                    gb.GB_Tb_Ca3C3.Text = ClueValuesStr[3];
                    break;
                case 14:
                    gb.GB_Tb_Ca3C4.Text = ClueValuesStr[4];
                    break;
                case 15:
                    gb.GB_Tb_Ca3C5.Text = ClueValuesStr[5];
                    break;
                case 16: // Category 4 Clue 1
                    gb.GB_Tb_Ca4C1.Text = ClueValuesStr[1];
                    break;
                case 17:
                    gb.GB_Tb_Ca4C2.Text = ClueValuesStr[2];
                    break;
                case 18:
                    gb.GB_Tb_Ca4C3.Text = ClueValuesStr[3];
                    break;
                case 19:
                    gb.GB_Tb_Ca4C4.Text = ClueValuesStr[4];
                    break;
                case 20:
                    gb.GB_Tb_Ca4C5.Text = ClueValuesStr[5];
                    break;
                case 21: //Category 5 Clue 1
                    gb.GB_Tb_Ca5C1.Text = ClueValuesStr[1];
                    break;
                case 22:
                    gb.GB_Tb_Ca5C2.Text = ClueValuesStr[2];
                    break;
                case 23:
                    gb.GB_Tb_Ca5C3.Text = ClueValuesStr[3];
                    break;
                case 24:
                    gb.GB_Tb_Ca5C4.Text = ClueValuesStr[4];
                    break;
                case 25:
                    gb.GB_Tb_Ca5C5.Text = ClueValuesStr[5];
                    break;
                case 26: // Category 6 Clue 1
                    gb.GB_Tb_Ca6C1.Text = ClueValuesStr[1];
                    break;
                case 27:
                    gb.GB_Tb_Ca6C2.Text = ClueValuesStr[2];
                    break;
                case 28:
                    gb.GB_Tb_Ca6C3.Text = ClueValuesStr[3];
                    break;
                case 29:
                    gb.GB_Tb_Ca6C4.Text = ClueValuesStr[4];
                    break;
                case 30:
                    gb.GB_Tb_Ca6C5.Text = ClueValuesStr[5];
                    break;
                default:
                    break;
            }
        }

        void SwitchClueValuesClear(int a)
        {
            // pretty much does the same thing as the above, but clears the clue value on the big board.
            // i am doing this because i am lazy and it works

            switch (a)
            {
                case 1: // Category 1, Clue 1
                    gb.GB_Tb_Ca1C1.Text = "";
                    break;
                case 2:
                    gb.GB_Tb_Ca1C2.Text = "";
                    break;
                case 3:
                    gb.GB_Tb_Ca1C3.Text = "";
                    break;
                case 4:
                    gb.GB_Tb_Ca1C4.Text = "";
                    break;
                case 5:
                    gb.GB_Tb_Ca1C5.Text = "";
                    break;
                case 6: // Category 2, Clue 1
                    gb.GB_Tb_Ca2C1.Text = "";
                    break;
                case 7:
                    gb.GB_Tb_Ca2C2.Text = "";
                    break;
                case 8:
                    gb.GB_Tb_Ca2C3.Text = "";
                    break;
                case 9:
                    gb.GB_Tb_Ca2C4.Text = "";
                    break;
                case 10:
                    gb.GB_Tb_Ca2C5.Text = "";
                    break;
                case 11: // Category 3 Clue 1
                    gb.GB_Tb_Ca3C1.Text = "";
                    break;
                case 12:
                    gb.GB_Tb_Ca3C2.Text = "";
                    break;
                case 13:
                    gb.GB_Tb_Ca3C3.Text = "";
                    break;
                case 14:
                    gb.GB_Tb_Ca3C4.Text = "";
                    break;
                case 15:
                    gb.GB_Tb_Ca3C5.Text = "";
                    break;
                case 16: // Category 4 Clue 1
                    gb.GB_Tb_Ca4C1.Text = "";
                    break;
                case 17:
                    gb.GB_Tb_Ca4C2.Text = "";
                    break;
                case 18:
                    gb.GB_Tb_Ca4C3.Text = "";
                    break;
                case 19:
                    gb.GB_Tb_Ca4C4.Text = "";
                    break;
                case 20:
                    gb.GB_Tb_Ca4C5.Text = "";
                    break;
                case 21: //Category 5 Clue 1
                    gb.GB_Tb_Ca5C1.Text = "";
                    break;
                case 22:
                    gb.GB_Tb_Ca5C2.Text = "";
                    break;
                case 23:
                    gb.GB_Tb_Ca5C3.Text = "";
                    break;
                case 24:
                    gb.GB_Tb_Ca5C4.Text = "";
                    break;
                case 25:
                    gb.GB_Tb_Ca5C5.Text = "";
                    break;
                case 26: // Category 6 Clue 1
                    gb.GB_Tb_Ca6C1.Text = "";
                    break;
                case 27:
                    gb.GB_Tb_Ca6C2.Text = "";
                    break;
                case 28:
                    gb.GB_Tb_Ca6C3.Text = "";
                    break;
                case 29:
                    gb.GB_Tb_Ca6C4.Text = "";
                    break;
                case 30:
                    gb.GB_Tb_Ca6C5.Text = "";
                    break;
                default:
                    break;
            }
        }

        void DailyDoubleSetup(int JeopardyRound)
        {
            // new for MP2023: read in the value for the DD locations from the XML file.
            // set them up in the DD Location array, and also recolour the reveal button
            // in the MCP to show board op where the DD is

            // param 3 in XMLGetDailyDouble is:
            // 0 = Single Jeopardy
            // 1 = Double Jeopardy

            DailyDoubleLoc[0] = Convert.ToInt32(XMLRead.XMLGetDailyDouble(GameFilePath, 1, JeopardyRound));
            DailyDoubleLoc[1] = Convert.ToInt32(XMLRead.XMLGetDailyDouble(GameFilePath, 2, JeopardyRound));

            switch (DailyDoubleLoc[0])
            {
                case 1: // Category 1, Clue 1
                    Btn_CA1CL1.Background = Brushes.Yellow;
                    break;
                case 2:
                    Btn_CA1CL2.Background = Brushes.Yellow;
                    break;
                case 3:
                    Btn_CA1CL3.Background = Brushes.Yellow;
                    break;
                case 4:
                    Btn_CA1CL4.Background = Brushes.Yellow;
                    break;
                case 5:
                    Btn_CA1CL5.Background = Brushes.Yellow;
                    break;
                case 6: // Category 2, Clue 1
                    Btn_CA1CL1.Background = Brushes.Yellow;
                    break;
                case 7:
                    Btn_CA2CL2.Background = Brushes.Yellow;
                    break;
                case 8:
                    Btn_CA2CL3.Background = Brushes.Yellow;
                    break;
                case 9:
                    Btn_CA2CL4.Background = Brushes.Yellow;
                    break;
                case 10:
                    Btn_CA2CL5.Background = Brushes.Yellow;
                    break;
                case 11: // Category 3 Clue 1
                    Btn_CA1CL1.Background = Brushes.Yellow;
                    break;
                case 12:
                    Btn_CA3CL2.Background = Brushes.Yellow;
                    break;
                case 13:
                    Btn_CA3CL3.Background = Brushes.Yellow;
                    break;
                case 14:
                    Btn_CA3CL4.Background = Brushes.Yellow;
                    break;
                case 15:
                    Btn_CA3CL5.Background = Brushes.Yellow;
                    break;
                case 16: // Category 4 Clue 1
                    Btn_CA4CL1.Background = Brushes.Yellow;
                    break;
                case 17:
                    Btn_CA4CL2.Background = Brushes.Yellow;
                    break;
                case 18:
                    Btn_CA4CL3.Background = Brushes.Yellow;
                    break;
                case 19:
                    Btn_CA4CL4.Background = Brushes.Yellow;
                    break;
                case 20:
                    Btn_CA4CL5.Background = Brushes.Yellow;
                    break;
                case 21: //Category 5 Clue 1
                    Btn_CA5CL1.Background = Brushes.Yellow;
                    break;
                case 22:
                    Btn_CA5CL2.Background = Brushes.Yellow;
                    break;
                case 23:
                    Btn_CA5CL3.Background = Brushes.Yellow;
                    break;
                case 24:
                    Btn_CA5CL4.Background = Brushes.Yellow;
                    break;
                case 25:
                    Btn_CA5CL5.Background = Brushes.Yellow;
                    break;
                case 26: // Category 6 Clue 1
                    Btn_CA6CL1.Background = Brushes.Yellow;
                    break;
                case 27:
                    Btn_CA6CL2.Background = Brushes.Yellow;
                    break;
                case 28:
                    Btn_CA6CL3.Background = Brushes.Yellow;
                    break;
                case 29:
                    Btn_CA6CL4.Background = Brushes.Yellow;
                    break;
                case 30:
                    Btn_CA6CL5.Background = Brushes.Yellow;
                    break;
                default:
                    break;
            }
            switch (DailyDoubleLoc[1])
            {
                case 1: // Category 1, Clue 1
                    Btn_CA1CL1.Background = Brushes.Yellow;
                    break;
                case 2:
                    Btn_CA1CL2.Background = Brushes.Yellow;
                    break;
                case 3:
                    Btn_CA1CL3.Background = Brushes.Yellow;
                    break;
                case 4:
                    Btn_CA1CL4.Background = Brushes.Yellow;
                    break;
                case 5:
                    Btn_CA1CL5.Background = Brushes.Yellow;
                    break;
                case 6: // Category 2, Clue 1
                    Btn_CA1CL1.Background = Brushes.Yellow;
                    break;
                case 7:
                    Btn_CA2CL2.Background = Brushes.Yellow;
                    break;
                case 8:
                    Btn_CA2CL3.Background = Brushes.Yellow;
                    break;
                case 9:
                    Btn_CA2CL4.Background = Brushes.Yellow;
                    break;
                case 10:
                    Btn_CA2CL5.Background = Brushes.Yellow;
                    break;
                case 11: // Category 3 Clue 1
                    Btn_CA1CL1.Background = Brushes.Yellow;
                    break;
                case 12:
                    Btn_CA3CL2.Background = Brushes.Yellow;
                    break;
                case 13:
                    Btn_CA3CL3.Background = Brushes.Yellow;
                    break;
                case 14:
                    Btn_CA3CL4.Background = Brushes.Yellow;
                    break;
                case 15:
                    Btn_CA3CL5.Background = Brushes.Yellow;
                    break;
                case 16: // Category 4 Clue 1
                    Btn_CA4CL1.Background = Brushes.Yellow;
                    break;
                case 17:
                    Btn_CA4CL2.Background = Brushes.Yellow;
                    break;
                case 18:
                    Btn_CA4CL3.Background = Brushes.Yellow;
                    break;
                case 19:
                    Btn_CA4CL4.Background = Brushes.Yellow;
                    break;
                case 20:
                    Btn_CA4CL5.Background = Brushes.Yellow;
                    break;
                case 21: //Category 5 Clue 1
                    Btn_CA5CL1.Background = Brushes.Yellow;
                    break;
                case 22:
                    Btn_CA5CL2.Background = Brushes.Yellow;
                    break;
                case 23:
                    Btn_CA5CL3.Background = Brushes.Yellow;
                    break;
                case 24:
                    Btn_CA5CL4.Background = Brushes.Yellow;
                    break;
                case 25:
                    Btn_CA5CL5.Background = Brushes.Yellow;
                    break;
                case 26: // Category 6 Clue 1
                    Btn_CA6CL1.Background = Brushes.Yellow;
                    break;
                case 27:
                    Btn_CA6CL2.Background = Brushes.Yellow;
                    break;
                case 28:
                    Btn_CA6CL3.Background = Brushes.Yellow;
                    break;
                case 29:
                    Btn_CA6CL4.Background = Brushes.Yellow;
                    break;
                case 30:
                    Btn_CA6CL5.Background = Brushes.Yellow;
                    break;
                default:
                    break;
            }
        }

        private void Rb_DoubleJeopardy_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("About to switch to Double Jeopardy. Is that okay?", "Round Switch", MessageBoxButton.YesNo);

            if(res == MessageBoxResult.Yes)
            {
                if (RoundSwitchWarn[1] != 0)
                {
                    MessageBox.Show("Warning: You have already played the Double Jeopardy round.\nAre you sure you want to go back to this round?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                }

                RoundSwitchWarn[1] = 1;

                JeopardyRound = 1;

                CategoryHide[0] = 0;
                CategoryHide[1] = 0;
                CategoryHide[2] = 0;
                CategoryHide[3] = 0;
                CategoryHide[4] = 0;
                CategoryHide[5] = 0;

                LoadClueArrays(1);

                //  DailyDoubleLoc[0] = Convert.ToInt32(XMLRead.XMLGetDailyDouble(GameFilePath, 1, 1));
                // DailyDoubleLoc[1] = Convert.ToInt32(XMLRead.XMLGetDailyDouble(GameFilePath, 2, 1));

                SetClueVals(1);

                // Set up Daily Doubles for Double Jeopardy
                DailyDoubleSetup(1);
            }
            else
            {
                Rb_DoubleJeopardy.IsChecked = false;
            }
        }

        private void SetClueVals(int JeopardyRound)
        {
            if(JeopardyRound == 0)
            {
                Tb_CA1.Text = "";
                Tb_CA6.Text = "";
                Tb_CA2.Text = "";
                Tb_CA3.Text = "";
                Tb_CA4.Text = "";
                Tb_CA5.Text = "";

                gb.GameBoard_Tb_Ca1.Text = "";
                gb.GameBoard_Tb_Ca2.Text = "";
                gb.GameBoard_Tb_Ca3.Text = "";
                gb.GameBoard_Tb_Ca4.Text = "";
                gb.GameBoard_Tb_Ca5.Text = "";
                gb.GameBoard_Tb_Ca6.Text = "";

                Btn_CA1CL1.Content = ClueValuesStr[1];
                Btn_CA1CL2.Content = ClueValuesStr[2];
                Btn_CA1CL3.Content = ClueValuesStr[3];
                Btn_CA1CL4.Content = ClueValuesStr[4];
                Btn_CA1CL5.Content = ClueValuesStr[5];

                Btn_CA1CL1.Background = Brushes.White;
                Btn_CA1CL2.Background = Brushes.White;
                Btn_CA1CL3.Background = Brushes.White;
                Btn_CA1CL4.Background = Brushes.White;
                Btn_CA1CL5.Background = Brushes.White;

                Btn_CA2CL1.Content = ClueValuesStr[1];
                Btn_CA2CL2.Content = ClueValuesStr[2];
                Btn_CA2CL3.Content = ClueValuesStr[3];
                Btn_CA2CL4.Content = ClueValuesStr[4];
                Btn_CA2CL5.Content = ClueValuesStr[5];

                Btn_CA2CL1.Background = Brushes.White;
                Btn_CA2CL2.Background = Brushes.White;
                Btn_CA2CL3.Background = Brushes.White;
                Btn_CA2CL4.Background = Brushes.White;
                Btn_CA2CL5.Background = Brushes.White;

                Btn_CA3CL1.Content = ClueValuesStr[1];
                Btn_CA3CL2.Content = ClueValuesStr[2];
                Btn_CA3CL3.Content = ClueValuesStr[3];
                Btn_CA3CL4.Content = ClueValuesStr[4];
                Btn_CA3CL5.Content = ClueValuesStr[5];

                Btn_CA3CL1.Background = Brushes.White;
                Btn_CA3CL2.Background = Brushes.White;
                Btn_CA3CL3.Background = Brushes.White;
                Btn_CA3CL4.Background = Brushes.White;
                Btn_CA3CL5.Background = Brushes.White;

                Btn_CA4CL1.Content = ClueValuesStr[1];
                Btn_CA4CL2.Content = ClueValuesStr[2];
                Btn_CA4CL3.Content = ClueValuesStr[3];
                Btn_CA4CL4.Content = ClueValuesStr[4];
                Btn_CA4CL5.Content = ClueValuesStr[5];

                Btn_CA4CL1.Background = Brushes.White;
                Btn_CA4CL2.Background = Brushes.White;
                Btn_CA4CL3.Background = Brushes.White;
                Btn_CA4CL4.Background = Brushes.White;
                Btn_CA4CL5.Background = Brushes.White;

                Btn_CA5CL1.Content = ClueValuesStr[1];
                Btn_CA5CL2.Content = ClueValuesStr[2];
                Btn_CA5CL3.Content = ClueValuesStr[3];
                Btn_CA5CL4.Content = ClueValuesStr[4];
                Btn_CA5CL5.Content = ClueValuesStr[5];

                Btn_CA5CL1.Background = Brushes.White;
                Btn_CA5CL2.Background = Brushes.White;
                Btn_CA5CL3.Background = Brushes.White;
                Btn_CA5CL4.Background = Brushes.White;
                Btn_CA5CL5.Background = Brushes.White;

                Btn_CA6CL1.Content = ClueValuesStr[1];
                Btn_CA6CL2.Content = ClueValuesStr[2];
                Btn_CA6CL3.Content = ClueValuesStr[3];
                Btn_CA6CL4.Content = ClueValuesStr[4];
                Btn_CA6CL5.Content = ClueValuesStr[5];

                Btn_CA6CL1.Background = Brushes.White;
                Btn_CA6CL2.Background = Brushes.White;
                Btn_CA6CL3.Background = Brushes.White;
                Btn_CA6CL4.Background = Brushes.White;
                Btn_CA6CL5.Background = Brushes.White;
            }
            else if (JeopardyRound == 1)
            {
                Tb_CA1.Text = "";
                Tb_CA6.Text = "";
                Tb_CA2.Text = "";
                Tb_CA3.Text = "";
                Tb_CA4.Text = "";
                Tb_CA5.Text = "";

                gb.GameBoard_Tb_Ca1.Text = "";
                gb.GameBoard_Tb_Ca2.Text = "";
                gb.GameBoard_Tb_Ca3.Text = "";
                gb.GameBoard_Tb_Ca4.Text = "";
                gb.GameBoard_Tb_Ca5.Text = "";
                gb.GameBoard_Tb_Ca6.Text = "";

                Btn_CA1CL1.Content = ClueValuesStr[6];
                gb.GB_Tb_Ca1C1.Text = ClueValuesStr[6];
                Btn_CA1CL2.Content = ClueValuesStr[7];
                gb.GB_Tb_Ca1C2.Text = ClueValuesStr[7];
                Btn_CA1CL3.Content = ClueValuesStr[8];
                gb.GB_Tb_Ca1C3.Text = ClueValuesStr[8];
                Btn_CA1CL4.Content = ClueValuesStr[9];
                gb.GB_Tb_Ca1C4.Text = ClueValuesStr[9];
                Btn_CA1CL5.Content = ClueValuesStr[10];
                gb.GB_Tb_Ca1C5.Text = ClueValuesStr[10];

                Btn_CA1CL1.Background = Brushes.White;
                Btn_CA1CL2.Background = Brushes.White;
                Btn_CA1CL3.Background = Brushes.White;
                Btn_CA1CL4.Background = Brushes.White;
                Btn_CA1CL5.Background = Brushes.White;

                Btn_CA2CL1.Content = ClueValuesStr[6];
                gb.GB_Tb_Ca2C1.Text = ClueValuesStr[6];
                Btn_CA2CL2.Content = ClueValuesStr[7];
                gb.GB_Tb_Ca2C2.Text = ClueValuesStr[7];
                Btn_CA2CL3.Content = ClueValuesStr[8];
                gb.GB_Tb_Ca2C3.Text = ClueValuesStr[8];
                Btn_CA2CL4.Content = ClueValuesStr[9];
                gb.GB_Tb_Ca2C4.Text = ClueValuesStr[9];
                Btn_CA2CL5.Content = ClueValuesStr[10];
                gb.GB_Tb_Ca2C5.Text = ClueValuesStr[10];

                Btn_CA2CL1.Background = Brushes.White;
                Btn_CA2CL2.Background = Brushes.White;
                Btn_CA2CL3.Background = Brushes.White;
                Btn_CA2CL4.Background = Brushes.White;
                Btn_CA2CL5.Background = Brushes.White;

                Btn_CA3CL1.Content = ClueValuesStr[6];
                gb.GB_Tb_Ca3C1.Text = ClueValuesStr[6];
                Btn_CA3CL2.Content = ClueValuesStr[7];
                gb.GB_Tb_Ca3C2.Text = ClueValuesStr[7];
                Btn_CA3CL3.Content = ClueValuesStr[8];
                gb.GB_Tb_Ca3C3.Text = ClueValuesStr[8];
                Btn_CA3CL4.Content = ClueValuesStr[9];
                gb.GB_Tb_Ca3C4.Text = ClueValuesStr[9];
                Btn_CA3CL5.Content = ClueValuesStr[10];
                gb.GB_Tb_Ca3C5.Text = ClueValuesStr[10];

                Btn_CA3CL1.Background = Brushes.White;
                Btn_CA3CL2.Background = Brushes.White;
                Btn_CA3CL3.Background = Brushes.White;
                Btn_CA3CL4.Background = Brushes.White;
                Btn_CA3CL5.Background = Brushes.White;

                Btn_CA4CL1.Content = ClueValuesStr[6];
                gb.GB_Tb_Ca4C1.Text = ClueValuesStr[6];
                Btn_CA4CL2.Content = ClueValuesStr[7];
                gb.GB_Tb_Ca4C2.Text = ClueValuesStr[7];
                Btn_CA4CL3.Content = ClueValuesStr[8];
                gb.GB_Tb_Ca4C3.Text = ClueValuesStr[8];
                Btn_CA4CL4.Content = ClueValuesStr[9];
                gb.GB_Tb_Ca4C4.Text = ClueValuesStr[9];
                Btn_CA4CL5.Content = ClueValuesStr[10];
                gb.GB_Tb_Ca4C5.Text = ClueValuesStr[10];

                Btn_CA4CL1.Background = Brushes.White;
                Btn_CA4CL2.Background = Brushes.White;
                Btn_CA4CL3.Background = Brushes.White;
                Btn_CA4CL4.Background = Brushes.White;
                Btn_CA4CL5.Background = Brushes.White;

                Btn_CA5CL1.Content = ClueValuesStr[6];
                gb.GB_Tb_Ca5C1.Text = ClueValuesStr[6];
                Btn_CA5CL2.Content = ClueValuesStr[7];
                gb.GB_Tb_Ca5C2.Text = ClueValuesStr[7];
                Btn_CA5CL3.Content = ClueValuesStr[8];
                gb.GB_Tb_Ca5C3.Text = ClueValuesStr[8];
                Btn_CA5CL4.Content = ClueValuesStr[9];
                gb.GB_Tb_Ca5C4.Text = ClueValuesStr[9];
                Btn_CA5CL5.Content = ClueValuesStr[10];
                gb.GB_Tb_Ca5C5.Text = ClueValuesStr[10];

                Btn_CA5CL1.Background = Brushes.White;
                Btn_CA5CL2.Background = Brushes.White;
                Btn_CA5CL3.Background = Brushes.White;
                Btn_CA5CL4.Background = Brushes.White;
                Btn_CA5CL5.Background = Brushes.White;

                Btn_CA6CL1.Content = ClueValuesStr[6];
                gb.GB_Tb_Ca6C1.Text = ClueValuesStr[6];
                Btn_CA6CL2.Content = ClueValuesStr[7];
                gb.GB_Tb_Ca6C2.Text = ClueValuesStr[7];
                Btn_CA6CL3.Content = ClueValuesStr[8];
                gb.GB_Tb_Ca6C3.Text = ClueValuesStr[8];
                Btn_CA6CL4.Content = ClueValuesStr[9];
                gb.GB_Tb_Ca6C4.Text = ClueValuesStr[9];
                Btn_CA6CL5.Content = ClueValuesStr[10];
                gb.GB_Tb_Ca6C5.Text = ClueValuesStr[10];

                Btn_CA6CL1.Background = Brushes.White;
                Btn_CA6CL2.Background = Brushes.White;
                Btn_CA6CL3.Background = Brushes.White;
                Btn_CA6CL4.Background = Brushes.White;
                Btn_CA6CL5.Background = Brushes.White;
            }


        }

        private void Rb_FinalJeopardy_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show("About to switch to Final Jeopardy. Is that okay?", "Round Switch", MessageBoxButton.YesNo);

            if (res == MessageBoxResult.Yes)
            {
                if (RoundSwitchWarn[2] != 0)
                {
                    MessageBox.Show("Warning: You have already played the Final Jeopardy round.\nAre you sure you want to go back to this round?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                }

                RoundSwitchWarn[2] = 1;

                Gr_FinalJ.IsEnabled = true;

                TexB_ClueText.Text = XMLRead.XMLGetClueText(GameFilePath, 0, 0, 3);
                Tb_ClueAnswer.Text = XMLRead.XMLGetClueAnswer(GameFilePath, 0, 0, 3);

                gb.GameBoard_Tb_Ca1.Text = " ";
                gb.GameBoard_Tb_Ca2.Text = " ";
                gb.GameBoard_Tb_Ca3.Text = " ";
                gb.GameBoard_Tb_Ca4.Text = " ";
                gb.GameBoard_Tb_Ca5.Text = " ";
                gb.GameBoard_Tb_Ca6.Text = " ";

                gb.GB_Tb_Ca1C1.Text = " ";
                gb.GB_Tb_Ca1C2.Text = " ";
                gb.GB_Tb_Ca1C3.Text = " ";
                gb.GB_Tb_Ca1C4.Text = " ";
                gb.GB_Tb_Ca1C5.Text = " ";

                gb.GB_Tb_Ca2C1.Text = " ";
                gb.GB_Tb_Ca2C2.Text = " ";
                gb.GB_Tb_Ca2C3.Text = " ";
                gb.GB_Tb_Ca2C4.Text = " ";
                gb.GB_Tb_Ca2C5.Text = " ";

                gb.GB_Tb_Ca3C1.Text = " ";
                gb.GB_Tb_Ca3C2.Text = " ";
                gb.GB_Tb_Ca3C3.Text = " ";
                gb.GB_Tb_Ca3C4.Text = " ";
                gb.GB_Tb_Ca3C5.Text = " ";

                gb.GB_Tb_Ca4C1.Text = " ";
                gb.GB_Tb_Ca4C2.Text = " ";
                gb.GB_Tb_Ca4C3.Text = " ";
                gb.GB_Tb_Ca4C4.Text = " ";
                gb.GB_Tb_Ca4C5.Text = " ";

                gb.GB_Tb_Ca5C1.Text = " ";
                gb.GB_Tb_Ca5C2.Text = " ";
                gb.GB_Tb_Ca5C3.Text = " ";
                gb.GB_Tb_Ca5C4.Text = " ";
                gb.GB_Tb_Ca5C5.Text = " ";

                gb.GB_Tb_Ca6C1.Text = " ";
                gb.GB_Tb_Ca6C2.Text = " ";
                gb.GB_Tb_Ca6C3.Text = " ";
                gb.GB_Tb_Ca6C4.Text = " ";
                gb.GB_Tb_Ca6C5.Text = " ";
            }
            else
            {
                Rb_FinalJeopardy.IsChecked = false;
            }
        }

        

        private void Btn_HideClue_Click(object sender, RoutedEventArgs e)
        {
            System.Media.SoundPlayer Timeout = new System.Media.SoundPlayer();
            Timeout.SoundLocation = @"data\sound\cluetimeout.wav";
            Timeout.Load();
            Timeout.Play();


            gb.Grid_ClueDisplay.Visibility = Visibility.Hidden;

            Btn_HideClue.IsEnabled = false;
        }

        private void Btn_CA1CL1_Click(object sender, RoutedEventArgs e)
        {
            //LoadClueAudio(ClueAudio[1]);
           // SetClueBG(CluePicture[1]);

            switch (ReadDailyDouble(1))
            {
                case 0: //No DD
                    DisplayClue(ClueText[1], 0, 0, ClueAudio[1], CluePicture[1], "nul");
                    break;
                case 1: //Daily Double Found
                    
                    DisplayClue(ClueText[1], 1, 0, ClueAudio[1], CluePicture[1], "nul");

                    ClueSpecialDD[0] = ClueText[1]; ClueSpecialDD[1] = ClueAudio[1]; ClueSpecialDD[2] = CluePicture[1]; ClueSpecialDD[3] = "nul";
                    break;
            }

            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[1];
            }
            else
            {
                ClueScore = ClueValuesInt[1] * 2;
            }

            CategoryHide[0] = CategoryHide[0] + 1;
            if(CategoryHide[0] == 5)
            {
                gb.GameBoard_Tb_Ca1.Text = "";
            }

            TexB_ClueText.Text = ClueText[1];
            Tb_ClueAnswer.Text = ClueAnswer[1];

            gb.GB_Tb_Ca1C1.Text = "";

            Btn_CA1CL1.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA1CL2_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(2))
            {
                case 0: //No DD
                    DisplayClue(ClueText[2], 0, 0, ClueAudio[2], CluePicture[2], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[2], 1, 0, ClueAudio[2], CluePicture[2], "nul");

                    ClueSpecialDD[0] = ClueText[2]; ClueSpecialDD[1] = ClueAudio[2]; ClueSpecialDD[2] = CluePicture[2]; ClueSpecialDD[3] = "nul";
                    break;
            }

            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[2];
            }
            else
            {
                ClueScore = ClueValuesInt[2] * 2;
            }

            CategoryHide[0] = CategoryHide[0] + 1;
            if (CategoryHide[0] == 5)
            {
                gb.GameBoard_Tb_Ca1.Text = "";
            }

            TexB_ClueText.Text = ClueText[2];
            Tb_ClueAnswer.Text = ClueAnswer[2];

            gb.GB_Tb_Ca1C2.Text = "";

            Btn_CA1CL2.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA1CL3_Click(object sender, RoutedEventArgs e)
        {

            switch (ReadDailyDouble(3))
            {
                case 0: //No DD
                    DisplayClue(ClueText[3], 0, 0, ClueAudio[3], CluePicture[3], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[3], 1, 0, ClueAudio[3], CluePicture[3], "nul");

                    ClueSpecialDD[0] = ClueText[3]; ClueSpecialDD[1] = ClueAudio[3]; ClueSpecialDD[2] = CluePicture[3]; ClueSpecialDD[3] = "nul";
                    break;
            }

            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[3];
            }
            else
            {
                ClueScore = ClueValuesInt[3] * 2;
            }

            CategoryHide[0] = CategoryHide[0] + 1;
            if (CategoryHide[0] == 5)
            {
                gb.GameBoard_Tb_Ca1.Text = "";
            }

            TexB_ClueText.Text = ClueText[3];
            Tb_ClueAnswer.Text = ClueAnswer[3];

            gb.GB_Tb_Ca1C3.Text = "";

            Btn_CA1CL3.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA1CL4_Click(object sender, RoutedEventArgs e)
        {
            switch(ReadDailyDouble(4))
            {
                case 0: //No DD
                    DisplayClue(ClueText[4], 0, 0, ClueAudio[4], CluePicture[4], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[4], 1, 0, ClueAudio[4], CluePicture[4], "nul");

                    ClueSpecialDD[0] = ClueText[4]; ClueSpecialDD[1] = ClueAudio[4]; ClueSpecialDD[2] = CluePicture[4]; ClueSpecialDD[3] = "nul";
                    break;           
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[4];
            }
            else
            {
                ClueScore = ClueValuesInt[4] * 2;
            }

            CategoryHide[0] = CategoryHide[0] + 1;
            if (CategoryHide[0] == 5)
            {
                gb.GameBoard_Tb_Ca1.Text = "";
            }

            TexB_ClueText.Text = ClueText[4];
            Tb_ClueAnswer.Text = ClueAnswer[4];

            gb.GB_Tb_Ca1C4.Text = "";

            Btn_CA1CL4.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA1CL5_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(5))
            {
                case 0: //No DD
                    DisplayClue(ClueText[5], 0, 0, ClueAudio[5], CluePicture[5], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[5], 1, 0, ClueAudio[5], CluePicture[5], "nul");

                    ClueSpecialDD[0] = ClueText[5]; ClueSpecialDD[1] = ClueAudio[5]; ClueSpecialDD[2] = CluePicture[5]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[5];
            }
            else
            {
                ClueScore = ClueValuesInt[5] * 2;
            }


            CategoryHide[0] = CategoryHide[0] + 1;
            if (CategoryHide[0] == 5)
            {
                gb.GameBoard_Tb_Ca1.Text = "";
            }

            TexB_ClueText.Text = ClueText[5];
            Tb_ClueAnswer.Text = ClueAnswer[5];

            gb.GB_Tb_Ca1C5.Text = "";

            Btn_CA1CL5.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA2CL1_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(6))
            {
                case 0: //No DD
                    DisplayClue(ClueText[6], 0, 0, ClueAudio[6], CluePicture[6], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[6], 1, 0, ClueAudio[6], CluePicture[6], "nul");

                    ClueSpecialDD[0] = ClueText[6]; ClueSpecialDD[1] = ClueAudio[6]; ClueSpecialDD[2] = CluePicture[6]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[1];
            }
            else
            {
                ClueScore = ClueValuesInt[1] * 2;
            }

            CategoryHide[1] = CategoryHide[1] + 1;
            if (CategoryHide[1] == 5)
            {
                gb.GameBoard_Tb_Ca2.Text = "";
            }

            TexB_ClueText.Text = ClueText[6];
            Tb_ClueAnswer.Text = ClueAnswer[6];

            gb.GB_Tb_Ca2C1.Text = "";

            Btn_CA2CL1.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA2CL2_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(7))
            {
                case 0: //No DD
                    DisplayClue(ClueText[7], 0, 0, ClueAudio[7], CluePicture[7], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[7], 1, 0, ClueAudio[7], CluePicture[7], "nul");

                    ClueSpecialDD[0] = ClueText[7]; ClueSpecialDD[1] = ClueAudio[7]; ClueSpecialDD[2] = CluePicture[7]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[2];
            }
            else
            {
                ClueScore = ClueValuesInt[2] * 2;
            }

            CategoryHide[1] = CategoryHide[1] + 1;
            if (CategoryHide[1] == 5)
            {
                gb.GameBoard_Tb_Ca2.Text = "";
            }

            TexB_ClueText.Text = ClueText[7];
            Tb_ClueAnswer.Text = ClueAnswer[7];

            gb.GB_Tb_Ca2C2.Text = "";

            Btn_CA2CL2.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA2CL3_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(8))
            {
                case 0: //No DD
                    DisplayClue(ClueText[8], 0, 0, ClueAudio[8], CluePicture[8], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[8], 1, 0, ClueAudio[8], CluePicture[8], "nul");

                    ClueSpecialDD[0] = ClueText[8]; ClueSpecialDD[1] = ClueAudio[8]; ClueSpecialDD[2] = CluePicture[8]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[3];
            }
            else
            {
                ClueScore = ClueValuesInt[3] * 2;
            }

            CategoryHide[1] = CategoryHide[1] + 1;
            if (CategoryHide[1] == 5)
            {
                gb.GameBoard_Tb_Ca2.Text = "";
            }

            TexB_ClueText.Text = ClueText[8];
            Tb_ClueAnswer.Text = ClueAnswer[8];

            gb.GB_Tb_Ca2C3.Text = "";

            Btn_CA2CL3.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA2CL4_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(9))
            {
                case 0: //No DD
                    DisplayClue(ClueText[9], 0, 0, ClueAudio[9], CluePicture[9], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[9], 1, 0, ClueAudio[9], CluePicture[9], "nul");

                    ClueSpecialDD[0] = ClueText[9]; ClueSpecialDD[1] = ClueAudio[9]; ClueSpecialDD[2] = CluePicture[9]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[4];
            }
            else
            {
                ClueScore = ClueValuesInt[4] * 2;
            }

            CategoryHide[1] = CategoryHide[1] + 1;
            if (CategoryHide[1] == 5)
            {
                gb.GameBoard_Tb_Ca2.Text = "";
            }

            TexB_ClueText.Text = ClueText[9];
            Tb_ClueAnswer.Text = ClueAnswer[9];

            gb.GB_Tb_Ca2C4.Text = "";

            Btn_CA2CL4.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA2CL5_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(10))
            {
                case 0: //No DD
                    DisplayClue(ClueText[10], 0, 0, ClueAudio[10], CluePicture[10], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[10], 1, 0, ClueAudio[10], CluePicture[10], "nul");

                    ClueSpecialDD[0] = ClueText[10]; ClueSpecialDD[1] = ClueAudio[10]; ClueSpecialDD[2] = CluePicture[10]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[5];
            }
            else
            {
                ClueScore = ClueValuesInt[5] * 2;
            }

            CategoryHide[1] = CategoryHide[1] + 1;
            if (CategoryHide[1] == 5)
            {
                gb.GameBoard_Tb_Ca2.Text = "";
            }

            TexB_ClueText.Text = ClueText[10];
            Tb_ClueAnswer.Text = ClueAnswer[10];

            gb.GB_Tb_Ca2C5.Text = "";

            Btn_CA2CL5.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA3CL1_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(11))
            {
                case 0: //No DD
                    DisplayClue(ClueText[11], 0, 0, ClueAudio[11], ClueAudio[11], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[11], 1, 0, ClueAudio[11], ClueAudio[11], "nul");

                    ClueSpecialDD[0] = ClueText[11]; ClueSpecialDD[1] = ClueAudio[11]; ClueSpecialDD[2] = CluePicture[11]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[1];
            }
            else
            {
                ClueScore = ClueValuesInt[1] * 2;
            }

            CategoryHide[2] = CategoryHide[2] + 1;
            if (CategoryHide[2] == 5)
            {
                gb.GameBoard_Tb_Ca3.Text = "";
            }

            TexB_ClueText.Text = ClueText[11];
            Tb_ClueAnswer.Text = ClueAnswer[11];

            gb.GB_Tb_Ca3C1.Text = "";

            Btn_CA3CL1.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA3CL2_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(12))
            {
                case 0: //No DD
                    DisplayClue(ClueText[12], 0, 0, ClueAudio[12], CluePicture[12], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[12], 1, 0, ClueAudio[12], CluePicture[12], "nul");

                    ClueSpecialDD[0] = ClueText[12]; ClueSpecialDD[1] = ClueAudio[12]; ClueSpecialDD[2] = CluePicture[12]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[2];
            }
            else
            {
                ClueScore = ClueValuesInt[2] * 2;
            }

            CategoryHide[2] = CategoryHide[2] + 1;
            if (CategoryHide[2] == 5)
            {
                gb.GameBoard_Tb_Ca3.Text = "";
            }

            TexB_ClueText.Text = ClueText[12];
            Tb_ClueAnswer.Text = ClueAnswer[12];

            gb.GB_Tb_Ca3C2.Text = "";

            Btn_CA3CL2.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA3CL3_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(13))
            {
                case 0: //No DD
                    DisplayClue(ClueText[13], 0, 0, ClueAudio[13], CluePicture[13], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[13], 1, 0, ClueAudio[13], CluePicture[13], "nul");

                    ClueSpecialDD[0] = ClueText[13]; ClueSpecialDD[1] = ClueAudio[13]; ClueSpecialDD[2] = CluePicture[13]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[3];
            }
            else
            {
                ClueScore = ClueValuesInt[3] * 2;
            }

            CategoryHide[2] = CategoryHide[2] + 1;
            if (CategoryHide[2] == 5)
            {
                gb.GameBoard_Tb_Ca3.Text = "";
            }

            TexB_ClueText.Text = ClueText[13];
            Tb_ClueAnswer.Text = ClueAnswer[13];

            gb.GB_Tb_Ca3C3.Text = "";

            Btn_CA3CL3.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA3CL4_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(14))
            {
                case 0: //No DD
                    DisplayClue(ClueText[14], 0, 0, ClueAudio[14], CluePicture[14], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[14], 1, 0, ClueAudio[14], CluePicture[14], "nul");

                    ClueSpecialDD[0] = ClueText[14]; ClueSpecialDD[1] = ClueAudio[14]; ClueSpecialDD[2] = CluePicture[14]; ClueSpecialDD[3] = "nul";
                    break;
            }

            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[4];
            }
            else
            {
                ClueScore = ClueValuesInt[4] * 2;
            }

            CategoryHide[2] = CategoryHide[2] + 1;
            if (CategoryHide[2] == 5)
            {
                gb.GameBoard_Tb_Ca3.Text = "";
            }

            TexB_ClueText.Text = ClueText[14];
            Tb_ClueAnswer.Text = ClueAnswer[14];

            gb.GB_Tb_Ca3C4.Text = "";

            Btn_CA3CL4.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA3CL5_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(15))
            {
                case 0: //No DD
                    DisplayClue(ClueText[15], 0, 0, ClueAudio[15], CluePicture[15], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[15], 1, 0, ClueAudio[15], CluePicture[15], "nul");

                    ClueSpecialDD[0] = ClueText[15]; ClueSpecialDD[1] = ClueAudio[15]; ClueSpecialDD[2] = CluePicture[15]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[5];
            }
            else
            {
                ClueScore = ClueValuesInt[5] * 2;
            }

            CategoryHide[2] = CategoryHide[2] + 1;
            if (CategoryHide[2] == 5)
            {
                gb.GameBoard_Tb_Ca3.Text = "";
            }

            TexB_ClueText.Text = ClueText[15];
            Tb_ClueAnswer.Text = ClueAnswer[15];

            gb.GB_Tb_Ca3C5.Text = "";

            Btn_CA3CL5.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA4CL1_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(16))
            {
                case 0: //No DD
                    DisplayClue(ClueText[16], 0, 0, ClueAudio[16], CluePicture[16], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[16], 1, 0, ClueAudio[16], CluePicture[16], "nul");

                    ClueSpecialDD[0] = ClueText[16]; ClueSpecialDD[1] = ClueAudio[16]; ClueSpecialDD[2] = CluePicture[16]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[1];
            }
            else
            {
                ClueScore = ClueValuesInt[1] * 2;
            }

            CategoryHide[3] = CategoryHide[3] + 1;
            if (CategoryHide[3] == 5)
            {
                gb.GameBoard_Tb_Ca4.Text = "";
            }

            TexB_ClueText.Text = ClueText[16];
            Tb_ClueAnswer.Text = ClueAnswer[16];

            gb.GB_Tb_Ca4C1.Text = "";

            Btn_CA4CL1.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA4CL2_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(17))
            {
                case 0: //No DD
                    DisplayClue(ClueText[17], 0, 0, ClueAudio[17], CluePicture[17], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[17], 1, 0, ClueAudio[17], CluePicture[17], "nul");

                    ClueSpecialDD[0] = ClueText[17]; ClueSpecialDD[1] = ClueAudio[17]; ClueSpecialDD[2] = CluePicture[17]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[2];
            }
            else
            {
                ClueScore = ClueValuesInt[2] * 2;
            }

            CategoryHide[3] = CategoryHide[3] + 1;
            if (CategoryHide[3] == 5)
            {
                gb.GameBoard_Tb_Ca4.Text = "";
            }

            TexB_ClueText.Text = ClueText[17];
            Tb_ClueAnswer.Text = ClueAnswer[17];

            gb.GB_Tb_Ca4C2.Text = "";

            Btn_CA4CL2.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA4CL3_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(18))
            {
                case 0: //No DD
                    DisplayClue(ClueText[18], 0, 0, ClueAudio[18], CluePicture[18], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[18], 1, 0, ClueAudio[18], CluePicture[18], "nul");

                    ClueSpecialDD[0] = ClueText[18]; ClueSpecialDD[1] = ClueAudio[18]; ClueSpecialDD[2] = CluePicture[18]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[3];
            }
            else
            {
                ClueScore = ClueValuesInt[3] * 2;
            }

            CategoryHide[3] = CategoryHide[3] + 1;
            if (CategoryHide[3] == 5)
            {
                gb.GameBoard_Tb_Ca4.Text = "";
            }

            TexB_ClueText.Text = ClueText[18];
            Tb_ClueAnswer.Text = ClueAnswer[18];

            gb.GB_Tb_Ca4C3.Text = "";

            Btn_CA4CL3.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA4CL4_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(19))
            {
                case 0: //No DD
                    DisplayClue(ClueText[19], 0, 0, ClueAudio[19], CluePicture[19], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[19], 1, 0, ClueAudio[19], CluePicture[19], "nul");

                    ClueSpecialDD[0] = ClueText[19]; ClueSpecialDD[1] = ClueAudio[19]; ClueSpecialDD[2] = CluePicture[19]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[4];
            }
            else
            {
                ClueScore = ClueValuesInt[4] * 2;
            }

            CategoryHide[3] = CategoryHide[3] + 1;
            if (CategoryHide[3] == 5)
            {
                gb.GameBoard_Tb_Ca4.Text = "";
            }

            TexB_ClueText.Text = ClueText[19];
            Tb_ClueAnswer.Text = ClueAnswer[19];

            gb.GB_Tb_Ca4C4.Text = "";

            Btn_CA4CL4.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA4CL5_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(20))
            {
                case 0: //No DD
                    DisplayClue(ClueText[20], 0, 0, ClueAudio[20], CluePicture[20], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[20], 1, 0, ClueAudio[20], CluePicture[20], "nul");

                    ClueSpecialDD[0] = ClueText[20]; ClueSpecialDD[1] = ClueAudio[20]; ClueSpecialDD[2] = CluePicture[20]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[5];
            }
            else
            {
                ClueScore = ClueValuesInt[5] * 2;
            }

            CategoryHide[3] = CategoryHide[3] + 1;
            if (CategoryHide[3] == 5)
            {
                gb.GameBoard_Tb_Ca4.Text = "";
            }

            TexB_ClueText.Text = ClueText[20];
            Tb_ClueAnswer.Text = ClueAnswer[20];

            gb.GB_Tb_Ca4C5.Text = "";

            Btn_CA4CL5.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA5CL1_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(21))
            {
                case 0: //No DD
                    DisplayClue(ClueText[21], 0, 0, ClueAudio[21], CluePicture[21], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[21], 1, 0, ClueAudio[21], CluePicture[21], "nul");

                    ClueSpecialDD[0] = ClueText[21]; ClueSpecialDD[1] = ClueAudio[21]; ClueSpecialDD[2] = CluePicture[21]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[1];
            }
            else
            {
                ClueScore = ClueValuesInt[1] * 2;
            }

            CategoryHide[4] = CategoryHide[4] + 1;
            if (CategoryHide[4] == 5)
            {
                gb.GameBoard_Tb_Ca5.Text = "";
            }

            TexB_ClueText.Text = ClueText[21];
            Tb_ClueAnswer.Text = ClueAnswer[21];

            gb.GB_Tb_Ca5C1.Text = "";

            Btn_CA5CL1.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA5CL2_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(22))
            {
                case 0: //No DD
                    DisplayClue(ClueText[22], 0, 0, ClueAudio[22], CluePicture[22], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[22], 1, 0, ClueAudio[22], CluePicture[22], "nul");

                    ClueSpecialDD[0] = ClueText[22]; ClueSpecialDD[1] = ClueAudio[22]; ClueSpecialDD[2] = CluePicture[22]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[2];
            }
            else
            {
                ClueScore = ClueValuesInt[2] * 2;
            }

            CategoryHide[4] = CategoryHide[4] + 1;
            if (CategoryHide[4] == 5)
            {
                gb.GameBoard_Tb_Ca5.Text = "";
            }

            TexB_ClueText.Text = ClueText[22];
            Tb_ClueAnswer.Text = ClueAnswer[22];

            gb.GB_Tb_Ca5C2.Text = "";

            Btn_CA5CL2.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA5CL3_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(23))
            {
                case 0: //No DD
                    DisplayClue(ClueText[23], 0, 0, ClueAudio[23], CluePicture[23], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[23], 1, 0, ClueAudio[23], CluePicture[23], "nul");

                    ClueSpecialDD[0] = ClueText[23]; ClueSpecialDD[1] = ClueAudio[23]; ClueSpecialDD[2] = CluePicture[23]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[3];
            }
            else
            {
                ClueScore = ClueValuesInt[3] * 2;
            }

            CategoryHide[4] = CategoryHide[4] + 1;
            if (CategoryHide[4] == 5)
            {
                gb.GameBoard_Tb_Ca5.Text = "";
            }

            TexB_ClueText.Text = ClueText[23];
            Tb_ClueAnswer.Text = ClueAnswer[23];

            gb.GB_Tb_Ca5C3.Text = "";

            Btn_CA5CL3.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA5CL4_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(24))
            {
                case 0: //No DD
                    DisplayClue(ClueText[24], 0, 0, ClueAudio[24], CluePicture[24], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[24], 1, 0, ClueAudio[24], CluePicture[24], "nul");

                    ClueSpecialDD[0] = ClueText[24]; ClueSpecialDD[1] = ClueAudio[24]; ClueSpecialDD[2] = CluePicture[24]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[4];
            }
            else
            {
                ClueScore = ClueValuesInt[4] * 2;
            }

            CategoryHide[4] = CategoryHide[4] + 1;
            if (CategoryHide[4] == 5)
            {
                gb.GameBoard_Tb_Ca5.Text = "";
            }

            TexB_ClueText.Text = ClueText[24];
            Tb_ClueAnswer.Text = ClueAnswer[24];

            gb.GB_Tb_Ca5C4.Text = "";

            Btn_CA5CL4.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA5CL5_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(25))
            {
                case 0: //No DD
                    DisplayClue(ClueText[25], 0, 0, ClueAudio[25], CluePicture[25], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[25], 1, 0, ClueAudio[25], CluePicture[25], "nul");

                    ClueSpecialDD[0] = ClueText[25]; ClueSpecialDD[1] = ClueAudio[25]; ClueSpecialDD[2] = CluePicture[25]; ClueSpecialDD[3] = "nul";
                    break;
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[5];
            }
            else
            {
                ClueScore = ClueValuesInt[5] * 2;
            }

            CategoryHide[4] = CategoryHide[4] + 1;
            if (CategoryHide[4] == 5)
            {
                gb.GameBoard_Tb_Ca5.Text = "";
            }

            TexB_ClueText.Text = ClueText[25];
            Tb_ClueAnswer.Text = ClueAnswer[25];

            gb.GB_Tb_Ca5C5.Text = "";

            Btn_CA5CL5.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA6CL1_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(26))
            {
                case 0: //No DD
                    DisplayClue(ClueText[26], 0, 0, ClueAudio[26], CluePicture[26], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[26], 1, 0, ClueAudio[26], CluePicture[26], "nul");

                    ClueSpecialDD[0] = ClueText[26]; ClueSpecialDD[1] = ClueAudio[26]; ClueSpecialDD[2] = CluePicture[26]; ClueSpecialDD[3] = "nul";
                    break;
            }

            if(ClueVideo[26] != "nul")
            {
                PlayVideoAndDispose(ClueVideo[26]);
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[1];
            }
            else
            {
                ClueScore = ClueValuesInt[1] * 2;
            }

            CategoryHide[5] = CategoryHide[5] + 1;
            if (CategoryHide[5] == 5)
            {
                gb.GameBoard_Tb_Ca6.Text = "";
            }

            TexB_ClueText.Text = ClueText[26];
            Tb_ClueAnswer.Text = ClueAnswer[26];

            gb.GB_Tb_Ca6C1.Text = "";

            Btn_CA6CL1.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA6CL2_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(27))
            {
                case 0: //No DD
                    DisplayClue(ClueText[27], 0, 0, ClueAudio[27], CluePicture[27], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[27], 1, 0, ClueAudio[27], CluePicture[27], "nul");

                    ClueSpecialDD[0] = ClueText[27]; ClueSpecialDD[1] = ClueAudio[27]; ClueSpecialDD[2] = CluePicture[27]; ClueSpecialDD[3] = "nul";
                    break;
            }

            if (ClueVideo[27] != "nul")
            {
                PlayVideoAndDispose(ClueVideo[27]);
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[2];
            }
            else
            {
                ClueScore = ClueValuesInt[2] * 2;
            }

            CategoryHide[5] = CategoryHide[5] + 1;
            if (CategoryHide[5] == 5)
            {
                gb.GameBoard_Tb_Ca6.Text = "";
            }

            TexB_ClueText.Text = ClueText[27];
            Tb_ClueAnswer.Text = ClueAnswer[27];

            gb.GB_Tb_Ca6C2.Text = "";

            Btn_CA6CL2.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA6CL3_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(28))
            {
                case 0: //No DD
                    DisplayClue(ClueText[28], 0, 0, ClueAudio[28], CluePicture[28], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[28], 1, 0, ClueAudio[28], CluePicture[28], "nul");

                    ClueSpecialDD[0] = ClueText[28]; ClueSpecialDD[1] = ClueAudio[28]; ClueSpecialDD[2] = CluePicture[28]; ClueSpecialDD[3] = "nul";
                    break;
            }

            if (ClueVideo[28] != "nul")
            {
                PlayVideoAndDispose(ClueVideo[28]);
            }

            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[3];
            }
            else
            {
                ClueScore = ClueValuesInt[3] * 2;
            }

            CategoryHide[5] = CategoryHide[5] + 1;
            if (CategoryHide[5] == 5)
            {
                gb.GameBoard_Tb_Ca6.Text = "";
            }

            TexB_ClueText.Text = ClueText[28];
            Tb_ClueAnswer.Text = ClueAnswer[28];

            gb.GB_Tb_Ca6C3.Text = "";

            Btn_CA6CL3.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA6CL4_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(29))
            {
                case 0: //No DD
                    DisplayClue(ClueText[29], 0, 0, ClueAudio[29], CluePicture[29], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[29], 1, 0, ClueAudio[29], CluePicture[29], "nul");

                    ClueSpecialDD[0] = ClueText[29]; ClueSpecialDD[1] = ClueAudio[29]; ClueSpecialDD[2] = CluePicture[29]; ClueSpecialDD[3] = "nul";
                    break;
            }

            if (ClueVideo[29] != "nul")
            {
                PlayVideoAndDispose(ClueVideo[29]);
            }


            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[4];
            }
            else
            {
                ClueScore = ClueValuesInt[4] * 2;
            }

            CategoryHide[5] = CategoryHide[5] + 1;
            if (CategoryHide[5] == 5)
            {
                gb.GameBoard_Tb_Ca6.Text = "";
            }

            TexB_ClueText.Text = ClueText[29];
            Tb_ClueAnswer.Text = ClueAnswer[29];

            gb.GB_Tb_Ca6C4.Text = "";

            Btn_CA6CL4.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_CA6CL5_Click(object sender, RoutedEventArgs e)
        {
            switch (ReadDailyDouble(30))
            {
                case 0: //No DD
                    DisplayClue(ClueText[30], 0, 0, ClueAudio[30], CluePicture[30], "nul");
                    break;
                case 1: //Daily Double Found
                    DisplayClue(ClueText[30], 1, 0, ClueAudio[30], CluePicture[30], "nul");

                    ClueSpecialDD[0] = ClueText[30]; ClueSpecialDD[1] = ClueAudio[30]; ClueSpecialDD[2] = CluePicture[30]; ClueSpecialDD[3] = "nul";
                    break;
            }

            if (ClueVideo[30] != "nul")
            {
                PlayVideoAndDispose(ClueVideo[30]);
            }

            if (JeopardyRound == 0)
            {
                ClueScore = ClueValuesInt[5];
            }
            else
            {
                ClueScore = ClueValuesInt[5] * 2;
            }

            CategoryHide[5] = CategoryHide[5] + 1;
            if (CategoryHide[5] == 5)
            {
                gb.GameBoard_Tb_Ca6.Text = "";
            }

            TexB_ClueText.Text = ClueText[30];
            Tb_ClueAnswer.Text = ClueAnswer[30];

            gb.GB_Tb_Ca6C5.Text = "";

            Btn_CA6CL5.Background = Brushes.Red;

            Btn_HideClue.IsEnabled = true;
        }

        private void Btn_SetP1Name_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(@"data\local\p1", Tb_P1Name.Text);

            gb.GB_Lbl_P1Name.Content = Tb_P1Name.Text;
        }

        private void Btn_SetP2Name_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(@"data\local\p2", Tb_P2Name.Text);

            gb.GB_Lbl_P2Name.Content = Tb_P2Name.Text;
        }

        private void Btn_SetP3Name_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(@"data\local\p3", Tb_P3Name.Text);

            gb.GB_Lbl_P3Name.Content = Tb_P3Name.Text;
        }

        private void Btn_P1Add_Click(object sender, RoutedEventArgs e)
        {
            PlayerScores[0] = PlayerScores[0] + ClueScore;

            CloseClueDisplay(1);

            Lbl_P1Score.Content = "$" + PlayerScores[0].ToString();

            File.WriteAllText(@"data\local\score1", PlayerScores[0].ToString());

            Tb_P1Name.Background = new SolidColorBrush(Colors.Aquamarine);
            Tb_P2Name.Background = new SolidColorBrush(Colors.White);
            Tb_P3Name.Background = new SolidColorBrush(Colors.White);

            gb.GB_P1_Correct.Content = "●";
            gb.GB_P2_Correct.Content = " ";
            gb.GB_P3_Correct.Content = " ";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_P2Lose_Click(object sender, RoutedEventArgs e)
        {
            PlayerScores[0] = PlayerScores[0] - ClueScore;

            Lbl_P1Score.Content = "$" + PlayerScores[0].ToString();

            File.WriteAllText(@"data\local\score1", PlayerScores[0].ToString());
        }

        private void Btn_P1DDWagerAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerScores[0] = PlayerScores[0] + Convert.ToInt32(Tb_P1DDWager.Text);

                Lbl_P1Score.Content = "$" + PlayerScores[0].ToString();

                if (ClueOpen == 1)
                {
                    CloseClueDisplay(1);
                }

                File.WriteAllText(@"data\local\score1", PlayerScores[0].ToString());
            }
            catch
            {
                GlobalErrorHandler(0, null);
            }

            Tb_P1DDWager.Text = "0";
        }

        private void Btn_P1DDWagerLose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerScores[0] = PlayerScores[0] - Convert.ToInt32(Tb_P1DDWager.Text);

                Lbl_P1Score.Content = "$" + PlayerScores[0].ToString();



                if (ClueOpen == 1)
                {
                    CloseClueDisplay(1);
                }

                File.WriteAllText(@"data\local\score1", PlayerScores[0].ToString());
            }
            catch
            {
                GlobalErrorHandler(0, null);
            }

            Tb_P1DDWager.Text = "0";
        }

        private void Btn_P2Add_Click(object sender, RoutedEventArgs e)
        {
            PlayerScores[1] = PlayerScores[1] + ClueScore;

            CloseClueDisplay(1);

            Lbl_P2Score.Content = "$" + PlayerScores[1].ToString();

            File.WriteAllText(@"data\local\score2", PlayerScores[1].ToString());

            Tb_P2Name.Background = new SolidColorBrush(Colors.Aquamarine);
            Tb_P1Name.Background = new SolidColorBrush(Colors.White);
            Tb_P3Name.Background = new SolidColorBrush(Colors.White);

            gb.GB_P1_Correct.Content = " ";
            gb.GB_P2_Correct.Content = "●";
            gb.GB_P3_Correct.Content = " ";
        }

        private void Btn_P2Lose_Click_1(object sender, RoutedEventArgs e)
        {
            PlayerScores[1] = PlayerScores[1] - ClueScore;

            Lbl_P2Score.Content = "$" + PlayerScores[1].ToString();

            File.WriteAllText(@"data\local\score2", PlayerScores[1].ToString());
        }

        private void Btn_P2DDWagerAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerScores[1] = PlayerScores[1] + Convert.ToInt32(Tb_P2DDWager.Text);

                Lbl_P2Score.Content = "$" + PlayerScores[1].ToString();



                if (ClueOpen == 1)
                {
                    CloseClueDisplay(1);
                }

                File.WriteAllText(@"data\local\score2", PlayerScores[1].ToString());
            }
            catch
            {
                GlobalErrorHandler(0, null);
            }

            Tb_P2DDWager.Text = "0";
        }

        private void Btn_P2DDWagerLose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerScores[1] = PlayerScores[1] - Convert.ToInt32(Tb_P2DDWager.Text);

                Lbl_P2Score.Content = "$" + PlayerScores[1].ToString();

                if (ClueOpen == 1)
                {
                    CloseClueDisplay(1);
                }

                File.WriteAllText(@"data\local\score2", PlayerScores[1].ToString());
            }
            catch
            {
                GlobalErrorHandler(0, null);
            }

            Tb_P2DDWager.Text = "0";
        }

        private void Btn_P3Add_Click(object sender, RoutedEventArgs e)
        {
            PlayerScores[2] = PlayerScores[2] + ClueScore;

            CloseClueDisplay(1);

            Lbl_P3Score.Content = "$" + PlayerScores[2].ToString();

            File.WriteAllText(@"data\local\score3", PlayerScores[2].ToString());

            Tb_P3Name.Background = new SolidColorBrush(Colors.Aquamarine);
            Tb_P1Name.Background = new SolidColorBrush(Colors.White);
            Tb_P2Name.Background = new SolidColorBrush(Colors.White);

            gb.GB_P1_Correct.Content = " ";
            gb.GB_P2_Correct.Content = " ";
            gb.GB_P3_Correct.Content = "●";
        }

        private void Btn_P3Lose_Click(object sender, RoutedEventArgs e)
        {
            PlayerScores[2] = PlayerScores[2] - ClueScore;

            Lbl_P3Score.Content = "$" + PlayerScores[2].ToString();

            File.WriteAllText(@"data\local\score3", PlayerScores[2].ToString());
        }

        private void Btn_P3DDWagerAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerScores[2] = PlayerScores[2] + Convert.ToInt32(Tb_P3DDWager.Text);

                Lbl_P3Score.Content = "$" + PlayerScores[2].ToString();

                if (ClueOpen == 1)
                {
                    CloseClueDisplay(1);
                }

                File.WriteAllText(@"data\local\score3", PlayerScores[2].ToString());
            }
            catch
            {
                GlobalErrorHandler(0, null);
            }


            Tb_P3DDWager.Text = "0";
        }

        private void Btn_P3DDWagerLose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlayerScores[2] = PlayerScores[2] - Convert.ToInt32(Tb_P3DDWager.Text);

                Lbl_P3Score.Content = "$" + PlayerScores[2].ToString();

                if (ClueOpen == 1)
                {
                    CloseClueDisplay(1);
                }

                File.WriteAllText(@"data\local\score3", PlayerScores[2].ToString());
            }
            catch
            {
                GlobalErrorHandler(0, null);
            }


            Tb_P3DDWager.Text = "0";
        }

        private void Rb_Jeopardy_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            int RndVal = rnd.Next(1, 30);

            MessageBox.Show(RndVal.ToString());
        }

        private void Btn_LoadGame_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Jeopardy Games (*.jep)|*.jep|All Files (*.*)|*.*";
            ofd.ShowDialog();

            if(ofd.FileName != "")
            {
                GameFilePath = ofd.FileName;

                if (XMLRead.XMLTest(ofd.FileName) != -1)
                {
                    Lbl_GameName.Content = XMLRead.XMLGetGameTitle(GameFilePath);

                    LoadClueArrays(0);
                    JeopardyControl.IsEnabled = true;
                }
            }
        }
        void GlobalErrorHandler(int err, string debug)
        {
            switch(err)
            {
                case 0:
                    MessageBox.Show("Wagers must be a number with no special formatting.\ni.e., a $2,000 wager must be entered exactly as:\n2000", "Invalid Format", MessageBoxButton.OK);
                    break;
                default:
                    MessageBox.Show("An unknown error occurred!\nGlobalErrorHandler last called in function: " + debug, "Unknown!", MessageBoxButton.OK);
                    break;
            }
        }

        void CheckCategoryHide(int CategoryIndex,int ClueDisplayedIndex)
        {

        }

        int ReadDailyDouble(int Clue)
        {
            if(DailyDoubleLoc.Contains(Clue))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        void CloseClueDisplay(int Silent)
        {
            ClueOpen = 0;

            gb.Grid_ClueDisplay.Visibility = Visibility.Hidden;
        }

        void SetClueBG(string bg, int a)
        {
            var ClueBG = new ImageBrush();

            try
            {
                if (bg == "nul")
                {
                    ClueBG.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\cluebg.png", UriKind.Absolute));

                    gb.TB_BGText.Visibility = Visibility.Visible;
                    gb.TB_FGText.Visibility = Visibility.Visible;
                }
                else
                {
                    ClueBG.ImageSource = new BitmapImage(new Uri(bg, UriKind.Absolute));

                    gb.TB_BGText.Visibility = Visibility.Hidden;
                    gb.TB_FGText.Visibility = Visibility.Hidden;
                }
            }
            catch
            {
                MessageBox.Show("Exception in MainWindow.xaml.cs:SetClueBG(), setting to default bg/text\n\ncouldn't set " + bg);

                ClueBG.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\cluebg.png", UriKind.Absolute));
                gb.TB_BGText.Visibility = Visibility.Visible;
                gb.TB_FGText.Visibility = Visibility.Visible;
            }

            gb.Grid_ClueDisplay.Background = ClueBG;
        }

        void LoadClueAudio(string FilePath)
        {
            //MessageBox.Show(FilePath);

            if(FilePath != "nul")
            {
                GPAudio.SoundLocation = @FilePath;
                GPAudio.Load();

                Btn_PlayAudio.IsEnabled = true;
            }
        }

        private void Btn_RevealDD_Click(object sender, RoutedEventArgs e)
        {
            //SetClueBG(null);

            gb.TB_BGText.Visibility = Visibility.Visible;
            gb.TB_FGText.Visibility = Visibility.Visible;

            if (ClueSpecialDD[2] != "nul")
            {
                gb.TB_BGText.Visibility = Visibility.Hidden;
                gb.TB_FGText.Visibility = Visibility.Hidden;

                DisplayClue("", 0, 0, ClueSpecialDD[1], ClueSpecialDD[2], ClueSpecialDD[3]);
            }
            else
            {
                DisplayClue(TexB_ClueText.Text, 0, 0, ClueSpecialDD[1], "nul", ClueSpecialDD[3]);
            }

            ClueSpecialDD[2] = "nul";


        }

        private void Rb_DoubleJeopardy_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Btn_FJCatReveal_Click(object sender, RoutedEventArgs e)
        {
            gb.TB_BGText.FontFamily = new System.Windows.Media.FontFamily("Helvetica LT Compressed");
            gb.TB_FGText.FontFamily = new System.Windows.Media.FontFamily("Helvetica LT Compressed");
            gb.TB_BGText.FontSize = 250;
            gb.TB_FGText.FontSize = 250;

            System.Media.SoundPlayer FP1 = new System.Media.SoundPlayer();
            FP1.SoundLocation = @"data\sound\final_ping.wav";
            FP1.Load();
            FP1.Play();

            DisplayClue(XMLRead.XMLGetClueText(GameFilePath, 0, 0, 2), 2, 0, "nul", "nul", "nul");
        }

        private void Btn_FJClueReveal_Click(object sender, RoutedEventArgs e)
        {
            string ClueText = XMLRead.XMLGetClueText(GameFilePath, 0, 0, 3);

            gb.TB_BGText.FontFamily = new System.Windows.Media.FontFamily("Korinna BT");
            gb.TB_FGText.FontFamily = new System.Windows.Media.FontFamily("Korinna BT");
            gb.TB_BGText.FontSize = 100;
            gb.TB_FGText.FontSize = 100;

            gb.TB_BGText.Text = ClueText;
            gb.TB_FGText.Text = ClueText;

            System.Media.SoundPlayer FP1 = new System.Media.SoundPlayer();
            FP1.SoundLocation = @"data\sound\final_ping.wav";
            FP1.Load();
            FP1.Play();
        }

        private void Btn_FJStart_Click(object sender, RoutedEventArgs e)
        {
            System.Media.SoundPlayer FinalMusic = new System.Media.SoundPlayer();
            FinalMusic.SoundLocation = @"data\sound\think.wav";
            FinalMusic.Load();
            FinalMusic.Play();
        }

        private void Btn_PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            GPAudio.Play();

            Btn_PlayAudio.IsEnabled = false;
        }

        private void RS232TestButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Please make sure the Game Show Controller is connected to the selected port and that the Controller is set to:\n\n9600 8-N-1\n\nPress OK when ready to test");
        }

        private void Cb_FillDblPoints_Checked(object sender, RoutedEventArgs e)
        {
            gb.Img_SplashScr.Visibility = Visibility.Hidden;

            gb.GB_Tb_Ca1C1.Text = "$200";
            gb.GB_Tb_Ca1C2.Text = "$400";
            gb.GB_Tb_Ca1C3.Text = "$600";
            gb.GB_Tb_Ca1C4.Text = "$800";
            gb.GB_Tb_Ca1C5.Text = "$1000";

            gb.GB_Tb_Ca2C1.Text = "$200";
            gb.GB_Tb_Ca2C2.Text = "$400";
            gb.GB_Tb_Ca2C3.Text = "$600";
            gb.GB_Tb_Ca2C4.Text = "$800";
            gb.GB_Tb_Ca2C5.Text = "$1000";

            gb.GB_Tb_Ca3C1.Text = "$200";
            gb.GB_Tb_Ca3C2.Text = "$400";
            gb.GB_Tb_Ca3C3.Text = "$600";
            gb.GB_Tb_Ca3C4.Text = "$800";
            gb.GB_Tb_Ca3C5.Text = "$1000";

            gb.GB_Tb_Ca4C1.Text = "$200";
            gb.GB_Tb_Ca4C2.Text = "$400";
            gb.GB_Tb_Ca4C3.Text = "$600";
            gb.GB_Tb_Ca4C4.Text = "$800";
            gb.GB_Tb_Ca4C5.Text = "$1000";

            gb.GB_Tb_Ca5C1.Text = "$200";
            gb.GB_Tb_Ca5C2.Text = "$400";
            gb.GB_Tb_Ca5C3.Text = "$600";
            gb.GB_Tb_Ca5C4.Text = "$800";
            gb.GB_Tb_Ca5C5.Text = "$1000";

            gb.GB_Tb_Ca6C1.Text = "$200";
            gb.GB_Tb_Ca6C2.Text = "$400";
            gb.GB_Tb_Ca6C3.Text = "$600";
            gb.GB_Tb_Ca6C4.Text = "$800";
            gb.GB_Tb_Ca6C5.Text = "$1000";
        }

        private void Btn_SponsorVideo1_Click(object sender, RoutedEventArgs e)
        {
            PlayVideoAndDispose("C:\\Videos\\Vid1.mp4");
        }

        private void Btn_SponsorVideo2_Click(object sender, RoutedEventArgs e)
        {
            PlayVideoAndDispose("C:\\Videos\\Vid2.mp4");
        }

        private void Btn_SponsorVideo3_Click(object sender, RoutedEventArgs e)
        {
            PlayVideoAndDispose("C:\\Videos\\Vid3.mp4");
        }

        private void Btn_SponsorVideo4_Click(object sender, RoutedEventArgs e)
        {
            PlayVideoAndDispose("C:\\Videos\\Vid4.mp4");
        }
    }
}
