using ABI_RC.Core.InteractionSystem;
using ABI_RC.Systems.IK;
using HarmonyLib;

namespace NAK.Melons.IKAdjustments.HarmonyPatches;

internal static class IKSystemPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(IKSystem), "Start")]
    private static void Postfix_IKSystem_Start(ref IKSystem __instance)
    {
        IKAdjustmentsMod.Manager = __instance.gameObject.AddComponent<IKOffsetManager>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(IKSystem), "ResetIkSettings")]
    private static void Postfix_IKSystem_ResetIkSettings()
    {
        IKAdjustmentsMod.Manager?.ResetAllOffsets();
    }
}

internal static class CVR_MenuManagerPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(CVR_MenuManager), "ToggleQuickMenu", new Type[] { typeof(bool) })]
    private static void Postfix_CVR_MenuManager_ToggleQuickMenu(bool show)
    {
        if (show) IKAdjustmentsMod.Manager?.ExitAdjustMode();
    }
}