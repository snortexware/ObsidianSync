using G.Sync.Service.MessageFactory.Strategy;
using G.Sync.Service.MessageFactory.Strategy.Enumerators;

namespace G.Sync.Service.MessageFactory
{
    public class MessageHandlerFactory
    {
        private readonly Dictionary<HandlerType, IHandler> _handlers = new()
        {
            { HandlerType.Vaults, new VaultsHandler() }
        };

        public IHandler CreateHandler(HandlerType type)
        {
            if (_handlers.TryGetValue(type, out var handler))
            {
                return handler;
            }
            else
            {
                throw new NotSupportedException($"Handler for type {type} is not supported.");
            }
        }
    }
}
