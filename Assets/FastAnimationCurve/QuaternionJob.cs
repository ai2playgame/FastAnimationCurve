using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace FastAnimationCurve
{
    // 3つのNativeArray<float>をQuaternion.Eulerを使ってQuaternionに変換するJob
    public struct QuaternionJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> rotateXInDegArray;
        [ReadOnly] public NativeArray<float> rotateYInDegArray;
        [ReadOnly] public NativeArray<float> rotateZInDegArray;
        [WriteOnly] public NativeArray<Quaternion> quaternionArray;

        public void Execute(int index)
        {
            var xDeg = rotateXInDegArray[index];
            var yDeg = rotateYInDegArray[index];
            var zDeg = rotateZInDegArray[index];
            quaternionArray[index] = Quaternion.Euler(xDeg, yDeg, zDeg);
        }
    }
    
    [BurstCompile]
    public struct QuaternionBurstJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> rotateXInDegArray;
        [ReadOnly] public NativeArray<float> rotateYInDegArray;
        [ReadOnly] public NativeArray<float> rotateZInDegArray;
        [WriteOnly] public NativeArray<Quaternion> quaternionArray;

        public void Execute(int index)
        {
            var xDeg = rotateXInDegArray[index];
            var yDeg = rotateYInDegArray[index];
            var zDeg = rotateZInDegArray[index];
            quaternionArray[index] = Quaternion.Euler(xDeg, yDeg, zDeg);
        }
    }
}