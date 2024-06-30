using UnityEngine;
using Warudo.Core;
using Warudo.Core.Attributes;
using Warudo.Core.Data;
using Warudo.Core.Data.Models;
using Warudo.Core.Graphs;
using Warudo.Core.Utils;

[NodeType(
    Id = "aea0fa04-6873-49cb-99d6-dc35b44e37b7",
    Title = "Count Up",
    Category = "Harutora script"
)]

public class CountUpNode : Node {
    private int count = 0;
    
    [DataInput]
    public int Start = 0;

    [DataInput]
    public int End = 5;

    [DataInput]
    public bool loop = false;

    [DataOutput]
    public int Count() => count;

    [FlowInput]
    [Label("ENTER")]
    public Continuation Enter() {
        if (count==End) {
            if (loop) count = Start;
        } else {
            count++;
        }
        return Exit;
    }

    [FlowInput]
    [Label("RESET")]
    public Continuation Reset() {
        count = Start;
        return null;
    }

    [FlowOutput]
    [Label("EXIT")]
    public Continuation Exit;

    protected override void OnCreate() {
        base.OnCreate();
        Watch<int>(nameof(Start), OnStartChanged);
        Watch<int>(nameof(End), OnEndChanged);
    }
    
    public void OnStartChanged(int from, int to) {
        if (to>=End) Start = from;
        Broadcast();
    }
    
    public void OnEndChanged(int from, int to) {
        if (to<=Start) End = from;
        Broadcast();
    }
}