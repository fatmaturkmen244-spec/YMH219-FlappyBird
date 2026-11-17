namespace FlappyBirdApp.Models;

/// <summary>
/// Oyun zorluk seviyelerini ve her seviyeye ait fizik/hız parametrelerini tanımlar.
/// </summary>
public enum DifficultyLevel
{
    Kolay = 0,
    Normal = 1,
    Zor = 2
}

public sealed class DifficultySettings
{
    public DifficultyLevel Level { get; }
    public float Gravity { get; }
    public float JumpImpulse { get; }
    public float BasePipeSpeed { get; }
    public int PipeGapHeight { get; }
    public float SpeedIncrementPerLevel { get; }

    private DifficultySettings(
        DifficultyLevel level,
        float gravity,
        float jumpImpulse,
        float basePipeSpeed,
        int pipeGapHeight,
        float speedIncrementPerLevel)
    {
        Level = level;
        Gravity = gravity;
        JumpImpulse = jumpImpulse;
        BasePipeSpeed = basePipeSpeed;
        PipeGapHeight = pipeGapHeight;
        SpeedIncrementPerLevel = speedIncrementPerLevel;
    }

    public static DifficultySettings FromLevel(DifficultyLevel level) => level switch
    {
        DifficultyLevel.Kolay => new DifficultySettings(
            DifficultyLevel.Kolay,
            gravity: 0.48f,
            jumpImpulse: -8.2f,
            basePipeSpeed: 3.6f,
            pipeGapHeight: 165,
            speedIncrementPerLevel: 0.35f),
        DifficultyLevel.Zor => new DifficultySettings(
            DifficultyLevel.Zor,
            gravity: 0.65f,
            jumpImpulse: -9.4f,
            basePipeSpeed: 5.5f,
            pipeGapHeight: 122,
            speedIncrementPerLevel: 0.65f),
        _ => new DifficultySettings(
            DifficultyLevel.Normal,
            gravity: 0.56f,
            jumpImpulse: -8.8f,
            basePipeSpeed: 4.4f,
            pipeGapHeight: 142,
            speedIncrementPerLevel: 0.50f)
    };
}
