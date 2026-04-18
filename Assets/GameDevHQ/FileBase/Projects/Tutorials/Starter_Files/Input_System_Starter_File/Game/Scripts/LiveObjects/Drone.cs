using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Game.Scripts.UI;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Drone : MonoBehaviour
    {
        private enum Tilt
        {
            NoTilt, Forward, Back, Left, Right
        }
        private Vector2 _moveInput;
        private Vector2 _yawInput;
        private float _thrustInput;
        [SerializeField]
        private Rigidbody _rigidbody;
        [SerializeField]
        private float _speed = 5f;
        private bool _inFlightMode = false;
        private Vector2 _yawInput2;
        [SerializeField]
        private Animator _propAnim;
        [SerializeField]
        private CinemachineVirtualCamera _droneCam;
        [SerializeField]
        private InteractableZone _interactableZone;
        private PlayerInputActions _input;

        public static event Action OnEnterFlightMode;
        public static event Action onExitFlightmode;

        private void OnEnable()
        {
            InitializeInput();
            InteractableZone.onZoneInteractionComplete += EnterFlightMode;
        }

        private void EnterFlightMode(InteractableZone zone)
        {
            if (_inFlightMode != true && zone.GetZoneID() == 4) // drone Scene
            {
                _propAnim.SetTrigger("StartProps");
                _droneCam.Priority = 11;
                _inFlightMode = true;
                OnEnterFlightMode?.Invoke();
                UIManager.Instance.DroneView(true);
                UIManager.Instance.SetInventoryVisible(false);
                _interactableZone.CompleteTask(4);
            }
        }

        private void ExitFlightMode()
        {            
            _droneCam.Priority = 9;
            _inFlightMode = false;
            UIManager.Instance.DroneView(false);            
        }

        private void Update()
        {
            if (!_inFlightMode) return;

            Vector2 move = _input.Drone.WASD.ReadValue<Vector2>();

            CalculateTilt(move);
            CalculateMovementUpdate(_yawInput);
        }

        private void FixedUpdate()
        {
            _rigidbody.AddForce(transform.up * (9.81f), ForceMode.Acceleration);
            if (_inFlightMode)
                CalculateMovementFixedUpdate();
        }

        private void CalculateMovementUpdate(Vector2 yawInput)
        {
            /*if (Input.GetKey(KeyCode.LeftArrow))
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y -= _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                var tempRot = transform.localRotation.eulerAngles;
                tempRot.y += _speed / 3;
                transform.localRotation = Quaternion.Euler(tempRot);
            }*/
            /*var rot = transform.localRotation.eulerAngles;*/
            transform.Rotate(0f, _yawInput.x * (_speed / 3f), 0f);
            /*transform.localRotation = Quaternion.Euler(rot);*/
        }

        private void CalculateMovementFixedUpdate()
        {

            /* if (Input.GetKey(KeyCode.Space))
             {
                 _rigidbody.AddForce(transform.up * _speed, ForceMode.Acceleration);
             }
             if (Input.GetKey(KeyCode.V))
             {
                 _rigidbody.AddForce(-transform.up * _speed, ForceMode.Acceleration);
             }*/
            float thrust = _input.Drone.Thrust.ReadValue<float>();
            _rigidbody.AddForce(transform.up * thrust * _speed, ForceMode.Acceleration);
        }

        private void CalculateTilt(Vector2 moveInput)
        {
            /*if (Input.GetKey(KeyCode.A)) 
                transform.rotation = Quaternion.Euler(00, transform.localRotation.eulerAngles.y, 30);
            else if (Input.GetKey(KeyCode.D))
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, -30);
            else if (Input.GetKey(KeyCode.W))
                transform.rotation = Quaternion.Euler(30, transform.localRotation.eulerAngles.y, 0);
            else if (Input.GetKey(KeyCode.S))
                transform.rotation = Quaternion.Euler(-30, transform.localRotation.eulerAngles.y, 0);
            else 
                transform.rotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);*/
            float x = moveInput.y * 30f;
            float z = -moveInput.x * 30f;
            transform.rotation = Quaternion.Euler(x, transform.localRotation.eulerAngles.y, z);
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterFlightMode;
            _input.Drone.Disable();
        }
        private void InitializeInput()
        {
            _input = new PlayerInputActions();
            _input.Enable();
            _input.Drone.Enable();
            _input.Drone.ArrowKeys.performed += ArrowKeys_performed;
            _input.Drone.Exit.performed += Exit_performed;
            _input.Drone.ArrowKeys.canceled += ArrowKeys_canceled;
        }
        private void ArrowKeys_canceled(InputAction.CallbackContext context)
        {
            _yawInput = Vector2.zero;
        }
        private void Exit_performed(InputAction.CallbackContext context)
        {
            _inFlightMode = false;
            onExitFlightmode?.Invoke();
            ExitFlightMode();
        }

        private void ArrowKeys_performed(InputAction.CallbackContext context)
        {
            _yawInput = context.ReadValue<Vector2>();
        }   
    }
}
