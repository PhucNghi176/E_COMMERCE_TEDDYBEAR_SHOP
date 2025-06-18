using System.Reflection;

namespace COMMAND.APPLICATION;
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}