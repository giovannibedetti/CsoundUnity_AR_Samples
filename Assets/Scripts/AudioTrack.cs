using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;
using UTIL = MathUtilities;

public class AudioTrack : MonoBehaviour
{
    [Tooltip("The id of the AudioTrack will be updated by CsoundManager, will be the index of CsoundManager.tracks")]
    public int id;
    [Tooltip("The desired volume of the AudioSource linked with this AudioTrack when audio output is enabled")]
    public float volume;
    [Tooltip("A debug object that will be shown when debugOn is true (and track is enabled)")]
    [SerializeField] GameObject debugObject;
    [Tooltip("The AudioSource linked with this AudioTrack")]
    [SerializeField] AudioSource audioSource;
    [Tooltip("The AudioClip to be loaded by the CsoundManager, to be played/analysed by Csound")]
    [SerializeField] AudioClip audioClip;
    [Tooltip("The expected range in amplitude of this AudioTrack")]
    public Vector2 expectedAmplitudeRange;
    [Tooltip("The expected range in frequency of this AudioTrack")]
    public Vector2 expectedFrequencyRange;
    [Tooltip("The reacting properties of this AudioTrack")]
    [SerializeField] ReactingProperty[] reactingProperties;
    bool isEnabled = false;
    bool debugOn = true;
    /// <summary>
    /// The amplitude value that will be updated by SetAmplitude 
    /// </summary>
    float amplitude;

    /// <summary>
    /// The frequency value that will be updated by SetFrequency
    /// TODO when frequency is in hertz the value is exponentially scaled
    /// </summary>
    float frequency;

    public bool IsEnabled { get => isEnabled; }
    public AudioClip AudioClip { get => audioClip; }

    public void ShowDebug(bool show)
    {
        if (isEnabled) debugObject.SetActive(show);
        debugOn = show;
    }

    /// <summary>
    /// Enable/Disable the visibility and audio output of this AudioTrack
    /// </summary>
    /// <param name="enable"></param>
    public void Enable(bool enable)
    {
        Debug.Log((enable ? "Enabling" : "Disabling") + " track " + name);
        foreach (var prop in reactingProperties)
        {
            if (prop.effect) prop.effect.gameObject.SetActive(enable);
            // TODO if reactingObject is parented to another object, the latter is not disabled and hence always visible
            if (prop.reactingObject) prop.reactingObject.gameObject.SetActive(enable);

        }
        this.gameObject.SetActive(enable);
        audioSource.volume = enable ? volume : 0;
        isEnabled = enable;
        debugObject.SetActive(debugOn && enable);

    }

    /// <summary>
    /// This method needs to be called per frame to update the reacting properties
    /// </summary>
    /// <param name="val">the amplitude of the track</param>
    public void SetAmplitude(float val)
    {
        amplitude = val;
        //Debug.Log($"Track {id} amplitude is: {amplitude}");
    }

    /// <summary>
    /// This method needs to be called per frame to update the reacting properties
    /// </summary>
    /// <param name="val">the base (strongest) frequency of the track</param>
    public void SetFrequency(float val)
    {
        frequency = val;
        //Debug.Log($"Track {id} frequency is: {frequency}");
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var property in reactingProperties)
        {
            var step = property.speed * Time.deltaTime;
            float value = 0f;
            Vector2 rng = Vector2.zero;

            // update value and rng with different values based on analysis type
            switch (property.analysisType)
            {
                case AnalysisType.AMP:
                    value = amplitude;
                    rng = expectedAmplitudeRange;
                    break;
                case AnalysisType.FREQ:
                    value = frequency;
                    rng = expectedFrequencyRange;
                    break;
            }
            // update the property value based on the previous choice
            property.value = value;
            // remap the float value
            float remapped = property.remap ?
                UTIL.Remap(property.value, rng.x, rng.y, property.desiredRange.x, property.desiredRange.y) :
                property.value;

            var remappedVector = UTIL.RemapVector(property.value, rng, property.min, property.max, property.clamp);

            if (property.log)
            {
                Debug.Log($"value: {property.value}, remapped: {remapped}");
                Debug.Log($"value: {property.value}, remappedVector: {remappedVector}");
            }

            switch (property.type)
            {
                case PropertyType.FLOAT:

                    if (property.material)
                        if (property.material.HasProperty(property.name))
                        {
                            var lerped = Mathf.Lerp(property.material.GetFloat(property.name), remapped, step);
                            property.material.SetFloat(property.name, lerped);
                        }
                    if (property.effect)
                        if (property.effect.HasFloat(property.name))
                        {
                            var lerped = Mathf.Lerp(property.effect.GetFloat(property.name), remapped, step);
                            property.effect.SetFloat(property.name, lerped);
                        }

                    // TODO Lerp float event
                    property.floatEvent?.Invoke(remapped);
                    break;
                //case PropertyType.FLOAT_ARRAY:
                //    Debug.LogWarning("PropertyType.FLOAT_ARRAY Not implemented yet");
                //    if (property.material) property.material.SetFloatArray(property.name, property.values);
                //    break;
                case PropertyType.POSITION:
                    property.reactingObject.transform.position = Vector3.MoveTowards(property.reactingObject.transform.position, remappedVector, step);
                    break;
                case PropertyType.LOCAL_POSITION:
                    property.reactingObject.transform.localPosition = Vector3.MoveTowards(property.reactingObject.transform.localPosition, remappedVector, step);
                    break;
                //case PropertyType.ROTATION:
                //    Debug.LogWarning("PropertyType.ROTATION Not implemented yet");
                //    break;
                //case PropertyType.LOCAL_ROTATION:
                //    Debug.LogWarning("PropertyType.LOCAL_ROTATION Not implemented yet");
                //    break;
                case PropertyType.SCALE:
                    property.reactingObject.transform.localScale = Vector3.MoveTowards(property.reactingObject.transform.localScale, remappedVector, step);
                    break;
                    //case PropertyType.COLOR:
                    //    Debug.LogWarning("PropertyType.COLOR Not implemented yet");
                    //    //if (property.material) property.material.SetColor(property.colorName, property.color);
                    //    break;
                    //case PropertyType.COLOR_ARRAY:
                    //    Debug.LogWarning("PropertyType.COLOR_ARRAY Not implemented yet");
                    //    //if (property.material) property.material.SetColorArray(property.name, property.colors);
                    //    break;
            }
        }
    }

    /// <summary>
    /// Only one property value can be updated at a time 
    /// Create more properties if more values need to be updated
    /// </summary>
    public enum PropertyType { FLOAT, /*FLOAT_ARRAY, */POSITION, LOCAL_POSITION, /*ROTATION, LOCAL_ROTATION,*/ SCALE, /*COLOR, COLOR_ARRAY*/ }

    /// <summary>
    /// The type of the analysis used to update the reacting property. 
    /// AMP uses the amplitude to update the value of the property
    /// FREQ uses frequency to update the value of the property
    /// TODO:
    /// another useful analysis type could be the FFT, to create array of floats representing the spectrum of the signal
    /// </summary>
    public enum AnalysisType { AMP, FREQ }

    /// <summary>
    /// Represents a property that will be animated based on Analysis values received
    /// </summary>
    [System.Serializable]
    public class ReactingProperty
    {
        [Header("ANALYSIS")]
        [Tooltip("The value that will be updated by this AudioTrack, depending on analysisType and type. This will be used to update this property")]
        public float value;
        [Tooltip("The type of the analysis used to update the reacting property.")]
        public AnalysisType analysisType;
        [Tooltip("The type of property to update. Only one property value can be updated at a time, create more properties if more values need to be updated.")]
        public PropertyType type;
        [Tooltip("If the value needs to be remapped between min and max values")]
        public bool remap;
        [Tooltip("If the resulting output has to be constrained in the desiredRange (for floats), or between min and max (for transforms). " +
            "If false, the resulting output can be out of desired values")]
        public bool clamp;
        [Tooltip("If the remapping calculus should be logged")]
        public bool log;
        [Tooltip("How fast the property reacts to input")]
        public float speed = 1f;
        [Space(2)]
        [Header("FLOAT")]
        [Tooltip("The min and max value the float value will be remapped to, when type is FLOAT")]
        public Vector2 desiredRange;
        [Tooltip("The event that will be called when property type is PropertyType.FLOAT")]
        public UnityEvent<float> floatEvent;
        //[Tooltip("NOT IMPLEMENTED: The values that will be updated by this AudioTrack, depending on analysisType and type (ie FFT)")]
        //public float[] values;
        [Space(2)]
        [Header("TRANSFORM")]
        [Tooltip("The min desired vector to which remap the value, when type is POSITION or SCALE")]
        public Vector3 min;
        [Tooltip("The max desired vector to which remap the value, when type is POSITION or SCALE")]
        public Vector3 max;
        [Tooltip("The gameObject that has to react, when type is POSITION or SCALE")]
        public GameObject reactingObject;
        [Space(2)]
        [Header("MATERIAL/EFFECT")]
        [Tooltip("The name of the material / effect exposed property to be updated")]
        public string name;
        [Tooltip("The material which will have a property named 'name' animated by the (un)mapped float value")]
        public Material material;
        [Tooltip("The visual effect which will have a property named 'name' animated by the (un)mapped float value")]
        public VisualEffect effect;
    }
}
