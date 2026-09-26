using System.IO;
using System.Linq;
using System.Text;
using MetadataEditor.Models;
using MetadataEditor.Services;
using Xunit;

namespace MetadataEditor.Tests;

public class NfoParsingTests
{
    private readonly string _samplesDir;

    public NfoParsingTests()
    {
        string current = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(current) && !Directory.Exists(Path.Combine(current, "Samples")))
        {
            string? parent = Path.GetDirectoryName(current);
            if (parent == null || parent == current) break;
            current = parent;
        }
        _samplesDir = Path.Combine(current, "Samples");
    }

    [Fact]
    public void TestMovieNfoParsingAndSerialization()
    {
        string moviePath = Path.Combine(_samplesDir, "sample_movie.nfo");
        Assert.True(File.Exists(moviePath), $"Sample movie file not found at: {moviePath}");

        var result = NfoParserService.LoadFromFile(moviePath);
        Assert.True(result.IsXml);
        Assert.Equal(NfoFileType.Movie, result.Metadata.FileType);
        Assert.Equal("Inception", result.Metadata.Title);
        Assert.Equal("2010", result.Metadata.Year);
        Assert.Equal("8.8", result.Metadata.Rating);
        Assert.Equal("tt1375666", result.Metadata.ImdbId);
        Assert.Equal("Christopher Nolan", result.Metadata.Directors.FirstOrDefault());
        Assert.Contains("Sci-Fi", result.Metadata.Genres);
        Assert.Equal(4, result.Metadata.Actors.Count);
        Assert.Equal("Leonardo DiCaprio", result.Metadata.Actors[0].Name);
        Assert.Equal("Dom Cobb", result.Metadata.Actors[0].Role);

        // Verify extra XML tags (like fileinfo/streamdetails) are preserved
        Assert.Contains(result.Metadata.ExtraNodes, x => x.TagName.Equals("fileinfo", StringComparison.OrdinalIgnoreCase));

        // Test serialization
        string serialized = NfoParserService.SerializeToXml(result.Metadata);
        Assert.Contains("<title>Inception</title>", serialized);
        Assert.Contains("<genre>Sci-Fi</genre>", serialized);
        Assert.Contains("<name>Leonardo DiCaprio</name>", serialized);
        Assert.Contains("<fileinfo>", serialized);
        Assert.Contains("<streamdetails>", serialized);
    }

    [Fact]
    public void TestTvShowNfoParsing()
    {
        string tvPath = Path.Combine(_samplesDir, "sample_tvshow.nfo");
        var result = NfoParserService.LoadFromFile(tvPath);

        Assert.True(result.IsXml);
        Assert.Equal(NfoFileType.TvShow, result.Metadata.FileType);
        Assert.Equal("Breaking Bad", result.Metadata.Title);
        Assert.Equal("2008", result.Metadata.Year);
        Assert.Equal("Ended", result.Metadata.Status);
        Assert.Equal("tt0903747", result.Metadata.ImdbId);
        Assert.Equal(2, result.Metadata.Actors.Count);
        Assert.Equal("Bryan Cranston", result.Metadata.Actors[0].Name);
    }

    [Fact]
    public void TestEpisodeNfoParsing()
    {
        string epPath = Path.Combine(_samplesDir, "sample_episode.nfo");
        var result = NfoParserService.LoadFromFile(epPath);

        Assert.True(result.IsXml);
        Assert.Equal(NfoFileType.Episode, result.Metadata.FileType);
        Assert.Equal("Ozymandias", result.Metadata.Title);
        Assert.Equal("Breaking Bad", result.Metadata.ShowTitle);
        Assert.Equal("5", result.Metadata.Season);
        Assert.Equal("14", result.Metadata.Episode);
        Assert.Equal("10.0", result.Metadata.Rating);
    }

    [Fact]
    public void TestMusicAlbumNfoParsing()
    {
        string albumPath = Path.Combine(_samplesDir, "sample_album.nfo");
        var result = NfoParserService.LoadFromFile(albumPath);

        Assert.True(result.IsXml);
        Assert.Equal(NfoFileType.MusicAlbum, result.Metadata.FileType);
        Assert.Equal("The Dark Side of the Moon", result.Metadata.Title);
        Assert.Equal("Pink Floyd", result.Metadata.Artist);
        Assert.Equal("1973", result.Metadata.Year);
        Assert.Equal(6, result.Metadata.Tracks.Count);
        Assert.Equal("Money", result.Metadata.Tracks[5].Title);
    }

    [Fact]
    public void TestSceneAsciiNfoParsing()
    {
        string asciiPath = Path.Combine(_samplesDir, "sample_scene_ascii.nfo");
        var result = NfoParserService.LoadFromFile(asciiPath);

        // Should NOT crash or fail, but cleanly load as PlainText
        Assert.False(result.IsXml);
        Assert.Equal(NfoFileType.PlainText, result.Metadata.FileType);
        Assert.NotEmpty(result.RawText);
        Assert.Contains("SCENE RELEASE INFORMATION FILE", result.RawText);
        Assert.Contains("Sample Classic Film Remaster", result.RawText);
    }

    [Fact]
    public void TestEncodingDetection()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            // UTF8 with BOM
            File.WriteAllText(tempFile, "<movie><title>Test</title></movie>", new UTF8Encoding(true));
            var (encBom, hasBom, _) = FileEncodingDetector.DetectEncoding(tempFile);
            Assert.True(hasBom);

            // UTF8 without BOM
            File.WriteAllText(tempFile, "<movie><title>Test</title></movie>", new UTF8Encoding(false));
            var (encNoBom, hasNoBom, _) = FileEncodingDetector.DetectEncoding(tempFile);
            Assert.False(hasNoBom);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void TestModifyAndSaveRoundtrip()
    {
        string tempFile = Path.GetTempFileName() + ".nfo";
        try
        {
            var meta = new NfoMetadata
            {
                FileType = NfoFileType.Movie,
                Title = "The Matrix",
                Year = "1999",
                Rating = "8.7",
                Plot = "A computer hacker learns from mysterious rebels about the true nature of his reality."
            };
            meta.Genres.Add("Action");
            meta.Genres.Add("Sci-Fi");
            meta.Actors.Add(new ActorItem { Name = "Keanu Reeves", Role = "Neo", Order = 0 });
            meta.Actors.Add(new ActorItem { Name = "Laurence Fishburne", Role = "Morpheus", Order = 1 });
            meta.ExtraNodes.Add(new XmlExtraItem { TagName = "customid", TagValue = "MATRIX-1999" });

            string xml = NfoParserService.SerializeToXml(meta);
            NfoParserService.SaveToFile(tempFile, xml, Encoding.UTF8, false);

            var reloaded = NfoParserService.LoadFromFile(tempFile);
            Assert.Equal("The Matrix", reloaded.Metadata.Title);
            Assert.Equal("1999", reloaded.Metadata.Year);
            Assert.Equal("8.7", reloaded.Metadata.Rating);
            Assert.Equal(2, reloaded.Metadata.Genres.Count);
            Assert.Equal(2, reloaded.Metadata.Actors.Count);
            Assert.Equal("Keanu Reeves", reloaded.Metadata.Actors[0].Name);
            Assert.Contains(reloaded.Metadata.ExtraNodes, x => x.TagName == "customid" && x.TagValue == "MATRIX-1999");
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void TestConvertPlainTextToMovie()
    {
        string text = "A scene release notes text file...";
        var meta = new NfoMetadata
        {
            FileType = NfoFileType.PlainText,
            RawText = text
        };

        // User changes type to Movie
        meta.FileType = NfoFileType.Movie;
        meta.Title = "Converted Movie";
        meta.Year = "2024";

        string xml = NfoParserService.SerializeToXml(meta);
        Assert.Contains("<movie>", xml);
        Assert.Contains("<title>Converted Movie</title>", xml);
        Assert.Contains("<year>2024</year>", xml);
    }
}

