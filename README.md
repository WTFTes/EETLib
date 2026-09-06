# EETLib

A .NET library for reading, modifying, and converting translation databases produced by the **ESM/ESP Translator** tool (the French Bethesda-plugin translation utility for Morrowind/Oblivion/Skyrim-style `.esm`/`.esp` mods).

It gives you a strongly-typed way to load a translator XML database, walk/query its entries, patch specific fields, deduplicate translation records, and move data between XML, tab-separated CSV, and MongoDB.

## Features

- **XML database access** (`EETLib.XML`)
  - Load an ESM/ESP Translator XML file into an in-memory, indexed structure (`XmlDatabase`) for fast lookup by group/field/editor-id/original-text hash.
  - Stream-process a database file node-by-node without loading a full model (`XmlHelper.TraverseAndModifyXml` / `ModifyXml`), optionally removing entries via `XmlActionType`.
  - Find duplicate/near-duplicate entries with `XmlDatabase.TrySquash()`.
  - Look up translation candidates for a given source string via `LookupCandidate`, `LookupByEdId`, `GetCandidates`, or an arbitrary predicate (`Find`).
- **CSV/TSV import-export** (`EETLib.CSV`)
  - Read and write `TranslationEntry` records as tab-delimited files (`CsvDatabase.Read`/`Write`), including reading every file in a directory (`ReadAll`) and splitting a large file into fixed-size chunks (`SplitIntoChunks`).
- **MongoDB storage** (`EETLib.DB.Mongo`)
  - `MongoConnection` wraps a cached `MongoClient`/`IMongoDatabase` per connection URI.
  - `ConventionHelper` registers a snake_case field-naming convention and ignores extra/unmapped elements, so `TranslationEntry` documents map cleanly to Mongo collections.

## Project structure

```
EETLib/
├── XML/
│   ├── XmlDatabase.cs          # indexed in-memory view over a translator XML file
│   ├── XmlHelper.cs            # low-level XML load/traverse/modify/save
│   ├── XmlTranslationEntry.cs  # one <ITEM>-like node, mapped to/from XML fields
│   ├── XmlTranslationStatus.cs # translation status enum (Validated, Ignored, ...)
│   └── XmlActionType.cs        # callback result: keep or remove a node
├── DB/
│   ├── TranslationEntry.cs     # plain POCO used by CSV/Mongo storage
│   ├── TranslationStatus.cs    # string status constants (none/translated/ignored)
│   └── Mongo/
│       ├── MongoConnection.cs      # cached MongoClient/IMongoDatabase accessor
│       └── ConventionHelper.cs     # BSON naming convention setup
└── CSV/
    └── CsvDatabase.cs          # tab-delimited read/write/split helpers
```

## The XML database format

ESM/ESP Translator XML files store one element per translatable field, with tags in French:

| XML tag       | Meaning                          | Mapped property        |
|---------------|-----------------------------------|-------------------------|
| `GRUP`        | Record group (`DIAL`, `ACTI`, …) | `Group`                |
| `ID`          | Record ID (mostly present for `INFO`) | `Id`               |
| `EDID`        | Editor ID (untranslated)         | `EdId`                  |
| `CHAMP`       | Field name (`FNAM`, `RNAM`, …)   | `FieldName`             |
| `ORIGINAL`    | Source text                      | `Original`               |
| `TRADUIT`     | Translated text                  | `Translated`             |
| `PERSO`       | Character/speaker                | `Perso`                  |
| `INDEX`       | Internal index (e.g. guild ranks) | `Index`                 |
| `STATUS`      | Numeric translation status        | `Status`                 |
| `COMMENTAIRE` | Translator comment               | `Comment`                 |
| `ICON`        | Icon reference                   | `Icon`                    |
| `IDSTEXTE`    | Text id                          | `IdsTexte`                |

`XmlTranslationEntry.ApplyValuesBack()` writes edited property values back onto the original `XmlNode`s, so a database can be loaded, modified in place, and re-serialized without losing unrelated attributes/structure.

## Requirements

- .NET 10.0 SDK
- [CsvHelper](https://www.nuget.org/packages/CsvHelper) 33.1.0
- [MongoDB.Driver](https://www.nuget.org/packages/MongoDB.Driver) 3.9.0

## Building

```bash
dotnet build EETLib.csproj
```

## Usage

### Load a database and inspect entries

```csharp
using EETLib.XML;

var db = new XmlDatabase("Cell.xml");

foreach (var entry in db.Entries)
    Console.WriteLine($"[{entry.Group}/{entry.FieldName}] {entry.Original} -> {entry.Translated}");

// Find every prior translation of the same source string, regardless of location
var candidates = db.GetCandidates("Some source text");
```

### Stream-modify a database and remove unwanted nodes

```csharp
using EETLib.XML;

XmlHelper.ModifyXml("in.xml", "out.xml", node =>
{
    if (node.FieldName == "FNAM" && string.IsNullOrWhiteSpace(node.Translated))
        return XmlActionType.Remove;

    return XmlActionType.None;
});
```

### Find and report duplicate entries

```csharp
var db = new XmlDatabase("Cell.xml");
var duplicateGroups = db.TrySquash();
```

### Read/write tab-delimited translation dumps

```csharp
using EETLib.CSV;

var entries = CsvDatabase.Read("translations.csv");
CsvDatabase.Write("translations_out.csv", entries);
CsvDatabase.SplitIntoChunks("translations.csv", chunkSize: 1000);
```

### Store entries in MongoDB

```csharp
using EETLib.DB;
using EETLib.DB.Mongo;

var connection = new MongoConnection("mongodb://localhost:27017", "translations");
var collection = connection.GetCollection<TranslationEntry>("entries");

collection.InsertMany(entries);
```

## Status

Early-stage / actively evolving library extracted from a translation-workflow tool. APIs may change without notice.
