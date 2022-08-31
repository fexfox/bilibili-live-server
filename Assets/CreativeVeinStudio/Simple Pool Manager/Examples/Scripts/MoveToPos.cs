using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPos : MonoBehaviour
{
    public float _moveSpeed = 3;
    private Transform _moveToTarget;

    void Update()
    {
        if (_moveToTarget == null) return;
        var moveTo = Vector3.Lerp(transform.position, _moveToTarget.position, _moveSpeed * Time.deltaTime);
        transform.position = moveTo;
    }

    internal void SetMoveToPos(Transform pos)
    {
        _moveToTarget = pos;
    }
}
