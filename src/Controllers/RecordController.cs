﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Snap.Genshin.Website.Entities;
using Snap.Genshin.Website.Entities.Record;
using Snap.Genshin.Website.Models;

namespace Snap.Genshin.Website.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RecordController : ControllerBase
    {
        public RecordController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private readonly ApplicationDbContext dbContext;

        [HttpGet("[Action]/{uid}")]
        public IActionResult CheckRecord([FromRoute()] string uid)
        {
            if (string.IsNullOrEmpty(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            var playerQuery = dbContext.Players.Where(player => player.Uid == uid);
            if (!playerQuery.Any())
            {
                return this.Success("查询成功", new { PeriodUploaded = true });
            }

            var player = playerQuery.Single();
            var recordQuery = dbContext.PlayerRecords.Where(record => record.PlayerId == player.InnerId);

            return this.Success("查询成功", new { PeriodUploaded = recordQuery.Any() });
        }

        [HttpPost("Upload")]
        // [Authorize(Policy = IdentityPolicyNames.CommonUser)]
        public async Task<IActionResult> UploadRecord([FromBody] Models.SnapGenshin.PlayerRecord record)
        {
            if (string.IsNullOrEmpty(Request.Headers.Authorization))
            {
                return Unauthorized();
            }

            #region 更新角色信息
            Player? player = dbContext.Players
                .Where(player => player.Uid == record.Uid)
                .Include(player => player.Avatars)
                .SingleOrDefault();

            if (player is null)
            {
                player = new Player()
                {
                    Uid = record.Uid,
                    Avatars = new List<AvatarDetail>()
                };
                dbContext.Players.Add(player);
            }
            player.Avatars.Clear();

            IEnumerable<AvatarDetail>? newAvatars = record.PlayerAvatars
                .Select(avatar => new AvatarDetail()
                {
                    AvatarId = avatar.Id,
                    AvatarLevel = avatar.Level,
                    WeaponId = avatar.Weapon.Id,
                    WeaponLevel = avatar.Weapon.Level,
                    AffixLevel = avatar.Weapon.AffixLevel,
                    ActivedConstellationNum = avatar.ActivedConstellationNum,
                    ReliquarySets = avatar.ReliquarySets
                    .Select(r => new ReliquarySetDetail()
                    {
                        Id = r.Id,
                        Count = r.Count,
                        UnionId = $"{r.Id}-{r.Count}"
                    }).ToList()
                });
            player.Avatars = newAvatars.ToList();

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            #endregion

            #region 更新深渊数据
            // 删除旧记录
            PlayerRecord? oldPlayerRecord = dbContext.PlayerRecords
                .Where(record => record.PlayerId == player.InnerId)
                .FirstOrDefault();

            if (oldPlayerRecord is not null)
            {
                dbContext.PlayerRecords.Remove(oldPlayerRecord);
            }

            // 插入新记录
            dbContext.PlayerRecords.Add(new PlayerRecord()
            {
                PlayerId = player.InnerId,
                SpiralAbyssLevels = record.PlayerSpiralAbyssesLevels
                .Select(level => new SpiralAbyssLevel
                {
                    FloorIndex = level.FloorIndex,
                    LevelIndex = level.LevelIndex,
                    Star = level.Star,
                    Battles = level.Battles
                    .Select(battle => new SpiralAbyssBattle
                    {
                        Avatars = battle.AvatarIds
                        .Select(avatar => new SpiralAbyssAvatar
                        {
                            AvatarId = avatar
                        }).ToList(),
                        BattleIndex = battle.BattleIndex,
                    }).ToList()
                }).ToList()
            });

            await dbContext.SaveChangesAsync().ConfigureAwait(false);
            #endregion

            return this.Success($"{record.Uid}-数据上传成功");
        }
    }
}
