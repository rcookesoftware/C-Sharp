namespace CountDownGame.Models;

public class GameSettings
{
    public int MaxRounds { get; set; } = 6;     
    public int RoundSeconds { get; set; } = 30;

    // App theme: "System" | "Light" | "Dark"
    public string Theme { get; set; } = "System";
}


