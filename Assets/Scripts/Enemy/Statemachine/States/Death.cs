using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : Base
{
    private StateManager controller;
    public Death(StateManager stateController)
    {
        controller = stateController;
    }

    public override void OnEnter() { }
    public override void OnUpdate() { }
    public override void OnExit() { }
}
