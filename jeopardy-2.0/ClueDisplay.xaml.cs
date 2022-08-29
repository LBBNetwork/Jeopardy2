/* Filename: ClueDisplay.xaml.cs
   Author: neko2k (neko2k@beige-box.com)
   Website: http://www.beige-box.com
   Description: Test form? Might become actual clue displayer



   The following source code is CONFIDENTIAL and PROPRIETARY PROPERTY
   of The Little Beige Box and MAY NOT BE RELEASED under PENALTY OF LAW.

   This file Copyright (c) 2019 The Little Beige Box.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace jeopardy_2._0
{
    /// <summary>
    /// Interaction logic for ClueDisplay.xaml
    /// </summary>
    public partial class ClueDisplay : Window
    {
        public string ClueTextArg { get; set; }
        public int DailyDoubleArg { get; set; }
        public int NetworkEnabled { get; set; }

        //NetworkCode NC = new NetworkCode();

        DispatcherTimer DT = new DispatcherTimer();

        MediaPlayer ClueTimeout = new MediaPlayer();

        ReadXML XMLRead = new ReadXML();

        public ClueDisplay(string ClueText, int DailyDouble, int NetworkEnabled)
        {
            /* ClueText: The text of the clue from the ClueText array
             * DailyDouble: 0 for regular clues, 1 if Daily Double
             */

            InitializeComponent();

            this.ClueTextArg = ClueText;
            this.DailyDoubleArg = DailyDouble;
            this.NetworkEnabled = NetworkEnabled;

        }

         /*   TB_FGText.Text = ClueText;
            TB_BGText.Text = ClueText;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Img_DailyDouble.Visibility = Visibility.Collapsed;

            System.Media.SoundPlayer DD = new System.Media.SoundPlayer();

            CD_Lbl_P1Name.Content = File.ReadAllText(@"data\local\p1");
            CD_Lbl_P2Name.Content = File.ReadAllText(@"data\local\p2");
            CD_Lbl_P3Name.Content = File.ReadAllText(@"data\local\p3");

            if (DailyDoubleArg == 1) //Display Daily Double
            {
                

                try
                {
                    var DDImg = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\dailydouble.png", UriKind.Absolute);
                    Img_DailyDouble.Source = new BitmapImage(DDImg);
                }
                catch
                {
                    var DDImg = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\default\dd_default.png", UriKind.Absolute);
                    Img_DailyDouble.Source = new BitmapImage(DDImg);
                }
                // = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\dailydouble.png", UriKind.Absolute);
                

                Grid_PlayerScores.Visibility = Visibility.Visible;

                Img_DailyDouble.Visibility = Visibility.Visible;

               // System.Media.SoundPlayer DD = new System.Media.SoundPlayer();
                DD.SoundLocation = @"data\sound\dailydouble.wav";
                DD.Load();
                DD.Play();
            }
            else if(DailyDoubleArg == 2) //FJ Category Reveal
            {
                Grid_PlayerScores.Visibility = Visibility.Visible;

                Lbl_Fg.FontFamily = new System.Windows.Media.FontFamily("Helvetica LT Compressed");
                Lbl_Bg.FontFamily = new System.Windows.Media.FontFamily("Helvetica LT Compressed");
                Lbl_Fg.FontSize = 250;
                Lbl_Bg.FontSize = 250;

               // TB_BGText.Text = "EGYPTIAN MYTHOLOGY";
              //  TB_FGText.Text = "EGYPTIAN MYTHOLOGY";

                DD.SoundLocation = @"data\sound\final_ping.wav";
                DD.Load();
                DD.Play();
            }

            try
            {
                Screen scr = Screen.AllScreens[1];

                System.Drawing.Rectangle rect = scr.WorkingArea;
                this.Top = rect.Top;
                this.Left = rect.Left;
            }
            catch
            {
                Screen scr = Screen.AllScreens[0];

                System.Drawing.Rectangle rect = scr.WorkingArea;
                this.Top = rect.Top;
                this.Left = rect.Left;
            }

            DT.Interval = TimeSpan.FromMilliseconds(250);
            DT.Tick += CloseWindow;
            DT.Start();
        }

        // UGLY HACK BLAH
        void CloseWindow(object sender, EventArgs e)
        {
            try
            {
                //if(NetworkEnabled != 1)
                //{
                    if (File.Exists(@"data\local\closeclue"))
                    {
                        DT.Stop();

                        File.Delete(@"data\local\closeclue");
                        File.Delete(@"data\local\clueopen");

                        System.Media.SoundPlayer Timeout = new System.Media.SoundPlayer();
                        Timeout.SoundLocation = @"data\sound\cluetimeout.wav";
                        Timeout.Load();
                        Timeout.Play();

                        Close();
                    }
                    else if(File.Exists(@"data\local\closecluesilent"))
                    {
                        DT.Stop();

                        File.Delete(@"data\local\closecluesilent");
                        File.Delete(@"data\local\clueopen");

                        Close();
                    }
                    else if (File.Exists(@"data\local\dd"))
                    {
                        File.Delete(@"data\local\dd");

                        Img_DailyDouble.Visibility = Visibility.Collapsed;
                        Grid_PlayerScores.Visibility = Visibility.Collapsed;
                    }
                    else if(File.Exists(@"data\local\fjclue"))
                    {

                        File.Delete(@"data\local\fjclue");

                        Grid_PlayerScores.Visibility = Visibility.Visible;

                        Lbl_Fg.FontFamily = new System.Windows.Media.FontFamily("Korinna BT");
                        Lbl_Bg.FontFamily = new System.Windows.Media.FontFamily("Korinna BT");
                        Lbl_Fg.FontSize = 100;
                        Lbl_Bg.FontSize = 100;

                        string fj = System.IO.File.ReadAllText("fjpath.txt");

                        TB_BGText.Text = XMLRead.XMLGetClueText(fj, 0, 0, 3);
                        TB_FGText.Text = XMLRead.XMLGetClueText(fj, 0, 0, 3);

                        System.Media.SoundPlayer DD = new System.Media.SoundPlayer();

                        DD.SoundLocation = @"data\sound\final_ping.wav";
                        DD.Load();
                        DD.Play();
                    }

                    CD_Lbl_P1Score.Content = "$" + File.ReadAllText(@"data\local\score1");
                    CD_Lbl_P2Score.Content = "$" + File.ReadAllText(@"data\local\score2");
                    CD_Lbl_P3Score.Content = "$" + File.ReadAllText(@"data\local\score3");
                //}
                /*else
                {
                    if(NC.NCReadCloseClueDisplay(0) == @"G:\jeopardy\misc\closeclue")
                    {
                        DT.Stop();

                       // File.Delete(@"G:\jeopardy\misc\closeclue");

                        System.Media.SoundPlayer Timeout = new System.Media.SoundPlayer();
                        Timeout.SoundLocation = @"data\sound\cluetimeout.wav";
                        Timeout.Load();
                        Timeout.Play();

                        Close();
                    }
                    else if(NC.NCReadCloseClueDisplay(1) == @"G:\jeopardy\misc\closecluesilent")
                    {
                        DT.Stop();
                        Close();
                    }

                }

            }
            catch (System.Threading.Tasks.TaskCanceledException)
            {
                DT.Stop();
                Close();
            }
        }*/

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DT.Stop();
        }
    }
}
