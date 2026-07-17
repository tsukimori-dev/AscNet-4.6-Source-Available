using AscNet.Common.Database;
using AscNet.Common.MsgPack;

namespace AscNet.GameServer.Handlers;

internal static class StudyProgressModule
{
    internal static void SendLoginState(Session session)
    {
        StageDatum[] passedStudyStages = session.stage.Stages.Values
            .Where(stage => stage.Passed && CurrentClientStudyTables.TryGetStage(stage.StageId, out _))
            .ToArray();

        Dictionary<int, List<uint>> completedPracticeStages = new();
        Dictionary<int, List<StageDatum>> completedTeachingStages = new();
        foreach (StageDatum stage in passedStudyStages)
        {
            if (CurrentClientStudyTables.TryGetPracticeChapterId(stage.StageId, out int chapterId))
            {
                if (!completedPracticeStages.TryGetValue(chapterId, out List<uint>? stageIds))
                    completedPracticeStages.Add(chapterId, stageIds = new());
                stageIds.Add((uint)stage.StageId);
            }

            if (CurrentClientStudyTables.TryGetTeachingActivityIds(stage.StageId, out IReadOnlyList<int> activityIds))
            {
                foreach (int activityId in activityIds)
                {
                    if (!completedTeachingStages.TryGetValue(activityId, out List<StageDatum>? stages))
                        completedTeachingStages.Add(activityId, stages = new());
                    stages.Add(stage);
                }
            }
        }

        session.SendPush(new NotifyPracticeData
        {
            ChapterInfos = completedPracticeStages
                .OrderBy(pair => pair.Key)
                .Select(pair => new NotifyPracticeData.NotifyPracticeDataChapterInfo
                {
                    Id = pair.Key,
                    FinishStages = pair.Value.Distinct().Order().ToList()
                })
                .ToList()
        });

        session.SendPush("NotifyTeachingActivityInfo", MessagePackPayloads.Serialize(new Dictionary<string, object?>
        {
            ["ActivityInfo"] = completedTeachingStages
                .OrderBy(pair => pair.Key)
                .Select(pair => new Dictionary<string, object?>
                {
                    ["Id"] = pair.Key,
                    ["TreasureRecord"] = Array.Empty<int>(),
                    ["StarRecords"] = pair.Value
                        .GroupBy(stage => stage.StageId)
                        .OrderBy(group => group.Key)
                        .Select(group => new Dictionary<string, object?>
                        {
                            ["Id"] = group.Key,
                            ["StarsMark"] = group.Last().StarsMark
                        })
                        .ToArray()
                })
                .ToArray()
        }));
    }

    internal static void SendTeachingStageUpdate(Session session, StageDatum stage)
    {
        if (!stage.Passed || !CurrentClientStudyTables.TryGetTeachingActivityIds(stage.StageId, out _))
            return;

        session.SendPush("NotifyTeachingUpdateStageInfo", MessagePackPayloads.Serialize(new Dictionary<string, object?>
        {
            ["Info"] = new Dictionary<string, object?>
            {
                ["Id"] = stage.StageId,
                ["StarsMark"] = stage.StarsMark
            }
        }));
    }
}
