using System;
using System.Reflection;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using WorldlineTracker.WorldlineTrackerCode.Models;
using WorldlineTracker.WorldlineTrackerCode.Services;

namespace WorldlineTracker.WorldlineTrackerCode.Patches
{
    /// <summary>
    /// 敌人动作相关补丁
    /// </summary>
    public class EnemyPatches : BasePatch
    {
        private static bool _isPatching = false;

        public override void Apply(Harmony harmony)
        {
            try
            {
                LogInfo("Applying enemy patches...");

                // 补丁 CombatHistory.MonsterPerformedMove 方法
                var combatHistoryType = typeof(CombatHistory);
                var monsterPerformedMoveMethod = combatHistoryType.GetMethod("MonsterPerformedMove",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (monsterPerformedMoveMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(EnemyPatches).GetMethod("MonsterPerformedMovePostfix",
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(monsterPerformedMoveMethod, postfix: postfix);
                    LogInfo($"Patched CombatHistory.MonsterPerformedMove successfully");
                }
                else
                {
                    LogWarning("Could not find CombatHistory.MonsterPerformedMove method");
                }

                // 尝试补丁可能存在的敌人动作方法
                // 如果找到Creature.TakeAction或类似方法，可以在这里添加

                LogInfo("Enemy patches applied successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply enemy patches: {ex.Message}");
                throw;
            }
        }

        #region CombatHistory.MonsterPerformedMove 补丁

        private static void MonsterPerformedMovePostfix(CombatHistory __instance, CombatState combatState, MonsterModel monster, MoveState move, IEnumerable<Creature>? targets)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                string monsterName = monster.Title?.ToString() ?? "Unknown";
                string moveName = move?.Id?.ToString() ?? "Unknown";
                LogDebug($"Enemy action performed: {monsterName}, Move: {moveName}");
                
                // 记录敌人动作
                RecordEnemyAction(combatState, monster, move, targets);
            }
            catch (Exception ex)
            {
                LogError($"Error in MonsterPerformedMovePostfix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 记录敌人动作
        /// </summary>
        private static void RecordEnemyAction(CombatState combatState, MonsterModel monster, MoveState move, IEnumerable<Creature>? targets)
        {
            try
            {
                if (!BattleRecorder.Instance.IsRecording)
                {
                    // 如果还没开始记录，自动开始记录
                    BattleRecorder.Instance.StartRecording();
                }

                string enemyName = monster.Title?.ToString() ?? "Unknown";
                string action = move?.Id?.ToString() ?? "Unknown";
                string targetNames = "None";
                
                if (targets != null)
                {
                    var targetList = targets.Select(t => t.Name).ToList();
                    targetNames = string.Join(", ", targetList);
                }

                // 获取当前回合数
                int turnNumber = BattleRecorder.Instance.CurrentTurn;
                
                // 创建敌人动作记录
                var record = ActionRecord.CreateEnemyAction(turnNumber, enemyName, action, targetNames);
                BattleRecorder.Instance.AddRecord(record);
                
                LogDebug($"Recorded enemy action: {enemyName} -> {targetNames} (Action: {action})");
            }
            catch (Exception ex)
            {
                LogError($"Failed to record enemy action: {ex.Message}");
            }
        }

        #endregion
    }
}