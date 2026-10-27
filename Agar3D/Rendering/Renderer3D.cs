using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Agar3D.Model;

namespace Agar3D.Rendering
{
    public sealed class Renderer3D
    {
        private const float FieldOfView = MathHelper.PiOver4;
        private const float NearPlane = 100f;
        private const float FarPlane = 40000f;

        private readonly GraphicsDevice _device;

        private readonly BasicEffect _lit;
        private readonly BasicEffect _flat;
        private readonly BasicEffect _shadow;

        private readonly Mesh _sphereHigh;
        private readonly Mesh _sphereLow;
        private readonly Mesh _spikeBall;
        private readonly Mesh _disc;

        private readonly VertexBuffer _floor;
        private readonly VertexBuffer _grid;
        private readonly int _gridLineCount;
        private readonly VertexBuffer _walls;
        private readonly int _wallTriangleCount;

        public float Yaw;
        public float Pitch = 0.25f;

        public const float MinPitch = -1.1f;
        public const float MaxPitch = 1.1f;

        private const float CameraDistance = 260f;
        private const float CameraHeight = 120f;
        private const float CameraLookAhead = 50f;

        public Matrix View = Matrix.Identity;
        public Matrix Projection = Matrix.Identity;
        public Vector3 CameraPosition;

        public Renderer3D(GraphicsDevice device)
        {
            _device = device;

            _sphereHigh = MeshFactory.CreateSphere(device, 28, 18);
            _sphereLow = MeshFactory.CreateSphere(device, 10, 7);
            _spikeBall = MeshFactory.CreateSpikeBall(device);
            _disc = MeshFactory.CreateDisc(device, 28);

            _floor = MeshFactory.CreateFloor(device, GameWorld.WorldWidth, GameWorld.WorldDepth, new Color(235, 235, 235));
            _grid = MeshFactory.CreateGrid(device, GameWorld.WorldWidth, GameWorld.WorldDepth, 40f,
                new Color(210, 210, 210), out _gridLineCount);
            _walls = MeshFactory.CreateWalls(device, GameWorld.WorldWidth, GameWorld.WorldDepth, out _wallTriangleCount);

            _lit = new BasicEffect(device);
            _lit.LightingEnabled = true;
            _lit.PreferPerPixelLighting = true;
            _lit.AmbientLightColor = new Vector3(0.50f, 0.50f, 0.50f);
            _lit.DirectionalLight0.Enabled = true;
            _lit.DirectionalLight0.DiffuseColor = new Vector3(0.75f, 0.75f, 0.75f);
            _lit.DirectionalLight0.SpecularColor = new Vector3(0.45f, 0.45f, 0.45f);
            _lit.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-0.45f, -1.0f, -0.35f));
            _lit.DirectionalLight1.Enabled = false;
            _lit.DirectionalLight2.Enabled = false;
            _lit.SpecularColor = new Vector3(0.35f, 0.35f, 0.35f);
            _lit.SpecularPower = 28f;

            _flat = new BasicEffect(device);
            _flat.LightingEnabled = false;
            _flat.VertexColorEnabled = true;
            _flat.TextureEnabled = false;

            _shadow = new BasicEffect(device);
            _shadow.LightingEnabled = false;
            _shadow.VertexColorEnabled = false;
            _shadow.TextureEnabled = false;
            _shadow.DiffuseColor = new Vector3(0f, 0f, 0f);
            _shadow.Alpha = 0.28f;
        }

        public void UpdateCamera(GameWorld world)
        {
            float aspect = _device.Viewport.AspectRatio;

            Vector3 focus = new Vector3((float)world.FocusX, (float)world.FocusY, (float)world.FocusZ);
            float radius = (float)world.PlayerTotalRadius;

            float cosPitch = (float)Math.Cos(Pitch);
            Vector3 direction = new Vector3(
                (float)Math.Sin(Yaw) * cosPitch,
                (float)Math.Sin(Pitch),
                (float)Math.Cos(Yaw) * cosPitch);
            direction = Vector3.Normalize(direction);

            Vector3 cameraPos = focus - direction * (CameraDistance + radius * 1.6f)
                + Vector3.Up * (CameraHeight + radius * 0.75f);
            Vector3 lookTarget = focus + Vector3.Up * (18f + radius * 0.35f) + direction * CameraLookAhead;

            cameraPos.Y = Math.Max(20f, Math.Min(cameraPos.Y, GameWorld.WorldHeight - 20f));
            lookTarget.Y = Math.Max(10f, Math.Min(lookTarget.Y, GameWorld.WorldHeight - 10f));

            CameraPosition = cameraPos;
            View = Matrix.CreateLookAt(cameraPos, lookTarget, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(FieldOfView, aspect, NearPlane, FarPlane);
        }

        public bool TryGetGroundPoint(int screenX, int screenY, out double worldX, out double worldZ)
        {
            Viewport vp = _device.Viewport;
            Vector3 near = vp.Unproject(new Vector3(screenX, screenY, 0f), Projection, View, Matrix.Identity);
            Vector3 far = vp.Unproject(new Vector3(screenX, screenY, 1f), Projection, View, Matrix.Identity);
            Vector3 dir = far - near;

            worldX = 0;
            worldZ = 0;
            if (dir.Y > -1e-5f) return false;

            float t = -near.Y / dir.Y;
            Vector3 p = near + dir * t;
            worldX = p.X;
            worldZ = p.Z;
            return true;
        }

        /// <summary>
        /// Richtung vom Kamerapunkt durch einen Bildschirmpunkt (z. B. Mausposition oder Bildschirmmitte).
        /// Das ist die 3D-Blickrichtung, in die Bewegung, Split und Eject gehen.
        /// </summary>
        public Vector3 GetAimDirection(int screenX, int screenY)
        {
            Viewport vp = _device.Viewport;
            Vector3 near = vp.Unproject(new Vector3(screenX, screenY, 0f), Projection, View, Matrix.Identity);
            Vector3 far = vp.Unproject(new Vector3(screenX, screenY, 1f), Projection, View, Matrix.Identity);
            Vector3 dir = far - near;
            if (dir.LengthSquared() < 1e-9f) return Vector3.Forward;
            dir.Normalize();
            return dir;
        }

        /// <summary>Blickrichtung genau durch die Bildschirmmitte.</summary>
        public Vector3 GetViewDirection()
        {
            Viewport vp = _device.Viewport;
            return GetAimDirection(vp.Width / 2, vp.Height / 2);
        }

        /// <summary>
        /// Blickrichtung der Kamera aus Yaw und Pitch. Nach oben schauen = aufsteigen,
        /// nach unten schauen = absinken. Das ist die Richtung fuer Bewegung, Split und Eject.
        /// </summary>
        public Vector3 GetLookDirection()
        {
            float cosPitch = (float)Math.Cos(Pitch);
            Vector3 d = new Vector3(
                (float)Math.Sin(Yaw) * cosPitch,
                (float)Math.Sin(Pitch),
                (float)Math.Cos(Yaw) * cosPitch);
            d.Normalize();
            return d;
        }

        public void DrawWorld(GameWorld world, float time)
        {
            _device.BlendState = BlendState.Opaque;
            _device.DepthStencilState = DepthStencilState.Default;
            _device.RasterizerState = RasterizerState.CullNone;
            _device.SamplerStates[0] = SamplerState.LinearClamp;

            BoundingFrustum frustum = new BoundingFrustum(View * Projection);
            DrawStaticScene();

            _lit.View = View;
            _lit.Projection = Projection;

            SetMesh(_sphereLow);
            foreach (Food f in world.Foods)
            {
                float r = (float)f.Radius;
                float bob = (float)Math.Sin(time * 2.2f + f.BobPhase) * 1.6f;
                Vector3 center = new Vector3((float)f.X, (float)f.Y + bob, (float)f.Z);
                if (!IsVisible(frustum, center, r)) continue;
                DrawMesh(_sphereLow, Matrix.CreateScale(r) * Matrix.CreateTranslation(center), f.Color);
            }

            SetMesh(_sphereHigh);
            foreach (EjectedMass em in world.EjectedMasses)
            {
                float r = (float)em.Radius;
                Vector3 center = new Vector3((float)em.X, (float)em.Y, (float)em.Z);
                if (!IsVisible(frustum, center, r)) continue;
                DrawMesh(_sphereHigh, Matrix.CreateScale(r) * Matrix.CreateTranslation(center), em.Color);
            }

            foreach (Bot bot in world.Bots)
            {
                foreach (Cell c in bot.Cells)
                {
                    DrawCell(frustum, c);
                }
            }
            foreach (Cell c in world.PlayerCells)
            {
                DrawCell(frustum, c);
            }

            SetMesh(_spikeBall);
            Color virusColor = new Color(51, 204, 51);
            foreach (Virus v in world.Viruses)
            {
                float r = (float)v.Radius;
                Vector3 center = new Vector3((float)v.X, (float)v.Y, (float)v.Z);
                if (!IsVisible(frustum, center, r)) continue;
                Matrix m = Matrix.CreateScale(r) * Matrix.CreateRotationY(v.Spin) * Matrix.CreateTranslation(center);
                DrawMesh(_spikeBall, m, virusColor);
            }

            DrawShadows(world, frustum);
        }

        private void DrawStaticScene()
        {
            _flat.View = View;
            _flat.Projection = Projection;
            _flat.World = Matrix.Identity;
            _flat.CurrentTechnique.Passes[0].Apply();

            _device.SetVertexBuffer(_floor);
            _device.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);

            _device.SetVertexBuffer(_grid);
            _device.DrawPrimitives(PrimitiveType.LineList, 0, _gridLineCount);

            _device.SetVertexBuffer(_walls);
            _device.DrawPrimitives(PrimitiveType.TriangleList, 0, _wallTriangleCount);
        }

        private void DrawCell(BoundingFrustum frustum, Cell c)
        {
            float r = (float)c.Radius;
            Vector3 center = new Vector3((float)c.X, (float)c.Y, (float)c.Z);
            if (!IsVisible(frustum, center, r)) return;

            SetMesh(_sphereHigh);
            DrawMesh(_sphereHigh, Matrix.CreateScale(r) * Matrix.CreateTranslation(center), c.Color);
        }

        private void DrawShadows(GameWorld world, BoundingFrustum frustum)
        {
            _device.BlendState = BlendState.AlphaBlend;
            _device.DepthStencilState = DepthStencilState.DepthRead;
            _shadow.View = View;
            _shadow.Projection = Projection;
            SetMesh(_disc);

            foreach (EjectedMass em in world.EjectedMasses)
            {
                DrawShadow(frustum, (float)em.X, (float)em.Z, (float)em.Radius);
            }
            foreach (Bot bot in world.Bots)
            {
                foreach (Cell c in bot.Cells)
                {
                    DrawShadow(frustum, (float)c.X, (float)c.Z, (float)c.Radius);
                }
            }
            foreach (Cell c in world.PlayerCells)
            {
                DrawShadow(frustum, (float)c.X, (float)c.Z, (float)c.Radius);
            }
            foreach (Virus v in world.Viruses)
            {
                DrawShadow(frustum, (float)v.X, (float)v.Z, (float)v.Radius);
            }

            _device.BlendState = BlendState.Opaque;
            _device.DepthStencilState = DepthStencilState.Default;
        }

        private void DrawShadow(BoundingFrustum frustum, float x, float z, float radius)
        {
            Vector3 center = new Vector3(x + radius * 0.30f, 0.8f, z + radius * 0.22f);
            if (!IsVisible(frustum, center, radius * 1.2f)) return;
            _shadow.World = Matrix.CreateScale(radius * 1.05f, 1f, radius * 1.05f) * Matrix.CreateTranslation(center);
            _shadow.CurrentTechnique.Passes[0].Apply();
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _disc.PrimitiveCount);
        }

        private void SetMesh(Mesh mesh)
        {
            _device.SetVertexBuffer(mesh.Vertices);
            _device.Indices = mesh.Indices;
        }

        private void DrawMesh(Mesh mesh, Matrix world, Color color)
        {
            _lit.World = world;
            _lit.DiffuseColor = color.ToVector3();
            _lit.CurrentTechnique.Passes[0].Apply();
            _device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, mesh.PrimitiveCount);
        }

        private static bool IsVisible(BoundingFrustum frustum, Vector3 center, float radius)
        {
            return frustum.Contains(new BoundingSphere(center, radius)) != ContainmentType.Disjoint;
        }

        public void DrawNames(SpriteBatch sb, TextRenderer text, GameWorld world)
        {
            BoundingFrustum frustum = new BoundingFrustum(View * Projection);
            Viewport vp = _device.Viewport;
            Vector3 right = new Vector3(View.M11, View.M21, View.M31);

            foreach (Bot bot in world.Bots)
            {
                foreach (Cell c in bot.Cells)
                {
                    DrawName(sb, text, frustum, vp, right, c, true);
                }
            }
            foreach (Cell c in world.PlayerCells)
            {
                DrawName(sb, text, frustum, vp, right, c, false);
            }
        }

        private void DrawName(SpriteBatch sb, TextRenderer text, BoundingFrustum frustum, Viewport vp,
                              Vector3 cameraRight, Cell c, bool isBot)
        {
            if (!(c.Radius > 12 || isBot)) return;

            float r = (float)c.Radius;
            Vector3 center = new Vector3((float)c.X, (float)c.Y, (float)c.Z);
            if (!IsVisible(frustum, center, r)) return;

            Vector3 screenCenter = vp.Project(center, Projection, View, Matrix.Identity);
            Vector3 screenEdge = vp.Project(center + cameraRight * r, Projection, View, Matrix.Identity);
            float screenRadius = Math.Abs(screenEdge.X - screenCenter.X);

            float fontPixels = Math.Max(10f, screenRadius * 0.5f);
            const float baseSize = 32f;
            float scale = fontPixels / baseSize;

            text.Draw(sb, c.Name, new Vector2(screenCenter.X, screenCenter.Y), baseSize, true,
                Color.White, true, scale, true);
        }
    }
}