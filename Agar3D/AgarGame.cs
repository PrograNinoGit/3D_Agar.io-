using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Agar3D.Model;
using Agar3D.Rendering;

namespace Agar3D
{
    /// <summary>
    /// Das Spiel selbst: Fenster, Eingabe, Menü/Spiel-Umschaltung und HUD.
    /// MonoGame ruft Update() mit 60 Ticks pro Sekunde auf.
    /// </summary>
    public class AgarGame : Game
    {
        private enum Screen
        {
            Menu,
            Playing
        }

        private readonly GraphicsDeviceManager _graphics;

        private SpriteBatch _sprite;
        private Texture2D _pixel;
        private TextRenderer _text;
        private Renderer3D _renderer;
        private MenuScreen _menu;
        private GameWorld _world;

        private Screen _screen = Screen.Menu;
        private Difficulty _lastDifficulty = Difficulty.Easy;

        private KeyboardState _prevKeys;
        private MouseState _prevMouse;
        private int _ejectHeldTicks;

        public AgarGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferMultiSampling = true;
            _graphics.PreparingDeviceSettings += (sender, e) =>
            {
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
            };

            // Vollbild (ESC beendet das Spiel)
            DisplayMode mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
            _graphics.PreferredBackBufferWidth = mode.Width;
            _graphics.PreferredBackBufferHeight = mode.Height;
            _graphics.HardwareModeSwitch = false;   // randloses Vollbild
            _graphics.IsFullScreen = true;

            IsMouseVisible = false;
            Window.Title = "Agar.io Klon 3D";
        }

        protected override void LoadContent()
        {
            _sprite = new SpriteBatch(GraphicsDevice);

            _pixel = new Texture2D(GraphicsDevice, 1, 1);
            _pixel.SetData(new[] { Color.White });

            _text = new TextRenderer(GraphicsDevice);
            _renderer = new Renderer3D(GraphicsDevice);
            _menu = new MenuScreen();
        }

        private void StartGame(Difficulty difficulty)
        {
            _lastDifficulty = difficulty;
            _world = new GameWorld(difficulty);
            _screen = Screen.Playing;
            _ejectHeldTicks = 0;
            _renderer.UpdateCamera(_world);
        }

        // ---------- Update ----------

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keys = Keyboard.GetState();
            MouseState mouse = Mouse.GetState();

            if (keys.IsKeyDown(Keys.Escape))
            {
                Exit();
                return;
            }

            if (IsActive)
            {
                if (_screen == Screen.Menu)
                {
                    Difficulty? choice = _menu.Update(mouse, _prevMouse, GraphicsDevice.Viewport);
                    if (choice.HasValue)
                    {
                        StartGame(choice.Value);
                    }
                }
                else
                {
                    UpdatePlaying(keys, mouse);
                }
            }

            _prevKeys = keys;
            _prevMouse = mouse;

            base.Update(gameTime);
        }

        private bool Pressed(KeyboardState keys, Keys key)
        {
            return keys.IsKeyDown(key) && !_prevKeys.IsKeyDown(key);
        }

        private void UpdatePlaying(KeyboardState keys, MouseState mouse)
        {
            Viewport vp = GraphicsDevice.Viewport;
            Point center = new Point(vp.Width / 2, vp.Height / 2);

            // Maus-Look: Kamera drehen/neigen und Maus wieder in die Mitte setzen.
            if (mouse.X != center.X || mouse.Y != center.Y)
            {
                int dx = mouse.X - center.X;
                int dy = mouse.Y - center.Y;

                _renderer.Yaw -= dx * 0.0025f;
                _renderer.Pitch = MathHelper.Clamp(_renderer.Pitch + dy * 0.0018f,
                    Renderer3D.MinPitch, Renderer3D.MaxPitch);

                Mouse.SetPosition(center.X, center.Y);
            }

            // Q/E drehen die Kamera auch mit der Tastatur.
            if (keys.IsKeyDown(Keys.Q))
            {
                _renderer.Yaw -= 0.03f;
            }
            if (keys.IsKeyDown(Keys.E))
            {
                _renderer.Yaw += 0.03f;
            }

            // Mausrad neigt die Kamera.
            int wheel = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;
            if (wheel != 0)
            {
                _renderer.Pitch = MathHelper.Clamp(_renderer.Pitch + wheel * 0.0005f,
                        Renderer3D.MinPitch, Renderer3D.MaxPitch);
            }

            // 3D-Bewegung: Die Zellen schwimmen genau in Blickrichtung der Kamera.
            // Nach oben schauen = aufsteigen, nach unten schauen = absinken.
            // Split und Eject gehen in dieselbe Richtung.
            _world.SetAimDirection(_renderer.GetLookDirection());

            // --- Aktionen ---
            if (Pressed(keys, Keys.Space))
            {
                _world.TrySplit();
            }

            if (keys.IsKeyDown(Keys.W))
            {
                _ejectHeldTicks++;
                // sofort beim Drücken; gedrückt halten wiederholt nach kurzer Pause
                if (_ejectHeldTicks == 1 || (_ejectHeldTicks > 25 && _ejectHeldTicks % 4 == 0))
                {
                    _world.TryEject();
                }
            }
            else
            {
                _ejectHeldTicks = 0;
            }

            if (_world.GameOver)
            {
                if (Pressed(keys, Keys.R))
                {
                    StartGame(_lastDifficulty);
                    return;
                }
                if (Pressed(keys, Keys.M))
                {
                    _screen = Screen.Menu;
                    return;
                }
            }

            _world.Update();
            _renderer.UpdateCamera(_world);
        }

        // ---------- Draw ----------

        protected override void Draw(GameTime gameTime)
        {
            _text.BeginFrame();

            if (_screen == Screen.Menu)
            {
                GraphicsDevice.Clear(new Color(30, 30, 40));
                _sprite.Begin();
                _menu.Draw(_sprite, _pixel, _text, GraphicsDevice.Viewport, Mouse.GetState());
                _sprite.End();
            }
            else
            {
                GraphicsDevice.Clear(new Color(18, 18, 26));

                float time = (float)gameTime.TotalGameTime.TotalSeconds;
                _renderer.DrawWorld(_world, time);

                _sprite.Begin();
                _renderer.DrawNames(_sprite, _text, _world);
                DrawHud();
                if (_world.GameOver)
                {
                    DrawGameOver();
                }
                _sprite.End();
            }

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            Color white = Color.White;
            Color soft = new Color(225, 225, 225);

            _text.Draw(_sprite, "Score: " + _world.Score, new Vector2(15, 12), 26, true, white);

            int totalSize = (int)_world.PlayerTotalRadius;
            _text.Draw(_sprite,
                    "Größe: " + totalSize + "   Zellen: " + _world.PlayerCells.Count + "   Modus: " + _world.DifficultyLabel
                    + "   Höhe: " + (int)_world.FocusY,
                    new Vector2(15, 46), 17, false, soft);
            _text.Draw(_sprite, "Leertaste = Teilen   |   W = Masse ausstoßen",
                    new Vector2(15, 70), 17, false, soft);
            _text.Draw(_sprite, "Maus = Blickrichtung (hoch schauen = steigen, runter = sinken)   |   Q / E = drehen   |   Mausrad = neigen",
                    new Vector2(15, 94), 17, false, soft);
            _text.Draw(_sprite, "Vorsicht: grüne Stachelkugeln lassen dich platzen, wenn du zu groß bist!",
                    new Vector2(15, 118), 17, false, soft);
        }

        private void DrawGameOver()
        {
            Viewport vp = GraphicsDevice.Viewport;
            float cx = vp.Width / 2f;
            float cy = vp.Height / 2f;

            _sprite.Draw(_pixel, new Rectangle(0, 0, vp.Width, vp.Height), new Color(0, 0, 0, 150));

            _text.Draw(_sprite, "GAME OVER", new Vector2(cx, cy - 40), 64, true, Color.White, true);
            _text.Draw(_sprite, "Score: " + _world.Score, new Vector2(cx, cy + 20), 30, false, Color.White, true);
            _text.Draw(_sprite, "Drücke 'R' zum Neustart   |   'M' für das Menü", new Vector2(cx, cy + 65), 24, false,
                    Color.White, true);
        }

        protected override void UnloadContent()
        {
            if (_text != null)
            {
                _text.Dispose();
            }
            if (_pixel != null)
            {
                _pixel.Dispose();
            }
            base.UnloadContent();
        }
    }
}
