using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Game.Enviroment
{
    public class Door : NetworkBehaviour
    {
        [SerializeField] private List<Switch> _switches;
        [SerializeField] private NetworkAnimator _animCtrl;

        //The door may depends on multiple switches
        private Dictionary<Switch, bool> _activeSwitches = new Dictionary<Switch, bool>();

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (IsServer)
            {
                foreach (Switch doorSwicth in _switches)
                {
                    doorSwicth.OnSwitchChanged += OnSwitchChanged;
                    _activeSwitches.Add(doorSwicth, false);
                }
            }
        }

        private void OnSwitchChanged(Switch doorswitch, bool isactive)
        {
            _activeSwitches[doorswitch] = isactive;
            foreach (var doorSwitch in _switches)
            {
                //if there is at least 1 closed-door -> return -> will not open the door
                if (!_activeSwitches[doorSwitch])
                {
                    return;
                }
            }
            
            Debug.Log($"Open the door");
            _animCtrl.SetTrigger("OpenDoor");
        }
        
    }
}