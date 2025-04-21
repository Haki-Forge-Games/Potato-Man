using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour, ItemInstance
{
    [Header("Shoot Settings")]
    public float shootRange = 20f;
    public float impactMagnitude = 0f;
    public float impactDuration = 0f;
    public float fireRate = 0.5f;

    [Header("Bullets")]
    public int maxBullets = 2;

    [Header("Refferences")]
    public Transform firePoint;
    public Animator animator;
    public ParticleSystem muzzleFlash;

    public int currentBullets { get; set; } = 2;

    // setting data
    public void LoadData(ItemInstance data)
    {
        if (data is Gun gunData)
            currentBullets = gunData.currentBullets;
    }
}
