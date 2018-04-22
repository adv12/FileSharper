{
  "FileSourceExpanded": true,
  "ConditionExpanded": false,
  "FieldSourcesExpanded": false,
  "TestedProcessorsExpanded": false,
  "MatchedProcessorsExpanded": true,
  "FileSourceNode": {
    "FileSourceTypeName": "FileSharperCore.FileSources.DirectorySearchFileSource",
    "Description": "Searches through the specified directory for files matching the specified pattern.",
    "Parameters": {
      "Directory": "ENTER A DIRECTORY",
      "Recursive": true,
      "FilePattern": "*.txt",
      "SearchOrder": "SystemDefault",
      "IncludeHidden": false,
      "IncludeSystem": false
    }
  },
  "ConditionNode": {
    "ConditionTypeName": null,
    "Not": false,
    "Description": null,
    "Parameters": null,
    "ChildNodes": []
  },
  "FieldSourcesNode": {
    "FieldSourceNodes": [
      {
        "FieldSourceTypeName": null,
        "Description": null,
        "Parameters": null
      }
    ]
  },
  "TestedProcessorsNode": {
    "ProcessorNodes": [
      {
        "ProcessorTypeName": null,
        "Description": null,
        "InputFileSource": "OriginalFile",
        "Parameters": null,
        "ChildProcessorsNode": null
      }
    ]
  },
  "MatchedProcessorsNode": {
    "ProcessorNodes": [
      {
        "ProcessorTypeName": "FileSharperCore.Processors.Text.ReplaceTextProcessor",
        "Description": "Replace text, optionally using a regular expression",
        "InputFileSource": "OriginalFile",
        "Parameters": {
          "TextToMatch": "YOUR REGEX HERE",
          "ReplacementText": null,
          "MatchOnlyWithinSingleLine": true,
          "UseRegex": true,
          "RegexStartAndEndMatchPerLine": false,
          "RegexDotMatchesNewline": false,
          "CaseSensitive": false,
          "LineEndings": "SystemDefault",
          "OutputEncoding": "MatchInput",
          "FileName": "{DirectoryName}\\{NameWithoutExtension}{Extension}",
          "OverwriteExistingFile": true,
          "MoveOriginalToRecycleBin": true
        },
        "ChildProcessorsNode": null
      }
    ]
  },
  "LimitMatches": false,
  "MaxToMatch": 1000,
  "MaxResultsDisplayed": 200,
  "MaxExceptionsDisplayed": 20
}