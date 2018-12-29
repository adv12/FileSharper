// Copyright (c) 2018 Andrew Vardeman.  Published under the MIT license.
// See license.txt in the FileSharper distribution or repository for the
// full text of the license.

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace FileSharperCore.Util
{
    public class UpgradeUtil
    {
        private static Dictionary<string, string> s_UpgradeTypeMap = new Dictionary<string, string>
        {
            { "FileSharperCore.Conditions.FilePathContainsTextCondition",
                "FileSharperCore.Conditions.Filesystem.FilePathContainsTextCondition" },
            { "FileSharperCore.Processors.CopyFileProcessor",
                "FileSharperCore.Processors.Filesystem.CopyFileProcessor"}
        };

        public static void Upgrade(JObject obj)
        {
            UpgradeFileSourceNode(obj["FileSourceNode"] as JObject);

            JObject conditionNode = obj["ConditionNode"] as JObject;
            if (conditionNode != null)
            {
                UpgradeConditionNode(conditionNode);
            }

            // FieldSources used to be called Outputs
            JProperty outputsNode = obj.Property("OutputsNode");
            if (outputsNode != null)
            {
                ConvertOutputsNode((JObject)outputsNode.Value);
                JProperty newProp = new JProperty("FieldSourcesNode", outputsNode.Value);
                outputsNode.Replace(newProp);
            }

            UpgradeFieldSourcesNode(obj["FieldSourcesNode"] as JObject);

            UpgradeProcessorsNode(obj["TestedProcessorsNode"] as JObject);

            UpgradeProcessorsNode(obj["MatchedProcessorsNode"] as JObject);

        }

        public static void UpgradeFileSourceNode(JObject fileSourceNode)
        {
            JValue typeNameValue = fileSourceNode["FileSourceTypeName"] as JValue;
            string typeName = (string)typeNameValue?.Value;
            if (typeName != null)
            {
                UpgradeTypeName(ref typeName);
                FileSourceCatalog fsc = FileSourceCatalog.Instance;
                Type type = fsc.GetFileSourceType(typeName);
                if (type == null)
                {
                    type = fsc.FindFileSourceTypeWithSameName(typeName);
                }
                if (type != null)
                {
                    typeNameValue.Value = type.FullName;
                }
            }
        }

        public static void UpgradeConditionNode(JObject conditionNode)
        {
            JValue typeNameValue = conditionNode["ConditionTypeName"] as JValue;
            string typeName = (string)typeNameValue?.Value;
            if (typeName != null)
            {
                UpgradeTypeName(ref typeName);
                ConditionCatalog cc = ConditionCatalog.Instance;
                Type type = cc.GetConditionType(typeName);
                if (type == null)
                {
                    type = cc.FindConditionTypeWithSameName(typeName);
                }
                if (type != null)
                {
                    typeNameValue.Value = type.FullName;
                }
            }
            JArray childNodes = conditionNode["ChildNodes"] as JArray;
            foreach (JObject child in childNodes)
            {
                UpgradeConditionNode(child);
            }
        }

        public static void ConvertOutputsNode(JObject outputsNode)
        {
            JProperty outputNodes = outputsNode.Property("OutputNodes");
            if (outputNodes != null)
            {
                JProperty fieldSourceNodes = new JProperty("FieldSourceNodes", outputNodes.Value);
                outputNodes.Replace(fieldSourceNodes);
                JArray fieldSourceNodesArray = fieldSourceNodes.Value as JArray;
                foreach (JObject outputNode in fieldSourceNodesArray)
                {
                    JProperty outputTypeNameProperty = outputNode.Property("OutputTypeName");
                    JValue value = outputTypeNameProperty.Value as JValue;
                    string typeName = value.Value as string;
                    typeName = typeName?.Replace("Output", "FieldSource");
                    JProperty fieldSourceTypeNameProperty = new JProperty("FieldSourceTypeName",
                        new JValue(typeName));
                    outputTypeNameProperty.Replace(fieldSourceTypeNameProperty);
                }
            }
        }

        public static void UpgradeFieldSourcesNode(JObject fieldSourcesNode)
        {
            if (fieldSourcesNode != null)
            {
                JArray fieldSourceNodes = fieldSourcesNode["FieldSourceNodes"] as JArray;
                if (fieldSourceNodes != null)
                {
                    foreach (JObject fieldSourceNode in fieldSourceNodes)
                    {
                        UpgradeFieldSourceNode(fieldSourceNode);
                    }
                }
            }
        }

        public static void UpgradeFieldSourceNode(JObject fieldSourceNode)
        {
            JValue typeNameValue = fieldSourceNode["FieldSourceTypeName"] as JValue;
            string typeName = (string)typeNameValue?.Value;
            if (typeName != null)
            {
                UpgradeTypeName(ref typeName);
                FieldSourceCatalog fsc = FieldSourceCatalog.Instance;
                Type type = fsc.GetFieldSourceType(typeName);
                if (type == null)
                {
                    type = fsc.FindFieldSourceTypeWithSameName(typeName);
                }
                if (type != null)
                {
                    typeNameValue.Value = type.FullName;
                }
            }
        }

        public static void UpgradeProcessorsNode(JObject processorsNode)
        {
            if (processorsNode != null)
            {
                JArray processorNodes = processorsNode["ProcessorNodes"] as JArray;
                if (processorNodes != null)
                {
                    foreach (JObject processorNode in processorNodes)
                    {
                        UpgradeProcessorNode(processorNode);
                    }
                }
            }
        }

        public static void UpgradeProcessorNode(JObject processorNode)
        {
            JValue typeNameValue = processorNode["ProcessorTypeName"] as JValue;
            string typeName = (string)typeNameValue?.Value;
            if (typeName != null)
            {
                UpgradeTypeName(ref typeName);
                ProcessorCatalog pc = ProcessorCatalog.Instance;
                Type type = pc.GetProcessorType(typeName);
                if (type == null)
                {
                    type = pc.FindProcessorTypeWithSameName(typeName);
                }
                if (type != null)
                {
                    typeNameValue.Value = type.FullName;
                }
            }
            JObject childProcessorsNode = processorNode["ChildProcessorsNode"] as JObject;
            UpgradeProcessorsNode(childProcessorsNode);
        }

        public static void UpgradeTypeName(ref string typeName)
        {
            if (typeName == null)
            {
                return;
            }
            while (s_UpgradeTypeMap.ContainsKey(typeName))
            {
                typeName = s_UpgradeTypeMap[typeName];
            }
        }
    }
}
