using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using ExternalLocalizerJpPack.Config;
using ExternalLocalizerJpPack.Data;
using ExternalLocalizerJpPack.UI;
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

namespace ExternalLocalizerJpPack.Patches;

internal class ModlistPatch : ModSystem
{
    private static Asset<Texture2D> s_iconTexture = ExternalLocalizerJpPack.Instance.Assets.Request<Texture2D>("Images/icon_ja_small");
    private static Asset<Texture2D> s_alertIconTexture = ExternalLocalizerJpPack.Instance.Assets.Request<Texture2D>("Images/icon_ja_small_alert");

    private static Assembly s_tmlAssembly => typeof(ModLoader).Assembly;
    private static Type s_localModType => s_tmlAssembly.GetTypeOrThrow("Terraria.ModLoader.Core.LocalMod");

    private static readonly ConditionalWeakTable<UIElement, UIElement> s_iconElements = new();

#pragma warning disable IDE0052
    // Read by ILCodes
    private static int s_rightButtonsIndex = -1;
#pragma warning restore IDE0052

    public override void Load()
    {
        if (!ModListConfig.Instance.EnableJapaneseModIcon)
            return;

        // Load the icon textures
        s_iconTexture.Wait();
        s_alertIconTexture.Wait();

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
            if (ModLoader.TryGetMod("ModFolder", out Mod modFolder))
            {
                // Mod Folder System
                // https://github.com/TigerChenzzz/TerrariaModFolder
                var uIModItemType = modFolder.GetType().Assembly.GetTypeOrThrow("ModFolder.UI.UIFolderItems.Mods.UIModItemInFolderLoaded");
                MonoModHooks.Modify(uIModItemType.GetMethodOrThrow("OnInitialize"), (il) => AddJapaneseModIconToModFolder(il, uIModItemType));
                MonoModHooks.Add(uIModItemType.GetMethodOrThrow("get_RightButtonsLength"), (Func<object, int> orig, object instance) => (s_rightButtonsIndex = orig(instance)) + 1);
            }

            {
                // Default Mod List
                // https://github.com/tModLoader/tModLoader
                var uIModItemType = s_tmlAssembly.GetTypeOrThrow("Terraria.ModLoader.UI.UIModItem");
                MonoModHooks.Modify(uIModItemType.GetMethodOrThrow("OnInitialize"), (il) => AddJapaneseModIconToDefaultModList(il, uIModItemType, left: new StyleDimension(-46, 1f), top: new StyleDimension(0, 0f)));
            }
        }
    }

    private static void AddJapaneseModIconToModFolder(ILContext il, Type uIModItemType)
    {
        ILCursor c = new(il);

        var label1 = c.DefineLabel();

        c.GotoNext(MoveType.Before, (i) => i.MatchCall("ModFolder.UI.UIFolderItems.UIFolderItem", "AppendRightButtonsPanel"));

        // Get and pop RightButtonsLength to initialize s_rightButtonsIndex
        c.EmitLdarg0();
        c.EmitCallvirt(uIModItemType.GetMethodOrThrow("get_RightButtonsLength"));
        c.EmitPop();

        // Create Japanese Icon
        var uiImageType = uIModItemType.Assembly.GetTypeOrThrow("ModFolder.UI.Base.UIImageWithVisibility");
        CreateJapaneseIcon(il, c, uiImageType, localModField: uIModItemType.GetFieldOrThrow("_mod"), out var succeed, out var uIImage, out var tooltipTextGenerator);

        // Check if successful
        c.EmitLdloc(succeed);
        c.EmitBrfalse(label1);

        // Register in RightButtons
        c.EmitLdarg0();
        c.EmitLdfld(uIModItemType.GetFieldOrThrow("rightButtons"));
        c.EmitLdsfld(typeof(ModlistPatch).GetFieldOrThrow("s_rightButtonsIndex"));
        c.EmitLdloc(uIImage);
        c.EmitStelemRef();

        // Add to mouseOverTooltips
        c.EmitLdarg0();
        c.EmitLdfld(uIModItemType.GetFieldOrThrow("mouseOverTooltips"));
        c.EmitLdloc(uIImage);
        c.EmitLdloc(tooltipTextGenerator);
        c.EmitNewobj(typeof(ValueTuple<UIElement, Func<string>>).GetConstructorOrThrow([typeof(UIElement), typeof(Func<string>)]));
        c.EmitCallvirt(typeof(System.Collections.Generic.List<(UIElement, Func<string>)>).GetMethodOrThrow("Add"));

        c.MarkLabel(label1);
        c.EmitNop();
    }

    private static void AddJapaneseModIconToModManager(ILContext il, Type uIModItemType)
    {
        ILCursor c = new(il);

        var label1 = c.DefineLabel();

        // Goto end of method
        c.GotoNext((i) => i.MatchCall("ModManager.Content.ModsList.UIModItemNew", "Redesign"));

        // Create Japanese Icon
        CreateJapaneseIcon(il, c, typeof(UIImage), localModField: uIModItemType.GetFieldOrThrow("mod"), out var succeed, out var uIImage, out var tooltipTextGenerator);

        // Check if successful
        c.EmitLdloc(succeed);
        c.EmitBrfalse(label1);

        // Set position
        c.EmitLdloc(uIImage);
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
        c.EmitLdloc(uIImage);
        c.EmitLdloc(tooltipTextGenerator);
        c.EmitDelegate<Action<UIElement, Func<string>>>((element, tooltipTextGenerator) =>
            {
                var uIModsNewType = uIModItemType.Assembly.GetTypeOrThrow("ModManager.Content.ModsList.UIModsNew");
                var instance = uIModsNewType.GetMethodOrThrow("get_Instance").Invoke(null, null);
                var toolTipField = uIModsNewType.GetFieldOrThrow("Tooltip");
                element.OnUpdate += (_) =>
                {
                    if (element.IsMouseHovering)
                        toolTipField.SetValue(instance, tooltipTextGenerator());
                };
            }
        );

        // Append to flagsMarkers
        c.EmitLdarg0();
        c.EmitLdfld(uIModItemType.GetFieldOrThrow("flagsMarkers"));
        c.EmitLdloc(uIImage);
        c.EmitCall(typeof(UIElement).GetMethodOrThrow("Append"));

        c.MarkLabel(label1);
        c.EmitNop();
    }

    private static void AddJapaneseModIconToDefaultModList(ILContext il, Type uIModItemType, StyleDimension left = default, StyleDimension top = default)
    {
        ILCursor c = new(il);

        var label1 = c.DefineLabel();
        var label2 = c.DefineLabel();

        c.GotoNext((i) => i.MatchLdfld("Terraria.ModLoader.Core.LocalMod", "location"));
        c.GotoNext(MoveType.After, (i) => i.MatchCall("Terraria.UI.UIElement", "Append"));

        // Create Japanese Icon
        CreateJapaneseIcon(il, c, typeof(UIHoverImage), localModField: uIModItemType.GetFieldRecursiveOrThrow("_mod"), out var succeed, out var uIImage, out var tooltipTextGenerator);

        // Check if successful
        c.EmitLdloc(succeed);
        c.EmitBrfalse(label1);

        // Set position
        c.EmitLdloc(uIImage);
        c.EmitDelegate((UIElement element) =>
        {
            element.Left = left;
            element.Top = top;
        });

        // Set update event to show tooltip
        c.EmitLdloc(uIImage);
        c.EmitLdloc(tooltipTextGenerator);
        c.EmitDelegate<Action<UIHoverImage, Func<string>>>((element, tooltipTextGenerator) => element.TooltipTextGenerator = tooltipTextGenerator);

        // Append to Parent
        c.EmitLdarg0();
        c.EmitLdloc(uIImage);
        c.EmitCall(typeof(UIElement).GetMethodOrThrow("Append"));

        c.MarkLabel(label1);
        c.EmitNop();

        // Set the xOffset for the next element
        if (c.TryGotoNext((i) => i.Previous.MatchLdcI4(-40) && i.MatchStloc(out _)))
        {
            c.EmitLdloc(succeed);
            c.EmitBrfalse(label2);

            c.EmitLdloc(24);
            c.EmitAdd();

            c.MarkLabel(label2);
            c.EmitNop();
        }
    }

    private static void CreateJapaneseIcon(ILContext il, ILCursor c, Type uIImageType, FieldInfo localModField, out VariableDefinition succeed, out VariableDefinition uIImage, out VariableDefinition tooltipTextGenerator)
    {
        var csvEntryTypeRef = il.Method.Module.ImportReference(typeof(CsvEntry));
        var versionTypeRef = il.Method.Module.ImportReference(typeof(Version));
        var boolTypeRef = il.Method.Module.ImportReference(typeof(bool));
        var stringTypeRef = il.Method.Module.ImportReference(typeof(string));
        var uIImageTypeRef = il.Method.Module.ImportReference(uIImageType);
        var stringFuncTypeRef = il.Method.Module.ImportReference(typeof(Func<string>));

        var csvEntry = new VariableDefinition(csvEntryTypeRef);
        il.Method.Body.Variables.Add(csvEntry);

        var latestVersion = new VariableDefinition(versionTypeRef);
        il.Method.Body.Variables.Add(latestVersion);

        var modName = new VariableDefinition(stringTypeRef);
        il.Method.Body.Variables.Add(modName);

        uIImage = new VariableDefinition(uIImageTypeRef);
        il.Method.Body.Variables.Add(uIImage);

        tooltipTextGenerator = new VariableDefinition(stringFuncTypeRef);
        il.Method.Body.Variables.Add(tooltipTextGenerator);

        succeed = new VariableDefinition(boolTypeRef);
        il.Method.Body.Variables.Add(succeed);

        var label1 = c.DefineLabel();
        var label2 = c.DefineLabel();

        // Get Mod Name
        c.EmitLdarg0();
        c.EmitLdfld(localModField);
        c.EmitCallvirt(s_localModType.GetMethodOrThrow("get_Name"));
        c.EmitStloc(modName);

        // Get CsvEntry
        c.EmitLdloc(modName);
        c.EmitCall(typeof(TranslatedModList).GetMethodOrThrow("GetModByInternalName"));
        c.EmitStloc(csvEntry);

        // Check if CsvEntry is null
        c.EmitLdloc(csvEntry);
        c.EmitBrtrue(label1);

        // Set succeed to false and skip the rest
        c.EmitLdcI4(0);
        c.EmitStloc(succeed);
        c.EmitBr(label2);

        // Set succeed to true
        c.MarkLabel(label1);
        c.EmitLdcI4(1);
        c.EmitStloc(succeed);

        // Get latest version
        c.EmitLdarg0();
        c.EmitLdfld(localModField);
        c.EmitCallvirt(s_localModType.GetMethodOrThrow("get_Version"));
        c.EmitStloc(latestVersion);

        // Create Tooltip
        c.EmitLdloc(csvEntry);
        c.EmitLdloc(latestVersion);
        c.EmitDelegate<Func<CsvEntry, Version, Func<string>>>((modEntry, latestVersion) => () => CreateTooltip(modEntry, latestVersion));
        c.EmitStloc(tooltipTextGenerator);

        // Load Texture
        c.EmitLdloc(csvEntry);
        c.EmitLdloc(latestVersion);
        c.EmitDelegate<Func<CsvEntry, Version, Asset<Texture2D>>>((modEntry, latestVersion) =>
        {
            if (!ModListConfig.Instance.EnableOutdatedAlertIcon)
                return s_iconTexture;

            return modEntry.Version < latestVersion ? s_alertIconTexture : s_iconTexture;
        });

        // Create UIHoverImage
        c.EmitNewobj(uIImageType.GetConstructorOrThrow([typeof(Asset<Texture2D>)]));

        // Set properties on UIHoverImage
        c.EmitDup();
        c.EmitLdcI4(1);
        c.EmitStfld(typeof(UIImage).GetFieldOrThrow("RemoveFloatingPointsFromDrawPosition"));

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

        c.EmitStloc(uIImage);

        // Store UIHoverImage in s_iconElements for hover detection
        c.EmitLdsfld(typeof(ModlistPatch).GetFieldOrThrow("s_iconElements"));
        c.EmitLdarg0();
        c.EmitLdloc(uIImage);
        c.EmitCall(typeof(ConditionalWeakTable<UIElement, UIElement>).GetMethodOrThrow("Add"));

        c.MarkLabel(label2);
    }

    internal static string CreateTooltip(CsvEntry modEntry, Version latestVersion)
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

        if (ModListConfig.Instance.DisplayTranslatorsInTooltip)
        {
            builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.Translators").Value);
            builder.AppendLine(WrapText(modEntry.Translators));
            builder.AppendLine();
        }

        if (ModListConfig.Instance.DisplayNoteInTooltip && !string.IsNullOrEmpty(modEntry.DisplayNote))
        {
            builder.AppendLine(ExternalLocalizerJpPack.Instance.GetLocalization("ModListTooltip.Note").Value);
            builder.AppendLine(WrapText(modEntry.DisplayNote));
            builder.AppendLine();
        }

        if (modEntry.Version < latestVersion && ModListConfig.Instance.DisplayOutdatedVersionWarn)
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
