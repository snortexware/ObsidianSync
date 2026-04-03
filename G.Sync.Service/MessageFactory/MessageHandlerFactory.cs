using G.Sync.Service.MessageFactory.Strategy;
using G.Sync.Service.MessageFactory.Strategy.Enumerators;

namespace G.Sync.Service.MessageFactory
{
    public class MessageHandlerFactory(IEnumerable<IHandler> handlers) : IMessageHandlerFactory
    {
        private readonly Dictionary<HandlerType, IHandler> _handlers = handlers.ToDictionary(h => h.Type);

        public IHandler CreateHandler(HandlerType type)
        {
            if (_handlers.TryGetValue(type, out var handler))
                return handler;

            throw new NotSupportedException($"Handler for type {type} is not supported.");
        }
    }
}
