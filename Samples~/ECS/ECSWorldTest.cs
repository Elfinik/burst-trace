using UnityEngine;
using UnityEngine.UI;
#if BURST_TRACE_ENTITIES_SUPPORT
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;

namespace Elfinik.BurstTrace.Samples
{
    public class ECSWorldTest : MonoBehaviour
    {
        public static ECSWorldTest Instance;
        public Text outputText;

        void Start()
        {
            Instance = this;
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var testSystem = em.World.GetExistingSystemManaged<ECSWorldTestSystemBase>();
            if (testSystem == null)
            {
                testSystem = em.World.CreateSystemManaged<ECSWorldTestSystemBase>();
                var simulationGroup = em.World.GetOrCreateSystemManaged<SimulationSystemGroup>();
                simulationGroup.AddSystemToUpdateList(testSystem);
            }
            var entityFromStart = em.CreateEntity();
            em.SetName(entityFromStart, "Spawned From Start");
            em.AddComponentData(entityFromStart, new SpawnEntitySource { spawnFrom = TraceHandle.Capture() });
            em.AddBuffer<DamageRequest>(entityFromStart);
            em.AddBuffer<DamageRequestHistory>(entityFromStart);
            em.AddComponentData(entityFromStart, new Health { myHP = 10 });
            SpawnEntity(TraceHandle.Capture());
            for (int i = 0; i < 20; i++)
            {
                SpawnEntity(TraceHandle.Capture());
            }
        }
        public void SpawnEntity(TraceHandle prev)
        {
            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entityFromStart = em.CreateEntity();
            em.SetName(entityFromStart, "Spawned From Func");
            em.AddComponentData(entityFromStart, new SpawnEntitySource { spawnFrom = TraceHandle.Capture(prev) });
            em.AddBuffer<DamageRequest>(entityFromStart);
            em.AddBuffer<DamageRequestHistory>(entityFromStart);
            em.AddComponentData(entityFromStart, new Health { myHP = 10 });
        }
    }
    public struct SpawnEntitySource : IComponentData
    {
        public TraceHandle spawnFrom;
    }
    public struct DamageRequestHistory : IBufferElementData
    {
        public TraceHandle sendFrom;
        public float damageTime;
        public float damageValue;
        public bool processed;
    }
    public struct DamageRequest : IBufferElementData
    {
        public TraceHandle sendFrom;
        public float damageTime;
        public float damageValue;
        public bool processed;
    }
    public struct Health : IComponentData
    {
        public float myHP;
        public TraceHandle destroyedStackTrace;
    }
    public struct HistoryElement_Destroy : IBufferElementData
    {
        public Entity destroyedEntity;
        public TraceHandle lastDamageStackTrace;
        public TraceHandle destroyStackTrace;
    }

    [DisableAutoCreation]
    public partial class ECSWorldTestSystemBase : SystemBase
    {
        float nextUpdateTime = 0;
        private EntityQuery _query;
        private Entity destroyHistorySingleton;

        protected override void OnCreate()
        {
            _query = new EntityQueryBuilder(Unity.Collections.Allocator.Temp)
                   .WithAll<Health>()
                   .Build(this);
            destroyHistorySingleton = EntityManager.CreateEntity();
            EntityManager.SetName(destroyHistorySingleton, "History (Destroy)");
            EntityManager.AddBuffer<HistoryElement_Destroy>(destroyHistorySingleton);
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnUpdate()
        {
            var destroyHistorySingleton = this.destroyHistorySingleton;
            if (ECSWorldTest.Instance != null)
            {
                var historyBuffer = EntityManager.GetBuffer<HistoryElement_Destroy>(destroyHistorySingleton);
                var text = ECSWorldTest.Instance.outputText;
                text.text = "";
                if (historyBuffer.IsEmpty)
                    text.text = $"Here will be the history of destroyed entities";
                foreach (var item in historyBuffer)
                {
                    text.text += $"Entity {item.destroyedEntity} destroyed:\r\n{item.destroyStackTrace.ToProjectLink()}\r\nLast damage:\r\n{item.lastDamageStackTrace.ToProjectLink()}\r\n\r\n";
                }
            }

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var commandBuffer = ecbSingleton.CreateCommandBuffer(this.CheckedStateRef.WorldUnmanaged);
            var commandBufferPW = commandBuffer.AsParallelWriter();
            var systemLog = TraceHandle.Capture();

            if (SystemAPI.Time.ElapsedTime > nextUpdateTime)
            {
                var thisTime = (float)SystemAPI.Time.ElapsedTime;
                nextUpdateTime = (float)SystemAPI.Time.ElapsedTime + 1;
                Dependency = Entities.ForEach((ref DynamicBuffer<DamageRequest> damageRequests) =>
                {
                    damageRequests.Add(new DamageRequest
                    {
                        damageTime = thisTime,
                        damageValue = 1,
                        sendFrom = TraceHandle.Capture(systemLog)
                    });
                }).ScheduleParallel(Dependency);
                var job = new DelayedDamageJob
                {
                    systemLog = TraceHandle.Capture(systemLog),
                    ECB = commandBufferPW,
                    EntityType = SystemAPI.GetEntityTypeHandle(),
                    gameTime = thisTime,
                    rnd = new Unity.Mathematics.Random(1 + (uint)UnityEngine.Time.frameCount),
                };
                Dependency = job.ScheduleParallel(_query, Dependency);
            }
            Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, ref Health health, ref DynamicBuffer<DamageRequest> damageRequests, ref DynamicBuffer<DamageRequestHistory> damageRequestsHistory) =>
            {
                if (damageRequests.IsEmpty) return;
                foreach (var item in damageRequests)
                {
                    damageRequestsHistory.Add(new DamageRequestHistory { damageTime = item.damageTime, damageValue = item.damageValue, processed = item.processed, sendFrom = item.sendFrom });
                    health.myHP -= item.damageValue;
                    if (health.myHP <= 0)
                    {
                        commandBuffer.DestroyEntity(entity);
                        health.destroyedStackTrace = item.sendFrom;
                        Debug.LogError($"Entity {entity} destroyed. From {item.sendFrom.ToConsoleToken() }");
                        break;
                    }
                }
                damageRequests.Clear();
                if (damageRequestsHistory.Length > 10)
                {
                    damageRequestsHistory.RemoveAt(0);
                }
            }).Schedule(Dependency);
            Dependency = Entities.ForEach((ref DynamicBuffer<HistoryElement_Destroy> history) =>
            {
                while(history.Length > 50)
                    history.RemoveAt(0);
            }).Schedule(Dependency);
            Dependency = Entities.ForEach((Entity entity, int entityInQueryIndex, in Health health) =>
            {
                if (health.myHP <= 0)
                {
                    commandBuffer.DestroyEntity(entity);
                    commandBuffer.AppendToBuffer(destroyHistorySingleton, new HistoryElement_Destroy
                    {
                        destroyedEntity = entity,
                        destroyStackTrace = TraceHandle.Capture(),
                        lastDamageStackTrace = health.destroyedStackTrace
                    });
                }
            }).Schedule(Dependency);
        }
    }

    [BurstCompile]
    public struct DelayedDamageJob : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        [ReadOnly] public EntityTypeHandle EntityType;
        public Unity.Mathematics.Random rnd;
        public float gameTime;
        public TraceHandle systemLog;

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            var entities = chunk.GetNativeArray(EntityType);
            for (int i = 0; i < chunk.Count; i++)
            {
                ECB.AppendToBuffer(unfilteredChunkIndex, entities[i], new DamageRequest
                {
                    damageTime = gameTime,
                    damageValue = rnd.NextInt(1, 3),
                    sendFrom =
                    TraceHandle.Capture(systemLog)
                });
            }
        }
    }
}
#else
namespace Elfinik.BurstTrace.Samples
{
    public class ECSWorldTest : MonoBehaviour
    {
        public Text outputText;
    }
}
#endif