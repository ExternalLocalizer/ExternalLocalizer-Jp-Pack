using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ExternalLocalizerJpPack.Config;

internal class ModListConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static ModListConfig Instance => ModContent.GetInstance<ModListConfig>();

    [Header("Icon")]
    [DefaultValue(true)]
    [ReloadRequired]
    public bool EnableJapaneseModIcon { get; set; }

    [DefaultValue(false)]
    [ReloadRequired]
    public bool EnableOutdatedAlertIcon { get; set; }

    [Header("Tooltip")]
    [DefaultValue(false)]
    public bool DisplayTranslatorsInTooltip { get; set; }

    [DefaultValue(true)]
    public bool DisplayNoteInTooltip { get; set; }

    [DefaultValue(true)]
    public bool DisplayOutdatedVersionWarn { get; set; }
}
