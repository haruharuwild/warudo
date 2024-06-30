using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
using Warudo.Core.Localization;

[NodeType(
    Id = "60fc3a3f-09c5-4de7-8646-0be758518de3",
    Title = "Multi If Branch",
    Category = "Harutora script"
)]
public class MultiIfBranchNode : Node {

    private const int ConditionNumberMin = 1;
    private const int ConditionNumberMax = 9;

    [DataInput]
    [IntegerSlider(ConditionNumberMin, ConditionNumberMax)]
    public int ConditionNumber = ConditionNumberMin;

    [FlowInput]
    [Label("ENTER")]
    public Continuation Enter() {
        for (var i = 1; i <= ConditionNumber; i++) {
            if (GetDataInput<Boolean>("Condition" + i)) {
                InvokeFlow("Exit" + i);
                return null;
            }
        }
        InvokeFlow("ExitAllFalse");
        return null;
    }
    
    protected override void OnCreate() {
        base.OnCreate();
        Watch(nameof(ConditionNumber), OnConditionNumberChanged);
        OnConditionNumberChanged();
    }
    
    public void OnConditionNumberChanged() {
        
        ConditionNumber = Math.Clamp(ConditionNumber, ConditionNumberMin, ConditionNumberMax);

        // Build DataInputPort
        var conditionNumPort = DataInputPortCollection.GetPort(nameof(ConditionNumber));
        DataInputPortCollection.GetPorts().Clear();
        DataInputPortCollection.AddPort(conditionNumPort);
        for (var i = 1; i <= ConditionNumber; i++) {
            AddDataInputPort<Boolean>("Condition" + i, false, new DataInputProperties {
                label = "CONDITION".Localized() + " " + i
            });
        }
        // Build FlowOutputPort
        FlowOutputPortCollection.GetPorts().Clear();
        for (var i = 1; i <= ConditionNumber; i++) {
            AddFlowOutputPort("Exit" + i, new FlowOutputProperties {
                label = "EXIT".Localized() + " " + i,
                order = i
            });
        }
        AddFlowOutputPort("ExitAllFalse", new FlowOutputProperties {
            label = "IF_FALSE".Localized(),
            order = ConditionNumber + 1
        });
        Broadcast();
    }
}