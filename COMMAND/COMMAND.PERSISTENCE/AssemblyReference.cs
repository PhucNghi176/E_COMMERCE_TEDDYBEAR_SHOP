using System.Reflection;

namespace COMMAND.PERSISTENCE;
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}