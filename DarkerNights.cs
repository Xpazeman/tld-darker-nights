using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using MelonLoader;
using HarmonyLib;
using Il2Cpp;

namespace DarkerNights
{
    class DarkerNights : MelonMod
    {
        public static List<string> notReallyOutdoors = new List<string>
        {
            "DamTransitionZone"
        };

        public override void OnApplicationStart()
        {
            Settings.OnLoad();

            Debug.Log("[darker-nights] Version " + Assembly.GetExecutingAssembly().GetName().Version);
        }

        public static void ApplyNightIllumination(UniStormWeatherSystem uniStorm)
        {
            TODBlendState curState = uniStorm.GetTODBlendState();
            TODStateData todState = uniStorm.m_ActiveEnvironment.m_TodState;
            Weather weatherComponent = GameManager.GetWeatherComponent();

            float dnMulti = Settings.options.darknessMultiplier;

            // Get base values
            Color baseColor = todState.m_AmbientLight * uniStorm.m_AnimScalar_Ambient * uniStorm.m_MasterAmbientIntensityScalar;
            float baseMoon = todState.m_MoonLightIntensity * uniStorm.m_MoonPhaseIntensityScalar * uniStorm.m_AnimScalar_Directional * uniStorm.m_MasterMoonLightIntensityScalar;
            Color baseFresnel = todState.m_TerrainFresnelColor;
            Color baseSnow = todState.m_BlowingSnowColor;

            float nightPct = uniStorm.GetTODBlendPercent(curState);

            float curIntMod = 1f;

            // Apply moon phase modifier
            if (Settings.options.moonImpact > 0 && uniStorm.m_MoonPhaseSet)
            {
                float baseValue = UniStormWeatherSystem.m_MoonIntensityByPhaseScalars[uniStorm.GetMoonPhaseIndex()] - UniStormWeatherSystem.m_MoonIntensityByPhaseScalars[0];

                dnMulti += baseValue * Settings.options.moonImpact;
            }

            // Set aurora illumination
            float auroraFade = GameManager.GetAuroraManager().GetNormalizedAlphaSquare();

            if (Mathf.Abs(auroraFade) > 0.0001f && Settings.options.auroraIllumination)
            {
                Color auroraColour = GameManager.GetAuroraManager().GetAuroraColour();
                ColorHSV auroraModColor = auroraColour;

                auroraModColor.s *= Settings.options.auroraSaturation;
                auroraModColor.v *= Settings.options.auroraIntensity;

                float auroraLevel = Mathf.Clamp01(GameManager.GetAuroraManager().m_NormalizedActive / GameManager.GetAuroraManager().m_FullyActiveValue);

                if (dnMulti < 0.05f)
                {
                    dnMulti = Mathf.Lerp(dnMulti, 0.05f, auroraLevel);
                }

                baseColor = Color.Lerp(baseColor, auroraModColor, auroraLevel);
                baseFresnel = Color.Lerp(baseFresnel, auroraModColor, auroraLevel);
            }

            

            dnMulti = Mathf.Clamp01(dnMulti);

            // Lerp illumination intensity
            switch (curState)
            {
                case TODBlendState.DuskToNightStart:
                    curIntMod = Mathf.Lerp(1f, dnMulti, nightPct);
                    break;

                case TODBlendState.NightStartToNightEnd:
                    curIntMod = dnMulti;
                    break;

                case TODBlendState.NightEndToDawn:
                    curIntMod = Mathf.Lerp(dnMulti, 1f, nightPct);
                    break;
            }

            // Adjust base values
            ColorHSV colorHSV = baseColor;
            colorHSV.v *= curIntMod;

            ColorHSV fresnelHSV = baseFresnel;
            fresnelHSV.v *= curIntMod;

            ColorHSV snowHSV = baseSnow;
            snowHSV.v *= curIntMod;

            float moonInt = baseMoon * curIntMod;

            // Apply Illumination
            uniStorm.m_MoonLight.intensity = moonInt;
            Utils.SetAmbientLight(colorHSV);
            Shader.SetGlobalColor("_Fresnelcolor", fresnelHSV);
            try
            {
                weatherComponent.SetBlowingSnowColor(snowHSV);
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(UniStormWeatherSystem), "Update")]
    internal class UniStormWeatherSystem_Update
    {
        private static void Postfix(UniStormWeatherSystem __instance)
        {
            if (GameManager.IsOutDoorsScene(GameManager.m_ActiveScene) && !DarkerNights.notReallyOutdoors.Contains(GameManager.m_ActiveScene))
            {
                if (__instance.IsNightOrNightBlend())
                {
                    DarkerNights.ApplyNightIllumination(__instance);                    
                }
            }
        }
    }
}
