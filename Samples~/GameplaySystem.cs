using UnityEngine;
using Elfinik.BurstTrace;
using Elfinik.BurstTrace.Internal;
using System.Runtime.CompilerServices;

public class GameplaySystem : MonoBehaviour
{
    [Header("Damage History")]
    public float damageValue = 5;
    public TraceHandle damageSender;
    public int damageType = 1;


    private void Start()
    {
        OnUpdate();
    }
    private void OnUpdate()
    {
        FindEnemies(TraceHandle.Capture());
    }
    public void FindEnemies(TraceHandle burstTrace)
    {
        Damage(new DamageEvent { burstTrace = TraceHandle.Capture(burstTrace) });
    }
    public void Damage(DamageEvent damageEvent)
    {
        WriteDamageEvent(damageEvent);
    }

    public void WriteDamageEvent(DamageEvent damageEvent, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
    {
        this.damageSender = BurstTraceAdvanced.RegisterLog(damageEvent.burstTrace, memberName, sourceFilePath, sourceLineNumber);
    }
    public class DamageEvent
    {
        public TraceHandle burstTrace;
    }
}
