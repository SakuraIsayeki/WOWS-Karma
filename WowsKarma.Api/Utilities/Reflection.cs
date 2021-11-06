namespace WowsKarma.Api.Utilities;

public static class Reflection
{
	public static bool ImplementsInterface(this Type type, Type interfaceType) => type.GetInterfaces().Any(t => t == interfaceType);
}
