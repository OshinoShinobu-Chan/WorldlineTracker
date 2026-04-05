using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WorldlineTracker.WorldlineTrackerCode.Models;

namespace WorldlineTracker.WorldlineTrackerCode.Services
{
    /// <summary>
    /// 战斗记录管理器
    /// </summary>
    public class BattleRecorder
    {
        private static BattleRecorder? _instance;
        private readonly List<ActionRecord> _actions;
        private string _battleId;
        private DateTime _battleStartTime;
        private bool _isRecording;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static BattleRecorder Instance => _instance ??= new BattleRecorder();

        /// <summary>
        /// 当前是否正在记录
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// 当前战斗ID
        /// </summary>
        public string BattleId => _battleId;

        /// <summary>
        /// 动作数量
        /// </summary>
        public int ActionCount => _actions.Count;

        /// <summary>
        /// 当前回合数
        /// </summary>
        public int CurrentTurn { get; private set; } = 1;

        private BattleRecorder()
        {
            _actions = new List<ActionRecord>();
            _battleId = GenerateBattleId();
            _battleStartTime = DateTime.Now;
            _isRecording = false;
        }

        /// <summary>
        /// 开始记录战斗
        /// </summary>
        public void StartRecording()
        {
            if (_isRecording)
            {
                StopRecording();
            }

            _actions.Clear();
            _battleId = GenerateBattleId();
            _battleStartTime = DateTime.Now;
            CurrentTurn = 1;
            _isRecording = true;

            AddRecord(ActionRecord.CreateTurnStart(CurrentTurn, "System"));
            
            MainFile.Logger.Info($"Battle recording started: {_battleId}");
        }

        /// <summary>
        /// 停止记录战斗
        /// </summary>
        public void StopRecording()
        {
            if (!_isRecording) return;

            AddRecord(ActionRecord.CreateTurnEnd(CurrentTurn, "System"));
            _isRecording = false;

            // 自动保存战斗记录
            SaveToFile();

            MainFile.Logger.Info($"Battle recording stopped: {_battleId}, {_actions.Count} actions recorded");
        }

        /// <summary>
        /// 添加动作记录
        /// </summary>
        public void AddRecord(ActionRecord record)
        {
            if (!_isRecording) return;

            _actions.Add(record);
            MainFile.Logger.Debug($"Action recorded: {record}");
        }

        /// <summary>
        /// 添加卡牌播放记录
        /// </summary>
        public void RecordCardPlay(string cardId, string target, bool isPassive = false)
        {
            var record = ActionRecord.CreateCardPlay(CurrentTurn, cardId, target, isPassive);
            AddRecord(record);
        }

        /// <summary>
        /// 添加药水使用记录
        /// </summary>
        public void RecordPotionUse(string potionId, string target, bool isPassive = false)
        {
            var record = ActionRecord.CreatePotionUse(CurrentTurn, potionId, target, isPassive);
            AddRecord(record);
        }

        /// <summary>
        /// 添加遗物触发记录
        /// </summary>
        public void RecordRelicTrigger(string relicId, string target, bool isPassive = true)
        {
            var record = ActionRecord.CreateRelicTrigger(CurrentTurn, relicId, target, isPassive);
            AddRecord(record);
        }

        /// <summary>
        /// 添加回合开始记录
        /// </summary>
        public void RecordTurnStart(string initiator)
        {
            var record = ActionRecord.CreateTurnStart(CurrentTurn, initiator);
            AddRecord(record);
        }

        /// <summary>
        /// 添加回合结束记录
        /// </summary>
        public void RecordTurnEnd(string initiator)
        {
            var record = ActionRecord.CreateTurnEnd(CurrentTurn, initiator);
            AddRecord(record);
        }

        /// <summary>
        /// 进入下一回合
        /// </summary>
        public void NextTurn()
        {
            CurrentTurn++;
            MainFile.Logger.Info($"Advanced to turn {CurrentTurn}");
        }

        /// <summary>
        /// 获取所有动作记录
        /// </summary>
        public IReadOnlyList<ActionRecord> GetActions()
        {
            return _actions.AsReadOnly();
        }

        /// <summary>
        /// 获取指定回合的动作记录
        /// </summary>
        public IReadOnlyList<ActionRecord> GetActionsByTurn(int turn)
        {
            return _actions.Where(a => a.TurnNumber == turn).ToList().AsReadOnly();
        }

        /// <summary>
        /// 清除所有记录
        /// </summary>
        public void Clear()
        {
            _actions.Clear();
            CurrentTurn = 1;
            MainFile.Logger.Info("Battle records cleared");
        }

        /// <summary>
        /// 保存战斗记录到文件
        /// </summary>
        public void SaveToFile()
        {
            try
            {
                if (_actions.Count == 0) return;

                var battleData = new BattleData
                {
                    BattleId = _battleId,
                    StartTime = _battleStartTime,
                    EndTime = DateTime.Now,
                    TotalTurns = CurrentTurn,
                    Actions = _actions.ToList()
                };

                string json = JsonConvert.SerializeObject(battleData, Formatting.Indented);
                string filePath = GetBattleFilePath();

                // 确保目录存在
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, json);
                MainFile.Logger.Info($"Battle data saved to: {filePath}");
            }
            catch (Exception ex)
            {
                MainFile.Logger.Error($"Failed to save battle data: {ex.Message}");
            }
        }

        /// <summary>
        /// 从文件加载战斗记录
        /// </summary>
        public BattleData? LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MainFile.Logger.Error($"Battle file not found: {filePath}");
                    return null;
                }

                string json = File.ReadAllText(filePath);
                var battleData = JsonConvert.DeserializeObject<BattleData>(json);

                MainFile.Logger.Info($"Battle data loaded from: {filePath}");
                return battleData;
            }
            catch (Exception ex)
            {
                MainFile.Logger.Error($"Failed to load battle data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取所有战斗记录文件
        /// </summary>
        public List<string> GetBattleFiles()
        {
            var directory = GetBattleDirectory();
            if (!Directory.Exists(directory)) return new List<string>();

            return Directory.GetFiles(directory, "*.json").ToList();
        }

        /// <summary>
        /// 获取战斗记录文件路径
        /// </summary>
        private string GetBattleFilePath()
        {
            string fileName = $"Battle_{_battleId}_{_battleStartTime:yyyyMMdd_HHmmss}.json";
            return Path.Combine(GetBattleDirectory(), fileName);
        }

        /// <summary>
        /// 获取战斗记录目录
        /// </summary>
        private string GetBattleDirectory()
        {
            // 在实际Godot环境中，需要使用ProjectSettings.GlobalizePath等
            // 这里先使用相对路径，后续需要根据Godot API调整
            return Path.Combine("ModData", "WorldlineTracker", "Battles");
        }

        /// <summary>
        /// 生成战斗ID
        /// </summary>
        private string GenerateBattleId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        }
    }

    /// <summary>
    /// 战斗数据类（用于JSON序列化）
    /// </summary>
    public class BattleData
    {
        public string BattleId { get; set; } = default!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalTurns { get; set; }
        public List<ActionRecord> Actions { get; set; } = default!;
    }
}