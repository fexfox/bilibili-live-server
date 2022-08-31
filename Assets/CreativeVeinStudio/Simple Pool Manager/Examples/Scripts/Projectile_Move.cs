using System;
using System.Collections;
using UnityEngine;

public class Projectile_Move : MonoBehaviour
{
    [SerializeField] private float _speed = 100;
    [SerializeField] private MoveDirection _moveDirection = MoveDirection.Forward;

    private Vector3 _moveDir;
    private Rigidbody _rigidB;
    
    private void Awake()
    {
        _rigidB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        SetMoveDirection();
        transform.position += _moveDir * (_speed * Time.deltaTime);
    }

    private void SetMoveDirection()
    {
        switch (_moveDirection)
        {
            case MoveDirection.Left:
                _moveDir = -transform.right;
                break;
            case MoveDirection.Right:
                _moveDir = transform.right;
                break;
            case MoveDirection.Up:
                _moveDir = transform.up;
                break;
            case MoveDirection.Down:
                _moveDir = -transform.up;
                break;
            case MoveDirection.Forward:
                _moveDir = transform.forward;
                break;
            case MoveDirection.Backwards:
                _moveDir = -transform.forward;
                break;
            default:
                break;
        }
    }

    public enum MoveDirection
    {
        Left,
        Right,
        Up,
        Down,
        Forward,
        Backwards
    }
}