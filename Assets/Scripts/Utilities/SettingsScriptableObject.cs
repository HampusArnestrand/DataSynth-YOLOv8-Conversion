using UnityEngine;
using UnityEngine.Perception.Randomization.Parameters;
using UnityEngine.Perception.Randomization.Samplers;

/// <summary>
/// A ScritableObject that includes all the different parameters for the data generation
/// </summary>
[CreateAssetMenu(fileName = "Settings", menuName = "ScriptableObjects/SettingsScriptableObject", order = 1)]
public class SettingsScriptableObject : ScriptableObject
{
    //Foreground objects
    public FloatParameter objectSize = new FloatParameter { value = new UniformSampler(0.9f, 4.9f) };

    public float relativeSeperationDistance = 4f;
    //Camera settings
    public bool cameraBlur;
    public float cameraBlur_Probability;
    public FloatParameter blurIntensity = new() { value = new UniformSampler(0, 1, true, 0, 1) };
    
    public float size;
    //Occluder objects

    public float relativeOccluderSeperationDistance = 1.5f;

    public FloatParameter relativeOccluderSize = new FloatParameter { value = new UniformSampler(0.3f, 0.5f) };
    //Backgroundobjects
    public FloatParameter relativeBackgroundObjectSize = new FloatParameter { value = new UniformSampler(0.8f, 1.2f) };

    //Lights
    /// <summary>
    /// Sets the range of values that the intensity can take
    /// </summary>
    public FloatParameter lightIntensityParameter = new() { value = new UniformSampler(0f, 1f) };
    /// <summary>
    /// Sets the range of values that the hue can have
    /// </summary>
    public ColorRgbParameter lightColorParameter = new()
    {
        red = new UniformSampler(0.4f, 1f),
        green = new UniformSampler(0.4f, 1f),
        blue = new UniformSampler(0.4f, 1f),
        alpha = new ConstantSampler(1f)
    };
}
