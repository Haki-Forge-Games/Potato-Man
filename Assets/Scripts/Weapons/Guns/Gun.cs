using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Shoot Settings")]
    public float shootRange = 20f;
    public float impactMagnitude = 0f;
    public float impactDuration = 0f;
    public float fireRate = 0.5f;


    [Header("Refferences")]
    public Transform firePoint;
    public Animator animator;
    public ParticleSystem muzzleFlash;
}
