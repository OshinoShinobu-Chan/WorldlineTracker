using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using WorldlineTracker.WorldlineTrackerCode.Patches;

namespace WorldlineTracker.WorldlineTrackerCode;

//You're recommended but not required to keep all your code in this package and all your assets in the WorldlineTracker folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "WorldlineTracker"; //At the moment, this is used only for the Logger and harmony names.

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Logger.Info("WorldlineTracker mod initializing...");

        // 启用Harmony调试日志
        Harmony.DEBUG = true;

        Harmony harmony = new(ModId);

        // 应用所有补丁
        ApplyPatches(harmony);

        Logger.Info("WorldlineTracker mod initialized successfully");
    }

    private static void ApplyPatches(Harmony harmony)
    {
        try
        {
            Logger.Info("Applying Harmony patches...");

            // 应用卡牌相关补丁
            new CardPatches().Apply(harmony);

            // 应用战斗相关补丁
            new CombatPatches().Apply(harmony);

            // 应用药水相关补丁
            new PotionPatches().Apply(harmony);

            // 应用遗物相关补丁
            new RelicPatches().Apply(harmony);

            // 应用敌人动作相关补丁
            new EnemyPatches().Apply(harmony);

            Logger.Info("Harmony patches applied successfully");
        }
        catch (System.Exception ex)
        {
            Logger.Error($"Failed to apply patches: {ex.Message}");
            throw;
        }
    }
}
