using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;
using System.Collections;

namespace Assets.Scripts.ControllUnit
{
    public class PlayerControllInput : MonoBehaviour, IActionMapInputer
    {
        private bool isInputActive;

        

        public void ActionMapActivated() => isInputActive = true;

        public void ActionMapDeactivated() => isInputActive = false;

        public ActionMaps GetActionMap() => default;
    }
}
