namespace API.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class SkipTokenRevocationAttribute : Attribute
    {
        
    }
}