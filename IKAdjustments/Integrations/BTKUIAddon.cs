using BTKUILib;
using BTKUILib.UIObjects;
using System.Runtime.CompilerServices;

namespace NAK.Melons.IKAdjustments;

public static class BTKUIAddon
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Init()
    {
        //Add myself to the Misc Menu
        Page miscPage = QuickMenuAPI.MiscTabPage;
        Category miscCategory = miscPage.AddCategory(IKAdjustmentsMod.SettingsCategory);

        // Add button
        miscCategory.AddButton("Tracking Adjust", "", "Adjust tracking points in this mode. Grip to adjust. Trigger to reset.")
            .OnPress += new Action(() =>
            {
                if (IKAdjustmentsMod.Manager != null)
                {
                    IKAdjustmentsMod.Manager.EnterAdjustMode();
                }
            });

        // Reset Button
        miscCategory.AddButton("Reset Offsets", "", "Reset all tracked point offsets.")
            .OnPress += new Action(() =>
            {
                if (IKAdjustmentsMod.Manager != null)
                {
                    IKAdjustmentsMod.Manager.ResetAllOffsets();
                }
            });

        // Cyle GrabMode Button
        miscCategory.AddButton("Cycle Mode", "", "Cycle grab mode. Position, Rotation, or Both.")
            .OnPress += new Action(() =>
            {
                if (IKAdjustmentsMod.Manager != null)
                {
                    IKAdjustmentsMod.Manager.CycleAdjustMode();
                }
            });
    }

    private static void AddMelonToggle(ref Category category, MelonLoader.MelonPreferences_Entry<bool> entry)
    {
        category.AddToggle(entry.DisplayName, entry.Description, entry.Value).OnValueUpdated += b => entry.Value = b;
    }

    private static void AddMelonSlider(ref Page page, MelonLoader.MelonPreferences_Entry<float> entry, float min, float max, int decimalPlaces = 2)
    {
        page.AddSlider(entry.DisplayName, entry.Description, entry.Value, min, max, decimalPlaces).OnValueUpdated += f => entry.Value = f;
    }
}