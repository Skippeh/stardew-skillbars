using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillProgress
{
    public class FlooredCurve<TKey, TValue> where TKey : IComparable
    {
        private readonly List<Tuple<TKey, TValue>> values = new List<Tuple<TKey, TValue>>();

        public FlooredCurve(List<Tuple<TKey, TValue>> values)
        {
            if (values != null)
                this.values = new List<Tuple<TKey, TValue>>(values);
        }

        public FlooredCurve() : this(null)
        {
        }

        public TKey MaxPosition => values[values.Count - 1].Item1;

        public void AddPoint(TKey position, TValue value)
        {
            if (values.Any(tuple => tuple.Item1.CompareTo(position) == 0))
                throw new ArgumentException("There is already a point at this position.");
            
            values.Add(new Tuple<TKey, TValue>(position, value));
            values.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        }

        public TValue GetValue(TKey position)
        {
            for (int i = values.Count - 1; i >= 0; --i)
            {
                if (values[i].Item1.CompareTo(position) <= 0)
                    return values[i].Item2;
            }

            return values[0].Item2;
        }

        public float GetPercentageToNextPoint(TKey position)
        {
            var point1 = GetPreviousPoint(position);
            var point2 = GetNextPoint(position);

            if (point1.CompareTo(point2) == 0)
            {
                if (point1.CompareTo(values[values.Count - 1].Item1) == 0)
                    return 1f;

                return 0f;
            }

            double d1 = Convert.ToDouble(point1);
            double d2 = Convert.ToDouble(point2);
            double dPosition = Convert.ToDouble(position);
            
            double based2 = d2 - d1;
            double basedT = dPosition - d1;

            return (float) (basedT / based2);
        }

        public TKey GetPreviousPoint(TKey position)
        {
            for (int i = values.Count - 1; i >= 0; --i)
            {
                if (values[i].Item1.CompareTo(position) <= 0)
                {
                    return values[i].Item1;
                }
            }

            return values[0].Item1;
        }

        public TKey GetNextPoint(TKey position)
        {
            for (int i = 0; i < values.Count; ++i)
            {
                if (values[i].Item1.CompareTo(position) >= 0)
                {
                    if (i < values.Count - 1 && position.CompareTo(values[i].Item1) == 0)
                        return values[i + 1].Item1;

                    return values[i].Item1;
                }
            }

            return values[values.Count - 1].Item1;
        }
    }
}