using System;
using System.Collections.Generic;
using Unity.Entities;

namespace Waddle.GameplayBehaviours.Data
{
    public class WaitForSecondsBuffer : IComponentData
    {
        public List<WaitForSeconds> Seconds = new();
    }
    
    public struct WaitForSeconds
    {
        public float Seconds;
        public Action Continuation;
    }
}