using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

using System;
using System.Collections;
using System.Collections.Generic;

using Unity.Netcode;

public class StateManager : NetworkBehaviour
{
    [Header("References")]
    public Animator animator;
    public NavMeshAgent agent;

    [Header("Coliders")]
    public CapsuleCollider mainCollider;

    [Header("Ragdoll Rigidbodies")]
    public List<Rigidbody> rigidbodies = new List<Rigidbody>();

    [Header("Way Points")]
    public List<GameObject> waypoints = new List<GameObject>();

    [Header("Settings")]
    public int deathTimeInSeconds = 60;
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
        if (newState == null) return;

        currentState?.OnExit();
        currentState = newState;
        currentState?.OnEnter();

        if (currentState != null && startCallback != null)
        {
            startCallback();
        }
    }

    // change state to dath state when player shoot 
    public void ChangeStateToDeath()
    {
        ChangeState(deathState);
    }

    // detects player every frame with the interval of 0.1 second;
    public IEnumerator DetectPlayer(Action callback = null)
    {
        for (int i = -45; i <= 45; i += 5) // Steps of 5 degrees
        {
            Vector3 rotatedDirection = Quaternion.Euler(0, i, 0) * transform.forward; // rotates the ray on X, Z
            Debug.DrawRay(transform.position + Vector3.up * 2, rotatedDirection * range, Color.red);
            if (Physics.Raycast(transform.position + Vector3.up * 2, rotatedDirection, out RaycastHit hit, range))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    ChangeState(chaseState, callback);
                }
            }
        }
        yield return new WaitForSeconds(0.1f);
    }
}
