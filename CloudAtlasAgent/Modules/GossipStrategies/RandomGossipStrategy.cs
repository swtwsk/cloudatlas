namespace CloudAtlasAgent.Modules.GossipStrategies
{
    public class RandomGossipStrategy : GossipStrategyBase
    {
        protected override int GetZoneIndex(int maxLevel) => Random.Next(1, maxLevel);
    }
}
