using UnityEngine;

namespace FastAnimationCurve
{
    [System.Serializable]
    public struct CurveGenerationInfo
    {
        public float duration;
        public int numberOfKeys;
        public float minValue;
        public float maxValue;
    }
    
    public static class AnimationCurveGenerator
    {
        public static AnimationCurve GenerateRandomCurve(CurveGenerationInfo info)
        {
            Keyframe[] keys = new Keyframe[info.numberOfKeys];

            for (int i = 0; i < info.numberOfKeys; i++)
            {
                float time = i / (float)(info.numberOfKeys - 1) * info.duration;
                float value = Random.Range(info.minValue, info.maxValue);
                keys[i] = new Keyframe(time, value);
            }

            return new AnimationCurve(keys);
        }
    }
}