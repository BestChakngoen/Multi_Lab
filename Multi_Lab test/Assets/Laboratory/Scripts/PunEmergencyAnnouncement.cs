using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;


public class PunEmergencyAnnouncement : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [Header("Emergency Message")]
    public string emergencyMessage = "Emergency Mode Activated!";
    public TextMeshProUGUI emergencyText;
    
    // =========================
    // REGISTER / UNREGISTER
    // =========================
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // =========================
    // INPUT & SEND EVENT
    // =========================
    private void Update()
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            SendEmergencyEvent();
        }
    }

    private void SendEmergencyEvent()
    {
        object content = emergencyMessage;

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };

        SendOptions sendOptions = new SendOptions
        {
            Reliability = true
        };

        PhotonNetwork.RaiseEvent(
            PunRaiseEventCode.EMERGENCY_ANNOUNCEMENT,
            content,
            options,
            sendOptions
        );

        Debug.Log("[Sender] Emergency Event Sent");
    }

    // =========================
    // RECEIVE EVENT
    // =========================
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != PunRaiseEventCode.EMERGENCY_ANNOUNCEMENT)
            return;

        string message = photonEvent.CustomData as string;

        if (!string.IsNullOrEmpty(message))
        {
            Debug.Log($"[Emergency Announcement] {message}");
            ShowOnUI(message); // ⭐ แสดงผลบน UI
        }
    }


    private Coroutine hideRoutine;

    private void ShowOnUI(string message)
    {
        if (emergencyText == null) return;

        emergencyText.text = message;
        emergencyText.gameObject.SetActive(true);

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(HideAfterDelay(3f));
    }

    private IEnumerator HideAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        emergencyText.gameObject.SetActive(false);
    }


}