// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

namespace FileSharperCore.Util
{
    public class CompareUtil
    {
        public static MatchResultType Compare(double value1, ComparisonType comparisonType, double value2)
        {
            switch (comparisonType)
            {
                case ComparisonType.LessThan:
                    return value1 < value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.LessThanOrEqualTo:
                    return value1 <= value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.EqualTo:
                    return value1 == value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.GreaterThanOrEqualTo:
                    return value1 >= value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.GreaterThan:
                    return value1 > value2 ? MatchResultType.Yes : MatchResultType.No;
            }
            return MatchResultType.Yes;
        }

        public static MatchResultType Compare(int value1, ComparisonType comparisonType, int value2)
        {
            switch (comparisonType)
            {
                case ComparisonType.LessThan:
                    return value1 < value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.LessThanOrEqualTo:
                    return value1 <= value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.EqualTo:
                    return value1 == value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.GreaterThanOrEqualTo:
                    return value1 >= value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.GreaterThan:
                    return value1 > value2 ? MatchResultType.Yes : MatchResultType.No;
            }
            return MatchResultType.Yes;
        }

        public static MatchResultType Compare(PositiveFraction value1, ComparisonType comparisonType, PositiveFraction value2)
        {
            switch (comparisonType)
            {
                case ComparisonType.LessThan:
                    return value1 < value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.LessThanOrEqualTo:
                    return value1 <= value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.EqualTo:
                    return value1 == value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.GreaterThanOrEqualTo:
                    return value1 >= value2 ? MatchResultType.Yes : MatchResultType.No;
                case ComparisonType.GreaterThan:
                    return value1 > value2 ? MatchResultType.Yes : MatchResultType.No;
            }
            return MatchResultType.Yes;
        }

        public static MatchResultType Compare(double value1, SimpleComparisonType comparisonType, double value2)
        {
            switch (comparisonType)
            {
                case SimpleComparisonType.LessThan:
                    return value1 < value2 ? MatchResultType.Yes : MatchResultType.No;
                case SimpleComparisonType.GreaterThan:
                    return value1 > value2 ? MatchResultType.Yes : MatchResultType.No;
            }
            return MatchResultType.Yes;
        }

    }
}
