namespace CountDownGame.Models;

public class GameSettings
{
    // Gameplay
    public int MaxRounds { get; set; } = 6;     // default from brief
    public int RoundSeconds { get; set; } = 30; // default from brief

    // App theme: "System" | "Light" | "Dark"
    public string Theme { get; set; } = "System";
}


