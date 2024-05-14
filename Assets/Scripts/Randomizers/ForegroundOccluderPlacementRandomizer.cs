using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;

using UnityEngine.Perception.Randomization.Samplers;
using UnityEngine.Perception.Randomization.Utilities;

namespace SynthDet.Randomizers
{
    /// <summary>
    /// Creates a 2D layer of of evenly spaced GameObjects from a given list of prefabs
    /// </summary>
    [Serializable]
    [AddRandomizerMenu("SynthDet/Foreground Occluder Placement Randomizer")]
    public class ForegroundOccluderPlacementRandomizer : Randomizer
    {
        /// <summary>
        /// The Z offset component applied to the generated layer of GameObjects
        /// </summary>
        public float depth = 5;

        /// <summary>
        /// The minimum distance between all placed objects
        /// </summary>
        public FloatParameter occluderSeparationDistance = new FloatParameter { value = new UniformSampler(2f, 2f) };

        /// <summary>
        /// The size of the 2D area designated for object placement
        /// </summary>
        public Vector2 placementArea = new Vector2(6f, 6f);

        private SettingsScriptableObject settings;

        /// <summary>
        /// The list of prefabs sample and randomly place
        /// </summary>
        public CategoricalParameter<GameObject> prefabs;

        GameObject m_Container;
        GameObjectOneWayCache m_GameObjectOneWayCache;
    
        protected override void OnAwake()
        {
            m_Container = new GameObject("Foreground Occluders");
            var transform = scenario.transform;
            m_Container.transform.parent = transform;
            m_GameObjectOneWayCache = new GameObjectOneWayCache(
                m_Container.transform, prefabs.categories.Select(element => element.Item1).ToArray(), this);
        }

        /// <summary>
        /// Generates a foreground layer of objects at the start of each scenario iteration
        /// </summary>
        /// 
        protected override void OnScenarioStart()
        {
            settings = Resources.Load<SettingsScriptableObject>("Settings");
        }
        protected override void OnIterationStart()
        {
            var seed = SamplerState.NextRandomState();
            var seperationDistance = settings.size * settings.relativeOccluderSeperationDistance;
            var placementSamples = PoissonDiskSampling.GenerateSamples(
                placementArea.x, placementArea.y, seperationDistance, seed);
            var offset = new Vector3(placementArea.x, placementArea.y, 0f) * -0.5f;
        
            foreach (var sample in placementSamples)
            {
                //Sets the position and normalizes the size of the object
                var instance = m_GameObjectOneWayCache.GetOrInstantiate(prefabs.Sample());

                instance.transform.localPosition = Vector3.zero;
                instance.transform.localScale = Vector3.one;
                instance.transform.localRotation = Quaternion.identity;
                var bounds = SynthDet.Randomizers.ForegroundObjectPlacementRandomizer.ComputeBounds(instance);
                
                instance.transform.localPosition = new Vector3(sample.x, sample.y, depth) + offset - bounds.center;
                var scale = instance.transform.localScale;
                var magnitude = bounds.extents.magnitude;
                scale.Scale(new Vector3(1/magnitude, 1/magnitude, 1/magnitude));
                instance.transform.localScale = scale;

                //Sets the size of the occluder objects relative to the size of the foreground object to ensure
                //it only occludes partially
                instance.transform.localScale *= settings.size * settings.relativeOccluderSize.Sample();
            }
            
            placementSamples.Dispose();
        }
        
        /// <summary>
        /// Deletes generated foreground objects after each scenario iteration is complete
        /// </summary>
        protected override void OnIterationEnd()
        {
            m_GameObjectOneWayCache.ResetAllObjects();
        }
    }
}

