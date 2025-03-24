using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reload : MonoBehaviour
{
    [SerializeField] private Gun gun;
    [SerializeField] private Shoot shoot;
    [SerializeField] private Inputs inputs;

    private float pickUpDistance;
    private Camera camera;
    private RaycastHit hit;

    private void Start()
    {
        GameObject grandGrandparentObj = GetGrandGrandparentObject();
        if (grandGrandparentObj == null) return;

        Player player = grandGrandparentObj.GetComponent<Player>();
        pickUpDistance = player?.pickUpDistance ?? 10f;

        GameObject grandparentObj = GetGrandparentObject();
        if (grandparentObj == null) return;

        camera = grandparentObj.GetComponent<Camera>();
    }

    private void Update()
    {
        if (inputs.CheckPickUpPressed() && CheckIsBullet())
        {
            if (shoot.currentBullets < gun.maxBullets)
            {
                ReloadGun();
            }
            else
            {
                Debug.Log("Gun is full");
            }
        }
    }

    private bool CheckIsBullet()
    {
        if (camera == null) return false;
        return Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, pickUpDistance) && hit.collider.CompareTag("Bullets");
    }

    private void ReloadGun()
    {
        shoot.currentBullets += 1;
        Destroy(hit.collider.gameObject);

        Debug.Log("Reload Complete");
    }

    private GameObject GetGrandparentObject()
    {
        return transform.parent?.parent?.gameObject;
    }

    private GameObject GetGrandGrandparentObject()
    {
        return transform.parent?.parent?.parent?.gameObject;
    }
}