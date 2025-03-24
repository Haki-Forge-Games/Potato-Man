using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Idle : Base
{
    private StateManager controller;

    private Coroutine detectPlayerCoroutine;
    private Coroutine seeAroundCoroutine;

    public Idle(StateManager stateController)
    {
        controller = stateController;
    }

    public override void OnEnter()
    {
        seeAroundCoroutine = controller.StartCoroutine(WaitToSeeAround());
    }
    public override void OnUpdate()
    {
        detectPlayerCoroutine = controller.StartCoroutine(controller.DetectPlayer());
    }
    public override void OnExit()
    {
        if (seeAroundCoroutine != null)
        {
            controller.StopCoroutine(seeAroundCoroutine);
        }

        if (detectPlayerCoroutine != null)
        {
            controller.StopCoroutine(detectPlayerCoroutine);
        }
    }

    // wait for sometime before going to next state 
    private IEnumerator WaitToSeeAround()
    {
        yield return new WaitForSeconds(5);
        controller.ChangeState(controller.patrolState);
    }
}
