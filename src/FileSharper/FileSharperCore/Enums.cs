// Copyright (c) 2017 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

namespace FileSharperCore
{
    public enum MatchResultType
    {
        Yes,
        No,
        NotApplicable
    }

    public enum ProcessingResultType
    {
        Success,
        Failure,
        NotApplicable
    }

    public enum FileDateType
    {
        Created,
        Modified,
        Accessed
    }

    public enum TimeComparisonType
    {
        Before,
        After
    }

    public enum ComparisonType
    {
        LessThan,
        LessThanOrEqualTo,
        EqualTo,
        GreaterThanOrEqualTo,
        GreaterThan
    }

    public enum LineEndings
    {
        SystemDefault,
        Windows,
        Unix,
        OldMacOS
    }

    public enum DetectedLineEndings
    {
        NotApplicable,
        Windows,
        Unix,
        OldMacOS,
        Mixed
    }

    public enum SizeUnits
    {
        Bytes,
        Kilobytes,
        Megabytes,
        Gigabytes
    }

    public enum PrependAppend
    {
        Prepend,
        Append
    }

    public enum SearchOrder
    {
        SystemDefault,
        Alphabetical,
        ReverseAlphabetical,
        ModifiedDate,
        ReverseModifiedDate,
        Random
    }

    public enum ProcessorScope
    {
        Search,
        InputFile,
        GeneratedOutputFile
    }

    public enum MediaDimension
    {
        Width,
        Height
    }

    public enum MediaDimensionOrDimensions
    {
        WidthHeight,
        HeightWidth,
        Width,
        Height
    }

    public enum HowOften
    {
        Never,
        Sometimes,
        Always
    }

    public enum InputFileSource
    {
        OriginalFile,
        PreviousOutput,
        ParentInput
    }

    public enum ProcessInput
    {
        OriginalFile,
        GeneratedFiles
    }

    public enum ImageSaveFormat
    {
        Jpeg,
        Png,
        Gif,
        Tiff,
        Exif,
        Bitmap
    }

    public enum TrimType
    {
        Start,
        End,
        Both
    }

    public enum TextCase
    {
        Uppercase,
        Lowercase
    }

    public enum StringComparisonType
    {
        CaseInsensitive,
        CaseSensitive,
        Ordinal
    }

    public enum PathFormat
    {
        FullPath,
        NameThenDirectory,
        DirectoryThenName
    }

    public enum OutputEncodingType
    {
        MatchInput = 0,
        ASCII = 20127, // US-ASCII
        UTF8 = 65001, // Unicode (UTF-8)
        UTF7 = 65000, // Unicode (UTF-7)
        UTF16_LE = 1200, // Unicode (Little endian)
        UTF16_BE = 1201, // Unicode (Big endian)
        UTF32_LE = 12000, // Unicode (UTF-32)
        UTF32_BE = 12001, // Unicode (UTF-32 Big endian)
        EUCCN = 51936, // Chinese Simplified (EUC)
        EUCJP = 51932, // Japanese (EUC),
        EUCKR = 51949, // Korean (EUC),
        GB18030 = 54936, // Chinese Simplified (GB18030)
        GB2312 = 936, // Chinese Simplified (GB2312)
        HZ_GB_2312 = 52936, // Chinese Simplified (HZ)
        ISO2022_CN = 50227, // Chinese Simplified (ISO-2022)
        ISO2022_JP = 50220, // Japanese (JIS)
        ISO2022_JP_ALLOW_1_BYTE_KANA = 50221, // Japanese (JIS-Allow 1 byte Kana)
        ISO2022_JP_ALLOW_1_BYTE_KANA_SO_SI = 50222, // Japanese (JIS-Allow 1 byte Kana - SO/SI)
        ISO2022_KR = 50225, // Korean (ISO)
        ISO8859_1 = 28591, // Western European (ISO)
        ISO8859_8 = 28598, // Hebrew (ISO-Visual)
        ISO8859_8_I = 38598, // Hebrew (ISO-Logical)
        WIN1252 = 1252, // Western European (Windows)
        X_CP20936 = 20936, // Chinese Simplified (GB2312_80)
        X_CP20949 = 20949, // Korean Wansung
        X_ISCII_AS = 57006, // ISCII Assamese
        X_ISCII_BE = 57003, // ISCII Bengali
        X_ISCII_DE = 57002, // ISCII Devanagari
        X_ISCII_GU = 57010, // ISCII Gujarati
        X_ISCII_KA = 57008, // ISCII Kannada
        X_ISCII_MA = 57009, // ISCII Malayalam
        X_ISCII_OR = 57007, // ISCII Oriya
        X_ISCII_PA = 57011, // ISCII Punjabi
        X_ISCII_TA = 57004, // ISCII Tamil
        X_ISCII_TE = 57005, // ISCII Telugu
        X_MAC_CHINESESIMP = 10008, // Chinese Simplified (Mac)
        X_MAC_KOREAN = 10003 // Korean (Mac)
    }

    public enum DetectedEncodingType
    {
        None = 0,
        ASCII = 20127, // US-ASCII
        UTF8 = 65001, // Unicode (UTF-8)
        UTF7 = 65000, // Unicode (UTF-7)
        UTF16_LE = 1200, // Unicode (Little endian)
        UTF16_BE = 1201, // Unicode (Big endian)
        UTF32_LE = 12000, // Unicode (UTF-32)
        UTF32_BE = 12001, // Unicode (UTF-32 Big endian)
        EUCCN = 51936, // Chinese Simplified (EUC)
        EUCJP = 51932, // Japanese (EUC),
        EUCKR = 51949, // Korean (EUC),
        GB18030 = 54936, // Chinese Simplified (GB18030)
        GB2312 = 936, // Chinese Simplified (GB2312)
        HZ_GB_2312 = 52936, // Chinese Simplified (HZ)
        ISO2022_CN = 50227, // Chinese Simplified (ISO-2022)
        ISO2022_JP = 50220, // Japanese (JIS)
        ISO2022_JP_ALLOW_1_BYTE_KANA = 50221, // Japanese (JIS-Allow 1 byte Kana)
        ISO2022_JP_ALLOW_1_BYTE_KANA_SO_SI = 50222, // Japanese (JIS-Allow 1 byte Kana - SO/SI)
        ISO2022_KR = 50225, // Korean (ISO)
        ISO8859_1 = 28591, // Western European (ISO)
        ISO8859_8 = 28598, // Hebrew (ISO-Visual)
        ISO8859_8_I = 38598, // Hebrew (ISO-Logical)
        WIN1252 = 1252, // Western European (Windows)
        X_CP20936 = 20936, // Chinese Simplified (GB2312_80)
        X_CP20949 = 20949, // Korean Wansung
        X_ISCII_AS = 57006, // ISCII Assamese
        X_ISCII_BE = 57003, // ISCII Bengali
        X_ISCII_DE = 57002, // ISCII Devanagari
        X_ISCII_GU = 57010, // ISCII Gujarati
        X_ISCII_KA = 57008, // ISCII Kannada
        X_ISCII_MA = 57009, // ISCII Malayalam
        X_ISCII_OR = 57007, // ISCII Oriya
        X_ISCII_PA = 57011, // ISCII Punjabi
        X_ISCII_TA = 57004, // ISCII Tamil
        X_ISCII_TE = 57005, // ISCII Telugu
        X_MAC_CHINESESIMP = 10008, // Chinese Simplified (Mac)
        X_MAC_KOREAN = 10003 // Korean (Mac)
    }

}
