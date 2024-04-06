using Battlemage.Movement.Data;
using Battlemage.Movement.Utilities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.CharacterController;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using CharacterAspect = Battlemage.Movement.Data.CharacterAspect;
using FirstPersonPlayerVariableStepControlSystem = Battlemage.PlayerController.Systems.FirstPersonPlayerVariableStepControlSystem;

namespace Battlemage.Movement.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(PredictedFixedStepSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct BuildCharacterPredictedRotationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<LocalTransform, CharacterSettings>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new BuildCharacterPredictedRotationJob();
            state.Dependency = job.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
        public partial struct BuildCharacterPredictedRotationJob : IJobEntity
        {
            void Execute(ref LocalTransform localTransform, in CharacterViewRotation characterViewRotation)
            {
                CharacterUtilities.ComputeRotationFromYAngleAndUp(characterViewRotation.CharacterYDegrees, math.up(), out var tmpRotation);
                localTransform.Rotation = tmpRotation;
            }
        }
    }

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(TransformSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [BurstCompile]
    public partial struct BuildCharacterInterpolatedRotationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<LocalTransform, CharacterSettings>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new BuildCharacterInterpolatedRotationJob
            {
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(),
            };
            state.Dependency = job.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithNone(typeof(GhostOwnerIsLocal))]
        public partial struct BuildCharacterInterpolatedRotationJob : IJobEntity
        {
            public ComponentLookup<LocalTransform> LocalTransformLookup;
        
            void Execute(Entity entity, in CharacterSettings characterSettings, in CharacterViewRotation characterViewRotation)
            {
                if (LocalTransformLookup.TryGetComponent(entity, out var characterLocalTransform))
                {
                    CharacterUtilities.ComputeRotationFromYAngleAndUp(characterViewRotation.CharacterYDegrees, math.up(), out var tmpRotation);
                    characterLocalTransform.Rotation = tmpRotation;
                    LocalTransformLookup[entity] = characterLocalTransform;

                    if (LocalTransformLookup.TryGetComponent(characterSettings.ViewEntity, out var viewLocalTransform))
                    {
                        viewLocalTransform.Rotation = CharacterUtilities.CalculateLocalViewRotation(characterViewRotation.ViewPitchDegrees, 0f);
                        LocalTransformLookup[characterSettings.ViewEntity] = viewLocalTransform;
                    }
                }
            }
        }
    }

    [UpdateInGroup(typeof(KinematicCharacterPhysicsUpdateGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct FirstPersonCharacterPhysicsUpdateSystem : ISystem
    {
        private EntityQuery _characterQuery;
        private FirstPersonCharacterUpdateContext _context;
        private KinematicCharacterUpdateContext _baseContext;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _characterQuery = KinematicCharacterUtilities.GetBaseCharacterQueryBuilder()
                .WithAll<
                    CharacterSettings,
                    FirstPersonCharacterControl>()
                .Build(ref state);
        
            _context = new FirstPersonCharacterUpdateContext();
            _context.OnSystemCreate(ref state);
            
            _baseContext = new KinematicCharacterUpdateContext();
            _baseContext.OnSystemCreate(ref state);

            state.RequireForUpdate(_characterQuery);
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.HasSingleton<NetworkTime>())
                return;
        
            _context.OnSystemUpdate(ref state);
            _baseContext.OnSystemUpdate(ref state, SystemAPI.Time, SystemAPI.GetSingleton<PhysicsWorldSingleton>());

            var job = new FirstPersonCharacterPhysicsUpdateJob
            {
                Context = _context,
                BaseContext = _baseContext,
            };
            state.Dependency = job.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
        public partial struct FirstPersonCharacterPhysicsUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public FirstPersonCharacterUpdateContext Context;
            public KinematicCharacterUpdateContext BaseContext;
    
            void Execute(CharacterAspect characterAspect)
            {
                characterAspect.PhysicsUpdate(ref Context, ref BaseContext);
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                BaseContext.EnsureCreationOfTmpCollections();
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
            { }
        }
    }

    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(PredictedFixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(FirstPersonPlayerVariableStepControlSystem))]
    [UpdateAfter(typeof(BuildCharacterPredictedRotationSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct FirstPersonCharacterVariableUpdateSystem : ISystem
    {
        private EntityQuery _characterQuery;
        private FirstPersonCharacterUpdateContext _context;
        private KinematicCharacterUpdateContext _baseContext;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _characterQuery = KinematicCharacterUtilities.GetBaseCharacterQueryBuilder()
                .WithAll<
                    CharacterSettings,
                    FirstPersonCharacterControl>()
                .Build(ref state);
        
            _context = new FirstPersonCharacterUpdateContext();
            _context.OnSystemCreate(ref state);
            
            _baseContext = new KinematicCharacterUpdateContext();
            _baseContext.OnSystemCreate(ref state);

            state.RequireForUpdate(_characterQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _context.OnSystemUpdate(ref state);
            _baseContext.OnSystemUpdate(ref state, SystemAPI.Time, SystemAPI.GetSingleton<PhysicsWorldSingleton>());
        
            var variableUpdateJob = new FirstPersonCharacterVariableUpdateJob
            {
                Context = _context,
                BaseContext = _baseContext,
            };
            state.Dependency = variableUpdateJob.Schedule(state.Dependency);
        
            var viewJob = new FirstPersonCharacterViewJob
            {
                CharacterViewRotationLookup = SystemAPI.GetComponentLookup<CharacterViewRotation>(true),
            };
            state.Dependency = viewJob.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
        public partial struct FirstPersonCharacterVariableUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
        {
            public FirstPersonCharacterUpdateContext Context;
            public KinematicCharacterUpdateContext BaseContext;
    
            void Execute(CharacterAspect characterAspect)
            {
                characterAspect.VariableUpdate(ref Context, ref BaseContext);
            }

            public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                BaseContext.EnsureCreationOfTmpCollections();
                return true;
            }

            public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask, bool chunkWasExecuted)
            { }
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
        public partial struct FirstPersonCharacterViewJob : IJobEntity
        {
            [ReadOnly] 
            public ComponentLookup<CharacterViewRotation> CharacterViewRotationLookup;

            void Execute(ref LocalTransform localTransform, in CharacterView characterView)
            {
                if (CharacterViewRotationLookup.TryGetComponent(characterView.CharacterEntity, out var characterViewRotation))
                {
                    localTransform.Rotation = characterViewRotation.ViewLocalRotation;
                }
            }
        }
    }
}