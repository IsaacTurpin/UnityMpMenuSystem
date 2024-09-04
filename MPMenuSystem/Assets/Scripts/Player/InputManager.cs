using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

namespace Player.Manager
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;

        public Vector2 Move {  get; private set; }
        public Vector2 Look {  get; private set; }
        public bool Run {  get; private set; }
        public bool Jump { get; private set; }
        public bool Crouch { get; private set; }

        private InputActionMap _currentMap;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _runAction;
        private InputAction _jumpAction;
        private InputAction _crouchAction;

        private void Awake()
        {
            _currentMap = playerInput.currentActionMap;
            _moveAction = _currentMap.FindAction("Move");
            _lookAction = _currentMap.FindAction("Look");
            _runAction = _currentMap.FindAction("Run");
            _jumpAction = _currentMap.FindAction("Jump");
            _crouchAction = _currentMap.FindAction("Crouch");

            _moveAction.performed += OnMove;
            _lookAction.performed += OnLook;
            _runAction.performed += OnRun;
            _jumpAction.performed += OnJump;
            _crouchAction.started += OnCrouch;

            _moveAction.canceled += OnMove;
            _lookAction.canceled += OnLook;
            _runAction.canceled += OnRun;
            _jumpAction.canceled += OnJump;
            _crouchAction.canceled += OnCrouch;
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            Move = context.ReadValue<Vector2>();
        }

        private void OnLook(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
        }

        private void OnRun(InputAction.CallbackContext context)
        {
            Run = context.ReadValueAsButton();
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            //If set as tap - add "if(context.interaction is TapInteraction)" before code below
            Jump = context.ReadValueAsButton();
        }

        private void OnCrouch(InputAction.CallbackContext context)
        {
            Crouch = context.ReadValueAsButton();
        }

        private void OnEnable()
        {
            _currentMap.Enable();
        }

        private void OnDisable()
        {
            _currentMap.Disable();
        }
    }
}
