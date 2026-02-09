using MarkusSecundus.Utils.Extensions;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MP_Lobby : MonoBehaviourPunCallbacks
{
	[SerializeField] RectTransform _roomEntryPrototype;
	[SerializeField] Button _createRoomButton;
	[SerializeField] TMP_InputField _createRoomNameField;

	private void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
		Object.DontDestroyOnLoad(this);
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
		foreach(var room in roomList)
		{
			var entry = _roomEntryPrototype.gameObject.InstantiateWithTransform();
			entry.GetComponent<TMP_Text>().text = room.Name;
			entry.transform.GetComponent<Button>().onClick.AddListener(() => _joinRoom(room));
		}
	}

	void _joinRoom(RoomInfo room)
	{
		PhotonNetwork.JoinRoom(room.Name);
		Debug.Log($"Joining a room: '{room.Name}'");
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
