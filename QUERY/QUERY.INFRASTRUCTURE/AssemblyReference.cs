using System.Reflection;

namespace QUERY.INFRASTRUCTURE;
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}