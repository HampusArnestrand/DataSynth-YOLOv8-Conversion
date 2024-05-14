using System;
using UnityEngine;
using UnityEngine.Perception.Randomization.Randomizers;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Samplers;

namespace SynthDet.RandomizerTags
{
    /// <summary>
    /// Used in conjunction with a RotationRandomizer to vary the rotation of GameObjects
    /// </summary>
    [AddComponentMenu("SynthDet/RandomizerTags/My Rotation Randomizer Tag")]
    public class MyRotationRandomizerTag : RandomizerTag { 
        /// <summary>
        /// Makes sure the bottom of the object is not visible if enabled
        /// </summary>
        public bool noBottom = false;

        /// <summary>
        /// Restricts the possible rotations of the object if enabled
        /// </summary>
        public bool lessBottom = false;
        /// <summary>
        /// Specifies the range of possibles angles 
        /// </summary>
        public FloatParameter angle = new FloatParameter { value = new UniformSampler(0.5f, 6f) };


    }
}

