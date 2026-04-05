using HarmonyLib;
using WorldlineTracker.WorldlineTrackerCode;

namespace WorldlineTracker.WorldlineTrackerCode.Patches
{
    /// <summary>
    /// 补丁基类
    /// </summary>
    public abstract class BasePatch
    {
        /// <summary>
        /// 应用补丁
        /// </summary>
        public abstract void Apply(Harmony harmony);

        /// <summary>
        /// 日志记录
        /// </summary>
        protected static void LogInfo(string message) => MainFile.Logger.Info(message);

        /// <summary>
        /// 日志记录（调试）
        /// </summary>
        protected static void LogDebug(string message) => MainFile.Logger.Debug(message);

        /// <summary>
        /// 日志记录（警告）
        /// </summary>
        protected static void LogWarning(string message) => MainFile.Logger.Warn(message);

        /// <summary>
        /// 日志记录（错误）
        /// </summary>
        protected static void LogError(string message) => MainFile.Logger.Error(message);
    }
}