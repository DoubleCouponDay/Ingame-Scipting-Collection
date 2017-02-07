
using System;
using System.Collections.Generic;

using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace Ingame_Scripting_Collection12
{
    public class Program : MyGridProgram
    {
#region in-game
        public static class PrettyScaryDictionary
        {
            public const string AEE = "AEE";
            public const string AHH = "AHH"; 
            public const string AWW = "AWW"; 
            public const string BIH = "BIH"; 
            public const string DIH = "DIH"; 
            public const string EEE = "EEE"; 
            public const string EHH = "EHH"; 
            public const string EYE = "EYE"; 
            public const string FIH = "FIH"; 
            public const string GIH = "GIH"; 
            public const string HIH = "HIH"; 
            public const string HOH = "HOH"; 
            public const string IHH = "IHH"; 
            public const string JIH = "JIH"; 
            public const string KIH = "KIH"; 
            public const string KSS = "KSS"; 
            public const string LIH = "LIH"; 
            public const string MIH = "MIH"; 
            public const string NIH = "NIH"; 
            public const string OOO = "OOO"; 
            public const string OWE = "OWE"; 
            public const string PIH = "PIH"; 
            public const string RIH = "RIH"; 
            public const string SIH = "SIH"; 
            public const string THI = "THI"; 
            public const string TIH = "TIH"; 
            public const string UHH = "UHH"; 
            public const string VIH = "VIH"; 
            public const string WIH = "WIH"; 
            public const string YIH = "YIH"; 
            public const string ZIH = "ZIH";

            public static readonly string[] conversionTable = { //helps the ingame block convert phonemes to speaker numbers
                AEE, AHH, AWW, BIH, DIH, EEE, EHH, EYE, FIH, GIH, HIH, HOH, IHH, JIH, KIH, KSS, LIH, MIH, NIH, OOO, OWE, PIH, RIH, SIH, THI, TIH, UHH, VIH, WIH, YIH, ZIH 
            };

            /*
            SUPPORTED EXTENSIONS 
            this is a useless array but it lets me know which extensions can be added to dictionary words.
                "E",
                "S", "ES", "IES",
                "D", "ED", "IED",
                "Y", "RY", "LY", "ALLY",
                "R", "ER", "BER", "LER", "MER", "NER", "PER", "TER",
                "L", "AL",
                "ING", "BING", "LING", "MING", "NING", "PING", "TING",
                "EST",
                "ABLE", "BABLE", "LABLE", "MABLE", "NABLE", "PABLE", "TABLE",
            */        
             
            public static readonly string[] row = new string[0]; //for readability only.                                                   

            //use dictionary if you just cant get the syllable spacing you want.
            //its very easy to enter a word wrong. try to get entry values to the same length as its key.                                                         
            public static Dictionary <string, string[]> ordered = new Dictionary <string, string[]>() //ACRONYMS ARE NOT SUPPORTED
            {
                {"_A_", row}, { "ALL", new string[]{ AWW, " ", LIH } }, { "ALSO", new string[]{ AWW, LIH, SIH, OWE } }, { "AUTOMATIC", new string[]{ AWW, TIH, " ", OWE, MIH, AHH, TIH, IHH, KIH, } }, { "ALERT", new string[]{ UHH, " ", LIH, RIH, TIH} }, { "ABOARD", new string[]{ UHH, " ", BIH, AWW, " ", DIH} },
                {"_B_", row}, { "BEFORE", new string[]{ BIH, EEE, FIH, AWW, RIH, "", } },
                {"_C_", row}, { "COULD", new string[]{ KIH, OOO, " ", DIH, "", } }, { "CHINESE", new string[]{ KIH, HIH, EYE, NIH, EEE, " ", ZIH, } },
                {"_D_", row}, { "DO", new string[]{ DIH, OOO, } },
                {"_E_", row}, { "ENGINE", new string[]{ EHH, NIH, " ", JIH, IHH, NIH, } }, { "EARTHQUAKE", new string[]{ RIH, THI, " ", KIH, WIH, AEE, KIH, "", "", "", } }, { "END", new string[]{ EHH, NIH, DIH, } }, { "EVERY", new string[]{ EHH, VIH, RIH, "", EEE, } }, { "EXAMPLE", new string[]{ EHH, KSS, " ", UHH, MIH, PIH, LIH, } },
                {"_F_", row}, { "FIND", new string[]{ FIH, EYE, NIH, DIH, } },
                {"_G_", row}, { "GO", new string[]{ GIH, OWE, } }, { "GOOD", new string[]{ GIH, " ", OOO, DIH, } }, { "GIANT", new string[]{ JIH, EYE, UHH, NIH, TIH, } }, { "GET", new string[]{ GIH, EHH, TIH, } },
                {"_H_", row}, { "HELL", new string[]{ HIH, EHH, LIH, "", } }, { "HEY", new string[]{ HIH, AEE, "", } }, { "HARDWARE", new string[]{ HIH, UHH, RIH, DIH, " ", WIH, EHH, RIH } },
                {"_I_", row}, { "IM", new string[]{ EYE, MIH, } }, { "ILL", new string[]{ EYE, LIH, "", } }, { "INFINIT", new string[]{ IHH, NIH, FIH, IHH, NIH, IHH, TIH, } }, { "INTEREST", new string[]{ IHH, NIH, TIH, RIH, " ", EHH, SIH, TIH, } }, 
                {"_J_", row},
                {"_K_", row},
                {"_L_", row}, { "LAZY", new string[]{ LIH, AEE, ZIH, } }, { "LOAD", new string[]{ LIH, OWE, " ", DIH, } }, { "LIMIT", new string[]{ LIH, IHH, MIH, IHH, TIH, } }, { "LOOP", new string[]{ LIH, OOO, " ", PIH, } },
                {"_M_", row}, { "MADDEN", new string[]{ MIH, AHH, DIH, EHH, NIH, "", } }, { "MEME", new string[]{ MIH, EEE, " ", MIH, } }, { "MANUAL", new string[]{ MIH, AHH, NIH, OOO, LIH, ""} },
                {"_N_", row}, { "NIGHT", new string[]{ NIH, EYE, " ", TIH, "", } },  { "NURSERY", new string[]{ NIH, RIH, " ", SIH, UHH, RIH, EEE, } }, { "NOW", new string[]{ NIH, OWE, WIH, } },
                {"_O_", row}, { "OKAY", new string[]{ OWE, " ", KIH, AEE, } }, { "OVER", new string[]{ OWE, " ", VIH, RIH, } }, { "ON", new string[]{ HOH, NIH, } }, { "ONE", new string[]{ WIH, UHH, NIH, } }, { "ONCE", new string[]{ WIH, UHH, NIH, SIH, } },
                {"_P_", row}, { "PUT", new string[]{ PIH, OOO, TIH, } }, { "POSSIBILIT", new string[]{ PIH, HOH, SIH, IHH, BIH, " ", IHH, LIH, IHH, TIH, } }, { "PASTE", new string[]{ PIH, AEE, " ", SIH, TIH, } }, { "PRIVATE", new string[]{ PIH, RIH, EYE, VIH, UHH, TIH, "", } },
                {"_Q_", row},
                {"_R_", row}, { "RUN", new string[]{ RIH, UHH, NIH, } }, { "ROSA", new string[]{ RIH, OWE, SIH, UHH, } }, { "READ", new string[]{ RIH, EEE, " ", DIH, } },
                {"_S_", row}, { "SIR", new string[]{ SIH, RIH, "", } }, { "SCIENCE", new string[]{ SIH, EYE, " ", EHH, NIH, SIH, "", } }, { "SHE", new string[]{ SIH, HIH, EEE, } }, { "SHOULD", new string[]{ SIH, HIH, " ", OOO, DIH, "", } }, { "SMALL", new string[]{ SIH, MIH, AWW, LIH, "", } }, { "SOLUTION", new string[]{ SIH, HOH, LIH, OOO, SIH, HIH, UHH, NIH,} }, { "SURE", new string[]{ SIH, HIH, OOO, RIH,} },
                {"_T_", row}, { "THING", new string[]{ THI, IHH, " ", NIH, GIH, } }, { "TODAY", new string[]{ TIH, OOO, " ", DIH, AEE} }, { "THAT", new string[]{ THI, AHH, " ", TIH, } }, { "TO", new string[]{ TIH, OOO, } }, { "TWO", new string[]{ TIH, OOO, "", } }, { "TOO", new string[]{ TIH, OOO, "", } }, { "THERE", new string[]{ THI, EHH, " ", RIH, "", } }, { "THEIR", new string[]{ THI, EHH, " ", RIH, "", } },
                {"_U_", row}, { "UNIVERSE", new string[]{ YIH, OOO, " ", NIH, EEE, VIH, RIH, SIH, } }, { "USE", new string[]{ YIH, OOO, SIH, } },
                {"_V_", row}, { "VIDEO", new string[]{ VIH, IHH, DIH, EEE, OWE, } }, { "VARIATION", new string[]{ VIH, EHH, RIH, EEE, AEE, " ", HIH, UHH, NIH, } }, { "VERSION", new string[]{ VIH, RIH, " ", ZIH, HIH, UHH, NIH, } },
                {"_W_", row}, { "WOULD", new string[]{ WIH, OOO, " ", DIH, "", } }, { "WORD", new string[]{ WIH, RIH, " ", DIH, } },
                {"_X_", row},
                {"_Y_", row}, { "YOUR", new string[]{ YIH, " ", AWW, "", } },
                {"_Z_", row},
            };
        }

        //reference settings
        const string VERSION = "4";
        const float MAX_VOLUME = 1.0f; //these make changing the behaviour easier when you dont have to go through the whole program.
        const float MAX_RANGE = 500.0f;
        const int MAX_LETTERS = 100;
        const int SPACE_SIZE = 5;
        const int CLIP_LENGTH = 4;
        const int SYLLABLE_SIZE = 3;
        const int SPEAKER_GROUP_SIZE = 31;   

        //these three classes are a collection of like minded variables.
        static class State
        {                  
            public static bool initialised;
            public static bool sentenceStart = true;
            public static bool stopped;
            public static bool looping;
            public static bool endProgram = true;
            public static bool loadingPrepped;
            public static bool previousWasSpace;
        }

        static class Index
        {
            public static int pronunciation;
            public static int clips;
            public static int syllables; //an increment for the current syllable.
        }

        static class Playdata
        {
            public static string usersSentence;
            public static string timeline;
            public static string timelineCopy;
            public static int currentTimelineSize;
            public static int currentTick;
            public static int syllableMeasure; //a measure of how far along each syllable is.;
        }

        //objects
        List <IMyTerminalBlock> speakers = new List <IMyTerminalBlock>();
        IMyTerminalBlock timer;
        Pronunciation pronunciation = new Pronunciation();

        //------------------------------------------------------------------------------------------------//
        public void Main (string argument)    
        {
            string noEscapes = string.Format (@"{0}", argument); // @"" prevents user's regex inputs.
            string fixedCase = noEscapes.ToUpper();

            if (State.initialised == false)
            {
                Initialise();
            }  
        
            if (fixedCase.Length > MAX_LETTERS || //letter limit for performance reasons.
                Playdata.usersSentence.Length > MAX_LETTERS)
            {       
                Echo ("100 LETTER MAXIMUM WAS REACHED!");    
            }   
    
            else if (fixedCase == "[ STOP")
            {
                Stop();
            }    

            else if (fixedCase.Contains ("[8] ") ||
                     Playdata.usersSentence.Contains ("[8] ") )
            {
                State.looping = true;
                Load (fixedCase);
            }

            else if (fixedCase.Contains ("[ ") ||
                     Playdata.usersSentence.Contains ("[ ") )
            {
                Load (fixedCase);
            }        

            else if (State.stopped == false) //program is still looping but not playing sound.
            {     
                Stop(); 
            }         
    
            if (State.endProgram == false)
            {
                timer.ApplyAction ("TriggerNow");        
            }    
    
            else
            {
                State.endProgram = false; //now you only have to press run instead of recompiling.
                WelcomeMessage();
                Stop();
            } 
        }

        //function that fetches objects and variables once per compilation; this saves performance. for loops do not scale.
        void Initialise()
        {
            State.initialised = true;  
            State.loadingPrepped = false;
            timer = GridTerminalSystem.GetBlockWithName ("TTS timer"); //if the timer's name is wrong, exception will be thrown. 
            Playdata.syllableMeasure = 1;
            Playdata.usersSentence = "";
    
            if (timer == null)
            {
                State.endProgram = true;  
                State.stopped = true; 
                Echo ("TIMER HAS THE WRONG NAME! RENAME IT TO TTS timer");
            }
    
            for (int i = 0; i < SPEAKER_GROUP_SIZE; i++)
            {
                IMyTerminalBlock currentSpeaker = GridTerminalSystem.GetBlockWithName ("TTS speaker " + Convert.ToString (i));
        
                if (currentSpeaker != null)
                {
                    speakers.Add (currentSpeaker);
                    speakers[i].SetValueFloat ("RangeSlider", MAX_RANGE);
                    speakers[i].SetValueFloat ("VolumeSlider", 0.0f);
                    speakers[i].ApplyAction ("PlaySound"); 
                    speakers[i].ApplyAction ("StopSound"); //clear throat bug fixed here.
                    speakers[i].SetValueFloat ("VolumeSlider", MAX_VOLUME); 
            
                    if ((speakers[i] as IMySoundBlock).IsSoundSelected == false) //converts IMyTerminalBlock to IMySoundBlock to access it's fields.           
                    {
                        Echo ("A SPEAKER DOESNT HAVE A SOUND!");          
                    }            
                }
        
                else
                {       
                    Echo ("A SPEAKER ISNT NAMED CORRECTLY!");   
                }
            }
    
        }

        void WelcomeMessage()
        {
            Echo ("TTS ingame script - VERSION " + VERSION);
            Echo ("");
            Echo ("'[ message' runs once.");
            Echo ("'[8] message' loops infinitely.");
            Echo ("'[ stop' ends looping.");  
            Echo ("You are required to press run when ready.");      
        }

        //this function will extract what phonemes it can from the sentence and save performance by taking its sweet time.
        void Load (string fixedCase)
        {    
            if (State.sentenceStart == true)
            {   
                State.stopped = false; 
                State.endProgram = false;
        
                if (State.loadingPrepped == false)
                {
                    State.loadingPrepped = true;  
                    Playdata.usersSentence = fixedCase;
                    pronunciation.wordCounter.TakeNewSentence (Playdata.usersSentence);
                    Echo ("Loading...");            
                }
        
                else if (Index.pronunciation < Playdata.usersSentence.Length)
                {    
                    AddPhoneme();
                    Index.pronunciation++; 
                }   

                else
                {
                    State.sentenceStart = false;   
                    Playdata.timeline += "/"; //detection in Play() would fetch anything without this line.
                    Playdata.timelineCopy = Playdata.timeline;   
                    Echo ("Playing...");            
                }        
            }
    
            else
            {
                Play();
            }    
        } 
  
        //creates a new clip for the current letter.
        void AddPhoneme()
        {    
            string backupPhoneme; //this is for rare states such as extra spacing and QWI
            string primaryPhoneme = pronunciation.GetLettersPronunciation (Playdata.usersSentence, Index.pronunciation, out backupPhoneme);
            string[] addResults = {primaryPhoneme, backupPhoneme};
    
            for (int i = 0; i < addResults.Length; i++)
            {
                int necessaryFormat;

                if (addResults[i] != "")
                {
                    if (addResults[i] != " ")
                    {
                        necessaryFormat = Array.IndexOf (PrettyScaryDictionary.conversionTable, addResults[i]); //converts string to speaker block index

                        if (necessaryFormat != -1) //no match found lets my program know that no clip should be created.
                        {
                            int startPointTemp = Playdata.currentTimelineSize;   
                            int speakerChoice = necessaryFormat;
                            Playdata.timeline += "/" + startPointTemp + "#" + speakerChoice; 
                            Playdata.currentTimelineSize += CLIP_LENGTH; //timeline is expanded for duration after the clip is created.

                            if (Playdata.syllableMeasure == SYLLABLE_SIZE) //cues a space using the current setting SYLLABLE_SIZE.
                            {
                                if (State.previousWasSpace == false) //pronunciation class inserts spaces for low energy letters. i just dont need double spaces.
                                    {
                                        State.previousWasSpace = true;
                                        IncrementSyllables();
                                    }                           
                            
                                    else
                                    {
                                        State.previousWasSpace = false;
                                    }
                            }   
                
                            else
                            {
                                Playdata.syllableMeasure++;
                            }
                        }
                    }

                    else
                    {
                        IncrementSyllables();
                    }
                }    
            }
        }

        void IncrementSyllables()
        {
            Playdata.currentTimelineSize += SPACE_SIZE;
            Playdata.syllableMeasure = 1;
        }

        //this function is in charge of finding speakers not in use and assigning clips to that speaker.
        void Play()
        {    
            string tickString = Convert.ToString ("/" + Playdata.currentTick + "#");
    
            while (Playdata.timelineCopy.Contains (tickString))
            {                                                         //returns index of first character of input string found.
                int pointIndex = Playdata.timelineCopy.IndexOf (tickString); //Contains() already confirms this number will not be -1.
                ExtractClip (pointIndex);
            }    

            if (Playdata.currentTick >= Playdata.currentTimelineSize)    
            { 
                if (State.looping == true)
                {
                    Playdata.timelineCopy = Playdata.timeline;
                    Playdata.currentTick = 0;
                }    
        
                else
                {
                    State.endProgram = true;
                }    
            } 
    
            else
            {
                Playdata.currentTick++;
            }    
        }

        //performance light function that detects clips ready to play. 
        void ExtractClip (int pointIndex)
        { 
            bool extracted_num = false;                                  
            string tempNumber = "";                                                 
    
            //finds the point marker.
            while (Playdata.timelineCopy[pointIndex] != '#') 
            {
                pointIndex++;    
            }
    
            //add the speaker number it finds until it reaches the marker.
            while (extracted_num == false) 
            {
                pointIndex++; //ensures hash is not added to the temp clip number.
        
                if (Playdata.timelineCopy[pointIndex] != '/') //an indexed string is a single char; therefore use ''.
                {
                    tempNumber += Playdata.timelineCopy[pointIndex];
                }
        
                else
                {
                    extracted_num = true;
                    pointIndex--; //readies the logic to remove the data slot.
                }    
            }    
    
            //removes the data slot but leaves the forward slashes.
            while (Playdata.timelineCopy[pointIndex] != '/')
            {
                Playdata.timelineCopy = Playdata.timelineCopy.Remove (pointIndex, 1); //inputs index then count.
                pointIndex--;
            } 
            int numExtracted = Convert.ToInt32 (tempNumber);
            speakers[numExtracted].ApplyAction ("PlaySound");
        }

        //erases the previous clip mix.
        void Stop()
        { 
            Playdata.usersSentence = "";
            State.loadingPrepped = false;
            Playdata.syllableMeasure = 1;   

            State.stopped = true; //prevents running this function needlessly.
            State.sentenceStart = true;
            State.looping = false;
            State.endProgram = true;
    
            Index.pronunciation = 0;
            Index.clips = 0;
            Index.syllables = 0;
    
            Playdata.timeline = "";
            Playdata.timelineCopy = "";
            Playdata.currentTimelineSize = 0;
            Playdata.currentTick = 0;

            pronunciation.ResetBookmark();
        }

        class Pronunciation
        {
            //pronunciation reference: http://www.englishleap.com/other-resources/learn-english-pronunciation
            const int NEW_WORD = -1;
            const int NO_MATCH = -2;  
            const int MAX_EXTENSION_SIZE = 5;
            int placeholder = NEW_WORD;
            string[] dictionaryMatch;
            public WordCounter wordCounter = new WordCounter();

            //first searches the ditionary, then tries the secondary pronunciation if no match found.
            public string GetLettersPronunciation (string sentence, int letterIndex, out string secondary) 
            {
                secondary = "";
                string primaryPhoneme;        
                string currentWord = wordCounter.GetCurrentWord (NEW_WORD, ref placeholder); //this update is needed every time i increment a letter.          
                
                if (currentWord != " ")
                {                
                    if (placeholder == NEW_WORD)
                    {
                        string refinedQuery = ContinuallyRefineSearch (currentWord); //word ending simplifier to increase chances of a match.               
                
                        if (refinedQuery != "")
                        {
                            primaryPhoneme = TakeFromDictionary (true, sentence, letterIndex, out secondary);

                        }
            
                        else //if no match is found, use secondary pronunciation.
                        {
                            placeholder = NO_MATCH;
                            primaryPhoneme = AdjacentEvaluation (sentence, letterIndex, out secondary);
                        }
                    }

                    else if (placeholder != NO_MATCH) //takes over reading once a match is found in the dictionary.
                    {
                        primaryPhoneme = TakeFromDictionary (false, sentence, letterIndex, out secondary);
                    }

                    else
                    {
                        primaryPhoneme = AdjacentEvaluation (sentence, letterIndex, out secondary);
                    }
                }

                else
                {
                    primaryPhoneme = currentWord; //avoids setting placeholder in this scenario since an empty space cant reset it when needed.
                    secondary = " ";
                }

                placeholder = wordCounter.CheckForEnd (placeholder, NEW_WORD); //script needed to reset to default state but the mod did not.
                return primaryPhoneme;
            }

            string ContinuallyRefineSearch (string currentWord) //removes word's extensions one after another until it has none.
            {
                string refinedQuery = currentWord;

                while (PrettyScaryDictionary.ordered.TryGetValue (currentWord, out dictionaryMatch) == false)
                {          
                    string wordsExtension = EvaluateWordsEnding (currentWord); //returns / if the word is not long enough
                            
                    if (wordsExtension != "/") //i must only remove an extension if one exists or the word is length 4 or greater.
                    {
                        currentWord = currentWord.Remove ((currentWord.Length) - wordsExtension.Length, wordsExtension.Length);
                        refinedQuery = currentWord;
                    }

                    else
                    {
                        return "";
                    }
                }  
                return refinedQuery;
            }

                //returns the words extension if that phrase has been designed for, or / if word length is too small.
                string EvaluateWordsEnding (string currentWord) //unfortunately there is a ton of logic checks needed here because try catch does not work with in game scripts.
                {
                    string searchResult = "/"; //prevents calling this method again until the next word.

                    if (currentWord.Length >= MAX_EXTENSION_SIZE) //i dont really need to remove extensions unless the word is big enough.
                    {
                        bool E = (currentWord[currentWord.Length - 1] == 'E') ? true : false;
                        bool S = (currentWord[currentWord.Length - 1] == 'S') ? true : false;
                        bool D = (currentWord[currentWord.Length - 1] == 'D') ? true : false;
                        bool Y = (currentWord[currentWord.Length - 1] == 'Y') ? true : false;
                        bool R = (currentWord[currentWord.Length - 1] == 'R') ? true : false;                            
                        bool L = (currentWord[currentWord.Length - 1] == 'L') ? true : false;                            

                        bool ING = (currentWord[currentWord.Length - 3] == 'I' && 
                                    currentWord[currentWord.Length - 2] == 'N' && 
                                    currentWord[currentWord.Length - 1] == 'G') ? true : false;

                        bool EST = (currentWord[currentWord.Length - 3] == 'E' && 
                                    currentWord[currentWord.Length - 2] == 'S' && 
                                    currentWord[currentWord.Length - 1] == 'T') ? true : false;

                        bool ABLE = (currentWord[currentWord.Length - 4] == 'A' &&
                                     currentWord[currentWord.Length - 3] == 'B' && 
                                     currentWord[currentWord.Length - 2] == 'L' && 
                                     currentWord[currentWord.Length - 1] == 'E') ? true : false;  
            
                        if (E == true)
                        {
                            searchResult = "E";
                        }

                        else if (S == true)
                        {
                            bool ES = (currentWord[currentWord.Length - 2] == 'E') ? true : false;

                            if (ES == true)
                            {                                                     
                                bool IES = (currentWord[currentWord.Length - 3] == 'I') ? true : false;
                        
                                if (IES == true)
                                {
                                    searchResult = "IES";
                                }

                                else
                                {
                                    searchResult = "ES";
                                }
                            }
                
                            else
                            {
                                searchResult = "S";
                            }
                        }

                        else if (D == true)
                        {
                            bool ED = (currentWord[currentWord.Length - 2] == 'E') ? true : false;

                            if (ED == true)
                            {
                                bool IED = (currentWord[currentWord.Length - 3] == 'I') ? true : false;

                                if (IED == true)
                                {
                                    searchResult = "IED";
                                }
                    
                                else
                                {
                                    searchResult = "ED";
                                }
                            }

                            else
                            {
                                searchResult = "D";    
                            }
                        }

                        else if (Y == true)
                        {
                            bool RY = (currentWord[currentWord.Length - 2] == 'R') ? true : false;
                            bool LY = (currentWord[currentWord.Length - 2] == 'L') ? true : false;

                            if (LY == true)
                            {

                                bool ALLY = (currentWord[currentWord.Length - 4] == 'A' &&
                                                currentWord[currentWord.Length - 3] == 'L') ? true : false;  

                                if (ALLY == true)
                                {
                                    searchResult = "ALLY";
                                }

                                else
                                {
                                    searchResult = "LY";
                                }                        
                            }

                            else if (RY == true)
                            {
                                searchResult = "RY";
                            }

                            else
                            {
                                searchResult = "Y";
                            }                 
                        }            

                        else if (R == true)
                        {
                            bool ER = (currentWord[currentWord.Length - 2] == 'E') ? true : false;  

                            if (ER == true)
                            {
                                bool BER = (currentWord[currentWord.Length - 3] == 'B') ? true : false;
                                bool LER = (currentWord[currentWord.Length - 3] == 'L') ? true : false;
                                bool MER = (currentWord[currentWord.Length - 3] == 'M') ? true : false;
                                bool NER = (currentWord[currentWord.Length - 3] == 'N') ? true : false;
                                bool PER = (currentWord[currentWord.Length - 3] == 'P') ? true : false;
                                bool TER = (currentWord[currentWord.Length - 3] == 'T') ? true : false;

                                if (TER == true)
                                {
                                    searchResult = "TER";
                                }   
                  
                                else if (PER == true)
                                {
                                    searchResult = "PER";
                                }

                                else if (NER == true)
                                {
                                    searchResult = "NER";
                                }

                                else if (MER == true)
                                {
                                    searchResult = "MER";
                                }

                                else if (LER == true)
                                {
                                    searchResult = "LER";
                                }
                                     
                                else if (BER == true)
                                {
                                    searchResult = "BER";
                                } 
                               
                                else 
                                {
                                    searchResult = "ER";
                                } 
                            }

                            else
                            {
                                return "R";
                            }
                        }                      

                        if (L == true)
                        {
                            bool AL = (currentWord[currentWord.Length - 2] == 'A') ? true : false;

                            if (AL == true)
                            {
                                searchResult = "AL";
                            }

                            else
                            {
                                searchResult = "L";
                            }
                        }

                        else if (ING == true)
                        {
                            bool BING = (currentWord[currentWord.Length - 4] == 'B') ? true : false;
                            bool LING = (currentWord[currentWord.Length - 4] == 'L') ? true : false;
                            bool MING = (currentWord[currentWord.Length - 4] == 'M') ? true : false;
                            bool NING = (currentWord[currentWord.Length - 4] == 'N') ? true : false;                
                            bool PING = (currentWord[currentWord.Length - 4] == 'P') ? true : false;
                            bool TING = (currentWord[currentWord.Length - 4] == 'T') ? true : false;
                     
                            if (TING == true)
                            {
                                searchResult = "TING";
                            }   
                  
                            else if (PING == true)
                            {
                                searchResult = "PING";
                            }

                            else if (NING == true)
                            {
                                searchResult = "NING";
                            }

                            else if (MING == true)
                            {
                                searchResult = "MING";
                            }

                            else if (LING == true)
                            {
                                searchResult = "LING";
                            }
                                     
                            else if (BING == true)
                            {
                                searchResult = "BING";
                            } 
                               
                            else 
                            {
                                searchResult = "ING";
                            }          
                        }
            
                        else if (EST == true)
                        {
                            searchResult = "EST";
                        }

                        else if (ABLE == true)
                        {
                            bool BABLE = (currentWord[currentWord.Length - 5] == 'B') ? true : false;
                            bool LABLE = (currentWord[currentWord.Length - 5] == 'L') ? true : false;
                            bool MABLE = (currentWord[currentWord.Length - 5] == 'M') ? true : false;
                            bool NABLE = (currentWord[currentWord.Length - 5] == 'N') ? true : false;
                            bool PABLE = (currentWord[currentWord.Length - 5] == 'P') ? true : false;
                            bool TABLE = (currentWord[currentWord.Length - 5] == 'T') ? true : false;

                            if (TABLE == true)
                            {
                                searchResult = "TABLE";
                            }   
                
                            else if (PABLE == true)
                            {
                                searchResult = "PABLE";
                            }

                            else if (NABLE == true)
                            {
                                searchResult = "NABLE";
                            }

                            else if (MABLE == true)
                            {
                                searchResult = "MABLE";
                            }

                            else if (LABLE == true)
                            {
                                searchResult = "LABLE";
                            }

                            else if (BABLE == true)
                            {
                                searchResult = "BABLE";
                            }

                            else
                            {
                                searchResult = "ABLE";
                            }
                        }
                    }                         
                    return searchResult;
                }

            string TakeFromDictionary (bool isNewWord, string sentence, int letterIndex, out string secondary)
            {
                secondary = "";
                placeholder++; //for next time

                if (isNewWord == true)
                {
                    placeholder = 1;
                }
            
                if (placeholder - 1 < dictionaryMatch.Length)
                {
                    return dictionaryMatch[placeholder - 1]; //im confident this will never crash since dictionaryMatch is defined in all possible uses of this method
                }

                else
                {
                    return AdjacentEvaluation (sentence, letterIndex, out secondary); //reaching the end of a word in the dictionary either means i need to add an extension or i didnt put enough spaces 
                }
            }

                //AdjacentEvaluation is more efficient but its a complicated mess. catches anything not in the dictionary; including extensions.
                string AdjacentEvaluation (string sentence, int letterIndex, out string secondary)
                {
                    const string VOWELS = "AEIOU";
                    const string CONSONANTS = "BCDFGHJKLMNPQRSTVWXYZ";

                    string primary = "";
                    secondary = "";            

                    int intBefore = (letterIndex - 1 >= 0) ? (letterIndex - 1) : letterIndex; //these wil prevent out-of-bounds exception.
                    int intAfter = (letterIndex + 1 < sentence.Length) ? (letterIndex + 1) : letterIndex; 
                    int intTwoAfter = (letterIndex + 2 < sentence.Length) ? (letterIndex + 2) : letterIndex;
                    int intTwoBefore = (letterIndex - 2 >= 0) ? (letterIndex - 2) : letterIndex;

                    string before = (intBefore != letterIndex) ? Convert.ToString (sentence[intBefore]) : " "; //these 4 strings ensure i can correctly detect the start and end of a sentence.
                    string after = (intAfter != letterIndex) ? Convert.ToString (sentence[intAfter]) : " "; //using strings instead of chars saves lines since i need strings for Contains()
                    string twoBefore = (intTwoBefore != letterIndex) ? Convert.ToString (sentence[intTwoBefore]) : " "; //the false path must return a space string.
                    string twoAfter = (intTwoAfter != letterIndex) ? Convert.ToString (sentence[intTwoAfter]) : " ";        
                    string currentLetter = Convert.ToString (sentence[letterIndex]);

                    switch (currentLetter)
                    {
                        case "A": 
                            if ((before == "E" &&
                                 twoBefore != "R") &&
                                 CONSONANTS.Contains (after) &&
                                 after != "K")
                            {
                                ; //such as leaf, meat, real
                            }    
    
                            else if (before == "E" &&
                                     after == "D" &&
                                     twoAfter == "E")
                            {
                                primary = PrettyScaryDictionary.EEE;  //such as leader.                
                            }
        
                            else if (before == "U" ||  
                                     after == "W" ||
                                     after == "U" ||
                                    (before == "W" && 
                                     after == "T")) 
                            {
                                primary = PrettyScaryDictionary.AWW; //such as "raw", water
                            }

                            else if ((before == "E" && //break, steak
                                     (twoBefore == "R" || //great
                                      after == "K")) || 
                                     (after == "I" && 
                                      after != "R") ||
                                     (CONSONANTS.Contains (after) && //THE REPLACEMENT. space, ate, rake, grade
                                      twoAfter == "E") ||
                                    ((before == "T" ||
                                      before == "L") && //table
                                      twoBefore == " " &&
                                      after == "B") ||
                                     (after == "P" && //maple
                                      twoAfter == "L") ||
                                      after == "Y" ||
                                    ((before == "H" ||
                                      before == "R") && //phrase
                                      after == "S" &&
                                      twoAfter == "E") ||
                                     (after == "B" && //able
                                      twoAfter == "L"))     
                            {
                                primary = PrettyScaryDictionary.AEE;
                            }

                            else if ((before == "H" && //what
                                      after == "T") ||
                                     (after == "R" &&
                                      twoAfter != "E") || //far
                                     (before == " " && 
                                     (after == " " || //a
                                      after == "V")) || //available
                                     (after == "S" && //last
                                      twoAfter == "T") )
                            {
                                primary = PrettyScaryDictionary.UHH;
                            }    
            
                            else if (after == "R" && //compare, ware, hare, stare, care
                                     twoAfter == "E") 
                            {
                                primary = PrettyScaryDictionary.EHH;
                            }

                            else    
                            {
                                primary = PrettyScaryDictionary.AHH; //plottable,
                            }
                            break;
            
                        case "B":
                            if ((after != " " && //bomb
                                 before != "B") || //cobber
                                 VOWELS.Contains (before) ) //bob, silent B, cobber
                            {
                                if (after == "L")
                                {
                                    primary = " ";
                                    secondary = PrettyScaryDictionary.BIH;
                                }

                                else
                                {
                                    primary = PrettyScaryDictionary.BIH;
                                }
                            }
                            break;
            
                        case "C": 
                            if (after == "E" || 
                                after == "I" || 
                                after == "Y")
                            {
                                primary = PrettyScaryDictionary.SIH; //such as "sicily".
                            }
            
                            else 
                            {
                                primary = PrettyScaryDictionary.KIH; //such as "cat".
                            } 
                            break;
            
                        case "D":
                            if (after == "G")
                            {
                                ; //such as judge
                            }
            
                            else if (before != "D")
                            {
                                primary = PrettyScaryDictionary.DIH; //such as ladder.
                            }
                            break;
            
                        case "E":
                            if (twoBefore == "T" &&
                                after == " ")
                            {
                                primary = PrettyScaryDictionary.UHH; //such as the
                            } 
  
                            else if ((after == "A" && //late
                                     (before == "R" || 
                                      twoAfter == "K")) || //steak, break
                                      after == "U" ||
                                      before == "E" || //bee, speech
                                     (after == "R" && //ber
                                      before != "V" &&
                                      before != "E") ||
                                     (after == " " && //silent e at end
                                      twoBefore != " ") ||
                                     (after == "L" &&
                                      twoAfter == "Y") )                              
                            {
                                ; //such as queue, requirement, speech,
                            }    
        
                            else if (after == "W")
                            {
                                primary = PrettyScaryDictionary.OOO; //such as brew
                            }

                            else if ((after == "Y" &&
                                      twoAfter == "E") ||
                                      after == "I")
                            {
                                primary = PrettyScaryDictionary.EYE; //such as EYE 
                            }         
       
                            else if (after == "E" ||
                                     after == "A" ||
                                     before == "D" ||
                                     twoAfter == "D" ||
                                   ((before == "M" ||
                                     before == "H" ||
                                     before == "W" ||
                                     before == "B") &&
                                     after == " " &&
                                     twoBefore == " ") ||
                                    (twoBefore == "Y" && //maybe
                                     before == "B" &&
                                     after == " ") ||
                                    (after == "S" &&
                                     twoAfter == "E") ||
                                    (before == "I" && //ies
                                     twoAfter == " ") ||
                                    (before == "K" && //key
                                     after == "Y") ||
                                    (twoBefore == " " && //re
                                     before == "R") )
                            {                           
                                primary = PrettyScaryDictionary.EEE;    //such as "engineer", speech, me
                            }  
            
                            else if ((after != "E" && 
                                      after != "R" &&
                                      after != "W" &&
                                      twoAfter != " ") ||   
                                     (after == "R" &&
                                     (twoAfter == "E" ||
                                      before == "V")) ||
                                      after == "D" ||
                                     (after == "S" && //es
                                      twoAfter == " ") || 
                                     (after == "T" &&
                                      twoAfter == " ") )
                            {                                         
                                primary = PrettyScaryDictionary.EHH;  //such as silent E, there, fate
                            }   

                            else if (twoBefore == "R" && //prey
                                      before == "E" ||
                                      after == "Y")
                            {
                                primary = PrettyScaryDictionary.AEE;
                            }
                            break;
            
                        case "F": 
                            primary = PrettyScaryDictionary.FIH; //such as "follow".
                            break;
            
                        case "G":
                            if (before == "G" || //trigger
                                before == "H" || //high
                               (before == "N" && //ing
                                after == " ") ||
                               (before == "I" &&
                                after == "N") ) //design
                            {
                                ;
                            }

                            else if  (after == "E" &&
                                 before == "D" ||
                                (after == "I" && 
                                 twoAfter != "T") || 
                                 after == "Y")
                            {   
                                secondary = PrettyScaryDictionary.JIH;
                                primary = " "; //such as "gin", judgement, RNG
                            }
            
                            else
                            {
                                primary = PrettyScaryDictionary.GIH; //git,
                            }    
                            break;
            
                        case "H":
                            if (after == "N" ||
                               (before == "T" && //thigh
                                after != "U") || //github
                                before == "G" || //high
                                before == "O" ||
                                before == "P")
                            {
                                ;
                            }

                            else
                            {
                                primary = PrettyScaryDictionary.HIH;
                            }    
                            break;

                        case "I":
                            if ((after == "E" && //ies
                                 twoAfter == "S") )
                            {
                                ;
                            }

                            else if (after == "O" && //tion
                                twoAfter == "N" &&
                                (before == "T" ||
                                before == "S") )
                            {
                                primary = PrettyScaryDictionary.SIH;
                                secondary = PrettyScaryDictionary.HIH;
                            }

                            else if (after == "K" || //pike
                                    (CONSONANTS.Contains (after) && //we'll have to wait and see if this is a universal rule. if its not it has to be broken up.
                                     VOWELS.Contains (twoAfter) &&
                                     before != "R" &&
                                     twoAfter != "L" &&
                                     before != "A") || //sail
                                    (twoBefore == "K" &&
                                     before == "N") ||
                                    (after == "G" && //light
                                     twoAfter == "H") ||
                                    (twoAfter == "E" &&
                                     before != "G") ||
                                    (before == " " &&
                                     after == " ") ||
                                     after == "E" || //pie
                                   ((after == "L" || //filed
                                     after == "T") && //kite
                                     twoAfter == "E") ||
                                    (after == "G" && 
                                     twoAfter == "N") ) //sign 
                            {   
                                primary = PrettyScaryDictionary.EYE;
                            }
            
                            else if ((after == "N" && //running
                                      twoAfter == "G") ||
                                      before == "O") //point
                            {
                                primary = PrettyScaryDictionary.EEE; //such as running
                            }
    
                            else 
                            {
                                primary = PrettyScaryDictionary.IHH;  //such as 'felicity'.
                            }
                            break;
            
                        case "J":   
                            primary = PrettyScaryDictionary.JIH; //such as "jelly".
                            break;
            
                        case "K":   
                            if (after != "N" &&
                                before != "C")
                            {
                                primary = PrettyScaryDictionary.KIH; //such as silent K
                            }    
                            break;
            
                        case "L": 
                            if (after != "K" && 
                                after != "F" &&
                                before != "L") 
                            {
                                primary = PrettyScaryDictionary.LIH; //silent L, caller,
                            }
                            break;
            
                        case "M":   
                            if (before != "M")
                            {                        
                                primary = " ";
                                secondary = PrettyScaryDictionary.MIH; //such as "molten", drummer,
                            }    
                            break;
            
                        case "N":  
                            if (before != "N")
                            {    
                                primary = " ";
                                secondary = PrettyScaryDictionary.NIH;  //such as "nickel", planner, 

                            }    
                            break;
            
                        case "O":    
                            if ((after == "U" && //touch
                                 before == "T") ||
                                 before == "O" ||
                                (before == "W" && //word
                                 after == "R"))
                            {
                                ;
                            }                    
                        
                            else if (after == "R" || //for, lore, or, bore, core, store, tore, support
                                    (after == "U" && //four
                                     twoAfter == "R" &&
                                     before != "S") ||
                                     after == "I") //point, annoint, soil
                            {
                                primary = PrettyScaryDictionary.AWW;
                            }

                            else if ((after == "U" && //foul
                                     (before == "F" ||
                                      before == "P" || //pouch
                                      before == "L")) ) //slouch
                            {
                                primary = PrettyScaryDictionary.AHH; 
                            }
 
                            else if (after == "M" ||
                                    (before == "I" &&
                                     after == "N"))
                            {
                                primary = PrettyScaryDictionary.UHH; //such as computer, coming.
                            }   
            
                            else if (after == " " || //pro
                                    (before == "S" &&
                                     after == "U") ||
                                     after == "W" ||
                                    (CONSONANTS.Contains (after) && //sole, solo
                                     VOWELS.Contains (twoAfter)) ||
                                     (before == "B" && //both
                                      after == "T" && 
                                      twoAfter == "H"))
                            {
                                primary = PrettyScaryDictionary.OWE; //such as hello, soul
                            }   
            
                            else if ((CONSONANTS.Contains (before) && //told, sold, hold, gold, bold 
                                      after == "L") ||
                                     (before == " " &&
                                      after != "O") ||
                                     (after == "H" && //john
                                      twoAfter == "N") ||
                                     ((after == "T" && //sloth, cloth
                                      twoAfter == "H") &&
                                      before != "B") ||
                                     (before == "B" && //bot
                                      after == "T" &&
                                      twoAfter == " ") )
                            {
                                primary = PrettyScaryDictionary.HOH;                      
                            }    
            
                            else if (after == "O" ||
                                     after == "V" ||
                                     after == "U" ||
                                    (before == "T" &&
                                     after == "D") )
                            {
                                primary = PrettyScaryDictionary.OOO; //such as 'fool', today
                            }

                            else if (after == "X" ||
                                     after == "L" &&
                                     before != "O" ||
                                     after == "F")
                            {
                                primary = PrettyScaryDictionary.HOH;  //such as "oxygen".    
                            }   

                            else
                            {
                                primary = PrettyScaryDictionary.OWE;
                            }
                            break;
            
                        case "P":
                            if (after == "H") //phrase
                            {
                                primary = PrettyScaryDictionary.FIH;
                            }   

                            else if (before != "P") //potatoes, stoppable
                            {
                                primary = PrettyScaryDictionary.PIH;
                            }                    
                            break;
            
                        case "Q":                    
                            primary = PrettyScaryDictionary.KIH; //such as "query".     
                            secondary = PrettyScaryDictionary.WIH;
                            break;               
            
                        case "R":   
                            if (before != "R" /*&& //sparring
                                before != "O" && //for
                                twoBefore != "O"*/) //four
                            {                        
                                primary = PrettyScaryDictionary.RIH;
                            }
                            break;
            
                        case "S":   
                            if (after == "M")
                            {
                                primary = PrettyScaryDictionary.ZIH; //such as prism
                            }

                            else if (before != "S")
                            {
                                primary = PrettyScaryDictionary.SIH; //such as "slippery".
                            }                  
                            break;
            
                        case "T": 
                            if (after == "H" &&
                                twoAfter != "U") //github 
                            {
                                primary = " ";
                                secondary = PrettyScaryDictionary.THI; //such as "THInk".    
                            } 

                            else if (before != "T") //attic
                            {
                                if (before == "S") //est
                                {
                                    primary = " ";
                                    secondary = PrettyScaryDictionary.TIH;
                                }
                        
                                else
                                {
                                    primary = PrettyScaryDictionary.TIH;
                                }                        
                            }    
                            break;
            
                        case "U":
                            if (before == "Q" || //queue
                                before == "A" ||
                               (after == "R" &&
                                before != "O") ||
                               (before == "O" && //you
                               (twoBefore == "Y" ||
                                after == "L")) )  //soul
                            {
                                ;
                            }    

                            else if (before == "O" && //your
                                     after == "R")
                            {
                                primary = " ";
                            }

                            else if (before == "E" || 
                                     after == "E" ||
                                     before == "A" ||
                                     before == "R" || 
                                     after == "I" ||
                                    (before == "P" &&
                                     after == "L") )
                            {
                                primary = PrettyScaryDictionary.OOO;  //such as "cruelty", "eulogy".
                            }

                            else if (before == "O" ||
                                     after == "D" ||
                                    (after == "N" && //un
                                     before == " ") ||
                                     after == "P" || //update
                                    (before == "S" && //submit
                                     after == "B") ||
                                    (CONSONANTS.Contains (before) && //but, cut
                                    (after == "T" ||
                                     after == "C")) ||
                                    (before == "H" && //hub
                                     after == "B") ) 
                            {
                                primary = PrettyScaryDictionary.UHH; //such as touch, cut
                            }   

                            else if ((before == "B" || //buy
                                     before == "G") &&  //guy
                                     after == "Y")
                            {
                                primary = PrettyScaryDictionary.EYE;
                            }

                            else
                            {
                                primary = PrettyScaryDictionary.YIH; //fortune
                                secondary = PrettyScaryDictionary.OOO;
                            }
                            break;
            
                        case "V":    
                            primary = PrettyScaryDictionary.VIH; //such as "vector".
                            break;
            
                        case "W":
                            if (after != " ") //narrow, raw
                            {
                                primary = PrettyScaryDictionary.WIH;
                            }                   
                            break;
            
                        case "X":   
                            if (before == " ") 
                            {
                                primary = PrettyScaryDictionary.ZIH;   //such as "xylophone".  
                            }    
        
                            else  
                            {
                                primary = PrettyScaryDictionary.KSS; //such as "exit".                   
                            }
                            break;
            
                        case "Y":
                            if (((before == "U" || //buy
                                  before == "E") && //key
                                  after == " ") ||
                                  before == "A" || //maybe                                     
                                 (after == "E" && //eye
                                  before == "E") )
                            {
                                ; 
                            }

                            else if ((before == "C" && //bicycle
                                      after == "C") || 
                                     (before == "T" && //style
                                      after != " ") || //possibility
                                      before == "M" ||
                                      before == "H" ||
                                     (before == "L" && //fly
                                      twoBefore == "F") ||
                                      before == "K")  //sky
                            {
                                primary = PrettyScaryDictionary.EYE;
                            }

                            else if ((((twoBefore == "L" && 
                                         before == "L") || 
                                        before == "L" ||
                                        before == "R") &&
                                        after == " ") ||
                                        after == "B" ||
                                        VOWELS.Contains (twoBefore) ||
                                        before == "T")  
                            {
                                primary = PrettyScaryDictionary.EEE; //such as "flaky", negatively
                            }
            
                            else  
                            {
                                primary = PrettyScaryDictionary.YIH;  //such as "yam".
                            }
                            break;
            
                        case "Z":  
                            primary = PrettyScaryDictionary.ZIH; //such as "zorro".
                            break;
            
                        case " ": 
                            primary = " "; //such as "...dot dot dot im a pretentious wanker dot dot dot..."
                            break;
                    }
                    return primary;
                }
    
            public void ResetBookmark()
            {
                placeholder = NEW_WORD;
            }
        } 

        public class WordCounter //simply a collection of data without guarded sets. easy to pass.
        {                   
            int current_word;
            int current_letter;
            string[] words;

            public void TakeNewSentence (string inputSentence)
            {
                current_word = 0;
                current_letter = 0;        
                words = inputSentence.Split (' ');
            }

            public string GetCurrentWord (int NEW_WORD, ref int placeholder)
            { 
                if (current_word < words.Length)
                {
                    if (current_letter < words[current_word].Length)
                    {
                        current_letter++;
                        return words[current_word];
                    }

                    else
                    {
                        current_word++;
                        current_letter = 0;
                        placeholder = NEW_WORD; //a space will never have a placeholder.                    
                        return " "; //empty space is a valid return.
                    }
                }
        
                else
                {
                    return "";
                }
            }

            public int CheckForEnd (int placeholder, int NEW_WORD)
            {
                if (current_word == words.Length - 1 &&
                    current_letter == words[current_word].Length) //resets the placeholder since script doesnt construct a new class for every sentence.
                { 
                    placeholder = NEW_WORD;
                }
                return placeholder;
            }
        }
        #endregion in-game
    }
}