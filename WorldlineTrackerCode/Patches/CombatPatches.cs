using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Rooms;
using WorldlineTracker.WorldlineTrackerCode.Services;

namespace WorldlineTracker.WorldlineTrackerCode.Patches
{
    /// <summary>
    /// 战斗相关补丁
    /// </summary>
    public class CombatPatches : BasePatch
    {
        public override void Apply(Harmony harmony)
        {
            try
            {
                LogInfo("Applying combat patches...");

                // 补丁 CombatManager 的开始战斗方法
                var combatManagerType = typeof(CombatManager);
                
                // 尝试查找开始战斗的方法
                var startCombatMethod = combatManagerType.GetMethod("StartCombat", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (startCombatMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(CombatPatches).GetMethod("StartCombatPostfix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(startCombatMethod, postfix: postfix);
                    LogInfo($"Patched CombatManager.StartCombat successfully");
                }
                else
                {
                    LogWarning("Could not find CombatManager.StartCombat method");
                }

                // 尝试查找结束战斗的方法
                var endCombatMethod = combatManagerType.GetMethod("EndCombat", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (endCombatMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(CombatPatches).GetMethod("EndCombatPostfix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(endCombatMethod, postfix: postfix);
                    LogInfo($"Patched CombatManager.EndCombat successfully");
                }
                else
                {
                    LogWarning("Could not find CombatManager.EndCombat method");
                }

                // 尝试补丁回合相关方法
                var endPlayerTurnMethod = combatManagerType.GetMethod("EndPlayerTurn", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (endPlayerTurnMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(CombatPatches).GetMethod("EndPlayerTurnPostfix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(endPlayerTurnMethod, postfix: postfix);
                    LogInfo($"Patched CombatManager.EndPlayerTurn successfully");
                }
                else
                {
                    LogWarning("Could not find CombatManager.EndPlayerTurn method");
                }

                // 尝试补丁开始敌人回合方法
                var beginEnemyTurnMethod = combatManagerType.GetMethod("BeginEnemyTurn", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (beginEnemyTurnMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(CombatPatches).GetMethod("BeginEnemyTurnPostfix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(beginEnemyTurnMethod, postfix: postfix);
                    LogInfo($"Patched CombatManager.BeginEnemyTurn successfully");
                }
                else
                {
                    LogWarning("Could not find CombatManager.BeginEnemyTurn method");
                }

                LogInfo("Combat patches applied successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply combat patches: {ex.Message}");
                throw;
            }
        }

        #region 战斗开始/结束补丁

        private static void StartCombatPostfix(CombatRoom room)
        {
            try
            {
                LogInfo($"Combat started in room");
                
                // 开始记录战斗
                BattleRecorder.Instance.StartRecording();
                
                // 记录玩家回合开始
                BattleRecorder.Instance.RecordTurnStart("Player");
            }
            catch (Exception ex)
            {
                LogError($"Error in StartCombatPostfix: {ex.Message}");
            }
        }

        private static void EndCombatPostfix(CombatRoom room)
        {
            try
            {
                LogInfo($"Combat ended in room");
                
                // 停止记录战斗
                BattleRecorder.Instance.StopRecording();
            }
            catch (Exception ex)
            {
                LogError($"Error in EndCombatPostfix: {ex.Message}");
            }
        }

        #endregion

        #region 回合管理补丁

        private static void EndPlayerTurnPostfix()
        {
            try
            {
                LogInfo("Player turn ended");
                
                // 记录玩家回合结束
                BattleRecorder.Instance.RecordTurnEnd("Player");
                
                // 进入下一回合
                BattleRecorder.Instance.NextTurn();
            }
            catch (Exception ex)
            {
                LogError($"Error in EndPlayerTurnPostfix: {ex.Message}");
            }
        }

        private static void BeginEnemyTurnPostfix()
        {
            try
            {
                LogInfo("Enemy turn started");
                
                // 记录敌人回合开始
                BattleRecorder.Instance.RecordTurnStart("Enemy");
            }
            catch (Exception ex)
            {
                LogError($"Error in BeginEnemyTurnPostfix: {ex.Message}");
            }
        }

        #endregion
    }
}