using Newtonsoft.Json;

namespace AscNet.SDKServer.Models
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public partial class GameNotice
    {
        [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("Title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("Tag", NullValueHandling = NullValueHandling.Ignore)]
        public long? Tag { get; set; }

        [JsonProperty("Type", NullValueHandling = NullValueHandling.Ignore)]
        public long? Type { get; set; }

        [JsonProperty("Order", NullValueHandling = NullValueHandling.Ignore)]
        public long? Order { get; set; }

        [JsonProperty("LoginEject", NullValueHandling = NullValueHandling.Ignore)]
        public long? LoginEject { get; set; }

        [JsonProperty("BluePoint", NullValueHandling = NullValueHandling.Ignore)]
        public long? BluePoint { get; set; }

        [JsonProperty("Preload", NullValueHandling = NullValueHandling.Ignore)]
        public long? Preload { get; set; }

        [JsonProperty("ModifyTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? ModifyTime { get; set; }

        [JsonProperty("BeginTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? BeginTime { get; set; }

        [JsonProperty("EndTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? EndTime { get; set; }

        [JsonProperty("Content", NullValueHandling = NullValueHandling.Ignore)]
        public NoticeContent[] Content { get; set; }

        [JsonProperty("LoginPlatformList", NullValueHandling = NullValueHandling.Ignore)]
        public int[] LoginPlatformList { get; set; }

        public partial class NoticeContent
        {
            [JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
            public long? Id { get; set; }

            [JsonProperty("Title", NullValueHandling = NullValueHandling.Ignore)]
            public string Title { get; set; }

            [JsonProperty("Url", NullValueHandling = NullValueHandling.Ignore)]
            public string Url { get; set; }

            [JsonProperty("Order", NullValueHandling = NullValueHandling.Ignore)]
            public string Order { get; set; }
        }
    }
}
