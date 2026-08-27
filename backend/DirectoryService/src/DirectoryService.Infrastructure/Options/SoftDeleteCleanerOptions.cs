public record SoftDeleteCleanerOptions
{
    public TimeSpan Interval { get; set; } = TimeSpan.FromHours(1); // Значение по умолчанию

    public int RetentionDays { get; set; } = 30;

    public int BatchSize { get; set; } = 100;
}