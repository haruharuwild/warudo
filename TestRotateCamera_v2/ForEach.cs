using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Graphs;
using Warudo.Core.Localization;

[NodeType(
    Id = "0b1efc37-42c2-400e-a1b3-72852e880fa8",
    Title = "For Each",
    Category = "Harutora script"
)]

public class ForEachNode : Node {

    private int? index = null;
    private bool isRunning = false;
    private bool isCompleted = false;
    private object outputData = null;
    private Type type;
    private Type arrayType;
    
    [DataInput]
    [Label("TYPE")]
    public ForEachType Type = ForEachType.Integer;

    [DataInput]
    [Label("LOOP")]
    public Boolean Loop;

    [DataOutput]
    [Label("INDEX")]
    public int? Index() => index;

    [DataOutput]
    public bool IsRunning() => isRunning;

    [DataOutput]
    public bool IsCompleted() => isCompleted;

    [FlowInput]
    [Label("ENTER")]
    public Continuation Enter() {
        if (isCompleted) return Exit;
        var list = (Array)Convert.ChangeType(GetDataInput("List"), arrayType);
        if (!isRunning) {
            isRunning = true;
            index = 0;
        } else {
            index++;
            if (index==list.Length) {
                if (GetDataInput<Boolean>("Loop")) {
                    index = 0;
                } else {
                    isCompleted = true;
                    isRunning = false;
                }
            }
        }
        if (isRunning) {
            outputData = list.GetValue((int)index);
        } else {
            outputData = null;
        }
        return Exit;
    }

    [FlowInput]
    [Label("RESET")]
    public Continuation Reset() {
        StatusReset();
        return null;
    }
    
    [FlowOutput]
    [Label("EXIT")]
    public Continuation Exit;

    protected override void OnCreate() {
        base.OnCreate();
        Watch(nameof(Type), OnTypeChanged);
        OnTypeChanged();
    }
    
    public void OnTypeChanged() {
        // reset
        StatusReset();
        // get type
        type = GetTypeFromForEachType(GetDataInput<ForEachType>(nameof(Type)));
        // Rebuild the Input Port "List"
        DataInputPortCollection.RemovePort("List");
        arrayType = type.MakeArrayType();
        Array arrayInstance = Array.CreateInstance(type, 0);
        AddDataInputPort("List", arrayType, arrayInstance, new DataInputProperties {
            label = "LIST".Localized()
        });
        // Rebuild the Output Port "OutputData"
        DataOutputPortCollection.RemovePort("OutputData");
        AddDataOutputPort("OutputData", type, () => outputData, new DataOutputProperties {
            label = "OUTPUT_DATA".Localized()
        });
        Broadcast();
    }
    private void StatusReset() {
        index = null;
        isRunning = false;
        isCompleted = false;
    }
    private Type GetTypeFromForEachType(ForEachType forEachType)
    {
        switch (forEachType)
        {
            case ForEachType.Boolean:
                return typeof(Boolean);
            case ForEachType.Integer:
                return typeof(int);
            case ForEachType.Float:
                return typeof(float);
            case ForEachType.String:
                return typeof(string);
            case ForEachType.Vector3:
                return typeof(Vector3);
            default:
                throw new ArgumentException("Invalid ForEachType");
        }
    }
    public enum ForEachType {
        Boolean,
        Integer,
        Float,
        String,
        Vector3,
    }
}