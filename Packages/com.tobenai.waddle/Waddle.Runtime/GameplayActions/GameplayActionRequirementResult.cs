using Unity.Entities;

namespace Waddle.Runtime.GameplayActions
{
    public struct GameplayActionRequirementResult : IComponentData
    {
        private int _result;

        public readonly bool HasSucceeded(int requirementIndices)
        {
            return (_result & requirementIndices) == requirementIndices;
        }

        public void SetResult(int index, bool b)
        {
            var mask = 1 << index;
            _result = b ? (_result | mask) : (_result & ~mask);
        }
    }
}