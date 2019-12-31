using System;
using System.Collections.Generic;

namespace CloudAtlasAgent.Modules.GossipStrategies
{
    public class RoundRobinExponentialGossipStrategy : GossipStrategyBase
    {
        private int _currentMax = -1;
        private List<int> _exponentialIndexes;
        private int _currentIndex = 0;
        
        protected override int GetZoneIndex(int maxLevel)
        {
            if (_currentMax != maxLevel)
                PrepareNewExponentials(maxLevel);

            var toRet = _exponentialIndexes[_currentIndex];
            _currentIndex = (_currentIndex + 1) % _exponentialIndexes.Count;
            return toRet;
        }

        private void PrepareNewExponentials(int maxLevel)
        {
            _currentMax = maxLevel;
            var sum = (int) Math.Pow(2, maxLevel) - 1;
            _exponentialIndexes = new List<int>(sum);

            var j = 0;
            var k = 1;
            var power = maxLevel - 1;
            var currentSum = (int) Math.Pow(2, power);
            for (var i = 0; i < sum; i++, j++)
            {
                if (j >= currentSum)
                {
                    k++;
                    power--;
                    currentSum = (int) Math.Pow(2, power);
                    j = 0;
                }
                _exponentialIndexes.Add(k);
            }
        }
    }
}
