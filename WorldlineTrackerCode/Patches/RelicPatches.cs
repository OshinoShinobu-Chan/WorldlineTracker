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
    /// 遗物相关补丁
    /// </summary>
    public class RelicPatches : BasePatch
    {
        private static bool _isPatching = false;

        public override void Apply(Harmony harmony)
        {
            try
            {
                LogInfo("Applying relic patches...");

                // 补丁 AbstractModel.BeforeCombatStart 方法（遗物常用）
                var abstractModelType = typeof(AbstractModel);
                var beforeCombatStartMethod = abstractModelType.GetMethod("BeforeCombatStart",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (beforeCombatStartMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(RelicPatches).GetMethod("BeforeCombatStartPrefix",
                        BindingFlags.NonPublic | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(RelicPatches).GetMethod("BeforeCombatStartPostfix",
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(beforeCombatStartMethod, prefix: prefix, postfix: postfix);
                    LogInfo($"Patched AbstractModel.BeforeCombatStart successfully");
                }
                else
                {
                    LogWarning("Could not find AbstractModel.BeforeCombatStart method");
                }

                // 补丁 AbstractModel.AfterCardPlayed 方法（遗物常用）
                var afterCardPlayedMethod = abstractModelType.GetMethod("AfterCardPlayed",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (afterCardPlayedMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(RelicPatches).GetMethod("AfterCardPlayedPrefix",
                        BindingFlags.NonPublic | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(RelicPatches).GetMethod("AfterCardPlayedPostfix",
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(afterCardPlayedMethod, prefix: prefix, postfix: postfix);
                    LogInfo($"Patched AbstractModel.AfterCardPlayed successfully");
                }
                else
                {
                    LogWarning("Could not find AbstractModel.AfterCardPlayed method");
                }

                // 尝试补丁可能存在的遗物主动使用方法
                // 如果找到RelicModel.OnUse或类似方法，可以在这里添加

                LogInfo("Relic patches applied successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply relic patches: {ex.Message}");
                throw;
            }
        }

        #region BeforeCombatStart 补丁

        private static void BeforeCombatStartPrefix(AbstractModel __instance)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                // 只处理遗物
                if (__instance is RelicModel relic)
                {
                    LogDebug($"Relic BeforeCombatStart starting: {relic.Id.Entry}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in BeforeCombatStartPrefix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        private static void BeforeCombatStartPostfix(AbstractModel __instance)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                // 只处理遗物
                if (__instance is RelicModel relic)
                {
                    LogDebug($"Relic BeforeCombatStart completed: {relic.Id.Entry}");
                    
                    // 记录遗物触发
                    RecordRelicTrigger(relic, "BeforeCombatStart", "System");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in BeforeCombatStartPostfix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        #endregion

        #region AfterCardPlayed 补丁

        private static void AfterCardPlayedPrefix(AbstractModel __instance)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                // 只处理遗物
                if (__instance is RelicModel relic)
                {
                    LogDebug($"Relic AfterCardPlayed starting: {relic.Id.Entry}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in AfterCardPlayedPrefix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        private static void AfterCardPlayedPostfix(AbstractModel __instance)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                // 只处理遗物
                if (__instance is RelicModel relic)
                {
                    LogDebug($"Relic AfterCardPlayed completed: {relic.Id.Entry}");
                    
                    // 记录遗物触发
                    RecordRelicTrigger(relic, "AfterCardPlayed", "Player");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in AfterCardPlayedPostfix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 记录遗物触发动作
        /// </summary>
        private static void RecordRelicTrigger(RelicModel relic, string triggerType, string target)
        {
            try
            {
                if (!BattleRecorder.Instance.IsRecording)
                {
                    // 如果还没开始记录，自动开始记录
                    BattleRecorder.Instance.StartRecording();
                }

                string relicId = relic.Id.Entry;
                
                // 判断是否为被动触发（大多数遗物触发都是被动的）
                bool isPassive = triggerType != "ManualUse";
                
                BattleRecorder.Instance.RecordRelicTrigger(relicId, target, isPassive);
                
                LogDebug($"Recorded relic trigger: {relicId} -> {target} (Type: {triggerType}, Passive: {isPassive})");
            }
            catch (Exception ex)
            {
                LogError($"Failed to record relic trigger: {ex.Message}");
            }
        }

        #endregion
    }
}