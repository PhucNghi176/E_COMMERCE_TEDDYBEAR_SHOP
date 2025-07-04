using Ardalis.SmartEnum;

namespace CONTRACT.CONTRACT.CONTRACT.Enumerations;
public class SortOrder(string name, int value) : SmartEnum<SortOrder>(name, value)
{
    public static readonly SortOrder Ascending = new(nameof(Ascending), 1);
    public static readonly SortOrder Descending = new(nameof(Descending), 2);

    public static implicit operator SortOrder(string name)
    {
        return FromName(name);
    }

    public static implicit operator SortOrder(int value)
    {
        return FromValue(value);
    }

    public static implicit operator string(SortOrder status)
    {
        return status.Name;
    }

    public static implicit operator int(SortOrder status)
    {
        return status.Value;
    }
}