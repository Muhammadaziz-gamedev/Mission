using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.LiveObjects;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Scripts.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class Player : MonoBehaviour
    {
        private CharacterController _controller;
        private Animator _anim;
        [SerializeField]
        private float _speed = 5.0f;
        private bool _playerGrounded;
        [SerializeField]
        private Detonator _detonator;
        private bool _canMove = true;
        [SerializeField]
        private CinemachineVirtualCamera _followCam;
        [SerializeField]
        private GameObject _model;



        private PlayerInputActions _input;

        private InteractableZone _currentZone;



        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete += ReleasePlayerControl;
            Laptop.onHackEnded += ReturnPlayerControl;
            Forklift.onDriveModeEntered += ReleasePlayerControl;
            Forklift.onDriveModeExited += ReturnPlayerControl;
            Forklift.onDriveModeEntered += HidePlayer;
            Drone.OnEnterFlightMode += ReleasePlayerControl;
            Drone.onExitFlightmode += ReturnPlayerControl;
        }

        private void Start()
        {
            InitializeInput();
            _controller = GetComponent<CharacterController>();

            if (_controller == null)
                Debug.LogError("No Character Controller Present");

            _anim = GetComponentInChildren<Animator>();

            if (_anim == null)
                Debug.Log("Failed to connect the Animator");
        }

        private void Update()
        {
            if (_canMove == true)
                CalcutateMovement();

        }

        private void CalcutateMovement()
        {
            _playerGrounded = _controller.isGrounded;
            /*float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");*/
            var move = _input.Player.Walking.ReadValue<Vector2>();
            var h = move.x * 150f * Time.deltaTime;
            transform.Rotate(transform.up, h);

            var direction = transform.forward * move.y + transform.right * move.x;
            var velocity = direction * _speed;


            _anim.SetFloat("Speed", Mathf.Abs(velocity.magnitude));


            if (_playerGrounded)
                velocity.y = 0f;
            if (!_playerGrounded)
            {
                velocity.y += -20f * Time.deltaTime;
            }

            _controller.Move(velocity * Time.deltaTime);

        }

        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            switch (zone.GetZoneID())
            {
                case 1: //place c4
                    _detonator.Show();
                    break;
                case 2: //Trigger Explosion
                    TriggerExplosive();
                    break;
            }
        }

        private void ReleasePlayerControl()
        {
            _canMove = false;
            _followCam.Priority = 9;
        }

        private void ReturnPlayerControl()
        {
            _model.SetActive(true);
            _canMove = true;
            _followCam.Priority = 10;
        }

        private void HidePlayer()
        {
            _model.SetActive(false);
        }

        private void TriggerExplosive()
        {
            _detonator.TriggerExplosion();
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
            Laptop.onHackComplete -= ReleasePlayerControl;
            Laptop.onHackEnded -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= ReleasePlayerControl;
            Forklift.onDriveModeExited -= ReturnPlayerControl;
            Forklift.onDriveModeEntered -= HidePlayer;
            Drone.OnEnterFlightMode -= ReleasePlayerControl;
            Drone.onExitFlightmode -= ReturnPlayerControl;
        }
        private void InitializeInput()
        {
            _input = new PlayerInputActions();
            _input.Player.Enable();

            _input.Player.Pick.performed += ctx =>
            {
                if (_currentZone != null)
                    _currentZone.OnInteract();
            };

            _input.Player.Detonate.performed += ctx =>
            {
                if (_currentZone != null)
                    _currentZone.OnAction();
            };

            _input.Player.HoldActions.started += ctx =>
            {
                if (_currentZone != null)
                    _currentZone.OnHoldStart();
            };

            _input.Player.HoldActions.canceled += ctx =>
            {
                if (_currentZone != null)
                    _currentZone.OnHoldEnd();
            };
            _input.Player.Place.performed += ctx =>
            {
                if (_currentZone != null)
                    _currentZone.OnAction();
            };
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out InteractableZone zone))
                _currentZone = zone;
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out InteractableZone zone) && _currentZone == zone)
                _currentZone = null;
        }
    }

}

