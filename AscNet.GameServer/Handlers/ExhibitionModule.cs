using MessagePack;
using AscNet.Common.Database;
using AscNet.Common.MsgPack;
using AscNet.Common.Util;
using AscNet.Table.V2.share.equip;
using AscNet.Table.V2.share.exhibition;
using AscNet.Table.V2.share.reward;

namespace AscNet.GameServer.Handlers
{
    #region MsgPackScheme
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    [MessagePackObject(true)]
    public class GatherRewardRequest
    {
        public int Id;
    }

    [MessagePackObject(true)]
    public class GatherRewardResponse
    {
        public int Code;
        public List<RewardGoods> RewardGoods { get; set; } = new();
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    #endregion

    internal class ExhibitionModule
    {
        [RequestPacketHandler("GatherRewardRequest")]
        public static void HandleGatherRewardRequestHandler(Session session, Packet.Request packet)
        {
            GatherRewardRequest req = MessagePackSerializer.Deserialize<GatherRewardRequest>(packet.Content);
            ExhibitionRewardTable? exhibitionReward = TableReaderV2.Parse<ExhibitionRewardTable>().Find(x => x.Id == req.Id);
            if (exhibitionReward is null)
            {
                session.SendResponse(new GatherRewardResponse() { Code = 1 }, packet.Id);
                return;
            }

            if (session.player.GatherRewards.Contains(req.Id))
            {
                session.SendResponse(new GatherRewardResponse() { Code = 1 }, packet.Id);
                return;
            }

            if (!CanClaimExhibitionReward(session, exhibitionReward, out CharacterData character, out string claimFailure))
            {
                session.log.Warn($"GatherRewardRequest {req.Id} rejected before mutation: {claimFailure}");
                session.SendResponse(new GatherRewardResponse() { Code = 1 }, packet.Id);
                return;
            }

            List<RewardGoodsTable> rewardGoodsTables = [];
            if (exhibitionReward.RewardId is > 0)
            {
                rewardGoodsTables = RewardHandler.GetRewardGoods(exhibitionReward.RewardId.Value);
                string failure = "reward has no goods";
                if (rewardGoodsTables.Count == 0 || !CanGrantExhibitionRewards(rewardGoodsTables, out failure))
                {
                    session.log.Warn($"GatherRewardRequest {req.Id} rejected before mutation: {failure}");
                    session.SendResponse(new GatherRewardResponse() { Code = 1 }, packet.Id);
                    return;
                }
            }

            List<RewardGoods> rewardGoods = [];
            try
            {
                if (rewardGoodsTables.Count > 0)
                    rewardGoods = RewardHandler.GiveRewards(rewardGoodsTables, session);
            }
            catch (Exception ex)
            {
                session.log.Error($"GatherRewardRequest {req.Id} failed while granting rewards.", ex);
                session.SendResponse(new GatherRewardResponse() { Code = 1 }, packet.Id);
                return;
            }

            if (!session.player.AddGatherReward(req.Id))
            {
                session.log.Error($"GatherRewardRequest {req.Id} lost its duplicate-claim race after rewards were granted.");
                session.SendResponse(new GatherRewardResponse() { Code = 1 }, packet.Id);
                return;
            }

            if (exhibitionReward.LevelId > character.LiberateLv)
                character.LiberateLv = exhibitionReward.LevelId;

            session.player.Save();
            session.inventory.Save();
            session.character.Save();

            GatherRewardResponse rsp = new()
            {
                Code = 0,
                RewardGoods = rewardGoods
            };

            session.SendPush(new NotifyGatherReward() { Id = req.Id });
            session.SendResponse(rsp, packet.Id);
        }

        private static bool CanClaimExhibitionReward(
            Session session,
            ExhibitionRewardTable exhibitionReward,
            out CharacterData character,
            out string failure)
        {
            CharacterData? ownedCharacter = session.character.Characters.Find(characterData =>
                characterData.Id == exhibitionReward.CharacterId);
            if (ownedCharacter is null)
            {
                character = null!;
                failure = $"character {exhibitionReward.CharacterId} is not owned";
                return false;
            }
            character = ownedCharacter;

            ExhibitionRewardTable? previousReward = TableReaderV2.Parse<ExhibitionRewardTable>().Find(reward =>
                reward.CharacterId == exhibitionReward.CharacterId
                && reward.LevelId == exhibitionReward.LevelId - 1
                && reward.RewardId is > 0);
            if (previousReward is not null && !session.player.GatherRewards.Contains(previousReward.Id))
            {
                failure = $"previous exhibition reward {previousReward.Id} is not claimed";
                return false;
            }

            // Ability is derived by the client and is not persisted reliably by this
            // server, so only validate terminal conditions represented authoritatively
            // in the save: level and bound resonances on equipped 6-star gear.
            if (exhibitionReward.LevelId == 4 && exhibitionReward.SkillGroupId is > 0)
            {
                if (character.Level < 65)
                {
                    failure = $"character level {character.Level} is below 65";
                    return false;
                }

                int resonanceCount = CountEquippedResonances(session, (int)character.Id, minimumStar: 6);
                if (resonanceCount < 12)
                {
                    failure = $"character has {resonanceCount} qualifying resonances; 12 are required";
                    return false;
                }
            }

            failure = string.Empty;
            return true;
        }

        private static int CountEquippedResonances(Session session, int characterId, int minimumStar)
        {
            List<EquipTable> equipTables = TableReaderV2.Parse<EquipTable>();
            int resonanceCount = 0;
            foreach (EquipData equip in session.character.Equips.Where(equip => equip.CharacterId == characterId))
            {
                EquipTable? equipTable = equipTables.Find(table => table.Id == equip.TemplateId);
                if (equipTable is null || equipTable.Star < minimumStar)
                    continue;

                resonanceCount += (equip.ResonanceInfo ?? [])
                    .Count(resonance => resonance.CharacterId == characterId);
            }

            return resonanceCount;
        }

        private static bool CanGrantExhibitionRewards(
            IEnumerable<RewardGoodsTable> rewardGoodsTables,
            out string failure)
        {
            foreach (RewardGoodsTable rewardGoods in rewardGoodsTables)
            {
                RewardType? rewardType = RewardHandler.GetRewardType(rewardGoods);
                if (rewardType is null)
                {
                    failure = $"reward goods {rewardGoods.Id} has no supported reward type";
                    return false;
                }

                if (rewardType == RewardType.Item && !Inventory.IsValidClientItemId(rewardGoods.TemplateId))
                {
                    failure = $"reward goods {rewardGoods.Id} references unknown client item {rewardGoods.TemplateId}";
                    return false;
                }
            }

            failure = string.Empty;
            return true;
        }
    }
}
