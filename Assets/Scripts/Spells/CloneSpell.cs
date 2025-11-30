
using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using UnityEngine;

public class CloneSpell : AbstractSpell
{
	[SerializeField] float _buildupDuration_seconds = 3f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		var clone = character.gameObject.InstantiateWithTransform();
		var normalScale = clone.transform.localScale;
		clone.transform.localScale = Vector2.zero;
		clone.transform.DOScale(normalScale, _buildupDuration_seconds);
	}
}