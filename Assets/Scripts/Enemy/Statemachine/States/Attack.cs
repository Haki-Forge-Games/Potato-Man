using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : Base
{
    private StateManager controller;
    public Attack(StateManager stateController)
    {
        controller = stateController;
    }

    public override void OnEnter() { }
    public override void OnUpdate() { }
    public override void OnExit() { }
}
