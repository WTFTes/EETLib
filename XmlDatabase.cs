using System.Text;
using System.Xml;

namespace EETReader;

public class XmlDatabase
{
    public string FilePath => _path;
    
    private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, List<TranslationEntry>>>>> _nodes = new();

    private string _path;
    private XmlDocument _xml;

    public IEnumerable<TranslationEntry> Entries =>
        _nodes.Values
            .SelectMany(byChamp => byChamp.Values)
            .SelectMany(byEdid => byEdid.Values)
            .SelectMany(byHash => byHash.Values)
            .SelectMany(nodeList => nodeList);

    public IEnumerable<TranslationEntry> GetNodesByType(string groupName)
    {
        if (!_nodes.TryGetValue(groupName, out var byFieldName))
            yield break;

        foreach (var (_, byEdid) in byFieldName)
        foreach (var (_, byHash) in byEdid)
        foreach (var (_, nodeList) in byHash)
        foreach (var node in nodeList)
            yield return node;
    }

    public void Load(string path)
    {
        _path = path;

        _xml = XmlHelper.TraverseAndModifyXml(path, node =>
        {
            if (!_nodes.ContainsKey(node.Group))
                _nodes[node.Group] = new();

            if (!_nodes[node.Group].ContainsKey(node.FieldName))
                _nodes[node.Group][node.FieldName] = new();

            var edid = node.EdId ?? "";
            if (!_nodes[node.Group][node.FieldName].ContainsKey(edid))
                _nodes[node.Group][node.FieldName][edid] = new();

            if (!_nodes[node.Group][node.FieldName][edid].ContainsKey(node.Original.GetHashCode()))
                _nodes[node.Group][node.FieldName][edid][node.Original.GetHashCode()] = new();

            _nodes[node.Group][node.FieldName][edid][node.Original.GetHashCode()].Add(node);
        });
    }

    public TranslationEntry? LookupCandidate(TranslationEntry other, bool compareIndex = false, bool compareId = false)
    {
        if (!_nodes.TryGetValue(other.Group, out var byFieldName))
            return null;

        if (!byFieldName.TryGetValue(other.FieldName, out var byEdid))
            return null;

        var edid = other.EdId ?? "";
        if (!byEdid.TryGetValue(edid, out var byHash))
            return null;
        
        if (!byHash.TryGetValue(other.Original.GetHashCode(), out var nodeList))
            return null;

        return nodeList.FirstOrDefault(e => (!compareIndex || e.Index == other.Index) && (!compareId || e.Id == other.Id));
    }

    public void Save(string path)
    {
        using var writer = XmlWriter.Create(path, new()
        {
            Encoding = Encoding.UTF8,
            Indent = true,
        });

        _xml.WriteTo(writer);
    }

    public List<List<TranslationEntry>> TrySquash()
    {
        List<List<TranslationEntry>> unsquashed = new();

        foreach (var (_, byChamp) in _nodes)
        foreach (var (_, byEdid) in byChamp)
        foreach (var (_, byHash) in byEdid)
        foreach (var (_, nodeList) in byHash)
        {
            var grouped = nodeList.GroupBy(node => node.Id?.GetHashCode());
            foreach (var group in grouped)
            {
                var u = group.DistinctBy(entry => $"{entry.Group}:{entry.EdId}:{entry.FieldName}:{entry.Original?.GetHashCode()}:{entry.Translated?.GetHashCode()}").ToList();
                if (u.Count > 1)
                    unsquashed.Add(u);
            }
        }

        return unsquashed;
    }

    public IEnumerable<TranslationEntry> LookupByEdId(string edId) =>
        _nodes.Values
            .SelectMany(byChamp => byChamp.Values)
            .SelectMany(byEdid => byEdid.Where(kvp => kvp.Key == edId))
            .SelectMany(kvp => kvp.Value.SelectMany(byHash => byHash.Value));
    
    public IEnumerable<TranslationEntry> Find(Func<TranslationEntry, bool> m) => Entries.Where(m);
}
