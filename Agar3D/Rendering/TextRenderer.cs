using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SD = System.Drawing;

namespace Agar3D.Rendering
{
    /// <summary>
    /// Zeichnet Text, indem er mit System.Drawing (GDI+) in eine Textur gerendert wird.
    /// Dadurch braucht das Projekt keine SpriteFont-Dateien / Content-Pipeline,
    /// und Umlaute funktionieren automatisch. Die Texturen werden zwischengespeichert.
    /// </summary>
    public sealed class TextRenderer : IDisposable
    {
        private const string FontName = "Segoe UI";
        private const int MaxCachedTextures = 300;

        private readonly GraphicsDevice _device;
        private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();

        public TextRenderer(GraphicsDevice device)
        {
            _device = device;
        }

        /// <summary>Einmal pro Frame VOR dem Zeichnen aufrufen: räumt den Cache auf, falls er zu groß wurde.</summary>
        public void BeginFrame()
        {
            if (_cache.Count > MaxCachedTextures)
            {
                foreach (Texture2D t in _cache.Values)
                {
                    t.Dispose();
                }
                _cache.Clear();
            }
        }

        public Texture2D Get(string text, float pixelSize, bool bold)
        {
            string key = pixelSize.ToString("0.#") + "|" + (bold ? "b" : "r") + "|" + text;
            Texture2D tex;
            if (!_cache.TryGetValue(key, out tex))
            {
                tex = Render(text, pixelSize, bold);
                _cache[key] = tex;
            }
            return tex;
        }

        /// <summary>Größe, die der Text bei der gegebenen Schriftgröße (in Pixeln) einnimmt.</summary>
        public Vector2 Measure(string text, float pixelSize, bool bold)
        {
            Texture2D t = Get(text, pixelSize, bold);
            return new Vector2(t.Width, t.Height);
        }

        /// <summary>
        /// Zeichnet Text. <paramref name="scale"/> skaliert die fertige Textur (praktisch für Namen über Zellen).
        /// </summary>
        public void Draw(SpriteBatch sb, string text, Vector2 position, float pixelSize, bool bold,
                         Color color, bool centered = false, float scale = 1f, bool shadow = true)
        {
            Texture2D tex = Get(text, pixelSize, bold);
            Vector2 origin = centered ? new Vector2(tex.Width / 2f, tex.Height / 2f) : Vector2.Zero;

            if (shadow)
            {
                float offset = Math.Max(1f, pixelSize * scale * 0.06f);
                sb.Draw(tex, position + new Vector2(offset, offset), null, Color.Black * 0.65f,
                        0f, origin, scale, SpriteEffects.None, 0f);
            }
            sb.Draw(tex, position, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        private Texture2D Render(string text, float pixelSize, bool bold)
        {
            if (string.IsNullOrEmpty(text))
            {
                text = " ";
            }

            using (SD.Font font = new SD.Font(FontName, pixelSize,
                    bold ? SD.FontStyle.Bold : SD.FontStyle.Regular, SD.GraphicsUnit.Pixel))
            {
                SD.SizeF size;
                using (SD.Bitmap measureBmp = new SD.Bitmap(1, 1))
                using (SD.Graphics mg = SD.Graphics.FromImage(measureBmp))
                {
                    mg.TextRenderingHint = TextRenderingHint.AntiAlias;
                    size = mg.MeasureString(text, font);
                }

                int w = Math.Max(1, (int)Math.Ceiling(size.Width) + 4);
                int h = Math.Max(1, (int)Math.Ceiling(size.Height) + 4);

                byte[] bytes;
                int stride;
                using (SD.Bitmap bmp = new SD.Bitmap(w, h, PixelFormat.Format32bppArgb))
                {
                    using (SD.Graphics g = SD.Graphics.FromImage(bmp))
                    {
                        g.Clear(SD.Color.Transparent);
                        g.TextRenderingHint = TextRenderingHint.AntiAlias;
                        g.DrawString(text, font, SD.Brushes.White, 2f, 2f);
                    }

                    BitmapData data = bmp.LockBits(new SD.Rectangle(0, 0, w, h), ImageLockMode.ReadOnly,
                            PixelFormat.Format32bppArgb);
                    stride = data.Stride;
                    bytes = new byte[Math.Abs(stride) * h];
                    Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
                    bmp.UnlockBits(data);
                }

                // Weißer Text mit Alpha. MonoGame rechnet standardmäßig mit "premultiplied alpha",
                // deshalb wird der Alphawert in alle vier Kanäle geschrieben. Die Farbe kommt später
                // beim Zeichnen über den Tint.
                Color[] pixels = new Color[w * h];
                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        byte a = bytes[y * stride + x * 4 + 3];
                        pixels[y * w + x] = new Color((int)a, (int)a, (int)a, (int)a);
                    }
                }

                Texture2D tex = new Texture2D(_device, w, h);
                tex.SetData(pixels);
                return tex;
            }
        }

        public void Dispose()
        {
            foreach (Texture2D t in _cache.Values)
            {
                t.Dispose();
            }
            _cache.Clear();
        }
    }
}
