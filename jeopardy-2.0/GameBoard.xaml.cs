/* Filename: GameBoard.xaml.cs
   Author: neko2k (neko2k@beige-box.com)
   Website: http://www.beige-box.com
   Description: Code-behind for GameBoard.xaml



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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Threading;
using System.IO;

namespace jeopardy_2._0
{
    /// <summary>
    /// Interaction logic for GameBoard.xaml
    /// </summary>
    public partial class GameBoard : Window
    {
        DispatcherTimer GBTimer = new DispatcherTimer();

        public GameBoard()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {


            try
            {
                Screen scr = Screen.AllScreens[0];

                System.Drawing.Rectangle rect = scr.WorkingArea;

#if DEBUGSCREEN
                System.Windows.MessageBox.Show("This version of Jeopardy was compiled with DEBUGSCREEN set to true!\nIf you want to play Jeopardy correctly, recompile as DEBUG or RETAIL.\n(and if you're on-stage at a con, may God have mercy on your soul\nand may you have Visual Studio on your computer)");

                this.Top = 000;
                this.Left = 200;
#else   
                this.Top = rect.Top;
                this.Left = rect.Left;
#endif
            }
            catch
            {
                Screen scr = Screen.AllScreens[0];

                System.Drawing.Rectangle rect = scr.WorkingArea;
                this.Top = rect.Top;
                this.Left = rect.Left;
            }

            try
            {
                var SplashImg = new Uri(AppDomain.CurrentDomain.BaseDirectory + @"\data\gfx\splash.png", UriKind.Absolute);
                Img_SplashScr.Source = new BitmapImage(SplashImg);
            }
            catch
            {
                System.Windows.MessageBox.Show("Warning: We couldn't load \\data\\gfx\\splash.png for some reason");
            }
            

            GBTimer.Interval = TimeSpan.FromMilliseconds(250);
            GBTimer.Tick += GBTimer_Tick;
            GBTimer.Start();
        }

        private void GBTimer_Tick(object sender, EventArgs e)
        {
            string PlayerScore = "0";

            GB_Lbl_P1Score.Content = "$" + (PlayerScore = File.ReadAllText(@"data\local\score1"));
                if(Convert.ToInt32(PlayerScore) <= -1)
                {
                    GB_Lbl_P1Score.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    GB_Lbl_P1Score.Foreground = new SolidColorBrush(Colors.White);
                }
                
            GB_Lbl_P2Score.Content = "$" + (PlayerScore = File.ReadAllText(@"data\local\score2"));
                if (Convert.ToInt32(PlayerScore) <= -1)
                {
                    GB_Lbl_P2Score.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    GB_Lbl_P2Score.Foreground = new SolidColorBrush(Colors.White);
                }

            GB_Lbl_P3Score.Content = "$" + (PlayerScore = File.ReadAllText(@"data\local\score3"));
                if (Convert.ToInt32(PlayerScore) <= -1)
                {
                    GB_Lbl_P3Score.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    GB_Lbl_P3Score.Foreground = new SolidColorBrush(Colors.White);
                }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }
    }
}
