using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;

namespace ExternalLocalizerJpPack;

internal static class TranslatedModList
{
    public static List<CsvEntry> Mods { get; private set; }

    static TranslatedModList()
    {
        using var stream = ExternalLocalizerJpPack.Instance.GetFileStream("Localization/TMLHonyaku/TranslatedMods.csv");
        using var streamReader = new StreamReader(stream);
        using var csvReader = new CsvReader(streamReader);
        csvReader.Configuration.RegisterClassMap<CsvEntry.Mapper>();
        csvReader.Configuration.HasHeaderRecord = true;
        csvReader.Configuration.WillThrowOnMissingField = false;
        Mods = csvReader.GetRecords<CsvEntry>().ToList();
    }

    public static CsvEntry? GetModBySteamId(string steamId)
    {
        return Mods.FirstOrDefault(mod => mod.SteamId == steamId);
    }

    public static CsvEntry? GetModByInternalName(string internalName)
    {
        return Mods.FirstOrDefault(mod => mod.InternalName == internalName);
    }

    public static CsvEntry? GetModByDisplayName(string displayName)
    {
        return Mods.FirstOrDefault(mod => mod.DisplayName == displayName);
    }
}

internal class CsvEntry
{
    public string SteamId { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string InternalName { get; set; } = null!;
    public Version Version { get; set; } = null!;
    public string Translators { get; set; } = null!;
    public string Note { get; set; } = null!;

    public class Mapper : CsvHelper.Configuration.CsvClassMap<CsvEntry>
    {
        public Mapper()
        {
            this.Map(m => m.SteamId).Name("steam_id");
            this.Map(m => m.DisplayName).Name("display_name");
            this.Map(m => m.InternalName).Name("internal_name");
            this.Map(m => m.Version).Name("version").TypeConverter<VersionConverter>();
            this.Map(m => m.Translators).Name("translators");
            this.Map(m => m.Note).Name("note").Default(string.Empty);
        }

        public class VersionConverter : CsvHelper.TypeConversion.ITypeConverter
        {
            public bool CanConvertFrom(Type type) => type.IsAssignableTo(typeof(string));

            public bool CanConvertTo(Type type) => type.IsAssignableTo(typeof(Version));

            public object ConvertFromString(CsvHelper.TypeConversion.TypeConverterOptions options, string text)
                => Version.TryParse(text, out var version) ? version : new Version(0, 0, 0, 0);

            public string ConvertToString(CsvHelper.TypeConversion.TypeConverterOptions options, object value)
                => value is not Version version
                    ? throw new InvalidOperationException("Cannot convert to string: value is not a Version.")
                    : version.ToString();
        }
    }
}

