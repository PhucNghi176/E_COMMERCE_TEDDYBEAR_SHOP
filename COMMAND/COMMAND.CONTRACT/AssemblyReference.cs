using System.Reflection;

namespace COMMAND.CONTRACT;
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}