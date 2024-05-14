using System;
using SynthDet.RandomizerTags;
using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Samplers;

namespace SynthDet.Randomizers
{
    /// <summary>
    /// Randomizes the rotation of objects tagged with a RotationRandomizerTag
    /// </summary>
    [Serializable]
    [AddRandomizerMenu("SynthDet/My Rotation Randomizer")]
    public class MyRotationRandomizer : Randomizer
    {
        /// <summary>
        /// Defines the range of random rotations that can be assigned to tagged objects
        /// </summary>
        public Vector3Parameter rotation = new Vector3Parameter
        {
            x = new UniformSampler(0, 360),
            y = new UniformSampler(0, 360),
            z = new UniformSampler(0, 360)
        };

        private float x;
        private float y;
        private float z;

        /// <summary>
        /// Randomizes the rotation of tagged objects at the start of each scenario iteration
        /// </summary>
        protected override void OnIterationStart()
        {
            //Gets all prefabs with this tag attached to it
            var prefabs = tagManager.Query<MyRotationRandomizerTag>();

            foreach (var prefab in prefabs)
            {
                var myRotationRandomizerTag = prefab.GetComponent<MyRotationRandomizerTag>();

                //Makes sure the bottom of the object is not visible  
                if (myRotationRandomizerTag.noBottom){
                    z = UnityEngine.Random.Range(-180f, 180f);

                    if (-90 < z & z < 90){
                        x = UnityEngine.Random.Range(-90f, 90f);

                    } 
                    else {
                        x = UnityEngine.Random.Range(90f, 270f);

                    }
                    y = UnityEngine.Random.Range(0, 360);
                    prefab.transform.rotation = Quaternion.Euler(new Vector3(x, y, z));
                    prefab.transform.Rotate(90f, 0f, 0f, Space.World);
                    }
                //Restricts the possible rotations of the object
                else if(myRotationRandomizerTag.lessBottom){

                    prefab.transform.rotation = Quaternion.Euler(Vector3.zero);
                    prefab.transform.Rotate(0f, UnityEngine.Random.Range(0, 360), 0f, Space.World);
                    prefab.transform.Rotate(myRotationRandomizerTag.angle.Sample(), 0f, 0f, Space.Self);
                    prefab.transform.Rotate(0f, UnityEngine.Random.Range(0, 360), 0f, Space.Self);
                    prefab.transform.Rotate(90f, 0f, 0f, Space.World);
        
                }
                //Randomizes the rotation completely
                else{
                    prefab.transform.rotation = Quaternion.Euler(rotation.Sample());
                }
                

            }
        }
    }
}

