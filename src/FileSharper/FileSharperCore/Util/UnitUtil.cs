// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

namespace FileSharperCore.Util
{
    public class UnitUtil
    {
        public const double BYTES_PER_KILOBYTE_BINARY = 1024;
        public const double BYTES_PER_MEGABYTE_BINARY = 1024 * 1024;
        public const double BYTES_PER_GIGABYTE_BINARY = 1024 * 1024 * 1024;

        public const double BYTES_PER_KILOBYTE_METRIC = 1000;
        public const double BYTES_PER_MEGABYTE_METRIC = 1000 * 1000;
        public const double BYTES_PER_GIGABYTE_METRIC = 1000 * 1000 * 1000;

        public static double GetConversionFactor (SizeUnits fromUnits, SizeUnits toUnits, bool metric)
        {
            double toBytes = 1;
            double fromBytes = 1;
            switch (fromUnits)
            {
                case SizeUnits.Kilobytes:
                    toBytes = metric ? BYTES_PER_KILOBYTE_METRIC : BYTES_PER_KILOBYTE_BINARY;
                    break;
                case SizeUnits.Megabytes:
                    toBytes = metric ? BYTES_PER_MEGABYTE_METRIC : BYTES_PER_MEGABYTE_BINARY;
                    break;
                case SizeUnits.Gigabytes:
                    toBytes = metric ? BYTES_PER_GIGABYTE_METRIC : BYTES_PER_GIGABYTE_BINARY;
                    break;
            }
            switch (toUnits)
            {
                case SizeUnits.Kilobytes:
                    fromBytes = metric ? 1 / BYTES_PER_KILOBYTE_METRIC : 1 / BYTES_PER_KILOBYTE_BINARY;
                    break;
                case SizeUnits.Megabytes:
                    fromBytes = metric ? 1 / BYTES_PER_MEGABYTE_METRIC : 1 / BYTES_PER_MEGABYTE_BINARY;
                    break;
                case SizeUnits.Gigabytes:
                    fromBytes = metric ? 1 / BYTES_PER_GIGABYTE_METRIC : 1 / BYTES_PER_GIGABYTE_BINARY;
                    break;
            }
            return toBytes * fromBytes;
        }

        public static double ConvertSize(double value, SizeUnits fromUnits, SizeUnits toUnits, bool metric = false)
        {
            return value * GetConversionFactor(fromUnits, toUnits, metric);
        }

        public static string GetUnitSymbol(SizeUnits units)
        {
            switch (units)
            {
                case SizeUnits.Kilobytes:
                    return "kB";
                case SizeUnits.Megabytes:
                    return "MB";
                case SizeUnits.Gigabytes:
                    return "GB";
            }
            return "B";
        }
    }
}
