using System;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using ReLogic.Graphics;
using Steamworks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

using Version = System.Version;

namespace ExternalLocalizerJpPack;

internal class ModlistPatch : ModSystem
{
    private static Asset<Texture2D> s_iconTexture = null!;
    private static Asset<Texture2D> s_alertIconTexture = null!;

    private static Assembly s_tmlAssembly => typeof(ModLoader).Assembly;
    private static Type s_uIModItemType => s_tmlAssembly.GetTypeOrThrow("Terraria.ModLoader.UI.UIModItem");
    private static Type s_uIHoverImageType => s_tmlAssembly.GetTypeOrThrow("Terraria.ModLoader.UI.UIHoverImage");
    private static Type s_localModType => s_tmlAssembly.GetTypeOrThrow("Terraria.ModLoader.Core.LocalMod");

    public override void Load()
    {
        if (!Config.Instance.EnableJapaneseModIcon)
            return;

        // Load the icon textures
        s_iconTexture ??= this.Mod.Assets.Request<Texture2D>("Images/icon_ja_small");
        s_alertIconTexture ??= this.Mod.Assets.Request<Texture2D>("Images/icon_ja_small_alert");

        // Edit the UIModItem to add the Japanese icon
        MonoModHooks.Modify(s_uIModItemType.GetMethodOrThrow("OnInitialize"), this.DoPatch);
    }

    private void DoPatch(ILContext il)
    {
        ILCursor c = new(il);

        var intType = il.Method.Module.ImportReference(typeof(int));
        var xOffset = new VariableDefinition(intType);
        il.Method.Body.Variables.Add(xOffset);

        var csvEntryType = il.Method.Module.ImportReference(typeof(CsvEntry));
        var csvEntry = new VariableDefinition(csvEntryType);
        il.Method.Body.Variables.Add(csvEntry);

        var styleDimensionType = il.Method.Module.ImportReference(s_uIHoverImageType);
        var uIHoverImage = new VariableDefinition(csvEntryType);
        il.Method.Body.Variables.Add(uIHoverImage);

        var versionType = il.Method.Module.ImportReference(typeof(Version));
        var latestVersion = new VariableDefinition(versionType);
        il.Method.Body.Variables.Add(latestVersion);

        var stringType = il.Method.Module.ImportReference(typeof(string));
        var modName = new VariableDefinition(stringType);
        il.Method.Body.Variables.Add(modName);

        c.GotoNext((i) => i.MatchLdfld("Terraria.ModLoader.Core.LocalMod", "location"));
        c.GotoNext(MoveType.After, (i) => i.MatchCall("Terraria.UI.UIElement", "Append"));

        // Get Mod Name
        c.EmitLdarg0();
        c.EmitLdfld(s_uIModItemType.GetFieldOrThrow("_mod"));
        c.EmitCallvirt(s_localModType.GetMethodOrThrow("get_Name"));
        c.EmitStloc(modName);

        // Get latest version
        c.EmitLdarg0();
        c.EmitLdfld(s_uIModItemType.GetFieldOrThrow("_mod"));
        c.EmitCallvirt(s_localModType.GetMethodOrThrow("get_Version"));
        c.EmitStloc(latestVersion);

        // Get CsvEntry
        c.EmitLdloc(modName);
        c.EmitCall(typeof(TranslatedModList).GetMethodOrThrow("GetModByInternalName"));
        c.EmitStloc(csvEntry);

        // Check if CsvEntry is null
        var label1 = c.DefineLabel();
        var label2 = c.DefineLabel();
        c.EmitLdloc(csvEntry);
        c.EmitBrtrue(label1);

        // skip the rest of the patch if CsvEntry is null
        c.EmitLdcI4(0);
        c.EmitStloc(xOffset); // Set xOffset to 0
        c.EmitBr(label2); // Skip to the end of the patch

        c.MarkLabel(label1);

        // Set xOffset to -30
        c.EmitLdcI4(-24);
        c.EmitStloc(xOffset);

        // Load Texture
        c.EmitLdloc(csvEntry);
        c.EmitLdloc(latestVersion);
        c.EmitDelegate<Func<CsvEntry, Version, Asset<Texture2D>>>((modEntry, latestVersion) =>
        {
            if (!Config.Instance.EnableOutdatedAlertIcon)
                return s_iconTexture;

            return modEntry.Version < latestVersion ? s_alertIconTexture : s_iconTexture;
        });

        // Create Hover Text
        c.EmitLdloc(csvEntry);
        c.EmitLdloc(latestVersion);
        c.EmitCall(typeof(ModlistPatch).GetMethodOrThrow("CreateHoverText"));

        // Create UIHoverImage
        c.EmitNewobj(s_uIHoverImageType.GetConstructorOrThrow([typeof(Asset<Texture2D>), typeof(string)]));

        // Set properties on UIHoverImage
        c.EmitDup();
        c.EmitLdcI4(1);
        c.EmitStfld(typeof(UIImage).GetFieldOrThrow("RemoveFloatingPointsFromDrawPosition"));

        c.EmitDup();
        c.EmitLdcI4(1);
        c.EmitStfld(s_uIHoverImageType.GetFieldOrThrow("UseTooltipMouseText"));

        c.EmitDup();
        c.EmitLdflda(typeof(UIElement).GetFieldOrThrow("Left"));
        c.EmitLdcR4(-46);
        c.EmitStfld(typeof(StyleDimension).GetFieldOrThrow("Pixels"));

        c.EmitDup();
        c.EmitLdflda(typeof(UIElement).GetFieldOrThrow("Left"));
        c.EmitLdcR4(1);
        c.EmitCall(typeof(StyleDimension).GetMethodOrThrow("set_Percent"));

        // Set click event
        c.EmitDup();
        c.EmitLdloc(modName);
        c.EmitDelegate<Action<UIElement, string>>((element, modName) =>
        {
            element.OnLeftClick += (_, _) =>
            {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                var url = $"https://github.com/ExternalLocalizer/TMLHonyaku/tree/main/{modName}";
                try
                {
                    SteamFriends.ActivateGameOverlayToWebPage(url);
                }
                catch
                {
                    Utils.OpenToURL(url);
                }
            };
        });

        c.EmitStloc(uIHoverImage);

        // Append to Parent
        c.EmitLdarg0();
        c.EmitLdloc(uIHoverImage);
        c.EmitCall(typeof(UIElement).GetMethodOrThrow("Append"));

        c.MarkLabel(label2);

        // Set the xOffset for the next element
        c.GotoNext((i) => i.Previous.MatchLdcI4(-40) && i.MatchStloc(out _));
        c.EmitLdloc(xOffset);
        c.EmitAdd();
    }

    internal static string CreateHoverText(CsvEntry modEntry, Version latestVersion)
    {
        var builder = new System.Text.StringBuilder();

        if (modEntry.Version == latestVersion)
        {
            builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.Version").Format(modEntry.Version));
            builder.AppendLine();
        }
        else
        {
            builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.OutdatedVersion").Format(modEntry.Version, latestVersion));
            builder.AppendLine();

            if (Config.Instance.DisplayOutdatedVersionWarn)
            {
                builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.OutdatedVersionWarn").Value);
                builder.AppendLine();
            }
        }

        if (Config.Instance.DisplayTranslatorsInTooltip)
        {
            builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.Translators").Value);
            builder.AppendLine(WrapText(modEntry.Translators));
            builder.AppendLine();
        }

        if (Config.Instance.DisplayNoteInTooltip && !string.IsNullOrEmpty(modEntry.Note))
        {
            builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.Note").Value);
            builder.AppendLine(WrapText(modEntry.Note));
            builder.AppendLine();
        }

        builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.Click").Value);
        builder.AppendLine();

        return builder.ToString().TrimEnd('\n');
    }

    private static string WrapText(string text, int maxWidth = 400, int indentSize = 4, Asset<DynamicSpriteFont>? font = null)
    {

        font ??= FontAssets.MouseText;
        var indent = new string(' ', indentSize);

        text = font.Value.CreateWrappedText(text, maxWidth);
        text = indent + text;
        text = text.Replace("\n", "\n" + indent);

        return text.TrimEnd('\n');
    }
}
