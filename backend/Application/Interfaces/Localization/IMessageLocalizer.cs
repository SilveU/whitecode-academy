namespace Application.Interfaces.Localization
{
    public interface IMessageLocalizer<T> where T : class
    {
        string this[string key] { get; }
        string this[string key, params object[] arguments] { get; }
    }
}
