using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
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
    private static Type s_uIHoverImageType => s_tmlAssembly.GetTypeOrThrow("Terraria.ModLoader.UI.UIHoverImage");
    private static Type s_localModType => s_tmlAssembly.GetTypeOrThrow("Terraria.ModLoader.Core.LocalMod");

    private static readonly ConditionalWeakTable<UIElement, UIElement> s_iconElements = new();

    public override void Load()
    {
        if (!Config.Instance.EnableJapaneseModIcon)
            return;

        // Load the icon textures
        s_iconTexture ??= this.Mod.Assets.Request<Texture2D>("Images/icon_ja_small");
        s_alertIconTexture ??= this.Mod.Assets.Request<Texture2D>("Images/icon_ja_small_alert");

        // Edit the UIModItem to add the Japanese icon
        if (ModLoader.TryGetMod("ConciseModList", out Mod conciseModList))
        {
            // ConciseModList
            // https://github.com/Cyrillya/ConciseModList
            var uIModItemType = conciseModList.GetType().Assembly.GetTypeOrThrow("ConciseModList.ConciseUIModItem");
            MonoModHooks.Modify(uIModItemType.GetMethodOrThrow("OnInitialize"), (il) => AddJapaneseModIconToDefaultModList(il, uIModItemType, left: new StyleDimension(-21, 1f), top: new StyleDimension(21, 0f)));

            var modifier = (Func<UIElement, bool, bool> orig, UIElement element, bool checkOnly) => (s_iconElements.TryGetValue(element, out var iconElement) && iconElement.IsMouseHovering) || orig.Invoke(element, checkOnly);
            MonoModHooks.Add(uIModItemType.GetMethodOrThrow("HoveringOnAnyElement"), modifier);
            MonoModHooks.Modify(uIModItemType.GetMethodOrThrow("LeftClickEvent"), (il) => { }); // なぜかこれがないとHoveringOnAnyElementのフックが動作しない
        }
        else if (ModLoader.TryGetMod("ModManager", out Mod modManager))
        {
            // Mod Manager
            // https://github.com/Drovyng/ModManager
            var uIModItemType = modManager.GetType().Assembly.GetTypeOrThrow("ModManager.Content.ModsList.UIModItemNew");
            MonoModHooks.Modify(uIModItemType.GetMethodOrThrow("OnInitialize"), (il) => AddJapaneseModIconToModManager(il, uIModItemType));
        }
        else
        {
            // Default Mod List
            // https://github.com/tModLoader/tModLoader
            var uIModItemType = s_tmlAssembly.GetTypeOrThrow("Terraria.ModLoader.UI.UIModItem");
            MonoModHooks.Modify(uIModItemType.GetMethodOrThrow("OnInitialize"), (il) => AddJapaneseModIconToDefaultModList(il, uIModItemType, left: new StyleDimension(-46, 1f), top: new StyleDimension(0, 0f)));
        }
    }

    private static void AddJapaneseModIconToModManager(ILContext il, Type uIModItemType)
    {
        ILCursor c = new(il);

        var uIHoverImageType = il.Method.Module.ImportReference(s_uIHoverImageType);
        var uIHoverImage = new VariableDefinition(uIHoverImageType);
        il.Method.Body.Variables.Add(uIHoverImage);

        var label1 = c.DefineLabel();

        // Goto end of method
        c.GotoNext((i) => i.MatchCall("ModManager.Content.ModsList.UIModItemNew", "Redesign"));

        // Create Japanese Icon
        CreateJapaneseIcon(il, c, uIHoverImage, localModField: uIModItemType.GetFieldOrThrow("mod"));

        // Check if icon is null
        c.EmitLdloc(uIHoverImage);
        c.EmitLdnull();
        c.EmitCeq();
        c.EmitBrtrue(label1);

        // Set position
        c.EmitLdloc(uIHoverImage);
        c.EmitLdarg0();
        c.EmitLdfld(uIModItemType.GetFieldOrThrow("flagsMarkers"));
        c.EmitDelegate<Func<UIElement, float>>((flagsMarkers) => -flagsMarkers.Children.DefaultIfEmpty().Min(e => e?.Left.Pixels ?? 0));
        c.EmitDelegate((UIImage element, float offset) =>
        {
            element.NormalizedOrigin = Vector2.One * 0.5f;
            element.VAlign = 0.5f;
            element.HAlign = 1f;
            element.Left = new() { Pixels = -(offset + element.Width.Pixels) };
            // element.Width = new() { Pixels = 22 };
            // element.Height = new() { Pixels = 22 };
        });

        // Set update event to show tooltip
        c.EmitLdloc(uIHoverImage);
        c.EmitDelegate<Action<UIElement>>((element) =>
            {
                var uIModsNewType = uIModItemType.Assembly.GetTypeOrThrow("ModManager.Content.ModsList.UIModsNew");
                var instance = uIModsNewType.GetMethodOrThrow("get_Instance").Invoke(null, null);
                var toolTipField = uIModsNewType.GetFieldOrThrow("Tooltip");
                element.OnUpdate += (_) =>
                {
                    if (element.IsMouseHovering)
                    {
                        var org = s_uIHoverImageType.GetFieldOrThrow("HoverText").GetValue(element);
                        toolTipField.SetValue(instance, org);
                    }
                };
            }
        );

        // Append to flagsMarkers
        c.EmitLdarg0();
        c.EmitLdfld(uIModItemType.GetFieldOrThrow("flagsMarkers"));
        c.EmitLdloc(uIHoverImage);
        c.EmitCall(typeof(UIElement).GetMethodOrThrow("Append"));

        c.MarkLabel(label1);
        c.EmitNop();
    }

    private static void AddJapaneseModIconToDefaultModList(ILContext il, Type uIModItemType, StyleDimension left = default, StyleDimension top = default)
    {
        ILCursor c = new(il);

        var uIHoverImageType = il.Method.Module.ImportReference(s_uIHoverImageType);
        var uIHoverImage = new VariableDefinition(uIHoverImageType);
        il.Method.Body.Variables.Add(uIHoverImage);
        var boolType = il.Method.Module.ImportReference(typeof(bool));
        var isIconNull = new VariableDefinition(boolType);
        il.Method.Body.Variables.Add(isIconNull);

        var label1 = c.DefineLabel();
        var label2 = c.DefineLabel();

        c.GotoNext((i) => i.MatchLdfld("Terraria.ModLoader.Core.LocalMod", "location"));
        c.GotoNext(MoveType.After, (i) => i.MatchCall("Terraria.UI.UIElement", "Append"));

        // Create Japanese Icon
        CreateJapaneseIcon(il, c, uIHoverImage, localModField: uIModItemType.GetFieldOrThrow("_mod"));

        // Check if the icon element is null
        c.EmitLdloc(uIHoverImage);
        c.EmitLdnull();
        c.EmitCeq();
        c.EmitDup();
        c.EmitStloc(isIconNull);
        c.EmitBrtrue(label1);

        // Set position
        c.EmitLdloc(uIHoverImage);
        c.EmitDelegate((UIImage element) =>
        {
            element.Left = left;
            element.Top = top;
        });

        // Append to Parent
        c.EmitLdarg0();
        c.EmitLdloc(uIHoverImage);
        c.EmitCall(typeof(UIElement).GetMethodOrThrow("Append"));

        c.MarkLabel(label1);
        c.EmitNop();

        // Set the xOffset for the next element
        if (c.TryGotoNext((i) => i.Previous.MatchLdcI4(-40) && i.MatchStloc(out _)))
        {
            c.EmitLdloc(isIconNull);
            c.EmitBrtrue(label2);

            c.EmitLdloc(24);
            c.EmitAdd();

            c.MarkLabel(label2);
            c.EmitNop();
        }
    }

    private static void CreateJapaneseIcon(ILContext il, ILCursor c, VariableDefinition uIHoverImage, FieldInfo localModField)
    {
        var csvEntryType = il.Method.Module.ImportReference(typeof(CsvEntry));
        var csvEntry = new VariableDefinition(csvEntryType);
        il.Method.Body.Variables.Add(csvEntry);

        var versionType = il.Method.Module.ImportReference(typeof(Version));
        var latestVersion = new VariableDefinition(versionType);
        il.Method.Body.Variables.Add(latestVersion);

        var stringType = il.Method.Module.ImportReference(typeof(string));
        var modName = new VariableDefinition(stringType);
        il.Method.Body.Variables.Add(modName);

        // Get Mod Name
        c.EmitLdarg0();
        c.EmitLdfld(localModField);
        c.EmitCallvirt(s_localModType.GetMethodOrThrow("get_Name"));
        c.EmitStloc(modName);

        // Get latest version
        c.EmitLdarg0();
        c.EmitLdfld(localModField);
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

        c.EmitBr(label2); // Skip to the end of the patch
        c.MarkLabel(label1);

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

        // Set click event
        c.EmitDup();
        c.EmitLdloc(modName);
        c.EmitDelegate<Action<UIElement, string>>((element, modName) =>
        {
            element.OnLeftClick += (evt, _) =>
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

        // Store UIHoverImage in s_iconElements for hover detection
        c.EmitLdsfld(typeof(ModlistPatch).GetFieldOrThrow("s_iconElements"));
        c.EmitLdarg0();
        c.EmitLdloc(uIHoverImage);
        c.EmitCall(typeof(ConditionalWeakTable<UIElement, UIElement>).GetMethodOrThrow("Add"));

        c.MarkLabel(label2);
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
        }

        if (Config.Instance.DisplayTranslatorsInTooltip)
        {
            builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.Translators").Value);
            builder.AppendLine(WrapText(modEntry.Translators));
            builder.AppendLine();
        }

        if (Config.Instance.DisplayNoteInTooltip && !string.IsNullOrEmpty(modEntry.DisplayNote))
        {
            builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.Note").Value);
            builder.AppendLine(WrapText(modEntry.DisplayNote));
            builder.AppendLine();
        }

        if (modEntry.Version < latestVersion && Config.Instance.DisplayOutdatedVersionWarn)
        {
            builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.OutdatedVersionWarn").Value);
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
