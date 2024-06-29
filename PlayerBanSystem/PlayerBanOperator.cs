using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace eepyfemboi.PlayerBanSystem
{
    public class PlayerBanOperator : UdonSharpBehaviour
    {
        public string[] bannedPlayers;

        void Start()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;

            foreach (string user in bannedPlayers)
            {
                if (localPlayer != null && localPlayer.displayName == user)
                {
                    Vector3 pos = localPlayer.GetPosition();
                    Quaternion rot = localPlayer.GetRotation();

                    localPlayer.SetGravityStrength(-10);
                    localPlayer.TeleportTo(new Vector3(pos.x, pos.y + 100f, pos.z), rot);
                }
            }
        }
    }
}
