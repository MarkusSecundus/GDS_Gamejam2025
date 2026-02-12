using MarkusSecundus.Utils.Extensions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
	[SerializeField] TMP_Text _entryTemplate;

	private void Start()
	{
		_entryTemplate.gameObject.SetActive(false);
	}

	List<TMP_Text> _entries = new();

	public void DoUpdateLeaderboard()
	{
		foreach (var entry in _entries) { Destroy(entry.gameObject); }
		_entries.Clear();

		foreach(var player in GameObject.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).OrderByDescending(p=>p.Score))
		{
			var newEntry = _entryTemplate.gameObject.InstantiateWithTransform().GetComponent<TMP_Text>();
			newEntry.gameObject.SetActive(true);

			newEntry.text = string.Format(newEntry.text, player.Score, player.MainColor.ToHexString());
			Debug.Log(newEntry.text);

			_entries.Add(newEntry);
		}
		
	}
}
