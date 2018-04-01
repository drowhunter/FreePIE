using System;
using System.Collections.Generic;
using System.Diagnostics;
using FreePIE.Core.Contracts;
using FreePIE.Core.ScriptEngine.Globals.ScriptHelpers.Strategies;

namespace FreePIE.Core.ScriptEngine.Globals.ScriptHelpers
{
    [GlobalEnum]
    public enum Units
    {
        Degrees = 1,
        Radians = 2
    }

    [Global(Name = "filters")]
    public class FilterHelper : IScriptHelper
    {
        private readonly Dictionary<string, double> deltaLastSamples;
        private readonly Dictionary<string, double> simpleLastSamples;
        private readonly Dictionary<string, ContinuesRotationStrategy> continousRotationStrategies;
        private readonly Dictionary<string, Stopwatch> stopwatches; 

        public FilterHelper()
        {
            deltaLastSamples = new  Dictionary<string, double>();
            simpleLastSamples = new Dictionary<string, double>();
            continousRotationStrategies = new Dictionary<string, ContinuesRotationStrategy>();
            stopwatches = new Dictionary<string, Stopwatch>();
        }

        [NeedIndexer]
        public double simple(double x, double smoothing, string indexer)
        {
            if(smoothing < 0 || smoothing > 1)
                throw new ArgumentException("Smoothing must be a value between 0 and 1");

            var lastSample = x;
            if (simpleLastSamples.ContainsKey(indexer))
                lastSample = simpleLastSamples[indexer];

            lastSample = (lastSample*smoothing) + (x*(1 - smoothing));
            simpleLastSamples[indexer] = lastSample;

            return lastSample;
        }

        [NeedIndexer]
        public double delta(double x, string indexer)
        {
            var lastSample = x;
            if (deltaLastSamples.ContainsKey(indexer))
                lastSample = deltaLastSamples[indexer];

            deltaLastSamples[indexer] = x;

            return x - lastSample;
        }

        [Deprecated("continuousRotation")]
        [NeedIndexer]
        public double continousRotation(double x, string indexer)
        {
            return continuousRotation(x, indexer);
        }

        [Deprecated("continuousRotation")]
        [NeedIndexer]
        public double continousRotation(double x, Units unit, string indexer)
        {
            return continuousRotation(x, unit, indexer);
        }

        [NeedIndexer]
        public double continuousRotation(double x, string indexer)
        {
            return continuousRotation(x, Units.Radians, indexer);
        }

        [NeedIndexer]
        public double continuousRotation(double x, Units unit, string indexer)
        {
            if(!continousRotationStrategies.ContainsKey(indexer))
                continousRotationStrategies[indexer] = new ContinuesRotationStrategy(unit);

            var strategy = continousRotationStrategies[indexer];
            strategy.Update(x);

            return strategy.Out;
        }

        public double deadband(double x, double deadZone, double minY, double maxY)
        {
            var scaled = ensureMapRange(x, minY, maxY, -1, 1);
            var y = 0d;

            if (Math.Abs(scaled) > deadZone)
                y = ensureMapRange(Math.Abs(scaled), deadZone, 1, 0, 1) * Math.Sign(x);

            return ensureMapRange(y, -1, 1, minY, maxY);
        }

        public double deadband(double x, double deadZone)
        {
            if (Math.Abs(x) >= Math.Abs(deadZone))
                return x;

            return 0;
        }

        public double mapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return yMin + (yMax - yMin)*(x - xMin)/(xMax - xMin);
        }

        public double ensureMapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return Math.Max(Math.Min(((x - xMin)/(xMax - xMin))*(yMax - yMin) + yMin, yMax), yMin);
        }
        #region Target Funcions
        private struct sAxis
        {
            
            public char dir;
                                                     
            //char locked;
            //char relative;
            //int trim;
            int val;

            public CurveMode curveMode;
            /// <summary>
            /// S curve parameters
            /// </summary>
            public int lower,center,upper,curve;
            /// <summary>
            /// J curve parameter, zoom for Scurve
            /// </summary>
            public double ab;

        }

        public enum CurveMode
        {
            None, SCurve, JCurve, Custom
        }

        const int AMAX = 32767, AMAXF = 32768;
        double fcurve(double x, double lower, double center, double upper, double trim, int curve)
        {
            double m, M, cM, cm;
            m = lower + lower - 1;
            M = 1 - upper - upper;
            cM = center;
            cm = -cM;
            if (x < m)
                x = -1;
            else if (x < cm)
                if (curve == 0)
                    x = (x - cm) / (cm - m);
                else
                    x = (1 - Math.Exp((cm - x) * curve)) / (Math.Exp((cm - m) * curve) - 1);
            else if (x < cM)
                x = 0;
            else if (x < M)
                if (Math.Abs(curve) < 0.01)
                    x = (x - cM) / (M - cM);
                else
                    x = (Math.Exp((x - cM) * curve) - 1) / (Math.Exp((M - cM) * curve) - 1);
            else
                x = 1;
            x = x + trim;

            if (x < -1)
                x = -1;
            else if (x > 1)
                x = 1;
            return x;
        }
        double P2Curve(double x, double a, double b, double c)
        {
            return a * x * x + b * x + c;
        }

        /// <summary>
        /// linear interpolate
        /// </summary>
        double LI(double x, double y, double X, double Y, double v)
        {
            return ((Y - y) * v + X * y - x * Y) / (X - x);
        } // linear interpolate
        
        double GetCustomCurveValue(int p, double v)
        {
            return v;
            /*p = p + 1 & 0xffff;
            int n = keyalloc[p];    // list end
            int i = p + 1;
            if (i >= n)
                return v;
            while (i < n)
                if (keyalloc[i] < v)
                    i = i + 2;
                else if (i == p + 1)
                    return LI(0, 0, keyalloc[i], keyalloc[i + 1], v);
                else
                    return LI(keyalloc[i - 2], keyalloc[i - 1], keyalloc[i], keyalloc[i + 1], v);
            return LI(keyalloc[i - 2], keyalloc[i - 1], 100, 100, v);*/
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val">input value</param>
        /// <param name="inval">percents</param>
        /// <param name="outval">percents</param>
        /// <returns></returns>
        double SetJCurve(double val, double inval, double outval) // in, out = percents
        {
            var axdata = new sAxis();
            axdata.curveMode = CurveMode.JCurve;
            axdata.ab = 50 * (inval - outval) / (inval * (inval - 100));
            return AxisVal(val, axdata);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val">input value</param>
        /// <param name="lower">percent</param>
        /// <param name="center">percent</param>
        /// <param name="upper">percent</param>
        /// <param name="curve">-32..32</param>
        /// <param name="zoom">percent</param>
        /// <returns></returns>
        public double SetSCurve(double val,int lower = 0, int center = 0, int upper = 0, int curve = 0, double zoom = 0) 
        {
            var axdata = new sAxis
            {
                curveMode = CurveMode.SCurve,
                lower = lower,
                center = center,
                upper = upper,
                curve = curve,
                ab = zoom
            };
            return AxisVal(val, axdata);           
        }

        /*
        public struct Axis
        {
            public int coupling;
            public double cos, sin;

            public Axis(int yAxis, double v1, double v2) : this()
            {
            }
        }
        int RotateDXAxis(int XAxis, int YAxis, double angle) // clockwise angle, in degrees
        {
            angle = angle * Math.PI / 180;
            var xAxis = new Axis(YAxis, Math.Cos(angle), -Math.Sin(angle));

            Axis[XAxis].coupling = YAxis;
            Axis[XAxis].cos = Math.Cos(angle);
            Axis[XAxis].sin = -Math.Sin(angle);
            Axis[YAxis].coupling = XAxis;
            Axis[YAxis].cos = Math.Cos(angle);
            Axis[YAxis].sin = Math.Sin(angle);
        }*/
        private double AxisVal(double v, sAxis d)
        {
            switch (d.curveMode)
            {
                case CurveMode.SCurve:
                    v = AMAX * Math.Pow(1.41, d.ab) * fcurve(v / AMAXF, d.lower * 0.01, d.center * 0.01, d.upper * 0.01, 0, d.curve);
                    break;
                case CurveMode.JCurve:
                    v = AMAX * P2Curve(v / AMAXF, -d.ab, 1, d.ab);
                    break;
                case CurveMode.Custom://TODO
                    v = AMAX * GetCustomCurveValue((int)d.curveMode, v * 50 / AMAXF + 50) / 50 - AMAX; 
                    break;
               
            }

            return v * (1 + d.dir);
        }

        #endregion

        [NeedIndexer]
        public bool stopWatch(bool state, int milliseconds, string indexer)
        {
            if (!stopwatches.ContainsKey(indexer) && state)
            {
                stopwatches[indexer] = new Stopwatch();
                stopwatches[indexer].Start();
            }

            if (stopwatches.ContainsKey(indexer) && !state)
            {
                stopwatches[indexer].Stop();
                stopwatches.Remove(indexer);
            }

            if (!state) return false;

            var watch = stopwatches[indexer];
            if (watch.ElapsedMilliseconds >= milliseconds)
            {
                watch.Stop();
                stopwatches.Remove(indexer);
                return true;
            }

            return false;
        }
    }
}
