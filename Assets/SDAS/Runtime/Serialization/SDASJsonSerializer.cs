using System;
using System.IO;
using SDAS.Runtime.Core;
using UnityEngine;

namespace SDAS.Runtime.Serialization
{
    public static class SDASJsonSerializer
    {
        public static string Serialize(SDASProjectData data, bool prettyPrint = true)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            return JsonUtility.ToJson(data, prettyPrint);
        }

        public static SDASProjectData Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new ArgumentException("Input JSON is null or empty.", nameof(json));
            }

            var data = JsonUtility.FromJson<SDASProjectData>(json);
            if (data == null)
            {
                throw new InvalidDataException("Failed to deserialize SDASProjectData from JSON.");
            }

            data.chapters ??= new();
            data.siteData ??= new();
            data.mappingResult ??= new();

            return data;
        }

        public static void SaveToFile(string path, SDASProjectData data, bool prettyPrint = true)
        {
            var json = Serialize(data, prettyPrint);
            File.WriteAllText(path, json);
        }

        public static SDASProjectData LoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Cannot find SDAS JSON file.", path);
            }

            var json = File.ReadAllText(path);
            return Deserialize(json);
        }
    }
}
