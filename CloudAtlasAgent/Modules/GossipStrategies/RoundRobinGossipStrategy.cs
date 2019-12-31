namespace CloudAtlasAgent.Modules.GossipStrategies
{
    public class RoundRobinGossipStrategy : GossipStrategyBase
    {
        private int _nextIndex = 1;
        
        protected override int GetZoneIndex(int maxLevel)
        {
            var nextInd = _nextIndex;
            
            _nextIndex = (_nextIndex + 1) % maxLevel;
            if (_nextIndex == 0)
                _nextIndex = maxLevel;
            
            return nextInd;
        }
    }
}
