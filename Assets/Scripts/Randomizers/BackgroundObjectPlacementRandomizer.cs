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
    /// Creates multiple layers of evenly distributed but randomly placed objects
    /// </summary>
    [Serializable]
    [AddRandomizerMenu("SynthDet/Background Object Placement Randomizer")]
    public class BackgroundObjectPlacementRandomizer : Randomizer
    {
        /// <summary>
        /// The Z offset component applied to all generated background layers
        /// </summary>
        public float depth;

        /// <summary>
        /// The number of background layers to generate
        /// </summary>
        public int layerCount = 2;

        /// <summary>
        /// The minimum distance between placed background objects
        /// </summary>
        public float separationDistance = 0.5f;

        /// <summary>
        /// The 2D size of the generated background layers
        /// </summary>
        public Vector2 placementArea = new Vector2(6f, 6f);

        /// <summary>
        /// A categorical parameter for sampling random prefabs to place
        /// </summary>
        public CategoricalParameter<GameObject> prefabs;

        private SettingsScriptableObject settings;

        GameObject m_Container;
        GameObjectOneWayCache m_GameObjectOneWayCache;

        protected override void OnAwake()
        {
            m_Container = new GameObject("Background Distractors");
            m_Container.transform.parent = scenario.transform;
            m_GameObjectOneWayCache = new GameObjectOneWayCache(m_Container.transform,
                prefabs.categories.Select((element) => element.Item1).ToArray(), this);
        }

        protected override void OnScenarioStart()
        {
            //Loads the settings file which is used to get the chosen parameters
            settings = Resources.Load<SettingsScriptableObject>("Settings");
        }

        /// <summary>
        /// Generates background layers of objects at the start of each scenario iteration
        /// </summary>
        protected override void OnIterationStart()
        {
            for (var i = 0; i < layerCount; i++)
            {
                var seed = SamplerState.NextRandomState();
                //The seperationdistance is set depending on the size of the foreground objects, which
                //in turn decide the size of the occluder objects
                separationDistance = settings.size * 1.5f;
                //Generates random positions according to a Poisson Disc Sampler
                var placementSamples = PoissonDiskSampling.GenerateSamples(
                    placementArea.x, placementArea.y, separationDistance, seed);
                //Offset is used to make sure the objects are centered around the origin
                var offset = new Vector3(placementArea.x, placementArea.y, 0f) * -0.5f;

                //This loop places an object for every position sample created by the PoissonDiscSampler
                foreach (var sample in placementSamples)
                {
                    //Sets the position and normalizes the object size
                    var instance = m_GameObjectOneWayCache.GetOrInstantiate(prefabs.Sample());
                    instance.transform.position = new Vector3(sample.x, sample.y, separationDistance * -i + depth) + offset;

                    instance.transform.localScale = Vector3.one;
                    instance.transform.localRotation = Quaternion.identity;
                    var bounds = SynthDet.Randomizers.ForegroundObjectPlacementRandomizer.ComputeBounds(instance);
                
                    var scale = instance.transform.localScale;
                    var magnitude = bounds.extents.magnitude;
                    scale.Scale(new Vector3(1/magnitude, 1/magnitude, 1/magnitude));
                    instance.transform.localScale = scale;

                    //Sets the size of the background objects relative to the foreground object size
                    instance.transform.localScale *= settings.relativeBackgroundObjectSize.Sample() * settings.size;
                }

                placementSamples.Dispose();
            }
        }
        
        /// <summary>
        /// Deletes generated background objects after each scenario iteration is complete
        /// </summary>
        protected override void OnIterationEnd()
        {
            m_GameObjectOneWayCache.ResetAllObjects();
        }
    }
}