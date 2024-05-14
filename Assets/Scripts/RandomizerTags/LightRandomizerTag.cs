using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Rendering.HighDefinition;

namespace SynthDet.RandomizerTags
{
    [RequireComponent(typeof(Light))]
    [RequireComponent(typeof(HDAdditionalLightData))]
    [AddComponentMenu("SynthDet/RandomizerTags/MyLightRandomizerTag")]
    public class LightRandomizerTag : RandomizerTag
    {
        HDAdditionalLightData m_LightData;

        void Awake()
        {
            m_LightData = GetComponent<HDAdditionalLightData>();
        }
        
        /// <summary>
        /// Sets the intensity of the light that the tag is attached to
        /// </summary>
        public void SetIntensity(float intensity)
        {
            m_LightData.intensity = intensity;
        }
    }
}
