using UnityEngine;

namespace Waddle.AbilitySystem.Attributes.Authoring
{
    public class WaddleAttribute : ScriptableObject
    {
        [SerializeField] private byte _index;

        public byte Index => _index;
        
        internal void SetIndex(byte index)
        {
            _index = index;
        }
    }
}