using System;
using SynthDet.Randomizers;
using SynthDet.RandomizerTags;
using UnityEngine;
using UnityEngine.Perception.Randomization;
using UnityEngine.Perception.Utilities;
using UnityEditor;

namespace Runtime.AssetRoles
{
    [Serializable]
    public class ForegroundAssetRole : AssetRole<GameObject>
    {
        public override string label => "foreground";
        public override string description => "Foreground objects for CV training and prediction";

        public override void Preprocess(GameObject asset)
        {
            ConfigureLayerRecursive(asset);
            AddRandomizerTags(asset);
        }
        
        void ConfigureLayerRecursive(GameObject prefab)
        {
            prefab.layer = LayerMask.NameToLayer("Foreground");
            for (var i = 0; i < prefab.transform.childCount; i++)
            {
                var child = prefab.transform.GetChild(i).gameObject;
                ConfigureLayerRecursive(child);
            }
        }

        void AddRandomizerTags(GameObject prefab)
        {
            //Adds all the necessary Randomizertags to the foreground prefabs
            Utilities.GetOrAddComponent<ForegroundObjectMetricReporterTag>(prefab);
            Utilities.GetOrAddComponent<SynthDet.RandomizerTags.MyRotationRandomizerTag>(prefab);
            Utilities.GetOrAddComponent<UnityEngine.Perception.GroundTruth.LabelManagement.Labeling>(prefab);
            UnityEngine.Perception.GroundTruth.LabelManagement.Labeling labelingComponent = prefab.GetComponent<UnityEngine.Perception.GroundTruth.LabelManagement.Labeling>();
            //Makes the label from the prefab name
            labelingComponent.labels.Add(prefab.name.Replace("(Clone)", ""));
            
            
        }
    }
}
