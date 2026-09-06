using System.Text.RegularExpressions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace EETLib.DB.Mongo;

public static class ConventionHelper
{
    class MyConvention : IMemberMapConvention
    {
        public string Name => nameof(MyConvention);

        public void Apply(BsonMemberMap memberMap)
        {
            var name = Regex.Replace(memberMap.MemberName, "([^A-Z])([A-Z])",
                match => $"{match.Groups[1].Value}_{match.Groups[2].Value}");
            memberMap.SetElementName(name.ToLower());
        }
    }

    private static bool _initialized = false;

    public static void Setup()
    {
        if (_initialized)
            return;

        var myConventions = new ConventionPack();
        myConventions.Add(new MyConvention());
        myConventions.Add(new IgnoreExtraElementsConvention(true));

        ConventionRegistry.Register(
            "My Custom Conventions",
            myConventions,
            t => true);

        BsonClassMap.RegisterClassMap<TranslationEntry>(cm =>
        {
            cm.AutoMap();
            cm.SetIdMember(null);
        });

        _initialized = true;
    }
}
