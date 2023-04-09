using Unity.Burst;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

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
    
    [BurstCompile]
    public struct NewQuaternionBurstJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> rotateXInDegArray;
        [ReadOnly] public NativeArray<float> rotateYInDegArray;
        [ReadOnly] public NativeArray<float> rotateZInDegArray;
        [WriteOnly] public NativeArray<quaternion> quaternionArray;

        public void Execute(int index)
        {
            var xDeg = rotateXInDegArray[index];
            var yDeg = rotateYInDegArray[index];
            var zDeg = rotateZInDegArray[index];
            quaternionArray[index] = quaternion.Euler(xDeg, yDeg, zDeg);
        }
    }
    
    [BurstCompile]
    public struct KeyFrameGeneratorJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> xDegNativeArray;
        [ReadOnly] public NativeArray<float> yDegNativeArray;
        [ReadOnly] public NativeArray<float> zDegNativeArray;
        [ReadOnly] public NativeArray<float> timeNativeArray;
        [WriteOnly] public NativeArray<Keyframe> qxKeyFrameNativeArray;
        [WriteOnly] public NativeArray<Keyframe> qyKeyFrameNativeArray;
        [WriteOnly] public NativeArray<Keyframe> qzKeyFrameNativeArray;
        [WriteOnly] public NativeArray<Keyframe> qwKeyFrameNativeArray;

        public void Execute(int index)
        {
            var xDeg = xDegNativeArray[index];
            var yDeg = yDegNativeArray[index];
            var zDeg = zDegNativeArray[index];
            var q = quaternion.Euler(xDeg, yDeg, zDeg);
            qxKeyFrameNativeArray[index] = new Keyframe(timeNativeArray[index], q.value.x);
            qyKeyFrameNativeArray[index] = new Keyframe(timeNativeArray[index], q.value.y);
            qzKeyFrameNativeArray[index] = new Keyframe(timeNativeArray[index], q.value.z);
            qwKeyFrameNativeArray[index] = new Keyframe(timeNativeArray[index], q.value.w);
        }
    }
}