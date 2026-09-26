Add-Type -AssemblyName System.Drawing

$sizes = @(256, 128, 64, 48, 32, 16)
$images = @()

foreach ($size in $sizes) {
    $bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    $g.Clear([System.Drawing.Color]::Transparent)

    $pad = [math]::Max(1, [int]($size * 0.05))
    $rect = New-Object System.Drawing.Rectangle($pad, $pad, $size - 2*$pad, $size - 2*$pad)
    
    # Rounded background
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $radius = [math]::Max(2, [int]($size * 0.20))
    $d = $radius * 2
    $path.AddArc($rect.X, $rect.Y, $d, $d, 180, 90)
    $path.AddArc($rect.Right - $d, $rect.Y, $d, $d, 270, 90)
    $path.AddArc($rect.Right - $d, $rect.Bottom - $d, $d, $d, 0, 90)
    $path.AddArc($rect.X, $rect.Bottom - $d, $d, $d, 90, 90)
    $path.CloseFigure()

    # Dark gradient background
    $brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $rect,
        [System.Drawing.Color]::FromArgb(255, 30, 41, 59),
        [System.Drawing.Color]::FromArgb(255, 15, 23, 42),
        [System.Drawing.Drawing2D.LinearGradientMode]::ForwardDiagonal
    )
    $g.FillPath($brush, $path)

    # Vibrant border
    $pen = New-Object System.Drawing.Pen([System.Drawing.Color]::FromArgb(220, 56, 189, 248), [math]::Max(1.0, [float]($size * 0.04)))
    $g.DrawPath($pen, $path)

    # Accent film reel / text
    $textRect = New-Object System.Drawing.RectangleF($pad, $pad + $size * 0.18, $size - 2*$pad, $size * 0.6)
    $fontFamily = New-Object System.Drawing.FontFamily('Arial')
    $fontSize = [float]([math]::Max(6.0, $size * 0.28))
    $font = New-Object System.Drawing.Font($fontFamily, $fontSize, [System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        $rect,
        [System.Drawing.Color]::FromArgb(255, 56, 189, 248),
        [System.Drawing.Color]::FromArgb(255, 168, 85, 247),
        [System.Drawing.Drawing2D.LinearGradientMode]::Horizontal
    )
    $sf = New-Object System.Drawing.StringFormat
    $sf.Alignment = [System.Drawing.StringAlignment]::Center
    $sf.LineAlignment = [System.Drawing.StringAlignment]::Center

    $g.DrawString('NFO', $font, $textBrush, $textRect, $sf)

    $images += $bmp
}

$ms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($ms)
$bw.Write([uint16]0) # Reserved
$bw.Write([uint16]1) # Type: 1 = ICO
$bw.Write([uint16]$images.Count) # Count

$offset = 6 + (16 * $images.Count)
$pngStreams = @()
foreach ($img in $images) {
    $pms = New-Object System.IO.MemoryStream
    $img.Save($pms, [System.Drawing.Imaging.ImageFormat]::Png)
    $pngStreams += $pms
}

for ($i = 0; $i -lt $images.Count; $i++) {
    $w = if ($sizes[$i] -ge 256) { 0 } else { [byte]$sizes[$i] }
    $h = if ($sizes[$i] -ge 256) { 0 } else { [byte]$sizes[$i] }
    $bw.Write([byte]$w)
    $bw.Write([byte]$h)
    $bw.Write([byte]0) # Color palette
    $bw.Write([byte]0) # Reserved
    $bw.Write([uint16]1) # Color planes
    $bw.Write([uint16]32) # Bits per pixel
    $bw.Write([uint32]$pngStreams[$i].Length) # Size of image data
    $bw.Write([uint32]$offset) # Offset
    $offset += $pngStreams[$i].Length
}

for ($i = 0; $i -lt $images.Count; $i++) {
    $bw.Write($pngStreams[$i].ToArray())
}

[System.IO.File]::WriteAllBytes('Resources\app.ico', $ms.ToArray())
Write-Host "app.ico generated successfully!"
