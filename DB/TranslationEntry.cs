using EETLib.DB.Mongo;

namespace EETLib.DB;

public class TranslationEntry
{
    public string SourceTag { get; set; } = "";

    public string Group = ""; // DIAL, ACTI, etc
    public string Id = ""; // empty mostly, only present for INFO
    public string EdId = ""; // editor id, dialogue name or journal entry name for INFO (not translated)
    public string FieldName = ""; // FNAM, RNAM, etc
    public string OriginalText = "";
    public string TranslatedText = "";
    public string Comment = "";
    public string Status { get; set; } = TranslationStatus.None;
    
    public override string ToString()
    {
        return $"{OriginalText} -> {TranslatedText}";
    }
}
