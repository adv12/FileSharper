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

}
