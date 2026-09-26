using System.IO;
using System.Text;

namespace MetadataEditor.Services;

public class EncodingInfoItem
{
    public string DisplayName { get; set; } = string.Empty;
    public Encoding Encoding { get; set; } = Encoding.UTF8;
    public bool HasBom { get; set; }
}

public static class FileEncodingDetector
{
    private static readonly Encoding Cp437;

    static FileEncodingDetector()
    {
        try
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Cp437 = Encoding.GetEncoding(437);
        }
        catch
        {
            Cp437 = Encoding.Latin1;
        }
    }

    public static Encoding GetCp437() => Cp437;

    public static (Encoding encoding, bool hasBom, string name) DetectEncoding(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        return DetectEncoding(fs);
    }

    public static (Encoding encoding, bool hasBom, string name) DetectEncoding(Stream stream)
    {
        long initialPos = stream.Position;
        byte[] bom = new byte[4];
        int read = stream.Read(bom, 0, 4);

        // Check BOMs
        if (read >= 3 && bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
        {
            stream.Position = initialPos;
            return (new UTF8Encoding(true), true, "UTF-8 BOM");
        }
        if (read >= 2 && bom[0] == 0xFF && bom[1] == 0xFE)
        {
            stream.Position = initialPos;
            return (Encoding.Unicode, true, "UTF-16 LE");
        }
        if (read >= 2 && bom[0] == 0xFE && bom[1] == 0xFF)
        {
            stream.Position = initialPos;
            return (Encoding.BigEndianUnicode, true, "UTF-16 BE");
        }

        // Heuristic analysis on content
        stream.Position = 0;
        int maxSample = Math.Min(65536, (int)stream.Length);
        byte[] sample = new byte[maxSample];
        int sampleRead = stream.Read(sample, 0, maxSample);
        stream.Position = initialPos;

        if (sampleRead == 0)
        {
            return (new UTF8Encoding(false), false, "UTF-8");
        }

        // Check for invalid UTF-8 sequences and high-ASCII box-drawing characters
        bool isValidUtf8 = true;
        bool hasHighAscii = false;
        int i = 0;

        while (i < sampleRead)
        {
            byte b = sample[i];
            if (b > 127)
            {
                hasHighAscii = true;
                if ((b & 0xE0) == 0xC0) // 2-byte sequence
                {
                    if (i + 1 >= sampleRead || (sample[i + 1] & 0xC0) != 0x80)
                    {
                        isValidUtf8 = false;
                        break;
                    }
                    i += 2;
                }
                else if ((b & 0xF0) == 0xE0) // 3-byte sequence
                {
                    if (i + 2 >= sampleRead || (sample[i + 1] & 0xC0) != 0x80 || (sample[i + 2] & 0xC0) != 0x80)
                    {
                        isValidUtf8 = false;
                        break;
                    }
                    i += 3;
                }
                else if ((b & 0xF8) == 0xF0) // 4-byte sequence
                {
                    if (i + 3 >= sampleRead || (sample[i + 1] & 0xC0) != 0x80 || (sample[i + 2] & 0xC0) != 0x80 || (sample[i + 3] & 0xC0) != 0x80)
                    {
                        isValidUtf8 = false;
                        break;
                    }
                    i += 4;
                }
                else
                {
                    isValidUtf8 = false;
                    break;
                }
            }
            else
            {
                i++;
            }
        }

        if (isValidUtf8)
        {
            return (new UTF8Encoding(false), false, "UTF-8");
        }

        // Invalid UTF-8 bytes with high ASCII characters: this is standard CP437 ASCII Art or Windows-1252
        if (hasHighAscii)
        {
            return (Cp437, false, "CP437 (DOS / Scene ASCII)");
        }

        return (Encoding.Default, false, "ANSI / Windows-1252");
    }

    public static List<EncodingInfoItem> GetSupportedEncodings()
    {
        return new List<EncodingInfoItem>
        {
            new() { DisplayName = "UTF-8 (Standard)", Encoding = new UTF8Encoding(false), HasBom = false },
            new() { DisplayName = "UTF-8 with BOM", Encoding = new UTF8Encoding(true), HasBom = true },
            new() { DisplayName = "CP437 (DOS / Scene ASCII)", Encoding = Cp437, HasBom = false },
            new() { DisplayName = "Windows-1252 (ANSI)", Encoding = Encoding.GetEncoding(1252), HasBom = false },
            new() { DisplayName = "UTF-16 LE (Unicode)", Encoding = Encoding.Unicode, HasBom = true },
        };
    }
}
