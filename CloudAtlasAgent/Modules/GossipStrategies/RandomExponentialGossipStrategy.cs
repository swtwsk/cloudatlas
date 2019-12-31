using System;

namespace CloudAtlasAgent.Modules.GossipStrategies
{
    public class RandomExponentialGossipStrategy : GossipStrategyBase
    {
        protected override int GetZoneIndex(int maxLevel)
        {
            var x = Random.NextDouble();
            // Using discrete geometric distribution, as it decreases exponentially
            var got = Math.Min(
                maxLevel, 
                (int) Math.Floor(Math.Log(x, Math.E) / Math.Log(0.5, Math.E)) + 1);
            return got;
        }
    }
}
