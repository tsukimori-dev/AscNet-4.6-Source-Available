using Newtonsoft.Json.Linq;

namespace AscNet.Common.Util
{
    public static class JsonSnapshot
    {
        public static JObject LoadObject(string relativePath)
        {
            string path = ResolvePath(relativePath);
            return File.Exists(path)
                ? JObject.Parse(File.ReadAllText(path))
                : new JObject();
        }

        public static string LoadText(string relativePath)
        {
            string path = ResolvePath(relativePath);
            return File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        public static int ReadInt(JObject data, string name)
        {
            return data[name]?.Value<int>() ?? 0;
        }

        public static List<dynamic> ReadDynamicList(JToken? token)
        {
            if (token is not JArray array)
                return [];

            List<dynamic> result = new();
            foreach (JToken value in array)
            {
                result.Add(ReadDynamic(value)!);
            }

            return result;
        }

        public static dynamic? ReadDynamic(JToken? token)
        {
            if (token is null || token.Type == JTokenType.Null)
                return null;

            return token.Type switch
            {
                JTokenType.Object => ReadDynamicDictionary((JObject)token),
                JTokenType.Array => ReadDynamicList(token),
                JTokenType.Boolean => token.Value<bool>(),
                JTokenType.Integer => ReadInteger(token),
                JTokenType.Float => token.Value<double>(),
                JTokenType.String => token.Value<string>(),
                _ => null
            };
        }

        private static object ReadInteger(JToken token)
        {
            long value = token.Value<long>();
            return value >= int.MinValue && value <= int.MaxValue
                ? (int)value
                : value;
        }

        private static Dictionary<dynamic, dynamic> ReadDynamicDictionary(JObject data)
        {
            Dictionary<dynamic, dynamic> result = new();
            foreach (JProperty property in data.Properties())
            {
                dynamic key = int.TryParse(property.Name, out int numericKey)
                    ? numericKey
                    : property.Name;
                result[key] = ReadDynamic(property.Value);
            }

            return result;
        }

        public static string ResolvePath(string relativePath)
        {
            foreach (string root in CandidateRoots())
            {
                foreach (string candidatePath in RelativePathCandidates(relativePath))
                {
                    string directPath = Path.Combine(root, candidatePath);
                    if (File.Exists(directPath))
                        return directPath;

                    string resourcePath = Path.Combine(root, "Resources", candidatePath);
                    if (File.Exists(resourcePath))
                        return resourcePath;
                }
            }

            return Path.Combine("Resources", relativePath);
        }

        public static string ResolveDirectoryPath(string relativePath)
        {
            foreach (string root in CandidateRoots())
            {
                string directPath = Path.Combine(root, relativePath);
                if (Directory.Exists(directPath))
                    return directPath;

                string resourcePath = Path.Combine(root, "Resources", relativePath);
                if (Directory.Exists(resourcePath))
                    return resourcePath;
            }

            return Path.Combine("Resources", relativePath);
        }

        private static IEnumerable<string> CandidateRoots()
        {
            HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
            foreach (string start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
            {
                for (DirectoryInfo? directory = new(start); directory is not null; directory = directory.Parent)
                {
                    if (seen.Add(directory.FullName))
                        yield return directory.FullName;
                }
            }
        }

        private static IEnumerable<string> RelativePathCandidates(string relativePath)
        {
            string normalized = relativePath.Replace('\\', '/');
            const string tablePrefix = "table/";
            if (normalized.StartsWith(tablePrefix, StringComparison.OrdinalIgnoreCase))
            {
                string tableRelativePath = normalized[tablePrefix.Length..]
                    .Replace('/', Path.DirectorySeparatorChar);
                yield return Path.Combine(
                    ClientCompatibility.VersionedTableDirectoryName,
                    tableRelativePath);
            }

            yield return relativePath;
        }
    }
}
