using System.Text.Json;

namespace AscNet.Common
{
    public sealed class ProfileUnlockCatalog
    {
        public string SourceCommit { get; set; } = string.Empty;
        public List<int> HeadPortraitIds { get; set; } = [];
        public List<int> MedalIds { get; set; } = [];
        public List<long> ChatBoardIds { get; set; } = [];
        public List<int> NameplateIds { get; set; } = [];
        public BossSingleChallengeUnlockCatalog BossSingleChallengeUnlock { get; set; } = new();

        public static ProfileUnlockCatalog Load(string path)
        {
            ProfileUnlockCatalog catalog = JsonSerializer.Deserialize<ProfileUnlockCatalog>(
                File.ReadAllText(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidDataException($"{path}: profile unlock catalog is empty.");
            catalog.Validate(path);
            return catalog;
        }

        private void Validate(string path)
        {
            HeadPortraitIds = ValidateIds(HeadPortraitIds, nameof(HeadPortraitIds), path);
            MedalIds = ValidateIds(MedalIds, nameof(MedalIds), path);
            ChatBoardIds = ValidateIds(ChatBoardIds, nameof(ChatBoardIds), path);
            NameplateIds = ValidateIds(NameplateIds, nameof(NameplateIds), path);

            if (string.IsNullOrWhiteSpace(SourceCommit)
                || BossSingleChallengeUnlock.LevelType <= 0
                || BossSingleChallengeUnlock.SectionConfigId <= 0
                || BossSingleChallengeUnlock.FeatureGroupId <= 0
                || BossSingleChallengeUnlock.RequiredTotalScore <= 0)
            {
                throw new InvalidDataException($"{path}: source commit or Phantom Pain Cage challenge unlock data is invalid.");
            }
        }

        private static List<T> ValidateIds<T>(IEnumerable<T>? values, string name, string path)
            where T : struct, IComparable<T>
        {
            T zero = default;
            List<T> ids = (values ?? [])
                .Distinct()
                .Order()
                .ToList();
            if (ids.Count == 0 || ids.Any(id => id.CompareTo(zero) <= 0))
                throw new InvalidDataException($"{path}: {name} must contain positive IDs.");
            return ids;
        }
    }

    public sealed class BossSingleChallengeUnlockCatalog
    {
        public int LevelType { get; set; }
        public int SectionConfigId { get; set; }
        public int FeatureGroupId { get; set; }
        public int RequiredTotalScore { get; set; }
    }
}
