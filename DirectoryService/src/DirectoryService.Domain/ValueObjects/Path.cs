namespace DirectoryService.Domain.ValueObjects;

public record Path
{
    private const int MIN_LENGTH = 3;
    
    private const int MAX_LENGTH = 150;

    private Path(string value)
    {
        Value = value;
    }

    public string Value { get; }
    
    public static Path Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            
        }
        
        if (value.Length < MIN_LENGTH)
        {
            
        }

        if (value.Length > MAX_LENGTH)
        {
            
        }

        return new Path(value);
    }
}