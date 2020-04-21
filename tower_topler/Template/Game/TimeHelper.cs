using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Template
{
    /// <summary>
    /// Time helper measure current time, time increment from previous frame and FPS.
    /// </summary>
    /// <remarks>Call Update at begin of each frame.</remarks>
    public class TimeHelper
    {
        /// <summary>Timer.</summary>
        private Stopwatch _stopWatch;

        /// <summary>Frames counter for calculation of FPS.</summary>
        private int _counter = 0;

        /// <summary>FPS during last second.</summary>
        private int _fps = 0;
        /// <summary>FPS during last second.</summary>
        /// <value>FPS during last second.</value>
        public int FPS { get => _fps; }

        /// <summary>Time of last FPS update moment.</summary>
        private long _previousFPSMeasurementTime;

        /// <summary>Tics countVertices in previous frame.</summary>
        private long _previousTicks;

        /// <summary>Current time in seconds.</summary>
        private float _time;
        /// <summary>Current time in seconds.</summary>
        /// <value>Current time in seconds.</value>
        public float Time { get => _time; }

        /// <summary>Time, elapsed from previous frame.</summary>
        private float _deltaT;
        /// <summary>Time, elapsed from previous frame.</summary>
        /// <value>Time, elapsed from previous frame.</value>
        public float DeltaT { get => _deltaT; }

        /// <summary>Create and initialize timer.</summary>
        public TimeHelper()
        {
            _stopWatch = new Stopwatch();
            Reset();
        }

        /// <summary>Update all values.</summary>
        /// <remarks>Call this method at begin of each frame.</remarks>
        public void Update()
        {
            // Current tics counter value.
            long ticks = _stopWatch.Elapsed.Ticks;
            // Time calculation.
            _time = (float)ticks / TimeSpan.TicksPerSecond;
            _deltaT = (float)(ticks - _previousTicks) / TimeSpan.TicksPerSecond;
            // Update of previous tics counter value.
            _previousTicks = ticks;

            // FPS counter increment.
            _counter++;
            // If 1 second elapsed, then renew FPS.
            if (_stopWatch.ElapsedMilliseconds - _previousFPSMeasurementTime >= 1000)
            {
                _fps = _counter;
                _counter = 0;
                _previousFPSMeasurementTime = _stopWatch.ElapsedMilliseconds;
            }
        }

        /// <summary>Reset and startVertex timer.</summary>
        public void Reset()
        {
            _stopWatch.Reset();
            _counter = 0;
            _fps = 0;
            _stopWatch.Start();
            _previousFPSMeasurementTime = _stopWatch.ElapsedMilliseconds;
            _previousTicks = _stopWatch.Elapsed.Ticks;
        }
    }
}
