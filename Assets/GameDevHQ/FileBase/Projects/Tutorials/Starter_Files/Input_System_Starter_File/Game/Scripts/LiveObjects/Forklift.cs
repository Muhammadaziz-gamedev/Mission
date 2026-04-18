using System;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Forklift : MonoBehaviour
    {
        [SerializeField]
        private GameObject _lift, _steeringWheel, _leftWheel, _rightWheel, _rearWheels;
        [SerializeField]
        private Vector3 _liftLowerLimit, _liftUpperLimit;
        [SerializeField]
        private float _speed = 5f, _liftSpeed = 1f;
        [SerializeField]
        private CinemachineVirtualCamera _forkliftCam;
        [SerializeField]
        private GameObject _driverModel;
        private bool _inDriveMode = false;
        [SerializeField]
        private InteractableZone _interactableZone;
        private PlayerInputActions _input;
        public static event Action onDriveModeEntered;
        public static event Action onDriveModeExited;
        private bool _liftUpHeld;
        private bool _liftDownHeld;
        private void OnEnable()
        {
            InitializeInput();
            InteractableZone.onZoneInteractionComplete += EnterDriveMode;
        }

        private void EnterDriveMode(InteractableZone zone)
        {
            if (_inDriveMode !=true && zone.GetZoneID() == 5) //Enter ForkLift
            {
                _inDriveMode = true;
                _forkliftCam.Priority = 11;
                onDriveModeEntered?.Invoke();
                _driverModel.SetActive(true);
                _interactableZone.CompleteTask(5);
            }
        }

        private void ExitDriveMode()
        {
            _inDriveMode = false;
            _forkliftCam.Priority = 9;            
            _driverModel.SetActive(false);
            onDriveModeExited?.Invoke();
            
        }

        private void Update()
        {
            if (_inDriveMode == true)
            {
                LiftControls();
                CalcutateMovement();
                /*if (Input.GetKeyDown(KeyCode.Escape))
                    ExitDriveMode();*/
            }

        }

        private void CalcutateMovement()
        {
            var move = _input.ForkLift.WASD.ReadValue<Vector2>();
            float h = move.x /*Input.GetAxisRaw("Horizontal")*/;
            float v = move.y /*Input.GetAxisRaw("Vertical")*/;
            var direction = new Vector3(0, 0, v);
            var velocity = direction * _speed;
            transform.Translate(velocity * Time.deltaTime);

            if (Mathf.Abs(v) > 0)
            {
                var tempRot = transform.rotation.eulerAngles;               
                tempRot.y += h * _speed / 2;
                transform.rotation = Quaternion.Euler(tempRot);
            }
        }

        private void LiftControls()
        {
            if (_liftUpHeld)
                LiftUpRoutine();

            if (_liftDownHeld)
                LiftDownRoutine();
        }

        private void LiftUpRoutine()
        {
            if (_lift.transform.localPosition.y < _liftUpperLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y += Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y >= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftUpperLimit;
        }

        private void LiftDownRoutine()
        {
            if (_lift.transform.localPosition.y > _liftLowerLimit.y)
            {
                Vector3 tempPos = _lift.transform.localPosition;
                tempPos.y -= Time.deltaTime * _liftSpeed;
                _lift.transform.localPosition = new Vector3(tempPos.x, tempPos.y, tempPos.z);
            }
            else if (_lift.transform.localPosition.y <= _liftUpperLimit.y)
                _lift.transform.localPosition = _liftLowerLimit;
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= EnterDriveMode;
        }
        private void InitializeInput()
        {
            _input = new PlayerInputActions();
            _input.ForkLift.Enable();
            _input.ForkLift.Exit.performed += Exit_performed;
            _input.ForkLift.LiftUp.performed += LiftUp_performed;
            _input.ForkLift.LiftDown.performed += LiftDown_performed;
            _input.ForkLift.LiftUp.canceled += LiftUp_canceled;
            _input.ForkLift.LiftDown.canceled += LiftDown_canceled;
        }

        private void Exit_performed(InputAction.CallbackContext context)
        {
            ExitDriveMode();
        }

        private void LiftUp_performed(InputAction.CallbackContext context)
        {
            _liftUpHeld = true;
        }

        private void LiftUp_canceled(InputAction.CallbackContext context)
        {
            _liftUpHeld = false;
        }

        private void LiftDown_performed(InputAction.CallbackContext context)
        {
            _liftDownHeld = true;
        }

        private void LiftDown_canceled(InputAction.CallbackContext context)
        {
            _liftDownHeld = false;
        }
    }
}