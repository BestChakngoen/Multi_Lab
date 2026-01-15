using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;

public class PunWorldGroundColor : MonoBehaviourPunCallbacks
{
    [Header("Ground Renderer")]
    public Renderer groundRenderer;

    [Header("Color Preset (0–7)")]
    public Color[] colorPresets = new Color[8];

    private void Awake()
    {
        if (groundRenderer == null)
            groundRenderer = GetComponent<Renderer>();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);

        if (propertiesThatChanged.TryGetValue(
                PunWorldStateKey.GROUND_COLOR_INDEX,
                out object colorIndexObj))
        {
            int colorIndex = (int)colorIndexObj;
            ApplyColor(colorIndex);
        }
    }

    private void ApplyColor(int index)
    {
        if (index < 0 || index >= colorPresets.Length)
            return;

        groundRenderer.material.color = colorPresets[index];
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        // ผู้เล่นที่เข้าทีหลังต้อง sync สีปัจจุบัน
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(
                PunWorldStateKey.GROUND_COLOR_INDEX,
                out object colorIndexObj))
        {
            ApplyColor((int)colorIndexObj);
        }
    }
}