# Agar3D

Ein Agar.io-Klon in **3D**, geschrieben in C# mit MonoGame. Du schwimmst als Kugel durch eine würfelförmige Arena, frisst Futter und kleinere Zellen und weichst größeren aus. Alle 3D-Formen entstehen im Code, es werden keine Modelldateien gebraucht.

## Voraussetzungen

- .NET 8 SDK
- Visual Studio 2022 oder neuer (oder `dotnet` auf der Kommandozeile)
- MonoGame.Framework.DesktopGL 3.8.x (kommt per NuGet)
- Windows, weil `TextRenderer` System.Drawing nutzt und die Schrift "Segoe UI" verwendet

## Starten

```
dotnet run --project Agar3D
```

Oder in Visual Studio das Projekt öffnen und `F5` drücken. Das Spiel startet im Vollbild.

## Steuerung

| Taste | Aktion |
|---|---|
| Maus | Blickrichtung. Du schwimmst dorthin, wohin du schaust. Hoch schauen = steigen, runter schauen = sinken |
| Q / E | Kamera nach links / rechts drehen |
| Mausrad | Kamera neigen |
| Leertaste | Teilen (Split) in Blickrichtung |
| W | Masse ausstoßen (halten = wiederholen) |
| R | Neustart nach Game Over |
| M | Zurück ins Menü nach Game Over |
| Esc | Spiel beenden |

## Spielregeln

- **Fressen:** Eine Zelle frisst eine andere, wenn ihr Radius mehr als 10 % größer ist und die Mittelpunkte nah genug beieinander liegen. Der Abstand wird in 3D gemessen.
- **Futter:** Kleine Kugeln, die überall in der Arena schweben und dich wachsen lassen.
- **Split:** Teilt jede deiner Zellen in zwei Hälften. Die neue Zelle wird in Blickrichtung geschleudert. Nach einer Wartezeit verschmelzen die Teile wieder.
- **Masse ausstoßen:** Du verlierst etwas Masse und schießt ein Pellet in Blickrichtung. Ein Virus, das siebenmal gefüttert wird, spaltet einen neuen Virus ab.
- **Viren:** Grüne Stachelkugeln. Wenn du deutlich größer bist als ein Virus und hineinläuft, platzt du in mehrere Teile.
- **Arena:** 4000 × 4000 × 1200 Einheiten. Alle Objekte bleiben innerhalb, auch in der Höhe.

## Schwierigkeitsgrade

| Modus | Verhalten der Bots |
|---|---|
| Leicht | Wandern zufällig durch die Arena |
| Schwer | Meiden dich, wenn du größer bist, und jagen dich, wenn du kleiner bist |
| Extrem | Wie Schwer, außerdem teilen sich die Bots, um dich zu fangen |

## Projektstruktur

```
Agar3D/
├── Program.cs              Einstiegspunkt
├── AgarGame.cs             Fenster, Eingabe, HUD, Menü/Spiel-Umschaltung
├── MenuScreen.cs           Startmenü mit Schwierigkeitswahl
├── Model/
│   ├── GameWorld.cs        Spiellogik: Bewegung, Kollisionen, Bots, Spawning (alles 3D)
│   ├── Cell.cs             Zelle mit X/Y/Z, Radius, Richtung und Boost
│   ├── Bot.cs              Bot aus einer oder mehreren Zellen
│   ├── Food.cs             Futterkugel
│   ├── EjectedMass.cs      Ausgestoßenes Pellet
│   ├── Virus.cs            Stachelkugel
│   └── Difficulty.cs       Schwierigkeitsgrade
└── Rendering/
    ├── Renderer3D.cs       Kamera, Zeichnen der Welt, Namen über den Zellen
    ├── MeshFactory.cs      Erzeugt Kugeln, Stachelkugeln, Boden, Gitter und Wände im Code
    └── TextRenderer.cs     Text per GDI+ in Texturen, ohne SpriteFont-Dateien
```

## Hinweise für Entwickler

- Alle Objekte speichern den **Mittelpunkt** ihrer Kugel in `X`, `Y`, `Z`.
- `GameWorld.Update()` läuft mit 60 Ticks pro Sekunde und wird aus `AgarGame` aufgerufen.
- Die Bewegungsrichtung kommt aus `Renderer3D.GetLookDirection()` und wird über `GameWorld.SetAimDirection()` gesetzt. Split und Eject nutzen dieselbe Richtung.
- Die Weltgrößen stehen als Konstanten in `GameWorld` (`WorldWidth`, `WorldDepth`, `WorldHeight`).
