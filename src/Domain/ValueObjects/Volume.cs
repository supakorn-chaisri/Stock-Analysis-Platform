namespace StockAnalysis.Domain.ValueObjects;

public readonly record struct Volume
{
    public long Value { get; }

    public Volume(long value)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Volume cannot be negative");
        Value = value;
    }

    public static implicit operator long(Volume volume) => volume.Value;
    public static explicit operator Volume(long value) => new(value);

    public override string ToString() => Value.ToString("N0");
}