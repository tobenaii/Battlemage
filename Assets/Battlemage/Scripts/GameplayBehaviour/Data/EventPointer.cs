using System;
using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data
{
    public struct EventPointer
    {
        public IntPtr Pointer;
        
        public unsafe void Invoke(ref GameplayState gameplayState, ref Entity self)
        {
            ((delegate* unmanaged[Cdecl]<ref GameplayState, ref Entity, void>)Pointer)(ref gameplayState, ref self);
        }
        
        public unsafe void Invoke<T3>(ref GameplayState gameplayState, ref Entity self, ref T3 arg3)
        {
            ((delegate* unmanaged[Cdecl]<ref GameplayState, ref Entity, ref T3, void>)Pointer)(ref gameplayState, ref self, ref arg3);
        }
    }
}