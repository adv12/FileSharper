{
  "FileSourceExpanded": true,
  "ConditionExpanded": false,
  "FieldSourcesExpanded": false,
  "TestedProcessorsExpanded": false,
  "MatchedProcessorsExpanded": false,
  "FileSourceNode": {
    "FileSourceTypeName": "FileSharperCore.FileSources.DirectorySearchFileSource",
    "Description": "Searches through the specified directory for files matching the specified pattern.",
    "Parameters": {
      "Directory": "C:\\",
      "Recursive": true,
      "FilePattern": "*",
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
        "ProcessorTypeName": null,
        "Description": null,
        "InputFileSource": "OriginalFile",
        "Parameters": null,
        "ChildProcessorsNode": null
      }
    ]
  },
  "LimitMatches": false,
  "MaxToMatch": 1000,
  "MaxResultsDisplayed": 200,
  "MaxExceptionsDisplayed": 20
}