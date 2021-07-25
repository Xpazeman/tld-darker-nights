using System.IO;
using System.Reflection;
using ModSettings;

namespace DarkerNights
{
    internal class DarkerNightsSettings : JsonModSettings
    {
        [Section("Base Darkness")]

        [Name("Darkness Multiplier")]
        [Description("1 is vanilla default, 0 is totally pitch black. This will be affected by aurora and moon phase if those are set.")]
        [Slider(0f, 1f, 1, NumberFormat = "{0:F2}")]
        public float darknessMultiplier = 0f;

        [Section("Aurora Modifiers")]

        [Name("Aurora illumination")]
        [Description("Activate this for aurora to have an effect on night's brightness and color.")]
        public bool auroraIllumination = true;

        [Name("Aurora brightness")]
        [Description("How intense the aurora illumination will be.")]
        [Slider(0f, 1f, 1, NumberFormat = "{0:F2}")]
        public float auroraIntensity = 0.5f;

        [Name("Aurora color saturation")]
        [Description("How colorful the aurora illumination will be.")]
        [Slider(0f, 2f, 1, NumberFormat = "{0:F2}")]
        public float auroraSaturation = 1.2f;

        [Section("Moon Modifiers")]

        [Name("Moon phase impact")]
        [Description("How much of an extra impact moon phases will have. Set to 0 for no impact.")]
        [Slider(0f, 1f, 1, NumberFormat = "{0:F2}")]
        public float moonImpact = 0.2f;

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            if (field.Name == nameof(auroraIllumination))
            {
                RefreshFields();
            }
        }

        internal void RefreshFields()
        {
            SetFieldVisible(nameof(auroraIntensity), auroraIllumination);
            SetFieldVisible(nameof(auroraSaturation), auroraIllumination);
        }
    }

    internal static class Settings
    {
        public static DarkerNightsSettings options;

        public static void OnLoad()
        {
            options = new DarkerNightsSettings();
            options.RefreshFields();
            options.AddToModSettings("Darker Nights Settings");
        }
    }
}
