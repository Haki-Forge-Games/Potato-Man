using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamViewItem : MonoBehaviour
{
    [Header("Offset")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scaleOffset;

    [Header("Reference")]

    [Header("Left Hand")]
    public Transform leftTarget;
    public Transform leftHint;

    [Header("Right Hand")]
    public Transform rightTarget;
    public Transform rightHint;
}
