using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine.InputSystem;
using ExitGames.Client.Photon;
using System;
using Unity.Cinemachine;

public class PunNetworkManager : ConnectAndJoinRandom
{
    public static PunNetworkManager singleton;
    //public bool isUseMainCamera;
    public CinemachineCamera _vCam;
    public InputActionAsset _inputActions;

    [Header("Spawn Info")]
    [Tooltip("The prefab to use for representing the player")]
    public GameObject GamePlayerPrefab;


    public enum gamestate
    {
        None = 0,
        GameStart = 1,
        GamePlay = 2,
        GameOver = 3
    }

    public gamestate _currentGamestate = gamestate.GameStart;
    public gamestate CurrentGamestate
    {
        get { return _currentGamestate; }
        set {
            _currentGamestate = value;

            if (PhotonNetwork.CurrentRoom == null)
                return;

            Hashtable props = new Hashtable
            {
                { PunGameSetting.GAMESTATE, _currentGamestate.ToString() }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
    }

    /// <summary>
    /// Create delegate Method
    /// </summary>
    public delegate void GameStartCallback();
    public static event GameStartCallback OnGameStart;

    public delegate void GameOverCallback();
    public static event GameOverCallback OnGameOver;

    private void Awake()
    {
        singleton = this;

        //Add Reference Method to Delegate Method
        OnGameStart += GameStartSetting;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        Debug.Log("New Player. " + newPlayer.ToString());
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (PunUserNetControl.LocalPlayerInstance == null)
        {
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManagerHelper.ActiveSceneName);
            //PunNetworkManager.singleton.SpawnPlayer();
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            PhotonNetwork.Instantiate(GamePlayerPrefab.name,
                new Vector3(0f, 5f, 0f), Quaternion.identity, 0);

            //isGameStart = true;
            CurrentGamestate = gamestate.GameStart;
            PunNetworkManager.singleton.SpawnPlayer();
        }
        else
        {
            Debug.Log("Ignoring scene load for " + SceneManagerHelper.ActiveSceneName);
        }
    }

    private void GameStartSetting()
    {
        CurrentGamestate = gamestate.GamePlay;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        switch(_currentGamestate)
        {
            case gamestate.GameStart:
                OnGameStart();
                break;

            case gamestate.GamePlay:
                //Game Loop Logic
                
                break;
        }
    }

    public void gameStateUpdate(Hashtable propertiesThatChanged)
    {
        object gameStateFromProps;

        if (propertiesThatChanged.TryGetValue(PunGameSetting.GAMESTATE, out gameStateFromProps))
        {
            Debug.Log("GetStartTime Prop is : " + gameStateFromProps);
            _currentGamestate = (gamestate)Enum.Parse(typeof(gamestate), (string)gameStateFromProps);
        }

        if(_currentGamestate == gamestate.GameOver)
            OnGameOver();
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
        gameStateUpdate(propertiesThatChanged);
    }

}
