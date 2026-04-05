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
                
                // 补丁 AfterCombatRoomLoaded 方法（战斗开始）
                var afterCombatRoomLoadedMethod = combatManagerType.GetMethod("AfterCombatRoomLoaded", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (afterCombatRoomLoadedMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(CombatPatches).GetMethod("AfterCombatRoomLoadedPostfix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(afterCombatRoomLoadedMethod, postfix: postfix);
                    LogInfo($"Patched CombatManager.AfterCombatRoomLoaded successfully");
                }
                else
                {
                    LogWarning("Could not find CombatManager.AfterCombatRoomLoaded method");
                }

                // 补丁 EndCombatInternal 方法（战斗结束）
                var endCombatInternalMethod = combatManagerType.GetMethod("EndCombatInternal", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (endCombatInternalMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(CombatPatches).GetMethod("EndCombatInternalPrefix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(endCombatInternalMethod, prefix: prefix);
                    LogInfo($"Patched CombatManager.EndCombatInternal successfully");
                }
                else
                {
                    LogWarning("Could not find CombatManager.EndCombatInternal method");
                }

                // 补丁 EndPlayerTurnPhaseOneInternal 方法（结束玩家回合阶段一）
                var endPlayerTurnPhaseOneMethod = combatManagerType.GetMethod("EndPlayerTurnPhaseOneInternal", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (endPlayerTurnPhaseOneMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(CombatPatches).GetMethod("EndPlayerTurnPhaseOnePostfix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(endPlayerTurnPhaseOneMethod, postfix: postfix);
                    LogInfo($"Patched CombatManager.EndPlayerTurnPhaseOneInternal successfully");
                }
                else
                {
                    LogWarning("Could not find CombatManager.EndPlayerTurnPhaseOneInternal method");
                }

                // 补丁 EndPlayerTurnPhaseTwoInternal 方法（结束玩家回合阶段二，开始敌人回合）
                var endPlayerTurnPhaseTwoMethod = combatManagerType.GetMethod("EndPlayerTurnPhaseTwoInternal", 
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (endPlayerTurnPhaseTwoMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(CombatPatches).GetMethod("EndPlayerTurnPhaseTwoPostfix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(endPlayerTurnPhaseTwoMethod, postfix: postfix);
                    LogInfo($"Patched CombatManager.EndPlayerTurnPhaseTwoInternal successfully");
                }
                else
                {
                    LogWarning("Could not find CombatManager.EndPlayerTurnPhaseTwoInternal method");
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

        private static void AfterCombatRoomLoadedPostfix()
        {
            try
            {
                LogInfo($"Combat started (AfterCombatRoomLoaded)");
                
                // 开始记录战斗
                BattleRecorder.Instance.StartRecording();
                
                // 记录玩家回合开始
                BattleRecorder.Instance.RecordTurnStart("Player");
            }
            catch (Exception ex)
            {
                LogError($"Error in AfterCombatRoomLoadedPostfix: {ex.Message}");
            }
        }

        private static void EndCombatInternalPrefix()
        {
            try
            {
                LogInfo($"Combat ending (EndCombatInternal)");
                
                // 停止记录战斗
                BattleRecorder.Instance.StopRecording();
            }
            catch (Exception ex)
            {
                LogError($"Error in EndCombatInternalPrefix: {ex.Message}");
            }
        }

        #endregion

        #region 回合管理补丁

        private static void EndPlayerTurnPhaseOnePostfix()
        {
            try
            {
                LogInfo("Player turn phase one ended (准备进入敌人回合)");
                
                // 记录玩家回合结束
                BattleRecorder.Instance.RecordTurnEnd("Player");
            }
            catch (Exception ex)
            {
                LogError($"Error in EndPlayerTurnPhaseOnePostfix: {ex.Message}");
            }
        }

        private static void EndPlayerTurnPhaseTwoPostfix()
        {
            try
            {
                LogInfo("Player turn phase two ended (开始敌人回合)");
                
                // 记录敌人回合开始
                BattleRecorder.Instance.RecordTurnStart("Enemy");
                
                // 进入下一回合
                BattleRecorder.Instance.NextTurn();
            }
            catch (Exception ex)
            {
                LogError($"Error in EndPlayerTurnPhaseTwoPostfix: {ex.Message}");
            }
        }

        #endregion
    }
}