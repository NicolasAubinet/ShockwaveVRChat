using System;
using System.IO;
using System.Threading;
using OscLib;
using OscLib.Config;

namespace ShockwaveVRChat
{
    internal static class Program
    {
        internal static DevicesConfig Devices;
        internal static VRChatConfig VRChat;

        internal static VRChatSupport VRCSupport = new VRChatSupport();

        static Program()
        {
            string basefolder = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            string configfolder = Path.Combine(basefolder, "Config");
            if (!Directory.Exists(configfolder))
                Directory.CreateDirectory(configfolder);

            Devices = ConfigManager.CreateConfig<DevicesConfig>(configfolder, nameof(Devices));
            VRChat = ConfigManager.CreateConfig<VRChatConfig>(configfolder, nameof(VRChat));
        }

        internal static int Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;

            bool isFirst;
            Mutex mutex = new Mutex(true, BuildInfo.Name, out isFirst);
            if (!isFirst)
                return 0;

            WelcomeMessage();
            
            try
            {
                OscManager.Load();
                ConfigManager.LoadAll();

                Action originalConnectionAct = OscManager.Connection.OnFileModified;
                OscManager.Connection.OnFileModified = () =>
                {
                    Console.WriteLine();
                    Console.WriteLine("Connection.cfg Reloaded!");
                    Console.WriteLine();
                    PrintConnection();
                    originalConnectionAct();
                };
                PrintConnection();

                Devices.OnFileModified += () =>
                {
                    Console.WriteLine();
                    Console.WriteLine("Devices.cfg Reloaded!");
                    Console.WriteLine();
                    PrintDevices();
                };
                PrintDevices();

                VRChat.OnFileModified += () =>
                {
                    Console.WriteLine();
                    Console.WriteLine("VRChat.cfg Reloaded!");
                    Console.WriteLine();
                    PrintVRChat();
                };
                PrintVRChat();

                ShockwaveManager.Instance.InitializeSuit();

                OscManager.AttachOscAttributesFromAssembly(typeof(Program).Assembly);
                OscManager.Connect();
                VRCSupport.BeginInit();

                Console.WriteLine();
                Console.WriteLine("Awaiting Packets...");
                Console.WriteLine();
                Console.WriteLine("Please leave this application open to handle OSC Communication.");
                Console.WriteLine("Press ESC to Exit.");
                Console.WriteLine();

                while (Console.ReadKey(true).Key != ConsoleKey.Escape)
                    Thread.Sleep(1);
            }
            catch (Exception ex) { ErrorMessageBox(ex.ToString()); }

            Environment.Exit(0);
            return 0;
        }

        private static void ErrorMessageBox(string msg)
        {
            Console.Error.WriteLine(msg);
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            try
            {
                VRCSupport.EndInit();
                OscManager.Disconnect();
                ShockwaveManager.Instance.DisconnectSuit();
                ConfigManager.SaveAll();
            }
            catch (Exception ex) { ErrorMessageBox(ex.ToString()); }
        }

        private static void WelcomeMessage()
        {
            Console.WriteLine(Console.Title = $"{BuildInfo.Name} v{BuildInfo.Version}");
            Console.WriteLine("Created by Nicolas Aubinet");
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void PrintVRChat()
        {
            Console.WriteLine($"===== VRChat - Reactivity =====");
            Console.WriteLine();
            Console.WriteLine($"[AFK] = {VRChat.reactivity.Value.AFK}");
            Console.WriteLine($"[InStation] = {VRChat.reactivity.Value.InStation}");
            Console.WriteLine($"[Seated] = {VRChat.reactivity.Value.Seated}");
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine($"===== VRChat - Avatar OSC Config Reset =====");
            Console.WriteLine();
            Console.WriteLine($"[Enabled] = {VRChat.avatarOSCConfigReset.Value.Enabled}");
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void PrintConnection()
        {
            Console.WriteLine($"===== OSC Receiver =====");
            Console.WriteLine();
            Console.WriteLine($"[Port] = {OscManager.Connection.receiver.Value.Port}");
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine($"===== OSC Sender =====");
            Console.WriteLine();
            Console.WriteLine($"[Enabled] = {OscManager.Connection.sender.Value.Enabled}");
            Console.WriteLine($"[IP] = {OscManager.Connection.sender.Value.IP}");
            Console.WriteLine($"[Port] = {OscManager.Connection.sender.Value.Port}");
            Console.WriteLine();
            Console.WriteLine();
        }

        private static void PrintDevices()
        {
            Console.WriteLine("===== Devices =====");
            Console.WriteLine();

            PrintDevice("Vest", ShockwaveManager.HapticRegion.TORSO);

            PrintDevice("Upper Left Arm", ShockwaveManager.HapticRegion.LEFTUPPERARM);
            PrintDevice("Upper Right Arm", ShockwaveManager.HapticRegion.RIGHTUPPERARM);

            PrintDevice("Lower Left Arm", ShockwaveManager.HapticRegion.LEFTLOWERARM);
            PrintDevice("Lower Right Arm", ShockwaveManager.HapticRegion.RIGHTLOWERARM);

            PrintDevice("Upper Left Leg", ShockwaveManager.HapticRegion.LEFTUPPERLEG);
            PrintDevice("Upper Right Leg", ShockwaveManager.HapticRegion.RIGHTUPPERLEG);

            PrintDevice("Lower Left Leg", ShockwaveManager.HapticRegion.LEFTLOWERLEG);
            PrintDevice("Lower Right Leg", ShockwaveManager.HapticRegion.RIGHTLOWERLEG);

            Console.WriteLine();
        }

        private static void PrintDevice(string name, ShockwaveManager.HapticRegion hapticRegion)
        {
            Console.WriteLine($"[{name}  |  Enabled] = {Devices.HapticRegionToEnabled(hapticRegion)}");
            Console.WriteLine($"[{name}  |  Intensity] = {Devices.HapticRegionToIntensity(hapticRegion)}");

            Console.WriteLine();
        }
    }
}