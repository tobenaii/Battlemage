using System;

namespace Waddle.GameplayBehaviours.Data
{
    [Serializable]
    public struct GameplayEventHash
    {
        public ulong TypeHash;
        public int MethodHash;
    }
}