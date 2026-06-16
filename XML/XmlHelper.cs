using System.Text;
using System.Xml;

namespace EETReader.XML;

public static class XmlHelper
{
    public delegate XmlActionType TranslationEntryFuncCallback(XmlTranslationEntry node);
    
    public delegate void TranslationEntryActionCallback(XmlTranslationEntry node);

    public static void ModifyXml(string path, string outPath, TranslationEntryFuncCallback translationEntryFuncCallback)
    {
        var doc = TraverseAndModifyXml(path, translationEntryFuncCallback);

        // var text = JsonSerializer.Serialize(nodes, new JsonSerializerOptions()
        // {
        //     WriteIndented = true,
        // });

        // File.WriteAllText(Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(outPath) + ".json"), text);

        using var writer = XmlWriter.Create(outPath, new()
        {
            Encoding = Encoding.UTF8,
            Indent = true,
        });

        doc.WriteTo(writer);
    }

    public static XmlDocument TraverseAndModifyXml(string path, TranslationEntryActionCallback callback)
    {
        return TraverseAndModifyXml(path, (node) =>
        {
            callback(node);
            return XmlActionType.None;
        });
    }

    public static XmlDocument TraverseAndModifyXml(string path, TranslationEntryFuncCallback callback)
    {
        var doc = new XmlDocument();
        doc.Load(path);

        var tag = Path.GetFileName(path);

        var documentElement = doc.ChildNodes[1]!;

        List<XmlTranslationEntry> nodes = new();

        List<XmlNode> toRemove = new();
        foreach (XmlNode translationNode in documentElement.ChildNodes)
        {
            XmlTranslationEntry transEntry = new()
            {
                SourceTag = tag
            };
            
            foreach (XmlNode fieldNode in translationNode.ChildNodes)
            {
                transEntry.FieldsSource.Add(fieldNode.Name, fieldNode);

                switch (fieldNode.Name)
                {
                    case "GRUP":
                        transEntry.Group = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    case "ID":
                        transEntry.Id = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    case "EDID":
                        transEntry.EdId = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    case "CHAMP":
                        transEntry.FieldName = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    case "ORIGINAL":
                        transEntry.Original = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    case "TRADUIT":
                        transEntry.Translated = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    case "PERSO":
                        transEntry.Perso = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    case "INDEX":
                        transEntry.Index = int.Parse(fieldNode.InnerText);
                        break;
                    case "STATUS":
                        transEntry.Status = (XmlTranslationStatus)int.Parse(fieldNode.InnerText);
                        break;
                    case "COMMENTAIRE":
                        transEntry.Comment = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    case "ICON":
                        transEntry.Icon = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    case "IDSTEXTE":
                        transEntry.IdsTexte = fieldNode.ChildNodes.Count == 0 ? null : fieldNode.InnerText;
                        break;
                    default:
                        break;
                }
            }
            
            var action = callback(transEntry);

            transEntry.ApplyValuesBack();

            if (transEntry.HasError || action == XmlActionType.Remove)
                toRemove.Add(translationNode);

            nodes.Add(transEntry);
        }

        foreach (var node in toRemove)
            documentElement.RemoveChild(node);

        return doc;
    }
}
