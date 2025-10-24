using EtT.Core;

namespace EtT.Systems
{
    public sealed class Link13Service : ILink13Service
    {
        public void SendSystem(string msg) => GameEvents.RaiseLink13(msg);
    }
}