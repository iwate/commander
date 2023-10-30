namespace Iwate.AzureFunctions.Middlewares.Commander;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class PartitionLockAttribute : Attribute
{
    public PartitionLockAttribute()
    {
    }
}
