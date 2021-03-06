﻿using System;
using System.IO;
using System.Windows.Media;

namespace WorkTimer
{
    class Program
    {
        private const int DEFAULT_MINUTES = 25;

        private static GenericConfigFile _configFile;
        private static float _globalVolume;
        private static int _minutes;

        private static MediaPlayer _tickSound;
        private static MediaPlayer _tockSound;
        private static MediaPlayer _alarmSound;

        private static void Main(string[] args)
        {
            ReadConfigFile();
            SetupSounds();
            SetupMinutesAndMainLoop();
        }

        private static void SetupSounds()
        {
            const string ALARM_PATH = "Alarm.wav";
            if(File.Exists(ALARM_PATH))
            {
                _alarmSound = new MediaPlayer();
                _alarmSound.Open(new Uri(ALARM_PATH, UriKind.Relative));
                //TODO: load local value from settings
                _alarmSound.Volume = _globalVolume;
            }

            const string TICK_PATH = "Tick.wav";
            if (File.Exists(TICK_PATH))
            {
                _tickSound = new MediaPlayer();
                _tickSound.Open(new Uri(TICK_PATH, UriKind.Relative));
                //TODO: load local value from settings
                _tickSound.Volume = _globalVolume;
            }

            const string TOCK_PATH = "Tock.wav";
            if (File.Exists(TOCK_PATH))
            {
                _tockSound = new MediaPlayer();
                _tockSound.Open(new Uri(TOCK_PATH, UriKind.Relative));
                //TODO: load local value from settings
                _tockSound.Volume = _globalVolume;
            }
        }

        private static void SetupMinutesAndMainLoop()
        {
            SetupMinutes();

            MainLoop();
        }

        private static void MainLoop()
        {
            Console.Clear();

            var shouldRepeat = RepeatedTimer();

            while (shouldRepeat)
            {
                shouldRepeat = RepeatedTimer();
            }

            if (!shouldRepeat)
            {
                SetupMinutesAndMainLoop();
            }
        }

        private static void ReadConfigFile()
        {
            _configFile = GenericConfigFile.Load("config.ini");

            if (_configFile == null)
            {
                Console.WriteLine("Could not read config.ini");
                return;
            }

            if (!_configFile.TryGetFloat("volume", out _globalVolume))
            {
                _globalVolume = 1.0f;
                Console.WriteLine("Could not read volume");
            }
        }

        private static void SetupMinutes()
        {
            Console.Clear();
            Console.WriteLine("Please enter a time\n");

            Console.WriteLine("D/Return:    Default time: 25 minutes");
            Console.WriteLine("C:           Load time from config file ");
            Console.WriteLine("mm:          Enter minutes directly ");
            Console.WriteLine("X:           Exit program ");

            var line = Console.ReadLine().Trim();

            if(string.IsNullOrEmpty(line) || line.Equals("D", StringComparison.OrdinalIgnoreCase))
            {
                _minutes = DEFAULT_MINUTES;
            }

            else if(line.Equals("C", StringComparison.OrdinalIgnoreCase))
            {
                LoadMinutesFromConfig();
            }

            else if (line.Equals("X", StringComparison.OrdinalIgnoreCase))
            {
                System.Environment.Exit(0);
            }

            else if(GetInputMinutes(line, out int tmpMinutes))
            {
                _minutes = tmpMinutes;
            }

            else
            {
                Console.WriteLine("Input was invalid!");
                SetupMinutes();
            }
        }

        private static bool GetInputMinutes(string line, out int tmpMinutes)
        {
            if(int.TryParse(line, out tmpMinutes))
            {
                return true;
            }

            return false;
        }

        private static void LoadMinutesFromConfig()
        {   
            if (!_configFile.TryGetInt("minutes", out _minutes))
            {
                Console.WriteLine("Could not read minutes");
            }
        }

        private static bool RepeatedTimer()
        {
            SingleTimer();
            return RestartDialogue();
        }

        private static bool RestartDialogue()
        {
            Console.Clear();
            Console.WriteLine("Timer has finished!");
            Console.WriteLine("R: Restart, X: Exit, Else: Back to minutes setup");

            var key = Console.ReadKey();

            if (key.Key == ConsoleKey.R)
            {
                return true;
            }
            else if(key.Key == ConsoleKey.X)
            {
                Environment.Exit(0);
            }

            return false;
        }

        private static void TryPlaySound(MediaPlayer player)
        {
            if(player != null)
            {
                player.Stop();
                player.Play();
            }
        }

        private static void SingleTimer()
        {
            bool tick = true;
            bool isPaused = false;
            bool isRunning = true;

            int leftHours = 0;
            int leftMinutes = 0;
            int leftSeconds = 0;

            int totalSecondsLeft = _minutes * 60;

            while (isRunning)
            {
                while (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.P)
                    {
                        isPaused = !isPaused;

                        if (isPaused)
                        {
                            if(_tickSound != null)
                            {
                                _tickSound.Stop();
                            }

                            if (_tockSound != null)
                            {
                                _tockSound.Stop();
                            }

                            Console.WriteLine("- Timer is paused -");
                        }
                    }
                    else if (key.Key == ConsoleKey.S)
                    {
                        TryPlaySound(_alarmSound);
                        return;
                    }
                    else if (key.Key == ConsoleKey.X)
                    {
                        System.Environment.Exit(0);
                        return;
                    }
                }

                if (!isPaused)
                {
                    if (tick)
                    {
                        TryPlaySound(_tickSound);
                    }
                    else
                    {
                        TryPlaySound(_tockSound);
                    }

                    tick = !tick;

                    leftHours = (totalSecondsLeft / 3600);
                    leftMinutes = (totalSecondsLeft / 60) % 60;
                    leftSeconds = totalSecondsLeft % 60;

                    Console.Clear();
                    Console.WriteLine($"Time left: {leftHours.ToString("00")}:{leftMinutes.ToString("00")}:{leftSeconds.ToString("00")}");
                    Console.WriteLine($"P: Pause. S: Stop. X: Exit");

                    totalSecondsLeft--;

                    if (totalSecondsLeft <= 0)
                    {
                        isRunning = false;
                    }
                }

                System.Threading.Thread.Sleep(1000);
            }


            TryPlaySound(_alarmSound);
        }
    }
}
