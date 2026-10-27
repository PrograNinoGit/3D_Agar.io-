namespace Agar3D.Model
{
    public enum Difficulty
    {
        Easy,    // Zufällig wandernde Bots (ursprüngliches Verhalten)
        Hard,    // Bots meiden den Spieler, wenn er größer ist, und jagen ihn, wenn er kleiner ist
        Extreme  // Wie Hard, zusätzlich dürfen Bots sich teilen
    }
}
