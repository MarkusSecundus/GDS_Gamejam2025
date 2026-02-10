using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RoomListEntry : MonoBehaviour
{
	[SerializeField] TMP_Text roomNameLabel;
	[SerializeField] Button joinRoomButton;

	public void Setup(string text, UnityAction joinRoomCallback)
	{
		roomNameLabel.text = text;
		joinRoomButton.onClick.AddListener(joinRoomCallback);
	}
}
