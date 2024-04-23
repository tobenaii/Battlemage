using Battlemage.Attributes.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Waddle.Abilities.Data;
using Waddle.GameplayActions.Data;
using Waddle.GameplayAttributes.Data;
using Waddle.GameplayAttributes.Extensions;
using Waddle.Utilities;

namespace Battlemage.GameplayBehaviours.Data
{
    public ref struct GameplayState
    {
        private EntityManager _entityManager;
        private EntityCommandBuffer _ecb;
        private readonly TimeData _time;
        private readonly BlittableBool _isServer;
        
        public GameplayState(EntityManager entityManager, EntityCommandBuffer ecb, TimeData time, BlittableBool isServer)
        {
            _entityManager = entityManager;
            _ecb = ecb;
            _time = time;
            _isServer = isServer;
        }

        public T GetComponent<T>(Entity entity) where T : unmanaged, IComponentData
        {
            return _entityManager.GetComponentData<T>(entity);
        }
        
        public void SetComponent<T>(Entity entity, T component) where T : unmanaged, IComponentData
        {
            _entityManager.SetComponentData(entity, component);
        }

        public FixedString64Bytes GetEntityName(Entity entity)
        {
            _entityManager.GetName(entity, out var name);
            return name;
        }

        public void TryActivateAbility(Entity entity, Entity abilityPrefab)
        {
            var abilityRequirements =
                _entityManager.GetBuffer<AbilityActivationAttributeRequirement>(abilityPrefab);
            var requirements = _entityManager.GetBuffer<GameplayActionRequirement>(entity);
            var requests = _entityManager.GetBuffer<ActivateAbilityRequest>(entity);
            var requirementIndices = 0;
            
            foreach (var abilityRequirement in abilityRequirements)
            {
                requirements.Add(new GameplayActionRequirement()
                {
                    Attribute = abilityRequirement.Attribute,
                    Amount = abilityRequirement.Amount
                });
            
                requirementIndices |= (1 << requirements.Length - 1);
            }
            requests.Add(new ActivateAbilityRequest()
            {
                AbilityPrefab = abilityPrefab,
                RequirementIndices = requirementIndices
            });
        }
        
        public void MarkForDestroy(Entity entity)
        {
            if (_isServer)
                _ecb.DestroyEntity(entity);
            else
                _ecb.SetEnabled(entity, false);
        }

        public void DealDamage(Entity source, float amount, Entity target)
        {
            var attributeMap = _entityManager.GetBuffer<GameplayAttributeMap>(target).AsMap();
            var health = attributeMap[(byte)BattlemageAttribute.Health];
            health.CurrentValue -= amount;
            attributeMap[(byte)BattlemageAttribute.Health] = health;
        }

        public void ScheduleEvent(Entity entity, float time, FixedString64Bytes scheduledEvent)
        {
            _entityManager.GetBuffer<GameplayScheduledEvent>(entity).Add(new GameplayScheduledEvent()
            {
                TypeHash = TypeManager.GetTypeInfo<GameplayScheduledEvent>().StableTypeHash,
                MethodHash = scheduledEvent.GetHashCode(),
                Time = time,
                TimeStarted = _time.ElapsedTime
            });
        }
        
        public void AddOverlapSphereCallback(Entity entity, float radius, FixedString64Bytes scheduledEvent)
        {
            _entityManager.GetBuffer<GameplayOnHitEvent>(entity).Add(new GameplayOnHitEvent()
            {
                TypeHash = TypeManager.GetTypeInfo<GameplayOnHitEvent>().StableTypeHash,
                MethodHash = scheduledEvent.GetHashCode(),
                Radius = radius
            });
        }

        public float3 GetForward(Entity entity)
        {
            return _entityManager.GetComponentData<LocalTransform>(entity).Forward();
        }
        
        public float3 GetRight(Entity entity)
        {
            return _entityManager.GetComponentData<LocalTransform>(entity).Right();
        }
    }
}