using System.Text.RegularExpressions;
using System.Xml;

namespace EETReader.XML;

public class XmlTranslationEntry
{
    public string SourceTag { get; set; } = "";
    
    public string? Group { get; set; } // DIAL, ACTI, etc
    public string? Id { get; set; } // empty mostly, only present for INFO
    public string? EdId { get; set; } // editor id, dialogue name or journal entry name for INFO (not translated)
    public string? FieldName { get; set; } // FNAM, RNAM, etc
    public string? Original { get; set; }
    public string? Translated { get; set; }
    public string? Perso { get; set; } 
    
    public string? Comment { get; set; }
    
    public string? Icon { get; set; }
    
    public string? IdsTexte { get; set; }
    
    public int Index { get; set; } // internal index, used in guild ranks
    
    public XmlTranslationStatus Status { get; set; }

    public Dictionary<string, XmlNode> FieldsSource = [];

    private static Dictionary<string, string> _xmlFieldToStruct = new()
    {
        ["GRUP"] = nameof(Group), // ACTI, DIAL, etc
        ["ID"] = nameof(Id),      // bk_HomiliesOfBlessedAlmalexia, 
        ["EDID"] = nameof(EdId),
        ["CHAMP"] = nameof(FieldName),
        ["ORIGINAL"] = nameof(Original),
        ["TRADUIT"] = nameof(Translated),
        ["PERSO"] = nameof(Perso),
        ["INDEX"] = nameof(Index),
        ["STATUS"] = nameof(Status),
        ["COMMENTAIRE"] = nameof(Comment),
        ["ICON"] = nameof(Icon),
        ["IDSTEXTE"] = nameof(IdsTexte),
    };
    
    public bool ApplyValuesBack()
    {
        foreach (var (fieldName, node) in FieldsSource)
        {
            if (!_xmlFieldToStruct.TryGetValue(fieldName, out var structFieldName))
                continue;

            var member = GetType().GetProperty(structFieldName);
            if (member == null)
                continue;

            var val = member.GetValue(this);

            if (val == null)
                node.RemoveAll();
            else
                node.InnerText = val.ToString();

            // TODO: convert &#x4;
            if (Regex.IsMatch(node.InnerText, @"[\u0001-\u0004]"))
            {
                HasError = true;
                return false;
            }
        }

        HasError = false;
        return true;
    }

    public bool HasError { get; set; }
}