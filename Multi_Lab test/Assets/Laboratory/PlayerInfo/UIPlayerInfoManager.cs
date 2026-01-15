using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerInfoManager : MonoBehaviour
{
    public Slider _HealthBar;
    public Slider _SterminaBar;
    public TextMeshProUGUI _TextNickName;
    PunHealth _userHealth;
    PunStermina _userStermina;

    PhotonView photonView;
    public void Awake() {
        _userHealth = GetComponentInParent<PunHealth>();
        _userStermina = GetComponentInParent<PunStermina>();

        photonView = GetComponentInParent<PhotonView>();

        if (photonView.IsMine)
            SetLocalUI();
    }

    public void SetLocalUI()
    {
        //UI Control
        GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
        GetComponent<UIDirectionControl>().enabled = false;
    }

    public void SetNickName(string name)
    {
        if (_TextNickName != null)
            _TextNickName.text = name;
    }

    private void FixedUpdate()
    {
        if (_HealthBar != null && _userHealth != null)
        {
            int currentHealth = _userHealth.currentHealth;
            _HealthBar.value = (float)currentHealth / (float)100;
        }

        if (_HealthBar != null && _userHealth != null)
        {
            int currentStermina = _userStermina.currentStermina;
            _SterminaBar.value = (float)currentStermina / (float)100;
        }
    }
}
