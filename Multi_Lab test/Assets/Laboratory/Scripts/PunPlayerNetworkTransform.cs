using Photon.Pun;
using StarterAssets;
using UnityEngine;

/// <summary>
/// Handles the RUNTIME SYNCHRONIZATION of the character's transform using PUN2.
/// Its single responsibility is to serialize/deserialize state via OnPhotonSerializeView.
/// </summary>
[RequireComponent(typeof(FirstPersonController))]
public class PunPlayerNetworkTransform : MonoBehaviourPun, IPunObservable
{
    // Cached reference to the character's movement logic to get camera pitch.
    private FirstPersonController _controllerLogic;

    // // --- Variables to store network state for proxies ---
    // private Vector3 _networkPosition;
    // private Quaternion _networkRotation;
    // private float _networkCameraPitch;

    
    // =========================
    // STRUCT
    // =========================
    private struct State
    {
        public Vector3 position;
        public Quaternion rotation;
        public float cameraPitch;
        public double timestamp;
    }

    // =========================
    // SETTINGS
    // =========================
    [SerializeField] private int bufferSize = 20;
    [SerializeField] private float interpolationDelay = 0.1f; // 100 ms

    // =========================
    // BUFFER
    // =========================
    private State[] stateBuffer;
    private int stateCount;
    private void Awake()
    {
        // This script also needs a reference to the controller to get camera data.
        _controllerLogic = GetComponent<FirstPersonController>();
        stateBuffer = new State[bufferSize];
        stateCount = 0;
    }

    /// <summary>
    /// This is the heart of PUN2's state synchronization.
    /// It's called automatically by the PhotonView to send and receive data.
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // --- We are the owner of this object: SEND data ---
            // This code runs on the local player's machine.
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(_controllerLogic.GetCameraPitch());
        }
        else
        {
            // --- We are a proxy: RECEIVE data ---
            // This code runs on remote players' machines.
            // We receive the data in the same order we sent it.
            // _networkPosition = (Vector3)stream.ReceiveNext();
            // _networkRotation = (Quaternion)stream.ReceiveNext();
            // _networkCameraPitch = (float)stream.ReceiveNext();
            
            // PROXY → RECEIVE
            Vector3 pos = (Vector3)stream.ReceiveNext();
            Quaternion rot = (Quaternion)stream.ReceiveNext();
            float pitch = (float)stream.ReceiveNext();

            // Shift buffer (เลื่อนข้อมูลเก่า)
            for (int i = bufferSize - 1; i > 0; i--)
            {
                stateBuffer[i] = stateBuffer[i - 1];
            }

            // ใส่ข้อมูลใหม่ไว้หน้าสุด
            stateBuffer[0] = new State
            {
                position = pos,
                rotation = rot,
                cameraPitch = pitch,
                timestamp = info.SentServerTime
            };

            stateCount = Mathf.Min(stateCount + 1, bufferSize);
        }
    }

    void LateUpdate() {

        // This is where we read the network state for visual representation.
        if (!photonView.IsMine)
        {
            //InterpolateMovement();
            Interpolate();
        }
    }

    /// <summary>
    /// Handles the smooth interpolation for remote player objects.
    /// </summary>
    // private void InterpolateMovement()
    // {
    //     if (_controllerLogic == null) return;
    //
    //     float interpolationSpeed = 20f; // Tweak for smoothness
    //
    //     // Move the proxy character towards the synced position.
    //     transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * interpolationSpeed);
    //     transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * interpolationSpeed);
    //
    //     // Update the proxy's camera pitch.
    //     _controllerLogic.SetCameraPitch(_networkCameraPitch);
    // }
    private void Interpolate()
    {
        double renderTime = PhotonNetwork.Time - interpolationDelay;

        // หา state คู่ที่ครอบ renderTime
        for (int i = 0; i < stateCount - 1; i++)
        {
            if (stateBuffer[i].timestamp >= renderTime &&
                stateBuffer[i + 1].timestamp <= renderTime)
            {
                State newer = stateBuffer[i];
                State older = stateBuffer[i + 1];

                double length = newer.timestamp - older.timestamp;
                float t = 0f;

                if (length > 0.0001)
                    t = (float)((renderTime - older.timestamp) / length);

                // Interpolate
                transform.position = Vector3.Lerp(
                    older.position,
                    newer.position,
                    t
                );

                transform.rotation = Quaternion.Slerp(
                    older.rotation,
                    newer.rotation,
                    t
                );

                _controllerLogic.SetCameraPitch(
                    Mathf.Lerp(older.cameraPitch, newer.cameraPitch, t)
                );

                return;
            }
        }

        // Fallback 
        transform.position = stateBuffer[0].position;
        transform.rotation = stateBuffer[0].rotation;
        _controllerLogic.SetCameraPitch(stateBuffer[0].cameraPitch);
    }
}
