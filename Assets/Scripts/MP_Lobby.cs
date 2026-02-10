using MarkusSecundus.Utils.Extensions;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MP_Lobby : MonoBehaviourPunCallbacks
{
	[SerializeField] RoomListEntry _roomEntryPrototype;
	[SerializeField] Button _createRoomButton;
	[SerializeField] TMP_InputField _createRoomNameField;

	private void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
		//Object.DontDestroyOnLoad(this);
		_roomEntryPrototype.gameObject.SetActive(false);
		_createRoomButton.onClick.AddListener(() => _createNewRoom(_createRoomNameField.text));
	}
	public override void OnConnectedToMaster()
	{
		Debug.Log($"{Time.time} | Just connected to master!");
		PhotonNetwork.JoinLobby();
	}


	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		var roomEntryLayout = _roomEntryPrototype.transform.parent;
		foreach(RectTransform ch in roomEntryLayout)
		{
			if (ch.gameObject.activeInHierarchy)
				Destroy(ch.gameObject);
		}
		foreach (var room in roomList)
		{
			var entry = _roomEntryPrototype.gameObject.InstantiateWithTransform().GetComponent<RoomListEntry>();
			entry.Setup(room.Name, () => _joinRoom(room));
		}
	}

	void _joinRoom(RoomInfo room)
	{
		Debug.Log($"Joining a room: '{room?.Name}'");
		PhotonNetwork.AutomaticallySyncScene = true;
		Object.DontDestroyOnLoad(gameObject);
		void onLoaded(UnityEngine.SceneManagement.Scene s, LoadSceneMode mode)
		{
			SceneManager.sceneLoaded -= onLoaded;
			this.InvokeWithDelay(() =>
			{
				var playerSpawn = GameObject.FindWithTag("PlayerSpawn");
				var player = PhotonNetwork.Instantiate("Player", playerSpawn.transform.position, playerSpawn.transform.rotation, 0);
				Destroy(gameObject);
			}, 1.0f);
		};
		SceneManager.sceneLoaded += onLoaded;

		PhotonNetwork.JoinRoom(room.Name);
	}
	void _createNewRoom(string roomName)
	{
		if(string.IsNullOrWhiteSpace(roomName))
			roomName = System.Guid.NewGuid().ToString();
		bool success = PhotonNetwork.CreateRoom(roomName);
		Debug.Log($"Creating a new room '{roomName}' -> {success}");
	}

	public override void OnCreatedRoom()
	{
		Debug.Log("Created room!");
		PhotonNetwork.AutomaticallySyncScene = true;
		Object.DontDestroyOnLoad(gameObject);
		void onLoaded (UnityEngine.SceneManagement.Scene s, LoadSceneMode mode)
		{
			SceneManager.sceneLoaded -= onLoaded;
			this.InvokeWithDelay(() =>
			{
				var playerSpawn = GameObject.FindWithTag("PlayerSpawn");
				var player = PhotonNetwork.Instantiate("Player", playerSpawn.transform.position, playerSpawn.transform.rotation, 0);
				Destroy(gameObject);
			}, 1.0f);
		};
		SceneManager.sceneLoaded += onLoaded;

		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.LoadLevel("SampleScene");

	}

	public override void OnJoinedRoom()
	{
		Debug.Log("Joined room");


		// When we join the room, we want to instantiate a game character to the game 
		// Object will be instantiated by PhotonNetwork special command
		// Player needs to have PhotonView component (it is in `Resources` folder)
		//PhotonNetwork.Instantiate("Player", new Vector3(0, 0.5f, 0), Quaternion.identity, 0, new object[] {  });

	}
	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Debug.Log($"Create room failed({returnCode}): '{message}'");
	}
}
