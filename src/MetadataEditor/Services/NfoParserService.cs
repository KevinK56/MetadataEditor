using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using MetadataEditor.Models;

namespace MetadataEditor.Services;

public class NfoLoadResult
{
    public NfoMetadata Metadata { get; set; } = new();
    public string RawText { get; set; } = string.Empty;
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public bool HasBom { get; set; }
    public string EncodingName { get; set; } = "UTF-8";
    public bool IsXml { get; set; }
}

public static class NfoParserService
{
    public static NfoLoadResult LoadFromFile(string filePath)
    {
        var (encoding, hasBom, encodingName) = FileEncodingDetector.DetectEncoding(filePath);
        string rawText = File.ReadAllText(filePath, encoding);

        var result = new NfoLoadResult
        {
            RawText = rawText,
            Encoding = encoding,
            HasBom = hasBom,
            EncodingName = encodingName
        };

        // Try parsing XML
        if (TryExtractXml(rawText, out XDocument? doc) && doc?.Root != null)
        {
            result.IsXml = true;
            result.Metadata = ParseXmlToMetadata(doc, rawText);
        }
        else
        {
            // Non-XML or plain text / scene ASCII
            result.IsXml = false;
            result.Metadata = new NfoMetadata
            {
                FileType = NfoFileType.PlainText,
                RawText = rawText
            };
        }

        return result;
    }

    private static bool TryExtractXml(string text, out XDocument? doc)
    {
        doc = null;
        if (string.IsNullOrWhiteSpace(text)) return false;

        string trimmed = text.Trim();
        if (!trimmed.StartsWith("<") || !trimmed.EndsWith(">"))
        {
            // Some scene NFOs might have a small XML block inside, but if it doesn't look like XML, treat as text
            int xmlStart = trimmed.IndexOf("<");
            int xmlEnd = trimmed.LastIndexOf(">");
            if (xmlStart >= 0 && xmlEnd > xmlStart && (trimmed.StartsWith("<?xml") || trimmed.StartsWith("<movie") || trimmed.StartsWith("<tvshow") || trimmed.StartsWith("<episodedetails") || trimmed.StartsWith("<album") || trimmed.StartsWith("<artist")))
            {
                trimmed = trimmed.Substring(xmlStart, xmlEnd - xmlStart + 1);
            }
            else
            {
                return false;
            }
        }

        try
        {
            var settings = new XmlReaderSettings
            {
                CheckCharacters = false,
                DtdProcessing = DtdProcessing.Ignore,
                IgnoreWhitespace = false
            };

            using var stringReader = new StringReader(trimmed);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            doc = XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            return doc.Root != null;
        }
        catch
        {
            doc = null;
            return false;
        }
    }

    public static NfoMetadata ParseXmlToMetadata(XDocument doc, string rawText)
    {
        var root = doc.Root!;
        string rootName = root.Name.LocalName.ToLowerInvariant();
        var model = new NfoMetadata
        {
            RootElementName = root.Name.LocalName,
            RawText = rawText
        };

        switch (rootName)
        {
            case "movie":
                model.FileType = NfoFileType.Movie;
                break;
            case "tvshow":
                model.FileType = NfoFileType.TvShow;
                break;
            case "episodedetails":
            case "episode":
                model.FileType = NfoFileType.Episode;
                break;
            case "album":
                model.FileType = NfoFileType.MusicAlbum;
                break;
            case "artist":
                model.FileType = NfoFileType.MusicArtist;
                break;
            case "musicvideo":
                model.FileType = NfoFileType.MusicVideo;
                break;
            default:
                model.FileType = NfoFileType.GenericXml;
                break;
        }

        // Standard string fields
        model.Title = GetElementValue(root, "title", "name");
        model.OriginalTitle = GetElementValue(root, "originaltitle");
        model.SortTitle = GetElementValue(root, "sorttitle");
        model.Year = GetElementValue(root, "year");
        model.Premiered = GetElementValue(root, "premiered", "releasedate", "firstaired");
        model.Released = GetElementValue(root, "released", "releasedate");
        model.Plot = GetElementValue(root, "plot", "overview", "review", "biography", "desc");
        model.Outline = GetElementValue(root, "outline");
        model.Tagline = GetElementValue(root, "tagline");
        model.Runtime = GetElementValue(root, "runtime", "duration");
        model.Rating = GetElementValue(root, "rating", "userrating");
        model.Votes = GetElementValue(root, "votes");
        model.Mpaa = GetElementValue(root, "mpaa", "certification");
        model.Trailer = GetElementValue(root, "trailer");
        model.Id = GetElementValue(root, "id");
        model.ImdbId = GetElementValue(root, "imdbid", "imdb_id");
        model.TmdbId = GetElementValue(root, "tmdbid", "tmdb_id");
        model.TvdbId = GetElementValue(root, "tvdbid", "tvdb_id");
        model.Status = GetElementValue(root, "status", "showstatus");
        model.Season = GetElementValue(root, "season");
        model.Episode = GetElementValue(root, "episode");
        model.ShowTitle = GetElementValue(root, "showtitle", "tvshowtitle");
        model.Aired = GetElementValue(root, "aired", "airdate");
        model.Top250 = GetElementValue(root, "top250");

        // Set
        var setElement = root.Element("set");
        if (setElement != null)
        {
            model.SetName = setElement.Element("name")?.Value ?? setElement.Value;
            model.SetOverview = setElement.Element("overview")?.Value ?? string.Empty;
        }

        // Music specific
        model.Artist = GetElementValue(root, "artist", "artistdesc");
        model.Album = GetElementValue(root, "album");
        model.Label = GetElementValue(root, "label", "recordlabel");
        model.AlbumType = GetElementValue(root, "type", "albumtype");
        model.Review = GetElementValue(root, "review");
        model.Biography = GetElementValue(root, "biography");
        model.ArtistType = GetElementValue(root, "artisttype", "type");
        model.Gender = GetElementValue(root, "gender");
        model.Born = GetElementValue(root, "born");
        model.Formed = GetElementValue(root, "formed");
        model.Disbanded = GetElementValue(root, "disbanded");
        model.Died = GetElementValue(root, "died");

        // Collections: genres, studios, countries, directors, writers
        ExtractStringList(root, "genre", model.Genres);
        ExtractStringList(root, "studio", model.Studios);
        ExtractStringList(root, "country", model.Countries);
        ExtractStringList(root, "director", model.Directors);
        ExtractStringList(root, "credits", model.Writers);
        ExtractStringList(root, "writer", model.Writers);

        // Actors
        foreach (var actorElem in root.Elements("actor"))
        {
            string name = actorElem.Element("name")?.Value ?? actorElem.Value;
            string role = actorElem.Element("role")?.Value ?? string.Empty;
            string thumb = actorElem.Element("thumb")?.Value ?? string.Empty;
            int order = 0;
            if (int.TryParse(actorElem.Element("order")?.Value, out int ord))
            {
                order = ord;
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                model.Actors.Add(new ActorItem
                {
                    Name = name.Trim(),
                    Role = role.Trim(),
                    Thumb = thumb.Trim(),
                    Order = order
                });
            }
        }

        // Tracks (for music album)
        foreach (var trackElem in root.Elements("track"))
        {
            string pos = trackElem.Element("position")?.Value ?? trackElem.Attribute("num")?.Value ?? $"{model.Tracks.Count + 1}";
            string title = trackElem.Element("title")?.Value ?? trackElem.Value;
            string duration = trackElem.Element("duration")?.Value ?? string.Empty;

            model.Tracks.Add(new TrackItem
            {
                Position = pos.Trim(),
                Title = title.Trim(),
                Duration = duration.Trim()
            });
        }

        // Identify known elements so anything else goes into ExtraNodes
        var knownElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "title", "name", "originaltitle", "sorttitle", "year", "premiered", "released", "releasedate", "firstaired",
            "plot", "overview", "outline", "tagline", "runtime", "duration", "rating", "userrating", "votes",
            "mpaa", "certification", "trailer", "id", "imdbid", "imdb_id", "tmdbid", "tmdb_id", "tvdbid", "tvdb_id",
            "set", "top250", "status", "showstatus", "season", "episode", "showtitle", "tvshowtitle", "aired", "airdate",
            "artist", "artistdesc", "album", "label", "recordlabel", "type", "albumtype", "review", "biography",
            "artisttype", "gender", "born", "formed", "disbanded", "died",
            "genre", "studio", "country", "director", "credits", "writer", "actor", "track"
        };

        foreach (var elem in root.Elements())
        {
            string local = elem.Name.LocalName;
            if (!knownElements.Contains(local))
            {
                bool isBlock = elem.HasElements || elem.HasAttributes;
                string val = isBlock ? elem.ToString(SaveOptions.DisableFormatting) : elem.Value;
                model.ExtraNodes.Add(new XmlExtraItem
                {
                    TagName = local,
                    TagValue = val,
                    IsXmlBlock = isBlock
                });
            }
        }

        return model;
    }

    private static string GetElementValue(XElement root, params string[] elementNames)
    {
        foreach (var name in elementNames)
        {
            var elem = root.Element(name);
            if (elem != null && !string.IsNullOrWhiteSpace(elem.Value))
            {
                return elem.Value.Trim();
            }
        }
        return string.Empty;
    }

    private static void ExtractStringList(XElement root, string elementName, ObservableCollection<string> list)
    {
        foreach (var elem in root.Elements(elementName))
        {
            string val = elem.Value?.Trim() ?? string.Empty;
            if (!string.IsNullOrEmpty(val))
            {
                // If value contains "/" or "," separated items like "Action / Adventure"
                if (val.Contains('/') && !val.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = val.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var part in parts)
                    {
                        if (!list.Contains(part, StringComparer.OrdinalIgnoreCase))
                            list.Add(part);
                    }
                }
                else
                {
                    if (!list.Contains(val, StringComparer.OrdinalIgnoreCase))
                        list.Add(val);
                }
            }
        }
    }

    public static string SerializeToXml(NfoMetadata model)
    {
        string rootTag = model.FileType switch
        {
            NfoFileType.Movie => "movie",
            NfoFileType.TvShow => "tvshow",
            NfoFileType.Episode => "episodedetails",
            NfoFileType.MusicAlbum => "album",
            NfoFileType.MusicArtist => "artist",
            NfoFileType.MusicVideo => "musicvideo",
            _ => string.IsNullOrWhiteSpace(model.RootElementName) ? "metadata" : model.RootElementName
        };

        var root = new XElement(rootTag);

        // Core elements
        AddIfNotEmpty(root, "title", model.Title);
        AddIfNotEmpty(root, "originaltitle", model.OriginalTitle);
        AddIfNotEmpty(root, "sorttitle", model.SortTitle);

        if (model.FileType == NfoFileType.TvShow || model.FileType == NfoFileType.Episode)
        {
            AddIfNotEmpty(root, "showtitle", model.ShowTitle);
            AddIfNotEmpty(root, "season", model.Season);
            AddIfNotEmpty(root, "episode", model.Episode);
            AddIfNotEmpty(root, "status", model.Status);
            AddIfNotEmpty(root, "aired", model.Aired);
        }

        AddIfNotEmpty(root, "rating", model.Rating);
        AddIfNotEmpty(root, "votes", model.Votes);
        AddIfNotEmpty(root, "top250", model.Top250);
        AddIfNotEmpty(root, "year", model.Year);
        AddIfNotEmpty(root, "premiered", model.Premiered);
        AddIfNotEmpty(root, "released", model.Released);
        AddIfNotEmpty(root, "plot", model.Plot);
        AddIfNotEmpty(root, "outline", model.Outline);
        AddIfNotEmpty(root, "tagline", model.Tagline);
        AddIfNotEmpty(root, "runtime", model.Runtime);
        AddIfNotEmpty(root, "mpaa", model.Mpaa);

        // External IDs
        AddIfNotEmpty(root, "id", model.Id);
        AddIfNotEmpty(root, "imdbid", model.ImdbId);
        AddIfNotEmpty(root, "tmdbid", model.TmdbId);
        AddIfNotEmpty(root, "tvdbid", model.TvdbId);
        AddIfNotEmpty(root, "trailer", model.Trailer);

        // Movie Set
        if (!string.IsNullOrWhiteSpace(model.SetName))
        {
            var setElem = new XElement("set", new XElement("name", model.SetName));
            if (!string.IsNullOrWhiteSpace(model.SetOverview))
            {
                setElem.Add(new XElement("overview", model.SetOverview));
            }
            root.Add(setElem);
        }

        // Music fields
        if (model.FileType == NfoFileType.MusicAlbum || model.FileType == NfoFileType.MusicArtist || model.FileType == NfoFileType.MusicVideo)
        {
            AddIfNotEmpty(root, "artist", model.Artist);
            AddIfNotEmpty(root, "album", model.Album);
            AddIfNotEmpty(root, "label", model.Label);
            AddIfNotEmpty(root, "type", model.AlbumType);
            AddIfNotEmpty(root, "review", model.Review);
            AddIfNotEmpty(root, "biography", model.Biography);
            AddIfNotEmpty(root, "artisttype", model.ArtistType);
            AddIfNotEmpty(root, "gender", model.Gender);
            AddIfNotEmpty(root, "born", model.Born);
            AddIfNotEmpty(root, "formed", model.Formed);
            AddIfNotEmpty(root, "disbanded", model.Disbanded);
            AddIfNotEmpty(root, "died", model.Died);

            foreach (var trk in model.Tracks)
            {
                if (!string.IsNullOrWhiteSpace(trk.Title))
                {
                    root.Add(new XElement("track",
                        new XElement("position", trk.Position),
                        new XElement("title", trk.Title),
                        new XElement("duration", trk.Duration)
                    ));
                }
            }
        }

        // Lists
        foreach (var g in model.Genres) AddIfNotEmpty(root, "genre", g);
        foreach (var s in model.Studios) AddIfNotEmpty(root, "studio", s);
        foreach (var c in model.Countries) AddIfNotEmpty(root, "country", c);
        foreach (var d in model.Directors) AddIfNotEmpty(root, "director", d);
        foreach (var w in model.Writers) AddIfNotEmpty(root, "credits", w);

        // Actors
        foreach (var actor in model.Actors)
        {
            if (!string.IsNullOrWhiteSpace(actor.Name))
            {
                var actorElem = new XElement("actor",
                    new XElement("name", actor.Name),
                    new XElement("role", actor.Role),
                    new XElement("order", actor.Order)
                );
                if (!string.IsNullOrWhiteSpace(actor.Thumb))
                {
                    actorElem.Add(new XElement("thumb", actor.Thumb));
                }
                root.Add(actorElem);
            }
        }

        // Extra Nodes
        foreach (var extra in model.ExtraNodes)
        {
            if (!string.IsNullOrWhiteSpace(extra.TagName))
            {
                if (extra.IsXmlBlock)
                {
                    try
                    {
                        var parsed = XElement.Parse(extra.TagValue);
                        root.Add(parsed);
                        continue;
                    }
                    catch
                    {
                        // Fallback to text node
                    }
                }
                root.Add(new XElement(extra.TagName, extra.TagValue));
            }
        }

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            root
        );

        var sb = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\r\n",
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8
        };

        using (var writer = XmlWriter.Create(sb, settings))
        {
            doc.Save(writer);
        }

        return sb.ToString();
    }

    private static void AddIfNotEmpty(XElement parent, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parent.Add(new XElement(name, value.Trim()));
        }
    }

    public static void SaveToFile(string filePath, string content, Encoding encoding, bool hasBom)
    {
        var targetEncoding = hasBom ? (encoding is UTF8Encoding ? new UTF8Encoding(true) : encoding) : (encoding is UTF8Encoding ? new UTF8Encoding(false) : encoding);
        File.WriteAllText(filePath, content, targetEncoding);
    }
}
