using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace FastAnimationCurve
{
    public class Executor : MonoBehaviour
    {
        private void Start()
        {
            const int curveArraySize = 1000;
            const int numberOfKeysPerCurve = 100;
            
            // rotateX(Y,Z)CurvesInDegの生成処理にかかる時間を計測する
            AnimationCurve[] rotateXCurvesInDeg; 
            AnimationCurve[] rotateYCurvesInDeg; 
            AnimationCurve[] rotateZCurvesInDeg; 
            using (new TimeMeasurement("Generate AnimationCurves"))
            {
                rotateXCurvesInDeg = GenerateRadianAnimationCurves(curveArraySize, numberOfKeysPerCurve);
                rotateYCurvesInDeg = GenerateRadianAnimationCurves(curveArraySize, numberOfKeysPerCurve);
                rotateZCurvesInDeg = GenerateRadianAnimationCurves(curveArraySize, numberOfKeysPerCurve);
            }

            const int evaluateStep = 1000;
            // curveArraySize x evaluateStepのNativeArray
            float[,] rotateXInDegArrays = new float[curveArraySize, evaluateStep];
            float[,] rotateYInDegArrays = new float[curveArraySize, evaluateStep];
            float[,] rotateZInDegArrays = new float[curveArraySize, evaluateStep];
            
            // curveArraySize x evaluateStepの大きさのNativeArrayを生成する
            var rotateXInDegNativeArray = new NativeArray<float>(curveArraySize * evaluateStep, Allocator.TempJob);
            var rotateYInDegNativeArray = new NativeArray<float>(curveArraySize * evaluateStep, Allocator.TempJob);
            var rotateZInDegNativeArray = new NativeArray<float>(curveArraySize * evaluateStep, Allocator.TempJob);
            
            // 度数法で表現したカーブの値をevaluateStep分評価して、その値を配列に格納する
            using (new TimeMeasurement("Evaluate AnimationCurves"))
            {
                for (var i = 0; i < curveArraySize; ++i)
                {
                    for (var j = 0; j < evaluateStep; ++j)
                    {
                        // evaluateStep刻みで評価する
                        var time = (30f / (float)evaluateStep) * j;
                        var xDeg = rotateXCurvesInDeg[i].Evaluate(time);
                        var yDeg = rotateYCurvesInDeg[i].Evaluate(time);
                        var zDeg = rotateZCurvesInDeg[i].Evaluate(time);

                        // 評価して得られた値を2次元配列に格納する
                        rotateXInDegArrays[i, j] = xDeg;
                        rotateYInDegArrays[i, j] = yDeg;
                        rotateZInDegArrays[i, j] = zDeg;
                        
                        // 評価して得られた値をNativeArrayに格納する
                        // indexを計算する
                        var index = i * evaluateStep + j;
                        rotateXInDegNativeArray[index] = xDeg;
                        rotateYInDegNativeArray[index] = yDeg;
                        rotateZInDegNativeArray[index] = zDeg;
                    }
                }
            }
            
            // rotateX(Y,Z)InDegArraysの各要素をQuaternion.Eulerに渡して、Quaternionを生成する
            using (new TimeMeasurement("Quaternion.Euler Simple"))
            {
                for (var i = 0; i < curveArraySize; ++i)
                {
                    for (var j = 0; j < evaluateStep; ++j)
                    {
                        var rotateXInDeg = rotateXInDegArrays[i, j];
                        var rotateYInDeg = rotateYInDegArrays[i, j];
                        var rotateZInDeg = rotateZInDegArrays[i, j];
                        var rotation = Quaternion.Euler(rotateXInDeg, rotateYInDeg, rotateZInDeg);
                    }
                }
            }
            
            // QuaternionJobを使って、rotateX(Y,Z)InDegNativeArrayの各要素をQuaternion.Eulerに渡して、Quaternionを生成する
            using (new TimeMeasurement("Quaternion.Euler Job"))
            {
                var quaternionNativeArray =
                    new NativeArray<Quaternion>(curveArraySize * evaluateStep, Allocator.TempJob);
                var quaternionJob = new QuaternionJob()
                {
                    rotateXInDegArray = rotateXInDegNativeArray,
                    rotateYInDegArray = rotateYInDegNativeArray,
                    rotateZInDegArray = rotateZInDegNativeArray,
                    quaternionArray = quaternionNativeArray
                };
                quaternionJob.Schedule(
                        curveArraySize * evaluateStep,
                        100)
                    .Complete();

                // NativeArrayを破棄する
                quaternionNativeArray.Dispose();
            }
            
            // QuaternionBurstJobを使って、rotateX(Y,Z)InDegNativeArrayの各要素をQuaternion.Eulerに渡して、Quaternionを生成する
            using (new TimeMeasurement("Quaternion.Euler BurstJob"))
            {
                var quaternionNativeArray =
                    new NativeArray<Quaternion>(curveArraySize * evaluateStep, Allocator.TempJob);
                var quaternionJob = new QuaternionBurstJob()
                {
                    rotateXInDegArray = rotateXInDegNativeArray,
                    rotateYInDegArray = rotateYInDegNativeArray,
                    rotateZInDegArray = rotateZInDegNativeArray,
                    quaternionArray = quaternionNativeArray
                };
                quaternionJob.Schedule(
                        curveArraySize * evaluateStep,
                        100)
                    .Complete();

                // NativeArrayを破棄する
                quaternionNativeArray.Dispose();
            }
            
            // NativeArrayを破棄する
            rotateXInDegNativeArray.Dispose();
            rotateYInDegNativeArray.Dispose();
            rotateZInDegNativeArray.Dispose();
        }

        private static AnimationCurve[] GenerateRadianAnimationCurves(int arraySize, int numberOfKeysPerCurve)
        {
            // 大量のカーブ配列を生成する
            var curves = new AnimationCurve[arraySize];
            for (var i = 0; i < curves.Length; ++i)
            {
                // 巨大なRandomカーブを生成する
                var curve = AnimationCurveGenerator.RandomCurve(
                    new CurveGenerationInfo()
                    {
                        duration = 30,
                        numberOfKeys = numberOfKeysPerCurve,
                        maxValue = -180,
                        minValue = 180
                    }
                );
                curves[i] = curve;
            }

            return curves;
        }
    }
}