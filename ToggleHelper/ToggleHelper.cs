/*
 * 
 * this is a script i originally made to help with my world's optimization menu
 * theres probably something better than this but im just too dumb to find it :3
 * anyway i hope its useful to u, and have fun!
 * 
 * To use this, you need to have imported the VRChat SDK.
 * To put this script into your project, simply use the unity package at 
 * https://github.com/eepyfemboi/udon-sharp-scripts/raw/main/ToggleHelper/ToggleHelper.unitypackage 
 * 
*/

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace eepyfemboi.ToggleHelper
{
    public class ToggleHelper : UdonSharpBehaviour
    {
        public GameObject toToggle; // might make this a list later but from my experience udon doesnt like normal lists :/
        public int steps;
        private int step = 0; // using a steps system bcuz idk how to directly listen to the event and i dont wanna impact performance
        private Toggle toggle;

        public void Start()
        {
            toggle = gameObject.GetComponent<Toggle>();

            if (steps <= 0) // checking for default or possibly game breaking values
            {
                steps = 50;
            }
        }

        public void Update()
        {
            if (toToggle != null)
            {
                if (step < steps)
                {
                    step++;
                }
                else if (step == steps)
                {
                    step = 0;

                    if (toggle != null)
                    {
                        if (toggle.isOn)
                        {
                            toToggle.SetActive(true);
                        }
                        else
                        {
                            toToggle.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}
