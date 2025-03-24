using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : Base
{
    private StateManager controller;
    private Coroutine detectPlayerCoroutine;
    private bool isIdleState = false;

    // animations bools 
    private const string WALK = "WALK";
    private const string WALK_TO_SPRINT = "WALK_TO_SPRINT";

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

        controller.agent.speed = 3.5f;
        controller.agent?.SetDestination(controller.waypoints[randomIndex].transform.position);
        controller.animator?.SetBool(WALK, true);
    }


    public override void OnUpdate()
    {
        // sets back to idle if reaches point 
        if (!controller.agent.pathPending &&
    controller.agent.remainingDistance <= controller.agent.stoppingDistance &&
    !controller.agent.hasPath)
        {
            controller.ChangeState(controller.idleState);
            return;
        }

        detectPlayerCoroutine = controller.StartCoroutine(controller.DetectPlayer(EnterChaseCallback));

    }

    public override void OnExit()
    {
        controller.animator?.SetBool(WALK, false);

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
