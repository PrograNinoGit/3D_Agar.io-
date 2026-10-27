using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Agar3D.Rendering
{
    /// <summary>Ein fertig hochgeladenes Dreiecks-Netz (Vertex- und Index-Puffer).</summary>
    public sealed class Mesh
    {
        public VertexBuffer Vertices;
        public IndexBuffer Indices;
        public int PrimitiveCount;
    }

    /// <summary>Erzeugt alle 3D-Formen des Spiels selbst im Code - es werden keine Modelldateien benötigt.</summary>
    public static class MeshFactory
    {
        private static Mesh Build(GraphicsDevice gd, List<VertexPositionNormalTexture> verts, List<ushort> indices)
        {
            VertexBuffer vb = new VertexBuffer(gd, VertexPositionNormalTexture.VertexDeclaration,
                    verts.Count, BufferUsage.WriteOnly);
            vb.SetData(verts.ToArray());

            IndexBuffer ib = new IndexBuffer(gd, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
            ib.SetData(indices.ToArray());

            return new Mesh { Vertices = vb, Indices = ib, PrimitiveCount = indices.Count / 3 };
        }

        /// <summary>Kugel mit Radius 1 um den Ursprung.</summary>
        public static Mesh CreateSphere(GraphicsDevice gd, int slices, int stacks)
        {
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            AddSphereVertices(verts, slices, stacks, 1f);

            List<ushort> idx = new List<ushort>();
            for (int i = 0; i < stacks; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    int a = i * (slices + 1) + j;
                    int b = a + slices + 1;
                    idx.Add((ushort)a);
                    idx.Add((ushort)b);
                    idx.Add((ushort)(a + 1));
                    idx.Add((ushort)(a + 1));
                    idx.Add((ushort)b);
                    idx.Add((ushort)(b + 1));
                }
            }
            return Build(gd, verts, idx);
        }

        private static void AddSphereVertices(List<VertexPositionNormalTexture> verts, int slices, int stacks, float radius)
        {
            for (int i = 0; i <= stacks; i++)
            {
                float theta = MathHelper.Pi * i / stacks;
                float st = (float)Math.Sin(theta);
                float ct = (float)Math.Cos(theta);
                for (int j = 0; j <= slices; j++)
                {
                    float phi = MathHelper.TwoPi * j / slices;
                    Vector3 n = new Vector3(st * (float)Math.Cos(phi), ct, st * (float)Math.Sin(phi));
                    verts.Add(new VertexPositionNormalTexture(n * radius, n,
                            new Vector2((float)j / slices, (float)i / stacks)));
                }
            }
        }

        /// <summary>
        /// Stachelkugel für die Viren: eine kleinere Kugel mit vielen kegelförmigen Stacheln.
        /// Außendurchmesser (Stachelspitzen) = 1.
        /// </summary>
        public static Mesh CreateSpikeBall(GraphicsDevice gd)
        {
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            List<ushort> idx = new List<ushort>();

            const int slices = 20;
            const int stacks = 14;
            const float coreRadius = 0.80f;

            // Kern
            AddSphereVertices(verts, slices, stacks, coreRadius);
            for (int i = 0; i < stacks; i++)
            {
                for (int j = 0; j < slices; j++)
                {
                    int a = i * (slices + 1) + j;
                    int b = a + slices + 1;
                    idx.Add((ushort)a);
                    idx.Add((ushort)b);
                    idx.Add((ushort)(a + 1));
                    idx.Add((ushort)(a + 1));
                    idx.Add((ushort)b);
                    idx.Add((ushort)(b + 1));
                }
            }

            // Stacheln gleichmäßig auf der Kugel verteilen (Fibonacci-Spirale)
            const int spikeCount = 56;
            const int sides = 8;
            const float baseRadius = 0.11f;
            const float tipLength = 1.0f;
            float goldenAngle = 2.3999632f;

            for (int s = 0; s < spikeCount; s++)
            {
                float y = 1f - 2f * (s + 0.5f) / spikeCount;
                float r = (float)Math.Sqrt(Math.Max(0f, 1f - y * y));
                float phi = s * goldenAngle;
                Vector3 dir = new Vector3((float)Math.Cos(phi) * r, y, (float)Math.Sin(phi) * r);
                dir.Normalize();

                Vector3 helper = Math.Abs(dir.Y) < 0.9f ? Vector3.Up : Vector3.Right;
                Vector3 u = Vector3.Normalize(Vector3.Cross(dir, helper));
                Vector3 v = Vector3.Cross(dir, u);

                Vector3 baseCenter = dir * (coreRadius - 0.03f);
                Vector3 tip = dir * tipLength;

                for (int k = 0; k < sides; k++)
                {
                    float a0 = MathHelper.TwoPi * k / sides;
                    float a1 = MathHelper.TwoPi * (k + 1) / sides;
                    Vector3 p0 = baseCenter + (u * (float)Math.Cos(a0) + v * (float)Math.Sin(a0)) * baseRadius;
                    Vector3 p1 = baseCenter + (u * (float)Math.Cos(a1) + v * (float)Math.Sin(a1)) * baseRadius;

                    Vector3 normal = Vector3.Cross(p1 - p0, tip - p0);
                    if (normal.LengthSquared() > 0f)
                    {
                        normal.Normalize();
                    }
                    // Normale muss nach außen zeigen
                    if (Vector3.Dot(normal, dir) < 0f)
                    {
                        normal = -normal;
                    }

                    int first = verts.Count;
                    verts.Add(new VertexPositionNormalTexture(p0, normal, Vector2.Zero));
                    verts.Add(new VertexPositionNormalTexture(p1, normal, Vector2.Zero));
                    verts.Add(new VertexPositionNormalTexture(tip, normal, Vector2.Zero));
                    idx.Add((ushort)first);
                    idx.Add((ushort)(first + 1));
                    idx.Add((ushort)(first + 2));
                }
            }

            return Build(gd, verts, idx);
        }

        /// <summary>Flache runde Scheibe (Radius 1) in der X/Z-Ebene - dient als Schatten unter den Kugeln.</summary>
        public static Mesh CreateDisc(GraphicsDevice gd, int segments)
        {
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            List<ushort> idx = new List<ushort>();

            verts.Add(new VertexPositionNormalTexture(Vector3.Zero, Vector3.Up, new Vector2(0.5f, 0.5f)));
            for (int i = 0; i <= segments; i++)
            {
                float a = MathHelper.TwoPi * i / segments;
                float cx = (float)Math.Cos(a);
                float cz = (float)Math.Sin(a);
                verts.Add(new VertexPositionNormalTexture(new Vector3(cx, 0, cz), Vector3.Up,
                        new Vector2(0.5f + cx * 0.5f, 0.5f + cz * 0.5f)));
            }
            for (int i = 0; i < segments; i++)
            {
                idx.Add(0);
                idx.Add((ushort)(i + 2));
                idx.Add((ushort)(i + 1));
            }
            return Build(gd, verts, idx);
        }

        // ---------- Statische Szene: Boden, Gitter, Wände (Vertexfarben, ohne Licht) ----------

        private static void AddQuad(List<VertexPositionColor> v, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color color)
        {
            v.Add(new VertexPositionColor(a, color));
            v.Add(new VertexPositionColor(b, color));
            v.Add(new VertexPositionColor(c, color));
            v.Add(new VertexPositionColor(a, color));
            v.Add(new VertexPositionColor(c, color));
            v.Add(new VertexPositionColor(d, color));
        }

        private static Color Shade(Color c, float factor)
        {
            return new Color((int)(c.R * factor), (int)(c.G * factor), (int)(c.B * factor));
        }

        /// <summary>Quader ohne Unterseite; die Seiten sind unterschiedlich abgedunkelt, damit er räumlich wirkt.</summary>
        private static void AddBox(List<VertexPositionColor> v, Vector3 min, Vector3 max, Color color)
        {
            float x0 = min.X, y0 = min.Y, z0 = min.Z;
            float x1 = max.X, y1 = max.Y, z1 = max.Z;

            // oben
            AddQuad(v, new Vector3(x0, y1, z0), new Vector3(x1, y1, z0), new Vector3(x1, y1, z1), new Vector3(x0, y1, z1), color);
            // +X / -X
            Color side = Shade(color, 0.80f);
            AddQuad(v, new Vector3(x1, y0, z0), new Vector3(x1, y0, z1), new Vector3(x1, y1, z1), new Vector3(x1, y1, z0), side);
            AddQuad(v, new Vector3(x0, y0, z0), new Vector3(x0, y1, z0), new Vector3(x0, y1, z1), new Vector3(x0, y0, z1), side);
            // +Z / -Z
            Color front = Shade(color, 0.65f);
            AddQuad(v, new Vector3(x0, y0, z1), new Vector3(x0, y1, z1), new Vector3(x1, y1, z1), new Vector3(x1, y0, z1), front);
            AddQuad(v, new Vector3(x0, y0, z0), new Vector3(x1, y0, z0), new Vector3(x1, y1, z0), new Vector3(x0, y1, z0), front);
        }

        public static VertexBuffer CreateFloor(GraphicsDevice gd, float width, float depth, Color color)
        {
            List<VertexPositionColor> v = new List<VertexPositionColor>();
            AddQuad(v, new Vector3(0, 0, 0), new Vector3(width, 0, 0), new Vector3(width, 0, depth), new Vector3(0, 0, depth), color);
            return ToBuffer(gd, v);
        }

        /// <summary>Gitterlinien alle <paramref name="step"/> Einheiten (wie das Gitter im 2D-Original).</summary>
        public static VertexBuffer CreateGrid(GraphicsDevice gd, float width, float depth, float step, Color color, out int lineCount)
        {
            List<VertexPositionColor> v = new List<VertexPositionColor>();
            const float lift = 0.4f; // knapp über dem Boden, damit nichts flackert

            for (float x = 0; x <= width + 0.01f; x += step)
            {
                v.Add(new VertexPositionColor(new Vector3(x, lift, 0), color));
                v.Add(new VertexPositionColor(new Vector3(x, lift, depth), color));
            }
            for (float z = 0; z <= depth + 0.01f; z += step)
            {
                v.Add(new VertexPositionColor(new Vector3(0, lift, z), color));
                v.Add(new VertexPositionColor(new Vector3(width, lift, z), color));
            }
            lineCount = v.Count / 2;
            return ToBuffer(gd, v);
        }

        /// <summary>Weltrand als Mauer mit gelb/dunklen Warnstreifen wie im 2D-Original.</summary>
        public static VertexBuffer CreateWalls(GraphicsDevice gd, float width, float depth, out int triangleCount)
        {
            List<VertexPositionColor> v = new List<VertexPositionColor>();

            const float thickness = 16f;
            const float height = 50f;
            const float segment = 30f;
            Color baseColor = new Color(45, 45, 60);
            Color stripe = new Color(255, 205, 60);

            // oben und unten (inkl. Ecken)
            int countX = (int)Math.Ceiling((width + 2 * thickness) / segment);
            for (int k = 0; k < countX; k++)
            {
                float x0 = -thickness + k * segment;
                float x1 = Math.Min(x0 + segment, width + thickness);
                Color c = (k % 2 == 0) ? stripe : baseColor;
                AddBox(v, new Vector3(x0, 0, -thickness), new Vector3(x1, height, 0), c);
                AddBox(v, new Vector3(x0, 0, depth), new Vector3(x1, height, depth + thickness), c);
            }

            // links und rechts (ohne Ecken, die sind schon oben/unten abgedeckt)
            int countZ = (int)Math.Ceiling(depth / segment);
            for (int k = 0; k < countZ; k++)
            {
                float z0 = k * segment;
                float z1 = Math.Min(z0 + segment, depth);
                Color c = (k % 2 == 0) ? stripe : baseColor;
                AddBox(v, new Vector3(-thickness, 0, z0), new Vector3(0, height, z1), c);
                AddBox(v, new Vector3(width, 0, z0), new Vector3(width + thickness, height, z1), c);
            }

            triangleCount = v.Count / 3;
            return ToBuffer(gd, v);
        }

        private static VertexBuffer ToBuffer(GraphicsDevice gd, List<VertexPositionColor> v)
        {
            VertexBuffer vb = new VertexBuffer(gd, VertexPositionColor.VertexDeclaration, v.Count, BufferUsage.WriteOnly);
            vb.SetData(v.ToArray());
            return vb;
        }
    }
}
