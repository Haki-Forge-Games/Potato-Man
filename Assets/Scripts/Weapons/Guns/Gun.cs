using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Shoot Settings")]
    public float shootRange = 20f;
    public int pelletCount = 20;
    public float spreadAngle = 10f;
    public float impactMagnitude = 0f;
    public float impactDuration = 0f;


    [Header("Fire Point")]
    public Transform firePoint;

    [Header("Refferences")]
    public Camera camera;
    public GameObject pelletPrefab;
    public ParticleSystem muzzleFlash;
}
