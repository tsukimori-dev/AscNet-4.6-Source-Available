using Newtonsoft.Json;

namespace AscNet.SDKServer.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class ServerVersionConfig
    {
        [JsonProperty("DocumentVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string DocumentVersion { get; set; } = string.Empty;

        [JsonProperty("LaunchModuleVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string LaunchModuleVersion { get; set; } = string.Empty;

        [JsonProperty("IndexMd5", NullValueHandling = NullValueHandling.Ignore)]
        public string IndexMd5 { get; set; } = string.Empty;

        [JsonProperty("IndexSha1", NullValueHandling = NullValueHandling.Ignore)]
        public string IndexSha1 { get; set; } = string.Empty;

        [JsonProperty("LaunchIndexSha1", NullValueHandling = NullValueHandling.Ignore)]
        public string LaunchIndexSha1 { get; set; } = string.Empty;

        [JsonProperty("IndexSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? IndexSize { get; set; }

        [JsonProperty("LaunchIndexSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? LaunchIndexSize { get; set; }

        [JsonProperty("DownloadMethod", NullValueHandling = NullValueHandling.Ignore)]
        public int? DownloadMethod { get; set; }

        [JsonProperty("ParallelChunkSize", NullValueHandling = NullValueHandling.Ignore)]
        public string? ParallelChunkSize { get; set; }

        [JsonProperty("Ready", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Ready { get; set; }

        [JsonProperty("BlockReason", NullValueHandling = NullValueHandling.Ignore)]
        public string? BlockReason { get; set; }

        [JsonProperty("Mainland", NullValueHandling = NullValueHandling.Ignore)]
        public ServerVersionConfig? Mainland { get; set; }
    }
}
