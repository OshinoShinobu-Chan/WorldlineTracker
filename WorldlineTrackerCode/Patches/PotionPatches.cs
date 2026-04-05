using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using WorldlineTracker.WorldlineTrackerCode.Models;
using WorldlineTracker.WorldlineTrackerCode.Services;

namespace WorldlineTracker.WorldlineTrackerCode.Patches
{
    /// <summary>
    /// 药水相关补丁
    /// </summary>
    public class PotionPatches : BasePatch
    {
        private static bool _isPatching = false;

        public override void Apply(Harmony harmony)
        {
            try
            {
                LogInfo("Applying potion patches...");

                // 补丁 PotionModel.OnUseWrapper 方法
                var potionModelType = typeof(PotionModel);
                var onUseWrapperMethod = potionModelType.GetMethod("OnUseWrapper",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (onUseWrapperMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(PotionPatches).GetMethod("OnUseWrapperPrefix",
                        BindingFlags.NonPublic | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(PotionPatches).GetMethod("OnUseWrapperPostfix",
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(onUseWrapperMethod, prefix: prefix, postfix: postfix);
                    LogInfo($"Patched PotionModel.OnUseWrapper successfully");
                }
                else
                {
                    LogWarning("Could not find PotionModel.OnUseWrapper method");
                }

                // 尝试补丁 UsePotionAction.ExecuteAction 方法
                var usePotionActionType = typeof(UsePotionAction);
                var executeActionMethod = usePotionActionType.GetMethod("ExecuteAction",
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (executeActionMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(PotionPatches).GetMethod("ExecuteActionPrefix",
                        BindingFlags.NonPublic | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(PotionPatches).GetMethod("ExecuteActionPostfix",
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(executeActionMethod, prefix: prefix, postfix: postfix);
                    LogInfo($"Patched UsePotionAction.ExecuteAction successfully");
                }
                else
                {
                    LogWarning("Could not find UsePotionAction.ExecuteAction method");
                }

                LogInfo("Potion patches applied successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply potion patches: {ex.Message}");
                throw;
            }
        }

        #region PotionModel.OnUseWrapper 补丁

        private static void OnUseWrapperPrefix(PotionModel __instance, Creature? target)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                LogDebug($"Potion use starting: {__instance.Id.Entry}, Target: {target?.Name}");
                
                // 这里可以添加动作记录逻辑
                // 后续会在BattleRecorder中实现
            }
            catch (Exception ex)
            {
                LogError($"Error in OnUseWrapperPrefix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        private static void OnUseWrapperPostfix(PotionModel __instance, Creature? target)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                LogDebug($"Potion use completed: {__instance.Id.Entry}, Target: {target?.Name}");
                
                // 记录药水使用动作
                RecordPotionUse(__instance, target);
            }
            catch (Exception ex)
            {
                LogError($"Error in OnUseWrapperPostfix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        #endregion

        #region UsePotionAction.ExecuteAction 补丁

        private static void ExecuteActionPrefix(UsePotionAction __instance)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                LogDebug($"UsePotionAction starting");
                
                // 这里可以添加动作记录逻辑
                // 后续会在BattleRecorder中实现
            }
            catch (Exception ex)
            {
                LogError($"Error in ExecuteActionPrefix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        private static void ExecuteActionPostfix(UsePotionAction __instance)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                LogDebug($"UsePotionAction completed");
                
                // 这里可以添加动作记录逻辑
                // 后续会在BattleRecorder中实现
            }
            catch (Exception ex)
            {
                LogError($"Error in ExecuteActionPostfix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 记录药水使用动作
        /// </summary>
        private static void RecordPotionUse(PotionModel potion, Creature? target)
        {
            try
            {
                if (!BattleRecorder.Instance.IsRecording)
                {
                    // 如果还没开始记录，自动开始记录
                    BattleRecorder.Instance.StartRecording();
                }

                string potionId = potion.Id.Entry;
                string targetName = target?.Name ?? "None";
                
                BattleRecorder.Instance.RecordPotionUse(potionId, targetName);
                
                LogDebug($"Recorded potion use: {potionId} -> {targetName}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to record potion use: {ex.Message}");
            }
        }

        #endregion
    }
}