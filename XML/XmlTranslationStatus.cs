namespace EETReader.XML;

public enum XmlTranslationStatus: int
{
    WaitingForValidation = 90,
    Ignored = -1,
    Validated = 99,
    Untranslated = 0,
    TranslatedFromIdentical = 98,
    TranslatedWithoutPunctuation = 80,
    OriginalFromDatabase = 1,
    CustomStatus = 150,
    OriginalEqTranslated = 52,
}
