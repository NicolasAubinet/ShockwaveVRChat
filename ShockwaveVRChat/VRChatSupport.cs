using System;
using System.Collections.Generic;
using OscLib;
using OscLib.Utils;
using OscLib.VRChat;
using Rug.Osc;
using System.Collections.Concurrent;
using System.Threading;
using OscLib.VRChat.Attributes;

namespace ShockwaveVRChat
{
    internal class VRChatSupport : ThreadedTask
    {
        private bool ShouldRun;
        private bool AFK;
        private bool InStation;
        private bool Seated;

        private const int HAPTICS_COUNT = 72;
        private int[] HapticContacts = new int[HAPTICS_COUNT];
        private const string AvatarParameterPrefix = "/avatar/parameters";

        private class VRChatPacket { }

        private class VRChatPacket_Contact : VRChatPacket
        {
            internal int hapticIndex;
            internal int intensity;
        }
        private ConcurrentQueue<VRChatPacket> PacketQueue = new ConcurrentQueue<VRChatPacket>();

        private class VRChatPacket_int : VRChatPacket { internal int value; }
        private class VRChatPacket_bool : VRChatPacket { internal bool value; }
        private class VRChatPacket_string : VRChatPacket { internal string value; }

        private class VRChatPacket_AvatarChange : VRChatPacket_string { }
        private class VRChatPacket_AFK : VRChatPacket_bool { }
        private class VRChatPacket_InStation : VRChatPacket_bool { }
        private class VRChatPacket_Seated : VRChatPacket_bool { }

        internal VRChatSupport() : base()
        {
            for (int hapticIndex = 1; hapticIndex <= HAPTICS_COUNT; hapticIndex++)
            {
                string path = $"{AvatarParameterPrefix}/Shockwave_{hapticIndex}";
                int hapticIndexConst = hapticIndex;
                OscManager.Attach(path, (OscMessage msg) => OnContact(msg, hapticIndexConst));
            }
        }

        public override bool BeginInitInternal()
        {
            if (ShouldRun)
                EndInit();

            ShouldRun = true;
            return true;
        }

        public override void WithinThread()
        {
            while (ShouldRun)
            {
                while (PacketQueue.TryDequeue(out VRChatPacket packet))
                {
                    if (packet is VRChatPacket_AFK)
                        AFK = ((VRChatPacket_AFK)packet).value;
                    else if (packet is VRChatPacket_InStation)
                        InStation = ((VRChatPacket_InStation)packet).value;
                    else if (packet is VRChatPacket_Seated)
                        Seated = ((VRChatPacket_Seated)packet).value;
                    else if (packet is VRChatPacket_AvatarChange)
                    {
                        ResetContacts();
                        if (Program.VRChat.avatarOSCConfigReset.Value.Enabled)
                            VRCAvatarConfig.RemoveFile(((VRChatPacket_AvatarChange)packet).value);
                    }
                    else if (packet is VRChatPacket_Contact)
                    {
                        VRChatPacket_Contact contactPacket = (VRChatPacket_Contact) packet;
                        SetHapticIntensity(contactPacket.hapticIndex, contactPacket.intensity);
                    }
                }

                SubmitHaptics();

                if (ShouldRun)
                {
                    Thread.Sleep(50);
                }
            }
        }

        public override bool EndInitInternal()
        {
            ShouldRun = false;
            while (IsAlive()) { Thread.Sleep(100); }
            return true;
        }

        [VRC_AFK]
        private static void OnAFK(bool status)
            => Program.VRCSupport?.PacketQueue.Enqueue(new VRChatPacket_AFK { value = status });

        [VRC_InStation]
        private static void OnInStation(bool status)
            => Program.VRCSupport?.PacketQueue.Enqueue(new VRChatPacket_InStation { value = status });

        [VRC_Seated]
        private static void OnSeated(bool status)
            => Program.VRCSupport?.PacketQueue.Enqueue(new VRChatPacket_Seated { value = status });

        [VRC_AvatarChange]
        private static void OnAvatarChange(string avatarId)
        {
            Console.WriteLine($"Avatar Changed to {avatarId}");
            Program.VRCSupport?.PacketQueue.Enqueue(new VRChatPacket_AvatarChange { value = avatarId });
        }

        private static void OnContact(OscMessage msg, int hapticIndex)
        {
            if ((msg == null) || (!(msg[0] is bool)))
                return;
            Program.VRCSupport?.PacketQueue.Enqueue(new VRChatPacket_Contact
            {
                hapticIndex = hapticIndex,
                intensity = ((bool)msg[0]) ? Program.Devices.HapticRegionToIntensity(HapticIndexToRegion(hapticIndex)) : 0,
            });
        }

        private void SubmitHaptics()
        {
            if ((AFK && !Program.VRChat.reactivity.Value.AFK) 
                || (InStation && !Program.VRChat.reactivity.Value.InStation) 
                || (Seated && !Program.VRChat.reactivity.Value.Seated))
                return;

            List<int> hapticIndices = new List<int>();
            List<float> hapticStrengths = new List<float>();

            for (int i = 0; i < HapticContacts.Length; i++)
            {
                if (HapticContacts[i] > 0)
                {
                    int hapticIndex = i + 1;
                    ShockwaveManager.HapticRegion region = HapticIndexToRegion(hapticIndex);
                    if (!Program.Devices.HapticRegionToEnabled(region))
                    {
                        continue;
                    }

                    float strength = HapticContacts[i] / 100f;
                    hapticIndices.Add(hapticIndex);
                    hapticStrengths.Add(strength);
                }
            }

            if (hapticIndices.Count > 0)
            {
                ShockwaveManager.Instance.sendHapticsPulse(hapticIndices.ToArray(), hapticStrengths.ToArray(), 50);
            }
        }

        private void ResetContacts()
        {
            for (int i = 0; i < HapticContacts.Length; i++)
            {
                HapticContacts[i] = 0;
            }
        }

        private void SetHapticIntensity(int hapticIndex, int intensity)
        {
            HapticContacts[hapticIndex - 1] = intensity;
        }

        private static ShockwaveManager.HapticRegion HapticIndexToRegion(int hapticIndex)
        {
            ShockwaveManager.HapticRegion region;
            if (hapticIndex > 0 && hapticIndex < 40)
            {
                region = ShockwaveManager.HapticRegion.TORSO;
            }
            else if (hapticIndex < 44)
            {
                region = ShockwaveManager.HapticRegion.LEFTUPPERARM;
            }
            else if (hapticIndex < 48)
            {
                region = ShockwaveManager.HapticRegion.LEFTLOWERARM;
            }
            else if (hapticIndex < 52)
            {
                region = ShockwaveManager.HapticRegion.RIGHTUPPERARM;
            }
            else if (hapticIndex < 56)
            {
                region = ShockwaveManager.HapticRegion.RIGHTLOWERARM;
            }
            else if (hapticIndex < 60)
            {
                region = ShockwaveManager.HapticRegion.LEFTUPPERLEG;
            }
            else if (hapticIndex < 64)
            {
                region = ShockwaveManager.HapticRegion.LEFTLOWERLEG;
            }
            else if (hapticIndex < 68)
            {
                region = ShockwaveManager.HapticRegion.RIGHTUPPERLEG;
            }
            else if (hapticIndex < 72)
            {
                region = ShockwaveManager.HapticRegion.RIGHTLOWERLEG;
            }
            else
            {
                throw new ArgumentOutOfRangeException($"Could not get region for haptic index {hapticIndex}");
            }

            return region;
        }
    }
}