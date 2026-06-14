using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using EETReader.DB;

namespace EETReader.CSV;

public static class CsvDatabase
{
    public static void Write(string path, IEnumerable<TranslationEntry> entries)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t" };

        using var writer = new StreamWriter(path, false, new UTF8Encoding(true));
        using var csv = new CsvWriter(writer, config);

        csv.Context.RegisterClassMap<TranslationEntryMap>();
        csv.WriteRecords(entries);
    }

    private sealed class TranslationEntryMap : ClassMap<TranslationEntry>
    {
        public TranslationEntryMap()
        {
            Map(e => e.Group).Name("Type");
            Map(e => e.EdId).Name("EdId");
            Map(e => e.OriginalText).Name("OriginalText");
            Map(e => e.TranslatedText).Name("TranslatedText");
            Map(e => e.Comment).Name("Comment");
        }
    }
}
