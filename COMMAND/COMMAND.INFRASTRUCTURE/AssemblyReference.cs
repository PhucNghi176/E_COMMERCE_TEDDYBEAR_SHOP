using System.Reflection;

namespace COMMAND.INFRASTRUCTURE;
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}