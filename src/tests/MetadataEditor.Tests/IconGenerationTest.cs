using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using Xunit;

namespace MetadataEditor.Tests;

public class IconGenerationTest
{
    [Fact]
    public void GenerateProperAppIcon()
    {
        // Path to src/MetadataEditor/Resources/app.ico
        string currentDir = AppDomain.CurrentDomain.BaseDirectory;
        string srcDir = Path.GetFullPath(Path.Combine(currentDir, @"..\..\..\..\..\"));
        string icoPath = Path.Combine(srcDir, @"MetadataEditor\Resources\app.ico");

        int[] sizes = new int[] { 256, 128, 64, 48, 32, 24, 16 };
        var bitmaps = new System.Collections.Generic.List<Bitmap>();

        foreach (int size in sizes)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.Clear(Color.Transparent);

                int pad = size >= 64 ? (int)(size * 0.05) : (size >= 32 ? 1 : 0);
                int rectSize = size - (2 * pad);
                var rect = new Rectangle(pad, pad, rectSize, rectSize);

                int cornerRadius = size switch
                {
                    >= 128 => (int)(size * 0.22),
                    >= 64 => (int)(size * 0.22),
                    >= 32 => 6,
                    >= 24 => 4,
                    _ => 3
                };

                // Draw Rounded Blue Background (gradient from #0284C7 to #1D4ED8)
                using (var path = CreateRoundedRectanglePath(rect, cornerRadius))
                {
                    using (var brush = new LinearGradientBrush(
                        rect,
                        Color.FromArgb(255, 2, 132, 199),
                        Color.FromArgb(255, 29, 78, 216),
                        LinearGradientMode.Vertical))
                    {
                        g.FillPath(brush, path);
                    }

                    // Border outline
                    float penWidth = size >= 64 ? Math.Max(1.5f, size * 0.02f) : 1f;
                    using (var pen = new Pen(Color.FromArgb(180, 96, 165, 250), penWidth))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                // Centered text with STRICT NoWrap flag
                using (var sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    sf.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.FitBlackBox;

                    using (var whiteBrush = new SolidBrush(Color.White))
                    using (var lightBlueBrush = new SolidBrush(Color.FromArgb(240, 248, 255)))
                    {
                        if (size == 16)
                        {
                            // 16x16 (Window Titlebar): Single-line ultra-sharp bold NFO perfectly centered
                            using var fontNfo = new Font("Arial", 6.0f, FontStyle.Bold, GraphicsUnit.Pixel);
                            g.DrawString("NFO", fontNfo, whiteBrush, new RectangleF(0, 0, 16, 16), sf);
                        }
                        else if (size == 24)
                        {
                            // 24x24: NFO + Edit
                            using var fontNfo = new Font("Segoe UI", 9.5f, FontStyle.Bold, GraphicsUnit.Pixel);
                            using var fontEdit = new Font("Segoe UI", 6.5f, FontStyle.Bold, GraphicsUnit.Pixel);
                            g.DrawString("NFO", fontNfo, whiteBrush, new RectangleF(0, 1, 24, 12), sf);
                            g.DrawString("Edit", fontEdit, lightBlueBrush, new RectangleF(0, 12, 24, 10), sf);
                        }
                        else if (size == 32)
                        {
                            // 32x32 (Windows Taskbar!):
                            // NFO centered in top half, Edit centered in bottom half
                            using var fontNfo = new Font("Segoe UI", 12.0f, FontStyle.Bold, GraphicsUnit.Pixel);
                            using var fontEdit = new Font("Segoe UI", 8.5f, FontStyle.Bold, GraphicsUnit.Pixel);
                            g.DrawString("NFO", fontNfo, whiteBrush, new RectangleF(0, 2, 32, 15), sf);
                            g.DrawString("Edit", fontEdit, lightBlueBrush, new RectangleF(0, 16, 32, 13), sf);
                        }
                        else if (size == 48)
                        {
                            // 48x48:
                            using var fontNfo = new Font("Segoe UI", 18.0f, FontStyle.Bold, GraphicsUnit.Pixel);
                            using var fontEdit = new Font("Segoe UI", 12.0f, FontStyle.Bold, GraphicsUnit.Pixel);
                            g.DrawString("NFO", fontNfo, whiteBrush, new RectangleF(0, 5, 48, 20), sf);
                            g.DrawString("Edit", fontEdit, lightBlueBrush, new RectangleF(0, 25, 48, 16), sf);
                        }
                        else
                        {
                            // 64, 128, 256:
                            float nfoSize = size * 0.34f;
                            float editSize = size * 0.20f;
                            using var fontNfo = new Font("Segoe UI", nfoSize, FontStyle.Bold, GraphicsUnit.Pixel);
                            using var fontEdit = new Font("Segoe UI", editSize, FontStyle.Bold, GraphicsUnit.Pixel);

                            float topY = size * 0.14f;
                            float topH = size * 0.40f;
                            float botY = size * 0.52f;
                            float botH = size * 0.32f;

                            g.DrawString("NFO", fontNfo, whiteBrush, new RectangleF(0, topY, size, topH), sf);
                            g.DrawString("Edit", fontEdit, lightBlueBrush, new RectangleF(0, botY, size, botH), sf);
                        }
                    }
                }
            }
            bitmaps.Add(bmp);
        }

        // Write Multi-Resolution Windows ICO
        WriteWindowsIcoFile(bitmaps, sizes, icoPath);
        Assert.True(File.Exists(icoPath));
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static void WriteWindowsIcoFile(System.Collections.Generic.List<Bitmap> bitmaps, int[] sizes, string outputPath)
    {
        using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);

        int count = bitmaps.Count;
        bw.Write((ushort)0); // reserved
        bw.Write((ushort)1); // type 1 = ICO
        bw.Write((ushort)count);

        var dataBuffers = new byte[count][];
        for (int i = 0; i < count; i++)
        {
            var bmp = bitmaps[i];
            int s = sizes[i];
            if (s >= 256)
            {
                using var ms = new MemoryStream();
                bmp.Save(ms, ImageFormat.Png);
                dataBuffers[i] = ms.ToArray();
            }
            else
            {
                using var ms = new MemoryStream();
                using var dibWriter = new BinaryWriter(ms);

                // BITMAPINFOHEADER (40 bytes)
                dibWriter.Write((uint)40);          // biSize
                dibWriter.Write((int)s);            // biWidth
                dibWriter.Write((int)(s * 2));      // biHeight (XOR mask + AND mask)
                dibWriter.Write((ushort)1);         // biPlanes
                dibWriter.Write((ushort)32);        // biBitCount (32bpp BGRA)
                dibWriter.Write((uint)0);           // biCompression (BI_RGB)
                dibWriter.Write((uint)(s * s * 4)); // biSizeImage
                dibWriter.Write((int)0);            // biXPelsPerMeter
                dibWriter.Write((int)0);            // biYPelsPerMeter
                dibWriter.Write((uint)0);           // biClrUsed
                dibWriter.Write((uint)0);           // biClrImportant

                // Bottom-up BGRA pixels for XOR mask
                for (int y = s - 1; y >= 0; y--)
                {
                    for (int x = 0; x < s; x++)
                    {
                        var c = bmp.GetPixel(x, y);
                        dibWriter.Write(c.B);
                        dibWriter.Write(c.G);
                        dibWriter.Write(c.R);
                        dibWriter.Write(c.A);
                    }
                }

                // AND mask (1 bit per pixel, padded to 32 bits per row)
                int andRowBytes = ((s + 31) / 32) * 4;
                byte[] andMask = new byte[andRowBytes * s];
                dibWriter.Write(andMask);

                dataBuffers[i] = ms.ToArray();
            }
        }

        uint offset = (uint)(6 + (16 * count));
        for (int i = 0; i < count; i++)
        {
            int s = sizes[i];
            bw.Write((byte)(s >= 256 ? 0 : s)); // Width
            bw.Write((byte)(s >= 256 ? 0 : s)); // Height
            bw.Write((byte)0); // Color palette
            bw.Write((byte)0); // Reserved
            bw.Write((ushort)1); // Planes
            bw.Write((ushort)32); // Bits per pixel
            bw.Write((uint)dataBuffers[i].Length); // Size
            bw.Write((uint)offset); // Offset
            offset += (uint)dataBuffers[i].Length;
        }

        for (int i = 0; i < count; i++)
        {
            bw.Write(dataBuffers[i]);
        }
    }
}
