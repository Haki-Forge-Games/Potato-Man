using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : Base
{
    private StateManager controller;
    public Idle(StateManager stateController)
    {
        controller = stateController;
    }

    public override void OnEnter()
    {
        controller.StartCoroutine(WaitToSeeAround());
    }
    public override void OnUpdate() { }
    public override void OnExit() { }

    // wait for sometime before going to next state 
    private IEnumerator WaitToSeeAround()
    {
        yield return new WaitForSeconds(5);

        controller.ChangeState(controller.patrolState);
    }
}
