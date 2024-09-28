using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Experimental.AI;
#pragma warning disable CS0618 // Type or member is obsolete

namespace Waddle.Runtime
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct AgentPathfindingSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (navAgent, navTarget, transform, waypoints) in 
                     SystemAPI.Query<RefRW<NavAgent>, RefRO<NavTarget>, RefRO<LocalTransform>, DynamicBuffer<Waypoint>>()
                         .WithChangeFilter<NavTarget>())
            { 
                var fromPosition = transform.ValueRO.Position;
                var toPosition = navTarget.ValueRO.Value;
                navAgent.ValueRW.CurrentWaypoint = 0;
                CalculatePath(fromPosition, toPosition, waypoints, ref state);
            }
        }
        
        private static void CalculatePath(float3 fromPosition, float3 toPosition, DynamicBuffer<Waypoint> waypointBuffer,
        ref SystemState state)
    {
        var query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 1000);


        var extents = new float3(1, 1, 1);

        var fromLocation = query.MapLocation(fromPosition, extents, 0);
        var toLocation = query.MapLocation(toPosition, extents, 0);

        const int maxPathSize = 100;

        if (query.IsValid(fromLocation) && query.IsValid(toLocation))
        {
            var status = query.BeginFindPath(fromLocation, toLocation);
            if (status == PathQueryStatus.InProgress)
            {
                status = query.UpdateFindPath(1000, out _);
                if (status == PathQueryStatus.Success)
                {
                    query.EndFindPath(out var pathSize);

                    var result = new NativeArray<NavMeshLocation>(pathSize + 1, Allocator.Temp);
                    var straightPathFlag = new NativeArray<StraightPathFlags>(maxPathSize, Allocator.Temp);
                    var vertexSide = new NativeArray<float>(maxPathSize, Allocator.Temp);
                    var polygonIds = new NativeArray<PolygonId>(pathSize + 1, Allocator.Temp);
                    var straightPathCount = 0;

                    query.GetPathResult(polygonIds);

                    var returningStatus = PathUtils.FindStraightPath
                    (
                        query,
                        fromPosition,
                        toPosition,
                        polygonIds,
                        pathSize,
                        ref result,
                        ref straightPathFlag,
                        ref vertexSide,
                        ref straightPathCount,
                        maxPathSize
                    );

                    if (returningStatus == PathQueryStatus.Success)
                    {
                        waypointBuffer.Clear();

                        foreach (var location in result)
                        {
                            if (location.position != Vector3.zero)
                            {
                                waypointBuffer.Add(new Waypoint{ Value = location.position });
                            }
                        }
                    }
                    straightPathFlag.Dispose();
                    polygonIds.Dispose();
                    vertexSide.Dispose();
                }
            }
        }
        query.Dispose();
    }
    }
}