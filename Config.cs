using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

namespace ExternalLocalizerJpPack;

internal class Config : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;
    public static Config Instance => ModContent.GetInstance<Config>();

    [Header("Icon")]
    [DefaultValue(true)]
    [ReloadRequired]
    public bool EnableJapaneseModIcon { get; set; }

    [DefaultValue(false)]
    public bool EnableOutdatedAlertIcon { get; set; }

    [Header("Tooltip")]
    [DefaultValue(false)]
    public bool DisplayTranslatorsInTooltip { get; set; }

    [DefaultValue(true)]
    public bool DisplayNoteInTooltip { get; set; }

    [DefaultValue(true)]
    public bool DisplayOutdatedVersionWarn { get; set; }
}
