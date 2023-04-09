using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace FastAnimationCurve
{
    public class Executor02 : MonoBehaviour
    {
        private const float Duration = 30f;
        
        private void Start()
        {
            const int curveArraySize = 1000; // 元となるAnimationCurveの総数
            const int numberOfKeysPerCurve = 100; // 元となるAnimationCurveのキー数
            const int evaluationStep = 1000; // 1つのAnimationCurveを評価するステップ数

            // -180 ~ 180の値域をとるカーブをcurveArraySize分生成する
            var originalXDegCurves= GenerateRadianAnimationCurves(curveArraySize, numberOfKeysPerCurve);
            var originalYDegCurves= GenerateRadianAnimationCurves(curveArraySize, numberOfKeysPerCurve);
            var originalZDegCurves= GenerateRadianAnimationCurves(curveArraySize, numberOfKeysPerCurve);
            
            // curveArraySize x evaluationStepのNativeArray
            var xDegNativeArray = new NativeArray<float>(curveArraySize * evaluationStep, Allocator.TempJob);
            var yDegNativeArray = new NativeArray<float>(curveArraySize * evaluationStep, Allocator.TempJob);
            var zDegNativeArray = new NativeArray<float>(curveArraySize * evaluationStep, Allocator.TempJob);
            // 各キーの値が、何秒にあたるかを格納するNativeArray
            var timeNativeArray = new NativeArray<float>(curveArraySize * evaluationStep, Allocator.TempJob);
            
            // original(X,Y,Z)DegCurvesの各要素をevaluateStep回評価する
            using (new TimeMeasurement("Evaluate AnimationCurves"))
            {
                for (var i = 0; i < curveArraySize; ++i)
                {
                    for (var j = 0; j < evaluationStep; ++j)
                    {
                        // evaluateStep刻みで評価する
                        var time = (Duration / evaluationStep) * j;
                        var xDeg = originalXDegCurves[i].Evaluate(time);
                        var yDeg = originalYDegCurves[i].Evaluate(time);
                        var zDeg = originalZDegCurves[i].Evaluate(time);
                        
                        // 評価して得られた値をNativeArrayに格納する
                        var index = i * evaluationStep + j;
                        xDegNativeArray[index] = xDeg;
                        yDegNativeArray[index] = yDeg;
                        zDegNativeArray[index] = zDeg;
                        timeNativeArray[index] = time;
                    }
                }
            }
            
            // KeyFrameGeneratorJobの実行結果を格納するNativeArrayを生成する
            var qxKeyFrameNativeArray = new NativeArray<Keyframe>(curveArraySize * evaluationStep, Allocator.TempJob);
            var qyKeyFrameNativeArray = new NativeArray<Keyframe>(curveArraySize * evaluationStep, Allocator.TempJob);
            var qzKeyFrameNativeArray = new NativeArray<Keyframe>(curveArraySize * evaluationStep, Allocator.TempJob);
            var qwKeyFrameNativeArray = new NativeArray<Keyframe>(curveArraySize * evaluationStep, Allocator.TempJob);
            
            // KeyFrameGeneratorJobを実行する
            using (new TimeMeasurement("Generate KeyFrames"))
            {
                var job = new KeyFrameGeneratorJob()
                {
                    xDegNativeArray = xDegNativeArray,
                    yDegNativeArray = yDegNativeArray,
                    zDegNativeArray = zDegNativeArray,
                    timeNativeArray = timeNativeArray,
                    qxKeyFrameNativeArray = qxKeyFrameNativeArray,
                    qyKeyFrameNativeArray = qyKeyFrameNativeArray,
                    qzKeyFrameNativeArray = qzKeyFrameNativeArray,
                    qwKeyFrameNativeArray = qwKeyFrameNativeArray
                };
                job.Schedule(curveArraySize * evaluationStep, 32)
                    .Complete();
            }

            // 最終的には、curveArraySize x (x, y, z, w)の4つ分のAnimationCurveを生成する
            // qxKeyFrameArrayを先頭からevaluationStep分だけ取り出して、AnimationCurveに変換する
            var qxAnimationCurves = new AnimationCurve[curveArraySize];
            var qyAnimationCurves = new AnimationCurve[curveArraySize];
            var qzAnimationCurves = new AnimationCurve[curveArraySize];
            var qwAnimationCurves = new AnimationCurve[curveArraySize];
            using (new TimeMeasurement("Create AnimationCurves"))
            {
                for (var i = 0; i < curveArraySize; ++i)
                {
                    var start = i * evaluationStep;
                    
                    qxAnimationCurves[i] = new AnimationCurve(qxKeyFrameNativeArray.GetSubArray(start, evaluationStep).ToArray());
                    qyAnimationCurves[i] = new AnimationCurve(qyKeyFrameNativeArray.GetSubArray(start, evaluationStep).ToArray());
                    qzAnimationCurves[i] = new AnimationCurve(qzKeyFrameNativeArray.GetSubArray(start, evaluationStep).ToArray());
                    qwAnimationCurves[i] = new AnimationCurve(qwKeyFrameNativeArray.GetSubArray(start, evaluationStep).ToArray());
                }
            }

            // すべてのNativeArrayを破棄する
            xDegNativeArray.Dispose();
            yDegNativeArray.Dispose();
            zDegNativeArray.Dispose();
            timeNativeArray.Dispose();
            qxKeyFrameNativeArray.Dispose();
            qyKeyFrameNativeArray.Dispose();
            qzKeyFrameNativeArray.Dispose();
            qwKeyFrameNativeArray.Dispose();
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
                        duration = Duration,
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