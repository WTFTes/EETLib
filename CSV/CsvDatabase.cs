using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using EETLib.DB;

namespace EETLib.CSV;

public static class CsvDatabase
{
    private static readonly CsvConfiguration WriteConfig;
    private static readonly CsvConfiguration ReadConfig;
    private static readonly UTF8Encoding Utf8Bom = new(true);

    static CsvDatabase()
    {
        WriteConfig = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = "\t" };
        ReadConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t",
            HeaderValidated = null,
            MissingFieldFound = null,
            Mode = CsvMode.NoEscape,
        };
    }

    public static void Write(string path, IEnumerable<TranslationEntry> entries)
    {
        using var writer = new StreamWriter(path, false, Utf8Bom);
        using var csv = new CsvWriter(writer, WriteConfig);
        csv.Context.RegisterClassMap<TranslationEntryMap>();
        csv.WriteRecords(entries);
    }

    public static List<TranslationEntry> Read(string path)
    {
        using var reader = new StreamReader(path, Utf8Bom);
        using var csv = new CsvReader(reader, ReadConfig);
        csv.Context.RegisterClassMap<TranslationEntryMap>();
        return csv.GetRecords<TranslationEntry>().ToList();
    }

    public static List<TranslationEntry> ReadAll(string path, string pattern = "*.csv")
    {
        var files = Directory.GetFiles(path, pattern);

        return files.SelectMany(Read).ToList();
    }

    public static void SplitIntoChunks(string path, int chunkSize)
    {
        var entries = Read(path);

        var chunks = entries.Chunk(chunkSize).ToArray();
        for (var i = 0; i < chunks.Length; ++i)
        {
            var chunk = chunks[i];
            var directory = Path.GetDirectoryName(path);
            var chunkName = Path.GetFileNameWithoutExtension(path) + $"_{i:D4}" + Path.GetExtension(path);

            Write(Path.Combine(directory, chunkName), chunk);
        }
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
