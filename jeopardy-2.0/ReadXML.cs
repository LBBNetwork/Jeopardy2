/* Filename: ReadXML.cs
   Author: neko2k (neko2k@beige-box.com)
   Website: http://www.beige-box.com
   Description: The magic where XML reading happens



   The following source code is CONFIDENTIAL and PROPRIETARY PROPERTY
   of The Little Beige Box and MAY NOT BE RELEASED under PENALTY OF LAW.

   This file Copyright (c) 2017-2020 The Little Beige Box.
*/

using System;
using System.Xml;
using System.Windows;

namespace jeopardy_2._0
{
    class ReadXML
    {
        XmlDocument XmlNodeReader = new XmlDocument();

        public int XMLTest(string FilePath)
        {
            try
            {
                XmlNodeReader.Load(FilePath);

                XmlNodeList XmlGetGameVersion = XmlNodeReader["Game"].GetElementsByTagName("Version");
                XmlNodeList XmlGetGameShow = XmlNodeReader["Game"].GetElementsByTagName("Show");

                if (XmlGetGameVersion[0].InnerText != "3")
                {
                    MessageBox.Show("You have tried to load a version " + XmlGetGameVersion[0].InnerText + " file. This build of Jeopardy expects version 3 files.\n\nCompatibility table:\nVersion 1: 2.0.219\nVersion 2: 2.1.310\nVersion 3: 2.3.350?", "File Load Error", MessageBoxButton.OK);

                    return -1;
                }

                if (XmlGetGameShow[0].InnerText != "Jeopardy")
                {
                    MessageBox.Show("This file is for a different game show.", "Incompatible Game", MessageBoxButton.OK);

                    return -1;
                }

                return 0;
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("Error loading game file.", "File Load Error", MessageBoxButton.OK);
                return -1;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("ReadXML.cs(XMLTest()): NullReferenceException: There was a problem loading the XML file. It may be corrupt.", "File Load Error", MessageBoxButton.OK);
                return -1;
            }
            catch (XmlException)
            {
                MessageBox.Show("ReadXML.cs(XMLTest()): XmlException: There was a problem loading the XML file. It may be corrupt.", "File Load Error", MessageBoxButton.OK);
                return -1;
            }
        }

        public string XMLGetGameTitle(string FilePath)
        {
            if (XMLTest(FilePath) != -1)
            {
                XmlNodeReader.Load(FilePath);

                XmlNodeList XmlGameTitle = XmlNodeReader["Game"].GetElementsByTagName("GameTitle");
                return XmlGameTitle[0].InnerText;
            }

            return "Error loading XML.";
        }

        public string XMLGetCategoryTitle(string FilePath, int CategoryNumber, int CategoryIndex, int JeopardyRound)
        {
            /*  FilePath: self-explanatory, path to the game xml
             *  CategoryNumber: What category to load
             *  CategoryIndex: Everything is stored/returned in an index starts at 0
             */

            try
            {
                XmlNodeReader.Load(FilePath);

                XmlNodeList XmlCategoryTitle;

                if (JeopardyRound == 0)
                {
                     XmlCategoryTitle = XmlNodeReader["Game"]["Jeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Title");
                }
                else if(JeopardyRound == 1)
                {
                    XmlCategoryTitle = XmlNodeReader["Game"]["DoubleJeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Title");
                }
                else
                {
                    XmlCategoryTitle = XmlNodeReader["Game"]["FinalJeopardy"].GetElementsByTagName("Title");
                }

                return XmlCategoryTitle[CategoryIndex].InnerText;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("There was a problem getting category names.\nAre there six categories in the game?", "Error Loading File", MessageBoxButton.OK);
                return "Null Category";
            }
            catch (XmlException)
            {
                MessageBox.Show("There was a problem getting category names.\nCheck that the tags are properly formatted.", "Error Loading File", MessageBoxButton.OK);
                return "XML Exception";
            }
        }

        public string XMLGetClueText(string FilePath, int CategoryNumber, int ClueIndex, int JeopardyRound)
        {
            try
            {
                XmlNodeReader.Load(FilePath);

                if(JeopardyRound == 0)
                {
                    XmlNodeList XmlClueText = XmlNodeReader["Game"]["Jeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Clue" + ClueIndex.ToString());
                    return XmlClueText[0].InnerText;
                }
                else if(JeopardyRound == 1)
                {
                    XmlNodeList XmlClueText = XmlNodeReader["Game"]["DoubleJeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Clue" + ClueIndex.ToString());
                    return XmlClueText[0].InnerText;
                }
                else if(JeopardyRound == 2)
                {
                    XmlNodeList XmlClueText = XmlNodeReader["Game"]["FinalJeopardy"].GetElementsByTagName("Title");
                    return XmlClueText[0].InnerText;
                }
                else
                {
                    XmlNodeList XmlClueText = XmlNodeReader["Game"]["FinalJeopardy"].GetElementsByTagName("Clue1");
                    return XmlClueText[0].InnerText;
                }             
           }
           catch (NullReferenceException)
           {
                return "NullReferenceException in XMLGetClueText CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + ClueIndex.ToString();
            }
           catch(XmlException)
            {
                return "XmlException in XMLGetClueText CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + ClueIndex.ToString();
            }
        }

        public string XMLGetCluePicture(string FilePath, int CategoryNumber, int AnswerIndex, int JeopardyRound)
        {
            try
            {
                XmlNodeReader.Load(FilePath);

                if (JeopardyRound == 0)
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["Jeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Picture" + AnswerIndex.ToString());
                    return XmlClueAnswer[0].InnerText;
                }
                else if (JeopardyRound == 1)
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["DoubleJeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Picture" + AnswerIndex.ToString());
                    return XmlClueAnswer[0].InnerText;
                }
                else
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["FinalJeopardy"].GetElementsByTagName("Picture1");
                    return XmlClueAnswer[0].InnerText;
                }
            }
            catch (NullReferenceException)
            {
                return "NullReferenceException in XMLGetCluePicture CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + AnswerIndex.ToString();
            }
            catch (XmlException)
            {
                return "XmlException in XMLGetCluePicture CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + AnswerIndex.ToString();
            }
        }

        public string XMLGetClueAudio(string FilePath, int CategoryNumber, int AnswerIndex, int JeopardyRound)
        {
            try
            {
                XmlNodeReader.Load(FilePath);

                if (JeopardyRound == 0)
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["Jeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Audio" + AnswerIndex.ToString());
                    return XmlClueAnswer[0].InnerText;
                }
                else if (JeopardyRound == 1)
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["DoubleJeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Audio" + AnswerIndex.ToString());
                    return XmlClueAnswer[0].InnerText;
                }
                else
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["FinalJeopardy"].GetElementsByTagName("Audio1");
                    return XmlClueAnswer[0].InnerText;
                }
            }
            catch (NullReferenceException)
            {
                return "NullReferenceException in XMLGetClueAudio CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + AnswerIndex.ToString();
            }
            catch (XmlException)
            {
                return "XmlException in XMLGetClueAudio CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + AnswerIndex.ToString();
            }
        }

        public string XMLGetClueVideo(string FilePath, int CategoryNumber, int AnswerIndex, int JeopardyRound)
        {
            try
            {
                XmlNodeReader.Load(FilePath);

                if (JeopardyRound == 0)
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["Jeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Video" + AnswerIndex.ToString());
                    return XmlClueAnswer[0].InnerText;
                }
                else if (JeopardyRound == 1)
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["DoubleJeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Video" + AnswerIndex.ToString());
                    return XmlClueAnswer[0].InnerText;
                }
                else
                {
                    return "What are you doing";
                }
            }
            catch (NullReferenceException)
            {
                return "NullReferenceException in XMLGetClueVideo CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + AnswerIndex.ToString();
            }
            catch (XmlException)
            {
                return "XmlException in XMLGetClueVideo CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + AnswerIndex.ToString();
            }
        }

        public string XMLGetCategoryIntroVideo(string FilePath, int JeopardyRound)
        {
            try
            {
                XmlNodeReader.Load(FilePath);

                XmlNodeList XmlCatIntroVideo;

                if (JeopardyRound == 0)
                {
                        XmlCatIntroVideo = XmlNodeReader["Game"]["Jeopardy"].GetElementsByTagName("VidIntro");
                }
                else
                {
                        XmlCatIntroVideo = XmlNodeReader["Game"]["DoubleJeopardy"].GetElementsByTagName("VidIntro");
                }

                return XmlCatIntroVideo[0].InnerText;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("There was a problem getting the video file location.", "Error Loading File", MessageBoxButton.OK);
                return "-1";
            }
            catch (XmlException)
            {
                MessageBox.Show("There was a problem getting video file location.\nCheck that the tags are properly formatted.", "Error Loading File", MessageBoxButton.OK);
                return "-2";
            }
        }
    
        public string XMLGetClueAnswer(string FilePath, int CategoryNumber, int AnswerIndex, int JeopardyRound)
        {
            try
            {
                XmlNodeReader.Load(FilePath);

                if (JeopardyRound == 0)
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["Jeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Answer" + AnswerIndex.ToString());
                    return XmlClueAnswer[0].InnerText;
                }
                else if (JeopardyRound == 1)
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["DoubleJeopardy"]["Category" + CategoryNumber.ToString()].GetElementsByTagName("Answer" + AnswerIndex.ToString());
                    return XmlClueAnswer[0].InnerText;
                }
                else
                {
                    XmlNodeList XmlClueAnswer = XmlNodeReader["Game"]["FinalJeopardy"].GetElementsByTagName("Answer1");
                    return XmlClueAnswer[0].InnerText;
                }
            }
            catch (NullReferenceException)
            { 
                return "NullReferenceException in XMLGetClueAnswer CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + AnswerIndex.ToString();
            }
            catch (XmlException)
            {
                return "XmlException in XMLGetClueAnswer CategoryNumber: " + CategoryNumber.ToString() + ", ClueNumber: " + AnswerIndex.ToString();
            }
        }

        public string XMLGetDailyDouble(string FilePath, int DDNum, int JeopardyRound)
        {
            /*  FilePath: self-explanatory, path to the game xml
            */

            try
            {
                XmlNodeReader.Load(FilePath);

                XmlNodeList XmlCategoryTitle;

                if (JeopardyRound == 0)
                {
                    if(DDNum == 1)
                    {
                        XmlCategoryTitle = XmlNodeReader["Game"]["Jeopardy"].GetElementsByTagName("DailyDouble1");
                    }
                    else
                    {
                        XmlCategoryTitle = XmlNodeReader["Game"]["Jeopardy"].GetElementsByTagName("DailyDouble2");
                    }
                    
                }
                else
                {
                    if (DDNum == 1)
                    {
                        XmlCategoryTitle = XmlNodeReader["Game"]["DoubleJeopardy"].GetElementsByTagName("DailyDouble1");
                    }
                    else
                    {
                        XmlCategoryTitle = XmlNodeReader["Game"]["DoubleJeopardy"].GetElementsByTagName("DailyDouble2");
                    }
                }

                return XmlCategoryTitle[0].InnerText;
            }
            catch (NullReferenceException)
            {
                MessageBox.Show("There was a problem getting Daily Double locations.", "Error Loading File", MessageBoxButton.OK);
                return "-1";
            }
            catch (XmlException)
            {
                MessageBox.Show("There was a problem getting Daily Double locations.\nCheck that the tags are properly formatted.", "Error Loading File", MessageBoxButton.OK);
                return "-2";
            }
        }

        /*public string XMLGetRoundTitle(string FilePath, int RoundTitleIndex)
        {
            try
            {
                XmlNodeReader.Load(FilePath);

                XmlNodeList XmlCategoryTitle = XmlNodeReader["Game"].GetElementsByTagName("DescRound");

                return XmlCategoryTitle[RoundTitleIndex].InnerText;
            }
            catch (NullReferenceException)
            {
                // MessageBox.Show("Warning: NullReferenceException in fastmoney.xml");

                return "-1";
            }
            catch (System.IO.FileNotFoundException)
            {
                MessageBox.Show("Warning: data\\rounds\\fastmoney\\fastmoney.xml not found");

                return "-1";
            }
        }*/

        /*public string XMLReadGameAnswers(string FilePath, int RoundNumber, int AnswerIndex)
        {
            /*  FilePath: self-explanatory, path to the game xml
             *  Answer: What set of answers to load, should always match CategoryIndex
             *  AnswerIndex: Block of answers for the given answer set
             *
            XmlNodeReader.Load(FilePath);

            XmlNodeList XmlAnswers = XmlNodeReader["Game"]["Round" + RoundNumber.ToString()]["Answers"].GetElementsByTagName("Title");

            try
            {
                return XmlAnswers[AnswerIndex].InnerText;
            }

            catch
            {
                return "NRE";
            }
        }*/

        /*public string XMLReadSurveyScores(string FilePath, int RoundNumber, int ScoreIndex)
        {
            XmlNodeReader.Load(FilePath);

            XmlNodeList XmlAnswers = XmlNodeReader["Game"]["Round" + RoundNumber.ToString()]["Scores"].GetElementsByTagName("Score");

            try
            {
                return XmlAnswers[ScoreIndex].InnerText;
            }
            catch
            {
                return "NRE";
            }

        }*/


        /*public int XMLReadNumOfTopAnswers(string FilePath, int RoundNumber)
        {
            XmlNodeReader.Load(FilePath);

            XmlNodeList XmlAnswers = XmlNodeReader["Game"]["Round" + RoundNumber.ToString()].GetElementsByTagName("TopAnswers");


            return (Convert.ToInt16(XmlAnswers[0].InnerText) + 1);
        }*/
    }
}
