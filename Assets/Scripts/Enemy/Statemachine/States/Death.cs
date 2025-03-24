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

    public override void OnEnter()
    {
        SetState();
    }

    public override void OnUpdate() { }
    public override void OnExit()
    {
        SetState(true);
    }

    //  set up ragdoll components 
    private void SetState(bool state = false)
    {
        if (controller.animator == null || controller.mainCollider == null || controller.agent == null) return;
        controller.agent.ResetPath();
        controller.animator.enabled = state;
        controller.mainCollider.enabled = state;

        if (controller.rigidbodies == null || controller.rigidbodies.Count <= 0) return;
        foreach (Rigidbody rb in controller.rigidbodies)
        {
            rb.isKinematic = state;
            rb.detectCollisions = !state;
        }

        controller.StartCoroutine(WaitForAwakeAgain());
    }

    private IEnumerator WaitForAwakeAgain()
    {
        yield return new WaitForSeconds(controller.deathTimeInSeconds);
        controller.ChangeState(controller.idleState);
    }
}
