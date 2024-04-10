using Battlemage.Networking.Utilities;
using Battlemage.PlayerController.Data;
using Unity.Burst;
using Unity.CharacterController;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using Waddle.FirstPersonCharacter.Data;
using BuildCharacterPredictedRotationSystem = Waddle.FirstPersonCharacter.Systems.BuildCharacterPredictedRotationSystem;
using FirstPersonCharacterVariableUpdateSystem = Waddle.FirstPersonCharacter.Systems.FirstPersonCharacterVariableUpdateSystem;

namespace Battlemage.PlayerController.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(FirstPersonCharacterVariableUpdateSystem))]
    [UpdateAfter(typeof(BuildCharacterPredictedRotationSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct FirstPersonPlayerVariableStepControlSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();   
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Data.PlayerController, PlayerCharacterInputs>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NetworkInputUtilities.GetCurrentAndPreviousTick(SystemAPI.GetSingleton<NetworkTime>(), out var currentTick, out var previousTick);

            var job = new FirstPersonPlayerVariableStepControlJob
            {
                CurrentTick = currentTick,
                PreviousTick = previousTick,
                CharacterControlLookup = SystemAPI.GetComponentLookup<FirstPersonCharacterControl>(),
            };
            state.Dependency = job.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
        public partial struct FirstPersonPlayerVariableStepControlJob : IJobEntity
        {
            public NetworkTick CurrentTick;
            public NetworkTick PreviousTick;
            public ComponentLookup<FirstPersonCharacterControl> CharacterControlLookup;

            void Execute(in DynamicBuffer<InputBufferData<PlayerCharacterInputs>> playerCommandsBuffer, in PlayerCharacterInputs playerCommands, in Data.PlayerController character)
            {
                NetworkInputUtilities.GetCurrentAndPreviousTickInputs(playerCommandsBuffer, CurrentTick, PreviousTick, out var currentTickInputs, out var previousTickInputs);

                if (CharacterControlLookup.HasComponent(character.Value))
                {
                    var characterControl = CharacterControlLookup[character.Value];
            
                    // Look
                    characterControl.LookYawPitchDegrees.x = NetworkInputUtilities.GetInputDelta(currentTickInputs.LookInputDelta.x, previousTickInputs.LookInputDelta.x);
                    characterControl.LookYawPitchDegrees.y = NetworkInputUtilities.GetInputDelta(currentTickInputs.LookInputDelta.y, previousTickInputs.LookInputDelta.y);
        
                    CharacterControlLookup[character.Value] = characterControl;
                }
            }
        }
    }

    [UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct FirstPersonPlayerFixedStepControlSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();   
            state.RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Data.PlayerController, PlayerCharacterInputs>().Build());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new FirstPersonPlayerFixedStepControlJob
            {
                LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true),
                CharacterControlLookup = SystemAPI.GetComponentLookup<FirstPersonCharacterControl>(),
            };
            state.Dependency = job.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
        public partial struct FirstPersonPlayerFixedStepControlJob : IJobEntity
        {
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            public ComponentLookup<FirstPersonCharacterControl> CharacterControlLookup;

            void Execute(in PlayerCharacterInputs playerCommands, in Data.PlayerController player)
            {
                if (CharacterControlLookup.HasComponent(player.Value))
                {
                    var characterControl = CharacterControlLookup[player.Value];

                    var characterRotation = LocalTransformLookup[player.Value].Rotation;

                    // Move
                    var characterForward = math.mul(characterRotation, math.forward());
                    var characterRight = math.mul(characterRotation, math.right());
                    characterControl.MoveVector = (playerCommands.MoveInput.y * characterForward) + (playerCommands.MoveInput.x * characterRight);
                    characterControl.MoveVector = MathUtilities.ClampToMaxLength(characterControl.MoveVector, 1f);

                    // Jump
                    characterControl.Jump = playerCommands.JumpPressed.IsSet;
                
                    CharacterControlLookup[player.Value] = characterControl;
                }
            }
        }
    }
}