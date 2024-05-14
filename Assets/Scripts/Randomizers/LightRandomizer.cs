using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;
using LightRandomizerTag = SynthDet.RandomizerTags.LightRandomizerTag;

namespace SynthDet.Randomizers
{
    /// <summary>
    /// Randomizes the intensity and hue of each light
    /// </summary>
    [Serializable]
    [AddRandomizerMenu("SynthDet/Light Randomizer")]
    public class LightRandomizer : Randomizer
    {
        

        private SettingsScriptableObject settings;

        protected override void OnScenarioStart()
        {
            settings = Resources.Load<SettingsScriptableObject>("Settings");
        }
        protected override void OnIterationStart()
        {   
            //Find all lights with a LightRandomizerTag
            var randomizerTags = tagManager.Query<LightRandomizerTag>();
            foreach (var tag in randomizerTags)
            {
                var light = tag.GetComponent<Light>();
                light.color = settings.lightColorParameter.Sample();
                tag.SetIntensity(settings.lightIntensityParameter.Sample());
            }
        }
    }
}