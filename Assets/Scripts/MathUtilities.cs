using UnityEngine;

public static class MathUtilities
{
    /// <summary>
    /// Remaps a float value from one range to another
    /// </summary>
    /// <param name="value">the value to be remapped</param>
    /// <param name="low1">the lowest value of the first range</param>
    /// <param name="high1">the highest value of the first range</param>
    /// <param name="low2">the lowest value of the second range</param>
    /// <param name="high2">the highest value of the second range</param>
    /// <returns></returns>
    public static float Remap(float value, float low1, float high1, float low2, float high2)
    {
        return low2 + (high2 - low2) * ((value - low1) / (high1 - low1));
    }

    /// <summary>
    /// Linearly remaps a Vector3 from min to max, based on the float value and its expected range. 
    /// The returned value can be clamped between min and max
    /// </summary>
    /// <param name="value"></param>
    /// <param name="expectedRange"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="clampValue"></param>
    /// <param name="logValues"></param>
    /// <returns></returns>
    public static Vector3 RemapVector(float value, Vector2 expectedRange, Vector3 min, Vector3 max, bool clampValue = false)
    {
        // find percentage
        var mappedValue = Remap(value, expectedRange.x, expectedRange.y, 0f, 1f);
        // clamp value if needed
        var clamped = clampValue ? Mathf.Clamp01(mappedValue) : mappedValue;
        // interpolate between the two points
        return Vector3.Lerp(min, max, clamped);
    }

    /// <summary>
    /// Linearly remaps a Color from min to max, based on the float value and its expected range.
    /// The returned value can be clamped between min and max, and values can be logged to console
    /// </summary>
    /// <param name="value"></param>
    /// <param name="expectedRange"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="clampValue"></param>
    /// <param name="logValues"></param>
    /// <returns></returns>
    public static Color RemapColor(float value, Vector2 expectedRange, Color min, Color max, bool clampValue = false)
    { // find percentage
        var mappedValue = Remap(value, expectedRange.x, expectedRange.y, 0f, 1f);
        // clamp value if needed
        var clamped = clampValue ? Mathf.Clamp01(mappedValue) : mappedValue;
        return Color.Lerp(min, max, clamped);
    }
}
