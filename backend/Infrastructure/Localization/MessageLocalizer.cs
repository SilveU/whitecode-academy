using Application.Interfaces.Localization;
using Microsoft.Extensions.Localization;

namespace Infrastructure.Localization
{
    public class MessageLocalizer<T> : IMessageLocalizer<T> where T : class
    {
        private readonly IStringLocalizer<T> _localizer;

        public MessageLocalizer(IStringLocalizer<T> localizer)
        {
            _localizer = localizer;
        }

        public string this[string key] => _localizer[key];

        public string this[string key, params object[] arguments] => _localizer[key, arguments];
    }
}
