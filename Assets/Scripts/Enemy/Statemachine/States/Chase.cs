using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : Base
{
    private StateManager controller;
    private GameObject target;

    // animations bools 
    private const string SPRINT = "SPRINT";
    private const string WALK_TO_SPRINT = "WALK_TO_SPRINT";

    public Chase(StateManager stateController)
    {
        controller = stateController;
    }

    public override void OnEnter()
    {
        target = GameObject.FindGameObjectsWithTag("Player")[0];
        controller.agent.speed = 10f;

        controller.animator?.SetBool(SPRINT, true);
    }

    public override void OnUpdate()
    {
        if (target == null) return;

        if (!IsInRange())
        {
            controller.ChangeState(controller.patrolState);
            return;
        }

        controller.agent?.SetDestination(target.transform.position);
    }

    public override void OnExit()
    {
        controller.animator?.SetBool(SPRINT, false);
        controller.animator?.SetBool(WALK_TO_SPRINT, false);
    }

    // check if the player is still in range 
    private bool IsInRange()
    {
        if (Vector3.Distance(target.transform.position, controller.transform.position) > controller.range) return false;
        return true;
    }
}
