using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using WorldlineTracker.WorldlineTrackerCode.Models;
using WorldlineTracker.WorldlineTrackerCode.Services;

namespace WorldlineTracker.WorldlineTrackerCode.Patches
{
    /// <summary>
    /// 卡牌相关补丁
    /// </summary>
    public class CardPatches : BasePatch
    {
        private static bool _isPatching = false;

        public override void Apply(Harmony harmony)
        {
            try
            {
                LogInfo("Applying card patches...");

                // 补丁 CardModel.OnPlayWrapper 方法
                var cardModelType = typeof(CardModel);
                var onPlayWrapperMethod = cardModelType.GetMethod("OnPlayWrapper", 
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (onPlayWrapperMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(CardPatches).GetMethod("OnPlayWrapperPrefix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(CardPatches).GetMethod("OnPlayWrapperPostfix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(onPlayWrapperMethod, prefix: prefix, postfix: postfix);
                    LogInfo($"Patched CardModel.OnPlayWrapper successfully");
                }
                else
                {
                    LogWarning("Could not find CardModel.OnPlayWrapper method");
                }

                // 补丁 PlayCardAction.ExecuteAction 方法
                var playCardActionType = typeof(PlayCardAction);
                var executeActionMethod = playCardActionType.GetMethod("ExecuteAction", 
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                
                if (executeActionMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(CardPatches).GetMethod("ExecuteActionPrefix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    var postfix = new HarmonyMethod(typeof(CardPatches).GetMethod("ExecuteActionPostfix", 
                        BindingFlags.NonPublic | BindingFlags.Static));
                    
                    harmony.Patch(executeActionMethod, prefix: prefix, postfix: postfix);
                    LogInfo($"Patched PlayCardAction.ExecuteAction successfully");
                }
                else
                {
                    LogWarning("Could not find PlayCardAction.ExecuteAction method");
                }

                LogInfo("Card patches applied successfully");
            }
            catch (Exception ex)
            {
                LogError($"Failed to apply card patches: {ex.Message}");
                throw;
            }
        }

        #region CardModel.OnPlayWrapper 补丁

        private static void OnPlayWrapperPrefix(CardModel __instance, Creature? target)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                LogDebug($"Card play starting: {__instance.Title}, Target: {target?.Name}");
                
                // 这里可以添加动作记录逻辑
                // 后续会在BattleRecorder中实现
            }
            catch (Exception ex)
            {
                LogError($"Error in OnPlayWrapperPrefix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        private static void OnPlayWrapperPostfix(CardModel __instance, Creature? target)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                LogDebug($"Card play completed: {__instance.Title}, Target: {target?.Name}");
                
                // 记录卡牌播放动作
                RecordCardPlay(__instance, target);
            }
            catch (Exception ex)
            {
                LogError($"Error in OnPlayWrapperPostfix: {ex.Message}");
            }
            finally
            {
                _isPatching = false;
            }
        }

        #endregion

        #region PlayCardAction.ExecuteAction 补丁

        private static void ExecuteActionPrefix(PlayCardAction __instance)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                LogDebug($"PlayCardAction starting");
                
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

        private static void ExecuteActionPostfix(PlayCardAction __instance)
        {
            try
            {
                if (_isPatching) return;
                _isPatching = true;

                LogDebug($"PlayCardAction completed");
                
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
        /// 记录卡牌播放动作
        /// </summary>
        private static void RecordCardPlay(CardModel card, Creature? target)
        {
            try
            {
                if (!BattleRecorder.Instance.IsRecording)
                {
                    // 如果还没开始记录，自动开始记录
                    BattleRecorder.Instance.StartRecording();
                }

                string cardId = card.Id.Entry;
                string targetName = target?.Name ?? "None";
                
                BattleRecorder.Instance.RecordCardPlay(cardId, targetName);
                
                LogDebug($"Recorded card play: {cardId} -> {targetName}");
            }
            catch (Exception ex)
            {
                LogError($"Failed to record card play: {ex.Message}");
            }
        }

        #endregion
    }
}