using MelonLoader;

namespace NAK.Melons.IKAdjustments;

public class IKAdjustmentsMod : MelonMod
{
    internal const string SettingsCategory = "IKAdjustments";
    internal static MelonLogger.Instance Logger;
    internal static IKOffsetManager Manager;

    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;

        ApplyPatches(typeof(HarmonyPatches.IKSystemPatches));
        ApplyPatches(typeof(HarmonyPatches.CVR_MenuManagerPatches));

        //BTKUILib Misc Tab
        if (MelonMod.RegisteredMelons.Any(it => it.Info.Name == "BTKUILib"))
        {
            Logger.Msg("Initializing BTKUILib support.");
            BTKUIAddon.Init();
        }
    }

    void ApplyPatches(Type type)
    {
        try
        {
            HarmonyInstance.PatchAll(type);
        }
        catch (Exception e)
        {
            Logger.Msg($"Failed while patching {type.Name}!");
            Logger.Error(e);
        }
    }
}