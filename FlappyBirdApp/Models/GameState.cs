namespace FlappyBirdApp.Models;

/// <summary>
/// Oyunun anlık skor, en yüksek skor, seviye, hız ve çalışma durumunu kapsüller.
/// </summary>
public sealed class GameState
{
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public int Level { get; private set; } = 1;
    public float CurrentSpeed { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }
    public DifficultySettings Difficulty { get; private set; }
    public string StatusMessage { get; private set; } = "Hazır — Başlamak için SPACE veya Yeni Oyun";

    public GameState(int initialHighScore, DifficultyLevel level = DifficultyLevel.Normal)
    {
        HighScore = initialHighScore;
        Difficulty = DifficultySettings.FromLevel(level);
        CurrentSpeed = Difficulty.BasePipeSpeed;
    }

    public void SetDifficulty(DifficultyLevel level)
    {
        Difficulty = DifficultySettings.FromLevel(level);
        if (!IsRunning)
        {
            CurrentSpeed = Difficulty.BasePipeSpeed;
        }
    }

    public void StartNewGame()
    {
        Score = 0;
        Level = 1;
        CurrentSpeed = Difficulty.BasePipeSpeed;
        IsRunning = true;
        IsPaused = false;
        IsGameOver = false;
        StatusMessage = "Oyun Devam Ediyor";
    }

    public void TogglePause()
    {
        if (!IsRunning || IsGameOver)
        {
            return;
        }

        IsPaused = !IsPaused;
        StatusMessage = IsPaused ? "Duraklatıldı (Devam için P)" : "Oyun Devam Ediyor";
    }

    public bool IncrementScore()
    {
        Score++;
        bool newHighScore = false;
        if (Score > HighScore)
        {
            HighScore = Score;
            newHighScore = true;
        }

        // Her 5 puanda bir seviye ve hız artışı (İsteğe bağlı geliştirme)
        Level = 1 + (Score / 5);
        CurrentSpeed = Math.Min(
            Difficulty.BasePipeSpeed + (Level - 1) * Difficulty.SpeedIncrementPerLevel,
            Difficulty.BasePipeSpeed + 4.0f);

        return newHighScore;
    }

    public void TriggerGameOver(string reason)
    {
        IsRunning = false;
        IsPaused = false;
        IsGameOver = true;
        StatusMessage = $"Oyun Bitti: {reason}";
    }

    public void SetScenarioForPreview(int score, int highScore, int level, bool isRunning, bool isGameOver, string message)
    {
        Score = score;
        HighScore = highScore;
        Level = level;
        CurrentSpeed = Difficulty.BasePipeSpeed + (level - 1) * Difficulty.SpeedIncrementPerLevel;
        IsRunning = isRunning;
        IsGameOver = isGameOver;
        IsPaused = false;
        StatusMessage = message;
    }
}
