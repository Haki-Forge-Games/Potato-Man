using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : Base
{
    private StateManager controller;
    private GameObject closestPlayer = null;

    // animations bools 
    private const string SPRINT = "SPRINT";
    private const string WALK_TO_SPRINT = "WALK_TO_SPRINT";

    public Chase(StateManager stateController)
    {
        controller = stateController;
    }

    public override void OnEnter()
    {
        controller.agent.speed = 10f;
        controller.animator?.SetBool(SPRINT, true);
        FindClosestPlayer();
    }

    public override void OnUpdate()
    {
        if (!IsInRange())
        {
            controller.ChangeState(controller.patrolState);
            return;
        }

        FindClosestPlayer();
    }

    public override void OnExit()
    {
        controller.animator?.SetBool(SPRINT, false);
        controller.animator?.SetBool(WALK_TO_SPRINT, false);
    }

    // check if the player is still in range 
    private bool IsInRange()
    {
        if (closestPlayer == null) return false;
        if (Vector3.Distance(closestPlayer.transform.position, controller.transform.position) > controller.range) return false;
        return true;
    }

    private void FindClosestPlayer()
    {
        float minDistance = float.MaxValue; // Initialize with the highest value

        foreach (var player in GameObject.FindGameObjectsWithTag("Player"))
        {
            float distance = (player.transform.position - controller.transform.position).sqrMagnitude; // Distance from the enemy
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player;
            }
        }

        if (closestPlayer == null) return;
        controller.agent?.SetDestination(closestPlayer.transform.position);
    }

}
