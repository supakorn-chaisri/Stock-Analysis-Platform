namespace StockAnalysis.Domain.ValueObjects;

public readonly record struct Symbol
{
    public string Value { get; }

    private Symbol(string value) => Value = value;

    public static Symbol Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Symbol cannot be empty", nameof(input));

        var parts = input.Split(':', 2);
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            throw new ArgumentException("Symbol must be in format EXCHANGE:TICKER", nameof(input));

        return new Symbol(input.ToUpperInvariant());
    }

    public static bool TryParse(string input, out Symbol symbol)
    {
        try
        {
            symbol = Parse(input);
            return true;
        }
        catch
        {
            symbol = default;
            return false;
        }
    }

    public string Exchange => Value.Split(':')[0];
    public string Ticker => Value.Split(':')[1];

    public override string ToString() => Value;

    public static implicit operator string(Symbol symbol) => symbol.Value;
    public static explicit operator Symbol(string value) => Parse(value);
}