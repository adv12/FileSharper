// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;

namespace FileSharperCore.Util
{
    public class PositiveFraction : IEquatable<PositiveFraction>
    {
        public static bool operator ==(PositiveFraction a, PositiveFraction b)
        {
            return a.Numerator == b.Numerator && a.Denominator == b.Denominator;
        }

        public static bool operator !=(PositiveFraction a, PositiveFraction b)
        {
            return !(a == b);
        }

        public static bool operator <(PositiveFraction a, PositiveFraction b)
        {
            return a != b && a.DoubleValue < b.DoubleValue;
        }

        public static bool operator <=(PositiveFraction a, PositiveFraction b)
        {
            return a != b && a.DoubleValue <= b.DoubleValue;
        }

        public static bool operator >(PositiveFraction a, PositiveFraction b)
        {
            return a != b && a.DoubleValue > b.DoubleValue;
        }

        public static bool operator >=(PositiveFraction a, PositiveFraction b)
        {
            return a != b && a.DoubleValue >= b.DoubleValue;
        }

        public static uint GCD(uint a, uint b)
        {
            uint t;
            while (b != 0)
            {
                t = b;
                b = a % b;
                a = t;
            }
            return a;
        }

        public uint Numerator { get; private set; }

        public uint Denominator { get; private set; }

        public double DoubleValue
        {
            get
            {
                return (double)Numerator / Denominator;
            }
        }

        public PositiveFraction(uint numerator, uint denominator)
        {
            uint gcd = GCD(numerator, denominator);
            Numerator = numerator / gcd;
            Denominator = denominator / gcd;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PositiveFraction);
        }

        public bool Equals(PositiveFraction other)
        {
            return other != null &&
                   Numerator == other.Numerator &&
                   Denominator == other.Denominator;
        }

        public override int GetHashCode()
        {
            var hashCode = -1534900553;
            hashCode = hashCode * -1521134295 + Numerator.GetHashCode();
            hashCode = hashCode * -1521134295 + Denominator.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return Numerator + ":" + Denominator;
        }
    }
}
