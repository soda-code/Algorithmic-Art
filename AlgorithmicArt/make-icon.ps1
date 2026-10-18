Add-Type -AssemblyName System.Drawing

function New-RoundedPath {
    param(
        [single]$x, [single]$y, [single]$w, [single]$h, [single]$r,
        [int]$seg = 8
    )

    if ($r -lt 0.05) { $r = 0.05 }
    if ($r -gt $w / 2) { $r = $w / 2 }
    if ($r -gt $h / 2) { $r = $h / 2 }

    $pts = [System.Collections.Generic.List[System.Drawing.PointF]]::new()
    $corners = @(
        @{ cx = $x + $r;       cy = $y + $r;       a0 = 180 },
        @{ cx = $x + $w - $r;  cy = $y + $r;       a0 = 270 },
        @{ cx = $x + $w - $r;  cy = $y + $h - $r;  a0 = 0 },
        @{ cx = $x + $r;       cy = $y + $h - $r;  a0 = 90 }
    )

    foreach ($c in $corners) {
        for ($i = 0; $i -le $seg; $i++) {
            $ang = [Math]::PI * ($c.a0 + 90.0 * $i / $seg) / 180.0
            $px = [single]($c.cx + $r * [Math]::Cos($ang))
            $py = [single]($c.cy + $r * [Math]::Sin($ang))
            $pts.Add([System.Drawing.PointF]::new($px, $py))
        }
    }

    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $path.AddPolygon($pts.ToArray())
    return $path
}

$outPath = Join-Path $PSScriptRoot 'src\AlgorithmicArt\app.ico'
$sizes = @(16, 24, 32, 48, 64, 128, 256)
$entries = [System.Collections.ArrayList]::new()

foreach ($size in $sizes) {
    $s = [single]$size
    $bmp = [System.Drawing.Bitmap]::new($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.Clear([System.Drawing.Color]::Transparent)

    # 背景：蓝色渐变圆角方块
    $pad = [single]($s * 0.05)
    $side = [single]($s - 2.0 * $pad)
    $rect = [System.Drawing.RectangleF]::new($pad, $pad, $side, $side)
    $bgPath = New-RoundedPath -x $pad -y $pad -w $side -h $side -r ([single]($s * 0.24))

    $c1 = [System.Drawing.Color]::FromArgb(255, 59, 130, 246)
    $c2 = [System.Drawing.Color]::FromArgb(255, 29, 78, 216)
    $brush = [System.Drawing.Drawing2D.LinearGradientBrush]::new(
        $rect, $c1, $c2, [System.Drawing.Drawing2D.LinearGradientMode]::Vertical)
    $g.FillPath($brush, $bgPath)

    # 柱状图：五根柱，中间橙色高亮
    $heights = @(0.34, 0.52, 0.72, 0.46, 0.62)
    $barW = [single]($s * 0.108)
    $gap = [single]($s * 0.052)
    $totalW = [single](5.0 * $barW + 4.0 * $gap)
    $startX = [single](($s - $totalW) / 2.0)
    $baseY = [single]($s * 0.80)
    $maxH = [single]($s * 0.48)
    $radius = [single]($barW * 0.32)

    for ($i = 0; $i -lt 5; $i++) {
        $h = [single]($maxH * $heights[$i])
        $x = [single]($startX + $i * ($barW + $gap))
        $y = [single]($baseY - $h)

        if ($i -eq 2) {
            $col = [System.Drawing.Color]::FromArgb(255, 251, 146, 60)
        } else {
            $col = [System.Drawing.Color]::FromArgb(240, 255, 255, 255)
        }

        $barPath = New-RoundedPath -x $x -y $y -w $barW -h $h -r $radius -seg 3
        $bb = [System.Drawing.SolidBrush]::new($col)
        $g.FillPath($bb, $barPath)
        $bb.Dispose()
        $barPath.Dispose()
    }

    # 基线
    if ($size -ge 24) {
        $penW = [single]([Math]::Max(1.0, $s * 0.022))
        $linePen = [System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(150, 255, 255, 255), $penW)
        $g.DrawLine($linePen, [single]($startX - $s * 0.035), $baseY, [single]($startX + $totalW + $s * 0.035), $baseY)
        $linePen.Dispose()
    }

    $brush.Dispose()
    $bgPath.Dispose()
    $g.Dispose()

    $ms = [System.IO.MemoryStream]::new()
    $bmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
    [void]$entries.Add(@{ Size = $size; Data = $ms.ToArray() })
    $ms.Dispose()
    $bmp.Dispose()
}

$fs = [System.IO.File]::Create($outPath)
$bw = [System.IO.BinaryWriter]::new($fs)

$bw.Write([UInt16]0)
$bw.Write([UInt16]1)
$bw.Write([UInt16]$entries.Count)

$offset = 6 + 16 * $entries.Count
foreach ($e in $entries) {
    $sz = $e.Size
    if ($sz -ge 256) { $dim = [Byte]0 } else { $dim = [Byte]$sz }
    $bw.Write($dim)
    $bw.Write($dim)
    $bw.Write([Byte]0)
    $bw.Write([Byte]0)
    $bw.Write([UInt16]1)
    $bw.Write([UInt16]32)
    $bw.Write([UInt32]$e.Data.Length)
    $bw.Write([UInt32]$offset)
    $offset += $e.Data.Length
}

foreach ($e in $entries) {
    $data = [byte[]]$e.Data
    $bw.Write($data, 0, $data.Length)
}

$bw.Flush()
$bw.Close()
$fs.Close()

Write-Output "ICON_OK $outPath"
