using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : Base
{
    private StateManager controller;
    private const string IDLE_TO_WALK = "IDLE_TO_WALK";
    private const string WALK_TO_SPRINT = "WALK_TO_SPRINT";


    private Coroutine detectPlayerCoroutine;
    private bool isIdleState = false;

    public Patrol(StateManager stateController)
    {
        controller = stateController;
    }

    public override void OnEnter()
    {
        if (controller.waypoints == null || controller.waypoints.Count == 0)
        {
            controller.ChangeState(controller.idleState);
            return;
        }

        int randomIndex = GetRandomPath(controller.waypoints.Count);

        if (randomIndex == -1)
        {
            controller.ChangeState(controller.idleState);
            return;
        }

        controller.agent?.SetDestination(controller.waypoints[randomIndex].transform.position);
        controller.animator?.SetBool(IDLE_TO_WALK, true);
    }


    public override void OnUpdate()
    {
        // sets back to idle if reaches point 
        if (!controller.agent.pathPending &&
    controller.agent.remainingDistance <= controller.agent.stoppingDistance &&
    !controller.agent.hasPath)
        {
            isIdleState = true;
            controller.ChangeState(controller.idleState);
            return;
        }

        detectPlayerCoroutine = controller.StartCoroutine(controller.DetectPlayer(EnterChaseCallback));

    }

    public override void OnExit()
    {
        if (isIdleState)
        {
            controller.animator?.SetBool(IDLE_TO_WALK, false);
            isIdleState = false;
        }

        if (detectPlayerCoroutine != null)
        {
            controller.StopCoroutine(detectPlayerCoroutine);
        }
    }

    // give a random number based on given count 
    private int GetRandomPath(int pathCount = 0)
    {
        if (pathCount == 0) return -1;

        int randomIndex = Random.Range(0, pathCount);
        return randomIndex;
    }

    private void EnterChaseCallback()
    {
        controller.animator?.SetBool(WALK_TO_SPRINT, true);
    }
}
