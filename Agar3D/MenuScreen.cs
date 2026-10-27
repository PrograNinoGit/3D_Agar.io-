using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Agar3D.Model;
using Agar3D.Rendering;

namespace Agar3D
{
    /// <summary>Hauptmenü mit der Schwierigkeitsauswahl (ehemals MenuPanel.java).</summary>
    public sealed class MenuScreen
    {
        private sealed class Card
        {
            public string Title;
            public string[] Lines;
            public Difficulty Difficulty;
            public Rectangle Bounds;
        }

        private readonly List<Vector2> _dots = new List<Vector2>();
        private readonly List<Color> _dotColors = new List<Color>();
        private readonly List<Card> _cards = new List<Card>();

        public MenuScreen()
        {
            // Ein paar zufällige "Futterpunkte" für die Agar.io-Optik im Hintergrund
            Random random = new Random();
            for (int i = 0; i < 100; i++)
            {
                _dots.Add(new Vector2(random.Next(3000), random.Next(2000)));
                _dotColors.Add(new Color(random.Next(200) + 30, random.Next(200) + 30, random.Next(200) + 30));
            }

            _cards.Add(new Card
            {
                Title = "Leicht",
                Lines = new[] { "Bots wandern zufällig umher" },
                Difficulty = Difficulty.Easy
            });
            _cards.Add(new Card
            {
                Title = "Schwer",
                Lines = new[] { "Bots meiden dich, wenn du größer bist -", "und jagen dich, wenn du kleiner bist" },
                Difficulty = Difficulty.Hard
            });
            _cards.Add(new Card
            {
                Title = "Extrem",
                Lines = new[] { "Wie Schwer, zusätzlich können Bots", "sich beim Jagen teilen" },
                Difficulty = Difficulty.Extreme
            });
        }

        /// <summary>Berechnet die Position der Karten. Alles skaliert mit der Fensterhöhe.</summary>
        private float Layout(Viewport vp, out float titleY)
        {
            float s = MathHelper.Clamp(vp.Height / 900f, 0.8f, 1.8f);

            float cardWidth = 460 * s;
            float gap = 14 * s;

            float totalCards = 0;
            foreach (Card c in _cards)
            {
                totalCards += (50 + c.Lines.Length * 20) * s + gap;
            }

            float blockHeight = 70 * s + 40 * s + totalCards + 60 * s;
            float top = Math.Max(20, (vp.Height - blockHeight) / 2f);

            titleY = top;

            float y = top + 70 * s + 40 * s;
            foreach (Card c in _cards)
            {
                float h = (50 + c.Lines.Length * 20) * s;
                c.Bounds = new Rectangle((int)((vp.Width - cardWidth) / 2f), (int)y, (int)cardWidth, (int)h);
                y += h + gap;
            }
            return s;
        }

        /// <summary>Gibt die gewählte Schwierigkeit zurück, wenn eine Karte angeklickt wurde, sonst null.</summary>
        public Difficulty? Update(MouseState mouse, MouseState previous, Viewport vp)
        {
            float titleY;
            Layout(vp, out titleY);

            bool clicked = mouse.LeftButton == ButtonState.Pressed && previous.LeftButton == ButtonState.Released;
            if (!clicked)
            {
                return null;
            }

            foreach (Card c in _cards)
            {
                if (c.Bounds.Contains(mouse.X, mouse.Y))
                {
                    return c.Difficulty;
                }
            }
            return null;
        }

        public void Draw(SpriteBatch sb, Texture2D pixel, TextRenderer text, Viewport vp, MouseState mouse)
        {
            float titleY;
            float s = Layout(vp, out titleY);

            // Klassisches Agar.io-Gitter im Hintergrund
            Color gridColor = new Color(45, 45, 58);
            for (int x = 0; x < vp.Width; x += 40)
            {
                sb.Draw(pixel, new Rectangle(x, 0, 1, vp.Height), gridColor);
            }
            for (int y = 0; y < vp.Height; y += 40)
            {
                sb.Draw(pixel, new Rectangle(0, y, vp.Width, 1), gridColor);
            }

            // Verstreute "Futterpunkte"
            for (int i = 0; i < _dots.Count; i++)
            {
                int x = (int)_dots[i].X % Math.Max(1, vp.Width);
                int y = (int)_dots[i].Y % Math.Max(1, vp.Height);
                sb.Draw(pixel, new Rectangle(x, y, 5, 5), _dotColors[i]);
            }

            float centerX = vp.Width / 2f;

            text.Draw(sb, "Agar.io Klon 3D", new Vector2(centerX, titleY + 35 * s), 56 * s, true, Color.White, true);
            text.Draw(sb, "Wähle deine Schwierigkeit", new Vector2(centerX, titleY + 88 * s), 20 * s, false,
                    new Color(200, 200, 200), true, 1f, false);

            Color border = new Color(90, 90, 110);
            foreach (Card c in _cards)
            {
                bool hover = c.Bounds.Contains(mouse.X, mouse.Y);
                Color fill = hover ? new Color(65, 65, 85) : new Color(45, 45, 60);

                sb.Draw(pixel, c.Bounds, border);
                sb.Draw(pixel, new Rectangle(c.Bounds.X + 1, c.Bounds.Y + 1, c.Bounds.Width - 2, c.Bounds.Height - 2), fill);

                float cx = c.Bounds.X + c.Bounds.Width / 2f;
                text.Draw(sb, c.Title, new Vector2(cx, c.Bounds.Y + 24 * s), 20 * s, true, Color.White, true, 1f, false);

                for (int i = 0; i < c.Lines.Length; i++)
                {
                    text.Draw(sb, c.Lines[i], new Vector2(cx, c.Bounds.Y + (48 + i * 20) * s), 13 * s, false,
                            new Color(205, 205, 205), true, 1f, false);
                }
            }

            Rectangle last = _cards[_cards.Count - 1].Bounds;
            text.Draw(sb, "ESC = Spiel beenden", new Vector2(centerX, last.Bottom + 40 * s), 14 * s, false,
                    new Color(150, 150, 150), true, 1f, false);
        }
    }
}
