using CONTRACT.CONTRACT.CONTRACT.Enumerations;

namespace CONTRACT.CONTRACT.CONTRACT.Extensions;
public static class SortOrderExtension
{
    public static SortOrder ConvertStringToSortOrder(string? sortOrder)
    {
        return !string.IsNullOrWhiteSpace(sortOrder)
            ? sortOrder.ToUpper().Equals("ASC")
                ? SortOrder.Ascending
                : SortOrder.Descending
            : SortOrder.Descending;
    }
}