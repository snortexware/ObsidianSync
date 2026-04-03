using G.Sync.Service.MessageFactory.Strategy;
using G.Sync.Service.MessageFactory.Strategy.Enumerators;

namespace G.Sync.Service.MessageFactory
{
    public interface IMessageHandlerFactory
    {
        IHandler CreateHandler(HandlerType type);
    }
}