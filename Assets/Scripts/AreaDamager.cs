using MarkusSecundus.Utils.Behaviors.Physics;
using UnityEngine;

public class AreaDamager : MonoBehaviour
{
	[SerializeField] CharacterController _toIgnore;
	[SerializeField] float Damage;

	public TriggerActivityInfo TriggerInfo { get; private set; }
	private void Start()
	{
		TriggerInfo = GetComponent<TriggerActivityInfo>();
	}

	public void DoDamage()
	{
		foreach(var c in TriggerInfo.GetActiveTriggers2D())
		{
			if (!c || !c.attachedRigidbody) continue;
			if (c.attachedRigidbody.gameObject == _toIgnore.gameObject) continue;
			CharacterController ctrl = c.attachedRigidbody.GetComponent<CharacterController>();
			if (!ctrl) continue;
			ctrl.DoDamage(Damage);
		}
	}
}
