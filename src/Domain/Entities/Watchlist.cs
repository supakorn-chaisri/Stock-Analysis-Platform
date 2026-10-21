using StockAnalysis.Domain.Enums;
using StockAnalysis.Domain.ValueObjects;

namespace StockAnalysis.Domain.Entities;

public class Watchlist
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public string Name { get; private set; } = string.Empty;
    public bool IsDefault { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<WatchlistSymbol> _symbols = new();
    public IReadOnlyList<WatchlistSymbol> Symbols => _symbols.AsReadOnly();

    private Watchlist() { } // EF Core

    public static Watchlist Create(Guid userId, string name, bool isDefault = false)
    {
        return new Watchlist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            IsDefault = isDefault,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void AddSymbol(Symbol symbol, int sortOrder = 0)
    {
        if (_symbols.Any(s => s.Symbol == symbol))
            return;

        _symbols.Add(new WatchlistSymbol
        {
            WatchlistId = Id,
            Symbol = symbol,
            SortOrder = sortOrder,
            AddedAt = DateTimeOffset.UtcNow
        });
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RemoveSymbol(Symbol symbol)
    {
        var item = _symbols.FirstOrDefault(s => s.Symbol == symbol);
        if (item != null)
        {
            _symbols.Remove(item);
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    public void UpdateName(string name)
        {
            Name = name;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void ReorderSymbols(IEnumerable<Symbol> orderedSymbols)
        {
            var dict = orderedSymbols.Select((s, i) => (s, i)).ToDictionary(x => x.s, x => x.i);
            foreach (var item in _symbols)
            {
                if (dict.TryGetValue(item.Symbol, out var order))
                    item.SortOrder = order;
            }
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }