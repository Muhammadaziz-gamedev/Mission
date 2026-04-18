using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Scripts.LiveObjects
{
    public class Crate : MonoBehaviour
    {
        [SerializeField] private float _punchDelay;
        [SerializeField] private GameObject _wholeCrate, _brokenCrate;
        [SerializeField] private Rigidbody[] _pieces;
        [SerializeField] private BoxCollider _crateCollider;
        [SerializeField] private InteractableZone _interactableZone;
        private float _currentForce = 1f;
        private float _breakHold;
        private float _holdTime = 0f;
        private bool _isReadyToBreak = false;
        private PlayerInputActions _input;
        private int _tapCount;
        private float _lastTapTime;
        [SerializeField] private float _tapWindow = 0.3f;
        private float _breakTimer;
        [SerializeField] private float _breakInterval = 0.25f;
        private List<Rigidbody> _brakeOff = new List<Rigidbody>();
        private float _cooldown;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += InteractableZone_onZoneInteractionComplete;
            InitializeInput();
        }
        private void InteractableZone_onZoneInteractionComplete(InteractableZone zone)
        {
            if (zone.GetZoneID() != _interactableZone.GetZoneID())
                return;
            Debug.Log("Zone interaction - Pieces left: " + _brakeOff.Count);

            if (!_isReadyToBreak)
            {
                _wholeCrate.SetActive(false);
                _brokenCrate.SetActive(true);
                _isReadyToBreak = true;
                return;
            }

            BreakPart(_currentForce);
        }

        private void Start()
        {
            _brakeOff.AddRange(_pieces);
            Debug.Log("Pieces count: " + _brakeOff.Count);

        }
        private void Update()
        {
            if (_cooldown > 0)
                _cooldown -= Time.deltaTime;
        }

        public void BreakPart(float forceMultiplier)
        {
            if (_brakeOff.Count == 0) return;

            int rng = Random.Range(0, _brakeOff.Count);
            Rigidbody rb = _brakeOff[rng];

            rb.constraints = RigidbodyConstraints.None;
            rb.AddForce(Vector3.one * forceMultiplier, ForceMode.Impulse);

            _brakeOff.RemoveAt(rng);
            if(_brakeOff.Count == 0)
            {
                FinishCrate();
            }
        }

        /*IEnumerator PunchDelay()
        {
            float delayTimer = 0;
            while (delayTimer < _punchDelay)
            {
                yield return new WaitForEndOfFrame();
                delayTimer += Time.deltaTime;
            }

            _interactableZone.ResetAction(_interactableZone.GetZoneID());
        }*/

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= InteractableZone_onZoneInteractionComplete;
        }
        /*private void HandleTap()
        {
            if (Time.time - _lastTapTime < _tapWindow)
                _tapCount++;
            else
                _tapCount = 1;

            _lastTapTime = Time.time;
        }*/
        private void InitializeInput()
        {
            _input = new PlayerInputActions();
            _input.Player.Enable();
            _input.Player.BrakeTap.performed += OnTap;
            _input.Player.BreakHold.performed += OnHold;

        }

        private void OnTap(InputAction.CallbackContext context)
        {
            if (!_isReadyToBreak) return;
            if (_cooldown > 0) return;

            _currentForce = 1f;
            TryBreak();
        }
        private void OnHold(InputAction.CallbackContext context)
        {
            if (!_isReadyToBreak) return;
            if (_cooldown > 0) return;

            _currentForce = 3f;
            TryBreak();
        }
        private void TryBreak()
        {
            if (_brakeOff.Count <= 0) return;

            BreakPart(_currentForce);
            _cooldown = 0.2f; 
        }
        private void FinishCrate()
        {
            _isReadyToBreak = false;
            _crateCollider.enabled = false;
            _brokenCrate.SetActive(false);
            _interactableZone.CompleteTask(_interactableZone.GetZoneID());
        }
    }
}

