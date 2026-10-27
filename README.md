# Agar.io Klon 3D (C# / MonoGame)

Portierung des Java-Projekts **Agar.io-KLon** (Swing/AWT, 2D) nach **C#** mit echter **3D-Darstellung**.
Die komplette Spiellogik (Bots, Splitten, Viren, Masse ausstoßen, Schwierigkeitsgrade) ist 1:1 übernommen –
nur die Darstellung ist jetzt 3D: Kugeln mit Beleuchtung, Schatten, Stachelkugel-Viren, Mauern und eine drehbare Verfolgerkamera.

## Starten in Visual Studio

1. **Visual Studio 2022 (17.8 oder neuer)** mit der Workload *„.NET-Desktopentwicklung“*.
   Benötigt wird das **.NET 8 SDK** (ist in VS 2022 17.8+ enthalten).
2. `Agar3D.sln` öffnen.
3. **F5** drücken. Beim ersten Start lädt Visual Studio die NuGet-Pakete (MonoGame) automatisch herunter.

Alternativ per Konsole: `dotnet run --project Agar3D`

## Steuerung

| Taste / Maus | Aktion |
|---|---|
| Maus | Zellen laufen zum Mauszeiger (auf dem Boden) |
| Leertaste | Teilen |
| W | Masse ausstoßen (gedrückt halten wiederholt) |
| Q / E | Kamera nach links / rechts drehen |
| Mausrad | Kamera neigen (flacher / steiler) |
| R / M | Nach Game Over: Neustart / Menü |
| ESC | Spiel beenden |

## Aufbau (Java → C#)

| Java | C# |
|---|---|
| `Main`, `GameFrame` | `Program.cs`, `AgarGame.cs` (Fenster, Menü/Spiel-Umschaltung, Eingabe, HUD) |
| `GamePanel` (Logik) | `GameWorld.cs` (Bewegung, KI, Kollisionen – ohne Rendering) |
| `GamePanel` (Zeichnen) | `Rendering/Renderer3D.cs` (3D-Szene, Kamera, Namen) |
| `MenuPanel` | `MenuScreen.cs` |
| `Cell`, `Bot`, `Food`, `Virus`, `EjectedMass`, `Difficulty` | `Model/*.cs` |

Aus Java `x / y` wurde in C# `X / Z`: Die Spielfläche liegt in der X/Z-Ebene, Y ist „oben“.
Die Maus wird per Strahl auf den Boden projiziert, dadurch steuert sie in 3D genauso wie im 2D-Original.

Alle 3D-Formen (Kugeln, Stachelkugel, Mauern, Boden) werden im Code erzeugt (`Rendering/MeshFactory.cs`),
es werden keine Modell- oder Content-Dateien benötigt. Text wird über `System.Drawing` in Texturen gerendert
(`Rendering/TextRenderer.cs`), deshalb läuft das Projekt nur unter Windows (`net8.0-windows`).
