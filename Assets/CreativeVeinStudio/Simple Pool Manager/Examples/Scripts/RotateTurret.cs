using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CreativeVeinStudio.Simple_Pool_Manager.Extras;
using UnityEngine.Serialization;

public class RotateTurret : MonoBehaviour
{
    public Transform turretHead;
    public Vector3 rotateDeg = Vector3.up;
    public float rotateSpeed = 0.5f;
    public bool shouldPingPong = false;

    private Transform _turretTrans;
    private Quaternion _startRot;

    // Start is called before the first frame update
    private void Start()
    {
        _startRot = turretHead.localRotation;
        if (turretHead != null) return;
        Debug.LogWarning("Please provide the Turret Head prefab to the exposed field.");
        Debug.Break();
    }

    // Update is called once per frame
    private void Update()
    {
        if (shouldPingPong)
            LocomotionActions.PingPongObjectByAxis(ref turretHead, rotateDeg, _startRot, rotateSpeed);
        else
            LocomotionActions.RotateOnSpecifiedAxis(ref turretHead, rotateDeg, rotateSpeed);
    }
}
