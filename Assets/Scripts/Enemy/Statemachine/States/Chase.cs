using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : Base
{
    private StateManager controller;
    private GameObject target;
    public Chase(StateManager stateController)
    {
        controller = stateController;
    }

    public override void OnEnter()
    {
        target = GameObject.FindGameObjectsWithTag("Player")[0];
        controller.agent.speed = 10f;
    }

    public override void OnUpdate()
    {
        IsInRange();

        if (target == null) return;
        controller.agent?.SetDestination(target.transform.position);
    }

    public override void OnExit() { }

    // check if the player is still in range 
    private bool IsInRange()
    {
        if (Vector3.Distance(target.transform.position, controller.transform.position) > controller.range)
        {
            Debug.Log("Out of renge");
            return false;
        }
        else
        {
            return true;
        }
    }
}
