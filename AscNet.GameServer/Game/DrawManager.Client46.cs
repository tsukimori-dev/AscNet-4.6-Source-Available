namespace AscNet.GameServer.Game
{
    internal partial class DrawManager
    {
        private const int Client46HelentineNormalDrawId = 1499;
        private const int Client46HelentineFateDrawId = 2493;
        private const int Client46HelentineCharacterId = 1_401_003;
        private const long Client46DrawStartTime = 1_784_242_800;

        private const string Client46NormalDrawBanner =
            "Assets/Product/Ui/ComponentPrefab/DrawCollaboration/UiDrawCollaborationCharacterNormalV4P5Power01.prefab";

        private const string Client46FateDrawBanner =
            "Assets/Product/Ui/ComponentPrefab/DrawCollaboration/UiDrawCollaborationCharacterFateV4P5Power01.prefab";

        private static readonly List<int> Client46CharacterPurchaseIds =
        [
            1969,
            2063,
            2064,
            2059,
            2060,
            2061,
            2062,
            2065,
            2066,
            2067,
            2068,
            2069,
            2070,
            2072
        ];

        private static DrawGroupDefinition[] BuildActiveDrawGroupDefinitions()
        {
            DrawGroupDefinition[] definitions = BuildGeneratedDrawGroupDefinitions();
            if (!IsClient46DrawCompatibilityEnabled())
                return definitions;

            return definitions.Select(definition => definition.Id switch
            {
                GroupThemedEventConstruct => definition with
                {
                    Banner = Client46NormalDrawBanner,
                    DefaultUseDrawIdDict = new()
                    {
                        [0] = Client46HelentineNormalDrawId,
                        [1] = 1498,
                        [5] = 1488
                    },
                    OptionalDrawIdList = [1488, 1498, Client46HelentineNormalDrawId],
                    TagBlackListDrawIds = [1498, 1488],
                    BannerBeginTime = Client46DrawStartTime,
                    BannerEndTime = 0,
                    StartTime = Client46DrawStartTime,
                    EndTime = 0
                },
                GroupFateThemedConstruct => definition with
                {
                    Banner = Client46FateDrawBanner,
                    DefaultUseDrawIdDict = new()
                    {
                        [0] = Client46HelentineFateDrawId,
                        [2] = 2492,
                        [6] = 2482
                    },
                    OptionalDrawIdList = [2482, 2492, Client46HelentineFateDrawId],
                    TagBlackListDrawIds = [2492, 2482],
                    BannerBeginTime = Client46DrawStartTime,
                    BannerEndTime = 0,
                    StartTime = Client46DrawStartTime,
                    EndTime = 0
                },
                _ => definition
            }).ToArray();
        }

        private static DrawInfoTemplate[] BuildActiveRetailDrawInfoTemplates()
        {
            DrawInfoTemplate[] templates = BuildGeneratedRetailDrawInfoTemplates();
            if (!IsClient46DrawCompatibilityEnabled())
                return templates;

            return
            [
                .. templates,
                BuildClient46HelentineDrawInfo(
                    Client46HelentineNormalDrawId,
                    GroupThemedEventConstruct,
                    maxBottomTimes: 60,
                    Client46NormalDrawBanner),
                BuildClient46HelentineDrawInfo(
                    Client46HelentineFateDrawId,
                    GroupFateThemedConstruct,
                    maxBottomTimes: 100,
                    Client46FateDrawBanner)
            ];
        }

        private static DrawInfoTemplate BuildClient46HelentineDrawInfo(
            int drawId,
            int groupId,
            int maxBottomTimes,
            string banner)
        {
            return new DrawInfoTemplate(
                drawId,
                groupId,
                DrawType: 3,
                UseItemId: 50005,
                UseItemCount: 250,
                BaseTodayCount: 0,
                BaseTotalCount: 0,
                BottomTimes: maxBottomTimes,
                MaxBottomTimes: maxBottomTimes,
                StartTime: Client46DrawStartTime,
                EndTime: 0,
                Banner: banner,
                Resources: new(),
                ResourceIds: new() { [1] = Client46HelentineCharacterId },
                BtnDrawCount: [1, 10],
                PurchaseUiType: [5, 6, 2],
                PurchaseId: [.. Client46CharacterPurchaseIds],
                ExPurchaseIds: [],
                CapacityCheckType: 0,
                GroupSubType: 0,
                IsTriggerSpecified: false,
                IsShowShop: false,
                IsShowBubble: false,
                UseTenDrawOnSaleTimes: 0,
                DailyLimitTimes: 0,
                ActivityLimitTimes: 0,
                ShowPriority: 0,
                UpGoodsId: 0);
        }

        private static bool IsClient46HelentineFirstPull(int drawId, int totalCountBeforePull)
        {
            return IsClient46DrawCompatibilityEnabled()
                && totalCountBeforePull == 0
                && drawId is Client46HelentineNormalDrawId or Client46HelentineFateDrawId;
        }

        private static bool IsClient46DrawCompatibilityEnabled()
        {
            return AscNet.Common.ClientCompatibility.IsConfiguredApplicationVersion(
                AscNet.Common.ClientCompatibility.SupportedApplicationVersion);
        }
    }
}
