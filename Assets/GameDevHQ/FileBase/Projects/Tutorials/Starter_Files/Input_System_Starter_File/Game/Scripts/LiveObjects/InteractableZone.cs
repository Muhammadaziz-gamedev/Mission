using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.UI;


namespace Game.Scripts.LiveObjects
{   
    public class InteractableZone : MonoBehaviour
    {
        private enum ZoneType
        {
            Collectable,
            Action,
            HoldAction
        }

        private enum KeyState
        {
            Press,
            PressHold
        }

        [SerializeField]
        private ZoneType _zoneType;
        [SerializeField]
        private int _zoneID;
        [SerializeField]
        private int _requiredID;

        [SerializeField]
        [Tooltip("Press the (---) Key to .....")]
        private string _displayMessage;
        [SerializeField]
        private GameObject[] _zoneItems;
        private bool _inZone = false;
        private bool _itemsCollected = false;
        private bool _actionPerformed = false;
        [SerializeField]
        private Sprite _inventoryIcon;
        [SerializeField]
        private KeyCode _zoneKeyInput;
        [SerializeField]
        private KeyState _keyState;
        [SerializeField]
        private GameObject _marker;

        private bool _inHoldState = false;

        private static int _currentZoneID = 0;
        public static int CurrentZoneID
        {
            get
            {
                return _currentZoneID;
            }
            set
            {
                _currentZoneID = value;

            }
        }
        public static event Action<InteractableZone> onZoneInteractionComplete;
        public static event Action<int> onHoldStarted;
        public static event Action<int> onHoldEnded;




        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += SetMarker;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _currentZoneID > _requiredID)
            {
                switch (_zoneType)
                {
                    case ZoneType.Collectable:
                        if (_itemsCollected == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to collect");
                        }
                        break;

                    case ZoneType.Action:
                        if (_actionPerformed == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the {_zoneKeyInput.ToString()} key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the {_zoneKeyInput.ToString()} key to perform action");
                        }
                        break;

                    case ZoneType.HoldAction:
                        _inZone = true;
                        if (_displayMessage != null)
                        {
                            string message = $"Press the F key to {_displayMessage}.";
                            UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                        }
                        else
                            UIManager.Instance.DisplayInteractableZoneMessage(true, $"Hold the {_zoneKeyInput.ToString()} key to perform action");
                        break;
                }
            }
        }

        private void Update()
        {
            if (_inZone == true)
            {

                /*                if (*//*Input.GetKeyDown(_zoneKeyInput) && _keyState != KeyState.PressHold*//* _input.Player.Pick.triggered)
                                {
                                    //press
                                    switch (_zoneType)
                                    {
                                        case ZoneType.Collectable:
                                            if (_itemsCollected == false)
                                            {
                                                CollectItems();
                                                _itemsCollected = true;
                                                UIManager.Instance.DisplayI nteractableZoneMessage(false);
                                            }
                                            break;
                                    }
                                }*/
                /*if (_input.Player.Detonate.triggered)
                {
                    switch (_zoneType)
                    {
                        case ZoneType.Action:
                            if (_actionPerformed == false)
                            {
                                PerformAction();
                                _actionPerformed = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;
                    }
                }*/

                /*if (_zoneType == ZoneType.HoldAction)
                {
                    if (_input.Player.HoldActions.IsPressed() && !_inHoldState)
                    {
                        _inHoldState = true;
                        PerformHoldAction();
                    }

                    if (!_input.Player.HoldActions.IsPressed() && _inHoldState)
                    {
                        _inHoldState = false;
                        onHoldEnded?.Invoke(_zoneID);
                    }
                }*/
            }
        }

        private void CollectItems()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(false);
            }

            UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);
            if (_zoneID != 6)
            {
                CompleteTask(_zoneID);
                onZoneInteractionComplete?.Invoke(this);
            }
        }
        private void PerformAction()
        {
            if (_actionPerformed) return;

            _actionPerformed = true;

            foreach (var item in _zoneItems)
                item.SetActive(true);

            if (_inventoryIcon != null)
                UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            onZoneInteractionComplete?.Invoke(this);
        }

        private void PerformHoldAction()
        {
            UIManager.Instance.DisplayInteractableZoneMessage(false);
            onHoldStarted?.Invoke(_zoneID);
        }

        public GameObject[] GetItems()
        {
            return _zoneItems;
        }

        public int GetZoneID()
        {
            return _zoneID;
        }

        public void CompleteTask(int zoneID)
        {
            if (zoneID != _zoneID) return;
            if (zoneID != _currentZoneID) return;

            _currentZoneID++;
            onZoneInteractionComplete?.Invoke(this);
        }

        public void ResetAction(int zoneID)
        {
            if (zoneID == _zoneID)
                _actionPerformed = false;
        }

        public void SetMarker(InteractableZone zone)
        {
            if (_zoneID == _currentZoneID)
                _marker.SetActive(true);
            else
                _marker.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (_zoneType == ZoneType.HoldAction && _inHoldState)
            {
                _inHoldState = false;
                onHoldEnded?.Invoke(_zoneID);
            }
            if (other.CompareTag("Player"))
            {
                _inZone = false;
                UIManager.Instance.DisplayInteractableZoneMessage(false);
            }
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= SetMarker;
        }
        public void OnInteract()
        {
            if (_zoneType != ZoneType.Collectable || _itemsCollected) return;

            _itemsCollected = true;
            CollectItems();
            UIManager.Instance.DisplayInteractableZoneMessage(false);
        }

        public void OnAction()
        {
            Debug.Log($"OnAction called - Zone ID: {_zoneID}, ActionPerformed: {_actionPerformed}, ZoneType: {_zoneType}");
            if (_zoneID != 6 && (_zoneType != ZoneType.Action || _actionPerformed))
                return;
            

            _actionPerformed = true;
            if (_zoneID == 6) Debug.Log("tryin");
            foreach (var item in _zoneItems)
                item.SetActive(false);

            UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            if (_zoneID != 6)
            {
                CompleteTask(_zoneID);
            }
            else
            {
                onZoneInteractionComplete?.Invoke(this);
            }

            UIManager.Instance.DisplayInteractableZoneMessage(false);
        }
        public void OnHoldStart()
        {
            if (_zoneType != ZoneType.HoldAction) return;

            _inHoldState = true;
            PerformHoldAction();

        }
        public void OnHoldEnd()
        {
            if (_zoneType != ZoneType.HoldAction) return;

            _inHoldState = false;
            onHoldEnded?.Invoke(_zoneID);
        }
    }
}


