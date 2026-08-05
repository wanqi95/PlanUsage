// 生成启动器/任务栏图标：白底圆角 + 黑字 "PU"，输出多尺寸 .ico 与 PNG。
// 编译：csc /nologo /out:icon-gen.exe make_icon.cs /r:System.Drawing.dll
// 运行：icon-gen.exe launcher\icon.ico launcher\icon-preview.png build\icon.ico build\icon.png
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

static class IconGen
{
    static readonly int[] SIZES = { 16, 24, 32, 48, 64, 128, 256 };

    static void Main(string[] args)
    {
        string icoPath = args.Length > 0 ? args[0] : Path.Combine("launcher", "icon.ico");
        string pngPath = args.Length > 1 ? args[1] : Path.Combine("launcher", "icon-preview.png");
        string buildIcoPath = args.Length > 2 ? args[2] : Path.Combine("build", "icon.ico");
        string buildPngPath = args.Length > 3 ? args[3] : Path.Combine("build", "icon.png");

        var images = new Bitmap[SIZES.Length];
        for (int i = 0; i < SIZES.Length; i++)
        {
            images[i] = MakeIcon(SIZES[i]);
        }

        WriteIco(icoPath, images);
        images[SIZES.Length - 1].Save(pngPath, ImageFormat.Png);
        WriteIco(buildIcoPath, images);

        string buildDir = Path.GetDirectoryName(buildPngPath);
        if (!string.IsNullOrEmpty(buildDir)) Directory.CreateDirectory(buildDir);
        using (var big = MakeIcon(512)) big.Save(buildPngPath, ImageFormat.Png);

        foreach (var b in images) b.Dispose();
        Console.WriteLine("icons written: " + icoPath + " / " + buildIcoPath + " / " + buildPngPath);
    }

    // 白底圆角方 + 黑色 "PU"
    static Bitmap MakeIcon(int s)
    {
        var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.Transparent);
            int radius = Math.Max(2, s / 5);
            using (var gp = RoundedRect(0, 0, s, s, radius))
            {
                g.FillPath(Brushes.White, gp);
            }
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            float fontPx = Math.Max(7f, s * 0.52f);
            using (var font = new Font("Segoe UI", fontPx, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString("PU", font, Brushes.Black, new RectangleF(0, 0, s, s), sf);
            }
        }
        return bmp;
    }

    static GraphicsPath RoundedRect(int x, int y, int w, int h, int r)
    {
        r = Math.Min(r, Math.Min(w, h) / 2);
        var p = new GraphicsPath();
        p.AddArc(x, y, r * 2, r * 2, 180, 90);
        p.AddArc(x + w - r * 2, y, r * 2, r * 2, 270, 90);
        p.AddArc(x + w - r * 2, y + h - r * 2, r * 2, r * 2, 0, 90);
        p.AddArc(x, y + h - r * 2, r * 2, r * 2, 90, 90);
        p.CloseFigure();
        return p;
    }

    static void WriteIco(string path, Bitmap[] images)
    {
        string dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        using (var fs = File.Create(path))
        using (var bw = new BinaryWriter(fs))
        {
            bw.Write((ushort)0);
            bw.Write((ushort)1);
            bw.Write((ushort)SIZES.Length);
            int offset = 6 + 16 * SIZES.Length;
            for (int i = 0; i < SIZES.Length; i++)
            {
                byte[] data = ToIcoImage(images[i]);
                byte dim = (byte)(SIZES[i] >= 256 ? 0 : SIZES[i]);
                bw.Write(dim);
                bw.Write(dim);
                bw.Write((byte)0);
                bw.Write((byte)0);
                bw.Write((ushort)1);
                bw.Write((ushort)32);
                bw.Write(data.Length);
                bw.Write(offset);
                offset += data.Length;
            }
            for (int i = 0; i < SIZES.Length; i++) bw.Write(ToIcoImage(images[i]));
        }
    }

    static byte[] ToIcoImage(Bitmap bmp)
    {
        int w = bmp.Width;
        int h = bmp.Height;
        var rect = new Rectangle(0, 0, w, h);
        var data = bmp.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try
        {
            byte[] row = new byte[data.Stride];
            int maskStride = ((w + 31) / 32) * 4;
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(40); // BITMAPINFOHEADER
                bw.Write(w);
                bw.Write(h * 2);
                bw.Write((ushort)1);
                bw.Write((ushort)32);
                bw.Write(0);
                bw.Write(w * h * 4);
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);
                bw.Write(0);
                for (int y = h - 1; y >= 0; y--)
                {
                    Marshal.Copy(data.Scan0 + y * data.Stride, row, 0, data.Stride);
                    bw.Write(row, 0, data.Stride);
                }
                bw.Write(new byte[maskStride * h]);
                return ms.ToArray();
            }
        }
        finally
        {
            bmp.UnlockBits(data);
        }
    }
}
