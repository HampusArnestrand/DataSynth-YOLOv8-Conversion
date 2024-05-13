using System;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Perception.Randomization;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;

using UnityEngine.Perception.Randomization.Samplers;
using UnityEngine.Perception.Randomization.Utilities;
using UnityEngine.Perception.GroundTruth;

using MySamplers;

namespace SynthDet.Randomizers
{
    /// <summary>
    /// Creates a 2D layer of evenly spaced GameObjects from a given list of prefabs
    /// </summary>
    [Serializable]
    [AddRandomizerMenu("SynthDet/Foreground Object Placement Randomizer")]
    public class ForegroundObjectPlacementRandomizer : Randomizer
    {
        /// <summary>
        /// The maximal amount of foreground objects that can be placed
        /// </summary>
        private int maxObjectCount = 30;

        public UnityEngine.Perception.GroundTruth.LabelManagement.IdLabelConfig idLabelConfig;
        
        public AssetSource<GameObject> foregroundAssets;
        private LocalAssetSourceLocation localAssetSourceLocation;
        GameObject[] m_ProcessedAssetInstances;
        IntegerParameter m_ObjectIndexParameter = new IntegerParameter();
        /// <summary>
        /// The z-coordinate of the plane where the objects are placed
        /// </summary>
        public float depth = 3f;
        /// <summary>
        /// The size of the 2D-area where the objects will be placed
        /// </summary>
        public Vector2 placementArea = new Vector2(5f, 5f);
        public Camera MainCamera;

        private SettingsScriptableObject settings;

        GameObject m_Container;
        GameObjectOneWayCache m_GameObjectOneWayCache;

        [Tooltip("Enable this to normalize mesh sizes across all included objects, so that they all have a similar size before any further scale adjustments are applied during randomization. Note that this flag greatly influences the size of the objects on the screen, so any scale randomization will need to be adjusted based on the state of this flag.")]
        public bool normalizeObjectBounds = true;
        
        protected override void OnScenarioStart()
        {
            //Loads the settings file which will be used to get the chosen parameters
            settings = Resources.Load<SettingsScriptableObject>("Settings");
            if (settings == null){
                Debug.LogError("No settings found");
            }

            //Ensures the camera is in the right position and has the right field-of-view
            MainCamera.transform.position = new Vector3(0, 0, 100);
            MainCamera.fieldOfView = 2.4f;    


            m_Container = new GameObject("Foreground Objects");
            var transform = scenario.transform;
            m_Container.transform.parent = transform;

            //Loads all prefabs from '3Dmodels' folder into foregroundAssets in order to be used in the scene
            var assets = Resources.LoadAll("3Dmodels", typeof(UnityEngine.Object));
            localAssetSourceLocation = new LocalAssetSourceLocation();
            foreach (var asset in assets)
            {
                localAssetSourceLocation.assets.Add(asset);
            }
            foregroundAssets.assetSourceLocation = localAssetSourceLocation;
            m_ProcessedAssetInstances = foregroundAssets.CreateProcessedInstances();
            
            //Checks if the prefabs contains meshes
            m_ProcessedAssetInstances = m_ProcessedAssetInstances.Where(p =>
            {
                var isValid = ComputeBoundsUnchecked(p).IsValid;
                if (!isValid)
                    Debug.LogError($"Object {p} does not contain a mesh");

                return isValid;
            }).ToArray();
            
            m_GameObjectOneWayCache = new GameObjectOneWayCache(m_Container.transform, m_ProcessedAssetInstances, this);
            m_ObjectIndexParameter.value = new UniformSampler(0, m_ProcessedAssetInstances.Length);
            
            ///Adds all the prefabs' names to the IdLabelConfig which handles the labeling of the annotations
            var labelNames = new System.Collections.Generic.List<UnityEngine.Perception.GroundTruth.LabelManagement.IdLabelEntry>();
            var i = 0;
            foreach (GameObject prefab in Resources.LoadAll("3Dmodels", typeof(GameObject)))
            {
                labelNames.Add(new UnityEngine.Perception.GroundTruth.LabelManagement.IdLabelEntry(){
                    id = i,
                    label = prefab.name.Replace("(UnityEngine.GameObject)", ""),
                });
                i++;
            }
            idLabelConfig.Init(labelNames);
        }
    
        /// <summary>
        /// Generates a foreground layer of objects at the start of each scenario iteration
        /// </summary>
        protected override void OnIterationStart()
        {
            PlaceObjects();
        }

        void PlaceObjects()
        {
            //Randomizes the size of the objects
            var scaling = settings.objectSize.Sample();
            //
            var seperationDistance = scaling * settings.relativeSeperationDistance;
            //This variable is used to communicate with the BackgroundPlacementRandomizer what object size is currently used
            settings.size = scaling;

            var spawnedCount = 0;
            var seed = SamplerState.NextRandomState();
            //Randomizes the position of the objects by utilizing Poisson Disc Sampling. This is used to ensure
            //that the objects don't intersect with each other
            var placementSamples = MySamplers.PoissonDiscSampler.Sampling(Vector2.zero, new Vector2(placementArea.x - (scaling*1.5f), placementArea.y - (scaling*1.5f)), seperationDistance);
            //This offset is used to ensure the objects are centered around the origin
            var offset = new Vector3(placementArea.x - (scaling*2f), placementArea.y - (scaling*2f), 0) * -0.5f;

            //This loop places an object for every position sample created by the PoissonDiscSampler
            foreach (var sample in placementSamples)
            {
                if (spawnedCount == maxObjectCount)
                    break;

                var index = Math.Min(m_ProcessedAssetInstances.Length, m_ObjectIndexParameter.Sample());
                var prefab = m_ProcessedAssetInstances[index];
                var instance = m_GameObjectOneWayCache.GetOrInstantiate(prefab);

                //Sets the position of the object and normalizes the size if enabled
                if (normalizeObjectBounds)
                {
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localScale = Vector3.one;
                    instance.transform.localRotation = Quaternion.identity;
                    var bounds = ComputeBounds(instance);
                
                    instance.transform.localPosition = new Vector3(sample.x, sample.y, depth) + offset - bounds.center;
                    var scale = instance.transform.localScale;
                    var magnitude = bounds.extents.magnitude;
                    scale.Scale(new Vector3(1/magnitude, 1/magnitude, 1/magnitude));
                    instance.transform.localScale = scale;
                }
                else
                {
                    instance.transform.position = new Vector3(sample.x, sample.y, depth) + offset;    
                }
                //Applies the randomized size
                instance.transform.localScale *= scaling;

                spawnedCount++;
                
            }

        }

        /// <summary>
        /// Deletes generated foreground objects after each scenario iteration is complete
        /// </summary>
        protected override void OnIterationEnd()
        {
            m_GameObjectOneWayCache.ResetAllObjects();
        }

        
        /// <summary>
        /// Computes the outer bounds of a gameObject. Is used to normalize the size of the
        /// </summary>
        public static Bounds ComputeBounds(GameObject gameObject)
        {
            var bounds = ComputeBoundsUnchecked(gameObject);
            if (!bounds.IsValid)
                throw new ArgumentException($"GameObject {gameObject.name} must have a MeshFilter in its hierarchy.");

            var result = new Bounds();
            result.SetMinMax(bounds.Min, bounds.Max);
            return result;
        }

        static SynthDetMinMaxAABB ComputeBoundsUnchecked(GameObject gameObject)
        {
            SynthDetMinMaxAABB aabb = new SynthDetMinMaxAABB(
                new float3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity), 
                new float3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity));
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter != null)
            {
                var bounds = meshFilter.sharedMesh.bounds;
                aabb = SynthDetMinMaxAABB.CreateFromCenterAndExtents(bounds.center, bounds.extents);
            }

            var transform = gameObject.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                var childAabb = ComputeBoundsUnchecked(transform.GetChild(i).gameObject);
                aabb.Encapsulate(childAabb);
            }

            aabb = SynthDetMinMaxAABB.Transform(float4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale), aabb);
            return aabb;
        }
        
    }
}

