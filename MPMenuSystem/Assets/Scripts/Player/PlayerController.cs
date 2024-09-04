using Player.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Player.PlayerControl
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float AnimBlendSpeed = 8.9f;
        [SerializeField] private Transform CameraRoot;
        [SerializeField] private Transform Camera;
        [SerializeField] private float UpperLimit = -40f;
        [SerializeField] private float BottomLimit = 70f;
        [SerializeField] private float LookSensitivity = 21.9f;
        [SerializeField, Range(10, 500)] private float JumpFactor = 260f;
        [SerializeField] private float AirResistance = 0.8f;
        [SerializeField] private float DistanceToGround = 0.8f;
        [SerializeField] private LayerMask GroundCheck;
        private Rigidbody _playerRigidbody;
        private InputManager _inputManager;
        private Animator _animator;
        private bool _hasAnimator;
        private bool _grounded;
        private int _xVelHash;
        private int _yVelHash;
        private int _jumpHash;
        private int _groundHash;
        private int _fallingHash;
        private int _zVelHash;
        private int _crouchHash;
        private float _xRotation;

        private const float _walkSpeed = 2f;
        private const float _runSpeed = 6f;
        private Vector2 _currentVelocity;

        private void Start()
        {
            _hasAnimator = TryGetComponent<Animator>(out _animator);
            _playerRigidbody = GetComponent<Rigidbody>();
            _inputManager = GetComponent<InputManager>();

            _xVelHash = Animator.StringToHash("X_Velocity");
            _yVelHash = Animator.StringToHash("Y_Velocity");
            _jumpHash = Animator.StringToHash("Jump");
            _groundHash = Animator.StringToHash("Grounded");
            _fallingHash = Animator.StringToHash("Falling");
            _zVelHash = Animator.StringToHash("Z_Velocity");
            _crouchHash = Animator.StringToHash("Crouch");
        }

        private void FixedUpdate()
        {
            if(IsOwner)
            {
                SampleGround();
                Move();
                HandleJump();
                HandleCrouch();
            }
        }

        private void LateUpdate()
        {
            if(IsOwner)
            {
                CamMovements();
            }
        }

        private void Move()
        {
            if(!_hasAnimator) return;

            float targetSpeed = _inputManager.Run ? _runSpeed : _walkSpeed;
            if (_inputManager.Crouch) targetSpeed = 1.5f;
            if (_inputManager.Move == Vector2.zero) targetSpeed = 0;

            if(_grounded)
            {
                _currentVelocity.x = Mathf.Lerp(_currentVelocity.x, _inputManager.Move.x * targetSpeed, AnimBlendSpeed * Time.fixedDeltaTime);
                _currentVelocity.y = Mathf.Lerp(_currentVelocity.y, _inputManager.Move.y * targetSpeed, AnimBlendSpeed * Time.fixedDeltaTime);

                var xVelDifference = _currentVelocity.x - _playerRigidbody.velocity.x;
                var zVelDifference = _currentVelocity.y - _playerRigidbody.velocity.z;

                _playerRigidbody.AddForce(transform.TransformVector(new Vector3(xVelDifference, 0, zVelDifference)), ForceMode.VelocityChange);
            }
            else
            {
                _playerRigidbody.AddForce(transform.TransformVector(new Vector3(_currentVelocity.x * AirResistance, 0, _currentVelocity.y * AirResistance)), ForceMode.VelocityChange);
            }
            _animator.SetFloat(_xVelHash, _currentVelocity.x);
            _animator.SetFloat(_yVelHash, _currentVelocity.y);
        }

        private void CamMovements()
        {
            if(!_hasAnimator) return;

            var Axis_X = _inputManager.Look.x;
            var Axis_Y = _inputManager.Look.y;
            Camera.position = CameraRoot.position;

            _xRotation -= Axis_Y * LookSensitivity * Time.smoothDeltaTime;
            _xRotation = Mathf.Clamp(_xRotation, UpperLimit, BottomLimit);

            Camera.localRotation = Quaternion.Euler(_xRotation, 0, 0);
            //transform.Rotate(Vector3.up, Axis_X * LookSensitivity * Time.deltaTime);
            _playerRigidbody.MoveRotation(_playerRigidbody.rotation * Quaternion.Euler(0, Axis_X * LookSensitivity * Time.smoothDeltaTime, 0));
        }

        private void HandleCrouch() => _animator.SetBool(_crouchHash, _inputManager.Crouch);

        private void HandleJump()
        {
            if(!_hasAnimator) return;
            if (!_inputManager.Jump) return;
            if (!_grounded) return;
            if (_inputManager.Crouch) return;
            _animator.SetTrigger(_jumpHash);
            //For bunny hop
            _playerRigidbody.AddForce(-_playerRigidbody.velocity.y * Vector3.up, ForceMode.VelocityChange);
            _playerRigidbody.AddForce(Vector3.up * JumpFactor, ForceMode.Impulse);
            _animator.ResetTrigger(_jumpHash);

        }

        public void JumpAddForce()
        {
            _playerRigidbody.AddForce(-_playerRigidbody.velocity.y * Vector3.up, ForceMode.VelocityChange);
            _playerRigidbody.AddForce(Vector3.up * JumpFactor, ForceMode.Impulse);
            _animator.ResetTrigger(_jumpHash);
        }

        private void SampleGround()
        {
            if (!_hasAnimator) return;

            RaycastHit hitInfo;
            if (Physics.Raycast(_playerRigidbody.worldCenterOfMass, Vector3.down, out hitInfo, DistanceToGround + 0.1f, GroundCheck))
            {
                //Grounded
                _grounded = true;
                SetAnimGrounded();
                return;
            }
            //Falling
            _grounded = false;
            _animator.SetFloat(_zVelHash, _playerRigidbody.velocity.y);
            SetAnimGrounded();
            return;
        }

        private void SetAnimGrounded()
        {
            _animator.SetBool(_fallingHash, !_grounded);
            _animator.SetBool(_groundHash, _grounded);
        }
    }
}
