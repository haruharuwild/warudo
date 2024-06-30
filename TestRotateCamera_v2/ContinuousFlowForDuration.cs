using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Graphs;
using Warudo.Core.Utils;

[NodeType(
    Id = "af120724-b475-4a40-8f1c-d9fbed53662c",
    Title = "Continuous Flow For Duration",
    Category = "Harutora script"
)]

public class ContinuousFlowForDurationNode : Node {
    private float remainingTime = 0.0f;
    private float elapsedTime = 0.0f;
    private bool isFlowing = false;
    
    [DataInput]
    [Label("Time(sec)")]
    [FloatSlider(0f, 3600f, 0.01f)]
    public float FlowTime = 0;

    [DataOutput]
    public float RemainingTime() => remainingTime;

    [DataOutput]
    public bool IsFlowing() => isFlowing;

    [FlowInput]
    [Label("ENTER")]
    public Continuation Enter() {
        if (!isFlowing) {
            remainingTime = FlowTime;
            elapsedTime = 0.0f;
            isFlowing = true;
        }
        return null;
    }
    [FlowInput]
    [Label("RESET")]
    public Continuation Reset() {
        if (isFlowing) {
            remainingTime = FlowTime;
            elapsedTime = 0.0f;
            isFlowing = false;
        }
        return null;
    }

    [FlowOutput]
    [Label("EXIT")]
    public Continuation Exit;

    protected override void OnCreate() {
        base.OnCreate();
        Watch(nameof(FlowTime), OnFlowTimeChanged);
    }
    public override void OnUpdate() {
        base.OnUpdate();
        if (isFlowing) {
            InvokeFlow(nameof(Exit));
            elapsedTime += Time.deltaTime;
            remainingTime = FlowTime - elapsedTime;
            if (remainingTime<=0.0f) {
                elapsedTime = FlowTime;
                remainingTime = 0.0f;
                isFlowing = false;
            };
        }
    }
    
    public void OnFlowTimeChanged() {
        if (FlowTime<0.0f) FlowTime = 0.0f;
        Broadcast();
    }
}