using UnityEngine;
using StarterAssets;
using Photon.Pun;
using UnityEngine.InputSystem;
using ExitGames.Client.Photon;
using Photon.Realtime;

[RequireComponent(typeof(PhotonView))]
public class PunUserNetControl : MonoBehaviourPunCallbacks , IPunInstantiateMagicCallback {
    [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
    public static GameObject LocalPlayerInstance;
    public Transform CameraRoot;

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        Debug.Log(info.photonView.Owner.ToString());
        Debug.Log(info.photonView.ViewID.ToString());

        // #Important
        // used in PunNetworkManager.cs
        // : we keep track of the localPlayer instance to prevent instanciation
        // when levels are synchronized
        if (photonView.IsMine) {
            LocalPlayerInstance = gameObject;
            GetComponentInChildren<MeshRenderer>().material.color = Color.blue;

            // Reference Camera on run-time
            PunNetworkManager.singleton._vCam.Follow = CameraRoot;

            // Reference Input on run-time
            PlayerInput _pInput = GetComponent<PlayerInput>();
            _pInput.actions = PunNetworkManager.singleton._inputActions;
        }
        else {
            GetComponent<FirstPersonController>().enabled = false;
            OnPlayerPropertiesUpdate(photonView.Owner, photonView.Owner.CustomProperties);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player target, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(target, changedProps);
        if (changedProps.ContainsKey(PunGameSetting.PLAYER_COLOR) &&
            target.ActorNumber == photonView.ControllerActorNr)
        {
            object colors;
            if (changedProps.TryGetValue(PunGameSetting.PLAYER_COLOR, out colors))
            {
                GetComponentInChildren<MeshRenderer>().material.color = PunGameSetting.GetColor((int)colors);
            }
            return;
        }
    }

}
