using System;
using System.Diagnostics;

namespace FastAnimationCurve
{
    // usingで囲んだ処理にかかった時間を計測するクラス
    public class TimeMeasurement : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly string _message;

        public TimeMeasurement(string message)
        {
            this._message = message;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            UnityEngine.Debug.Log($"{_message}: {_stopwatch.ElapsedMilliseconds}ms");
        }
    }
}