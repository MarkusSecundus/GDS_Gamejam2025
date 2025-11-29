using MarkusSecundus.Utils.Behaviors.Physics;
using UnityEngine;

public class AreaDamager : MonoBehaviour
{
	[SerializeField] CharacterController _toIgnore;
	[SerializeField] float Damage;

	TriggerActivityInfo _info;
	private void Start()
	{
		_info = GetComponent<TriggerActivityInfo>();
	}

	public void DoDamage()
	{
		foreach(var c in _info.GetActiveTriggers2D())
		{
			if (!c || !c.attachedRigidbody) continue;
			if (c.attachedRigidbody.gameObject == _toIgnore.gameObject) continue;
			CharacterController ctrl = c.attachedRigidbody.GetComponent<CharacterController>();
			if (!ctrl) continue;
			ctrl.DoDamage(Damage);
		}
	}
}
