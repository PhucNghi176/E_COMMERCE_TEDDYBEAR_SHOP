using System.Reflection;
using MassTransit;

namespace CONTRACT.CONTRACT.INFRASTRUCTURE.DependencyInjection.Extensions;
public static class NameFormatterExtensions
{
    public static string ToKebabCaseString(this MemberInfo member)
    {
        return KebabCaseEndpointNameFormatter.Instance.SanitizeName(member.Name);
    }
}

public class KebabCaseEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        return typeof(T).ToKebabCaseString();
    }
}