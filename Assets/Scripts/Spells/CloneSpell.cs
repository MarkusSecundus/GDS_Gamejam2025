
using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using UnityEngine;

public class CloneSpell : AbstractSpell
{
	[SerializeField] float _buildupDuration_seconds = 3f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		var clone = character.gameObject.InstantiateWithTransform().GetComponent<CharacterController>();
		var normalScale = clone.transform.localScale;
		clone.transform.localScale = Vector2.zero;
		if (clone is EnemyController enemy) enemy._isStillGrowing = true;
		clone.gameObject.SetActive(true);
		clone.transform.DOScale(normalScale, _buildupDuration_seconds).OnComplete(() => { if (clone is EnemyController enemy) enemy._isStillGrowing = false; });
	}
}