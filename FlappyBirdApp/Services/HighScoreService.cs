namespace FlappyBirdApp.Services;

/// <summary>
/// En yüksek skor bilgisini yerel dosyada saklar ve okur.
/// </summary>
public sealed class HighScoreService
{
    private readonly string _filePath;

    public HighScoreService(string? customFilePath = null)
    {
        _filePath = customFilePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highscore.txt");
    }

    public int LoadHighScore()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return 0;
            }

            string content = File.ReadAllText(_filePath).Trim();
            return int.TryParse(content, out int score) && score >= 0 ? score : 0;
        }
        catch
        {
            return 0;
        }
    }

    public bool SaveHighScore(int highScore)
    {
        try
        {
            File.WriteAllText(_filePath, highScore.ToString());
            return true;
        }
        catch
        {
            return false;
        }
    }
}
