namespace WowsKarma.Api.Infrastructure.Attributes;

/// <summary>
/// Provides an attribute to lock concurrency on a given action to a given user.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class UserAtomicLockAttribute : Attribute
{
    public UserAtomicLockAttribute() { }
}