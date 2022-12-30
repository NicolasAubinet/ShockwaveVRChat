using OscLib.Config;

namespace ShockwaveVRChat
{
    public abstract class DeviceCategoryBase : ConfigCategoryValue
    {
        public abstract bool GetEnabled();
        public abstract int GetIntensity();
    }

    public class DevicesConfigBase<T> : ConfigFile where T : DeviceCategoryBase
    {
        public ConfigCategory<T> Vest;

        public ConfigCategory<T> UpperArmLeft;
        public ConfigCategory<T> UpperArmRight;

        public ConfigCategory<T> LowerArmLeft;
        public ConfigCategory<T> LowerArmRight;

        public ConfigCategory<T> UpperLegLeft;
        public ConfigCategory<T> UpperLegRight;

        public ConfigCategory<T> LowerLegLeft;
        public ConfigCategory<T> LowerLegRight;

        public DevicesConfigBase(string filepath) : base(filepath)
        {
            Categories.AddRange(new ConfigCategory[]{
                Vest = new ConfigCategory<T>(nameof(Vest)),

                UpperArmLeft = new ConfigCategory<T>(nameof(UpperArmLeft)),
                UpperArmRight = new ConfigCategory<T>(nameof(UpperArmRight)),

                LowerArmLeft = new ConfigCategory<T>(nameof(LowerArmLeft)),
                LowerArmRight = new ConfigCategory<T>(nameof(LowerArmRight)),

                UpperLegLeft = new ConfigCategory<T>(nameof(UpperLegLeft)),
                UpperLegRight = new ConfigCategory<T>(nameof(UpperLegRight)),

                LowerLegLeft = new ConfigCategory<T>(nameof(LowerLegLeft)),
                LowerLegRight = new ConfigCategory<T>(nameof(LowerLegRight))
            });
        }

        public bool HapticRegionToEnabled(ShockwaveManager.HapticRegion region)
        {
            switch (region)
            {
                case ShockwaveManager.HapticRegion.TORSO:
                    return Vest.Value.GetEnabled();
                case ShockwaveManager.HapticRegion.LEFTUPPERARM:
                    return UpperArmLeft.Value.GetEnabled();
                case ShockwaveManager.HapticRegion.LEFTLOWERARM:
                    return LowerArmLeft.Value.GetEnabled();
                case ShockwaveManager.HapticRegion.RIGHTUPPERARM:
                    return UpperArmRight.Value.GetEnabled();
                case ShockwaveManager.HapticRegion.RIGHTLOWERARM:
                    return LowerArmRight.Value.GetEnabled();
                case ShockwaveManager.HapticRegion.LEFTUPPERLEG:
                    return UpperLegLeft.Value.GetEnabled();
                case ShockwaveManager.HapticRegion.LEFTLOWERLEG:
                    return LowerLegLeft.Value.GetEnabled();
                case ShockwaveManager.HapticRegion.RIGHTUPPERLEG:
                    return UpperLegRight.Value.GetEnabled();
                case ShockwaveManager.HapticRegion.RIGHTLOWERLEG:
                    return LowerLegRight.Value.GetEnabled();
                default:
                    return true;
            }
        }

        public int HapticRegionToIntensity(ShockwaveManager.HapticRegion region)
        {
            switch (region)
            {
                case ShockwaveManager.HapticRegion.TORSO:
                    return Vest.Value.GetIntensity();
                case ShockwaveManager.HapticRegion.LEFTUPPERARM:
                    return UpperArmLeft.Value.GetIntensity();
                case ShockwaveManager.HapticRegion.LEFTLOWERARM:
                    return LowerArmLeft.Value.GetIntensity();
                case ShockwaveManager.HapticRegion.RIGHTUPPERARM:
                    return UpperArmRight.Value.GetIntensity();
                case ShockwaveManager.HapticRegion.RIGHTLOWERARM:
                    return LowerArmRight.Value.GetIntensity();
                case ShockwaveManager.HapticRegion.LEFTUPPERLEG:
                    return UpperLegLeft.Value.GetIntensity();
                case ShockwaveManager.HapticRegion.LEFTLOWERLEG:
                    return LowerLegLeft.Value.GetIntensity();
                case ShockwaveManager.HapticRegion.RIGHTUPPERLEG:
                    return UpperLegRight.Value.GetIntensity();
                case ShockwaveManager.HapticRegion.RIGHTLOWERLEG:
                    return LowerLegRight.Value.GetIntensity();
                default:
                    return 100;
            }
        }
    }
}