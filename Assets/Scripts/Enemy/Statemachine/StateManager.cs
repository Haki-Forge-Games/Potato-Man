using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

using System;
using System.Collections;
using System.Collections.Generic;

public class StateManager : NetworkBehaviour
{
    [Header("References")]
    public Animator animator;
    public NavMeshAgent agent;

    [Header("Way Points")]
    public List<GameObject> waypoints = new List<GameObject>();

    [Header("Settings")]
    public float range = 50f;

    // current state 
    private Base currentState;

    // check weather in online or offline mode 
    private bool IsOnlineMode => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

    // States 
    public Idle idleState;
    public Patrol patrolState;
    public Chase chaseState;
    public Death deathState;


    private void Start()
    {
        InitializeSates();

        // setting up starting state 
        if (patrolState != null)
        {
            ChangeState(patrolState);
        }
    }

    private void Update()
    {
        if (IsOnlineMode && !IsServer) return;
        currentState?.OnUpdate();
    }

    private void InitializeSates()
    {
        // creating obj of the state classes 

        idleState = new Idle(this);
        patrolState = new Patrol(this);
        chaseState = new Chase(this);
        deathState = new Death(this);
    }

    public void ChangeState(Base newState = null, Action startCallback = null)
    {
        if (IsOnlineMode && !IsServer) return;
        if (newState == null) return;

        currentState?.OnExit();
        currentState = newState;
        currentState?.OnEnter();

        if (currentState != null && startCallback != null)
        {
            startCallback();
        }
    }

    public IEnumerator DetectPlayer(Action callback = null)
    {
        for (int i = -90; i <= 90; i += 5) // Steps of 5 degrees
        {
            Vector3 rotatedDirection = Quaternion.Euler(0, i, 0) * transform.forward; // rotates the ray on X, Z
            if (Physics.Raycast(transform.position + Vector3.up * 2, rotatedDirection, out RaycastHit hit, range))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    ChangeState(chaseState, callback);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }


}
