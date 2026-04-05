using System;
using System.Collections.Generic;

namespace WorldlineTracker.WorldlineTrackerCode.Models
{
    /// <summary>
    /// 动作类型枚举
    /// </summary>
    public enum ActionType
    {
        PlayCard,       // 出牌
        UsePotion,      // 使用药水
        RelicTrigger,   // 遗物触发（主动）
        PassiveEffect,  // 被动效果
        EndTurn,        // 结束回合
        StartTurn,      // 开始回合
        EnemyAction,    // 敌人动作
        DamageDealt,    // 造成伤害
        DamageReceived, // 受到伤害
        BlockGained,    // 获得格挡
        BlockLost,      // 失去格挡
        PowerGained,    // 获得能力
        PowerLost,      // 失去能力
        CardDrawn,      // 抽牌
        CardDiscarded,  // 弃牌
        CardExhausted   // 消耗牌
    }

    /// <summary>
    /// 动作记录类
    /// </summary>
    public class ActionRecord
    {
        /// <summary>
        /// 回合数
        /// </summary>
        public int TurnNumber { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 动作类型
        /// </summary>
        public ActionType Type { get; set; }

        /// <summary>
        /// 发起者（玩家/敌人名）
        /// </summary>
        public string Initiator { get; set; } = "";

        /// <summary>
        /// 目标
        /// </summary>
        public string Target { get; set; } = "";

        /// <summary>
        /// 卡牌ID（如适用）
        /// </summary>
        public string CardId { get; set; } = "";

        /// <summary>
        /// 药水ID（如适用）
        /// </summary>
        public string PotionId { get; set; } = "";

        /// <summary>
        /// 遗物ID（如适用）
        /// </summary>
        public string RelicId { get; set; } = "";

        /// <summary>
        /// 是否为被动触发
        /// </summary>
        public bool IsPassive { get; set; }

        /// <summary>
        /// 伤害值（如适用）
        /// </summary>
        public decimal DamageAmount { get; set; }

        /// <summary>
        /// 格挡值（如适用）
        /// </summary>
        public decimal BlockAmount { get; set; }

        /// <summary>
        /// 额外元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// 创建新的动作记录
        /// </summary>
        public ActionRecord()
        {
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// 创建卡牌播放记录
        /// </summary>
        public static ActionRecord CreateCardPlay(int turnNumber, string cardId, string target, bool isPassive = false)
        {
            return new ActionRecord
            {
                TurnNumber = turnNumber,
                Type = ActionType.PlayCard,
                Initiator = "Player",
                Target = target,
                CardId = cardId,
                IsPassive = isPassive
            };
        }

        /// <summary>
        /// 创建药水使用记录
        /// </summary>
        public static ActionRecord CreatePotionUse(int turnNumber, string potionId, string target, bool isPassive = false)
        {
            return new ActionRecord
            {
                TurnNumber = turnNumber,
                Type = ActionType.UsePotion,
                Initiator = "Player",
                Target = target,
                PotionId = potionId,
                IsPassive = isPassive
            };
        }

        /// <summary>
        /// 创建遗物触发记录
        /// </summary>
        public static ActionRecord CreateRelicTrigger(int turnNumber, string relicId, string target, bool isPassive = true)
        {
            return new ActionRecord
            {
                TurnNumber = turnNumber,
                Type = ActionType.RelicTrigger,
                Initiator = "Player",
                Target = target,
                RelicId = relicId,
                IsPassive = isPassive
            };
        }

        /// <summary>
        /// 创建回合开始记录
        /// </summary>
        public static ActionRecord CreateTurnStart(int turnNumber, string initiator)
        {
            return new ActionRecord
            {
                TurnNumber = turnNumber,
                Type = ActionType.StartTurn,
                Initiator = initiator,
                Target = ""
            };
        }

        /// <summary>
        /// 创建回合结束记录
        /// </summary>
        public static ActionRecord CreateTurnEnd(int turnNumber, string initiator)
        {
            return new ActionRecord
            {
                TurnNumber = turnNumber,
                Type = ActionType.EndTurn,
                Initiator = initiator,
                Target = ""
            };
        }

        /// <summary>
        /// 创建敌人动作记录
        /// </summary>
        public static ActionRecord CreateEnemyAction(int turnNumber, string enemyName, string action, string target)
        {
            return new ActionRecord
            {
                TurnNumber = turnNumber,
                Type = ActionType.EnemyAction,
                Initiator = enemyName,
                Target = target,
                Metadata = new Dictionary<string, object> { { "Action", action } }
            };
        }

        /// <summary>
        /// 创建伤害记录
        /// </summary>
        public static ActionRecord CreateDamageDealt(int turnNumber, string initiator, string target, decimal amount, string source = "")
        {
            var record = new ActionRecord
            {
                TurnNumber = turnNumber,
                Type = ActionType.DamageDealt,
                Initiator = initiator,
                Target = target,
                DamageAmount = amount
            };

            if (!string.IsNullOrEmpty(source))
            {
                record.Metadata["Source"] = source;
            }

            return record;
        }

        /// <summary>
        /// 转换为字符串表示
        /// </summary>
        public override string ToString()
        {
            return $"[Turn {TurnNumber}] {Timestamp:HH:mm:ss} {Type} - {Initiator} -> {Target}";
        }
    }
}