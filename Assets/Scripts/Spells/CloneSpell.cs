
using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Primitives;
using UnityEngine;

public class CloneSpell : AbstractSpell
{
	[SerializeField] float _buildupDuration_seconds = 3f;
	[SerializeField] Vector2 _placementOffset = new Vector2(0.3f, 0.3f);
	protected override void DamageTheCharacter(CharacterController character)
	{
		var clone = character.gameObject.InstantiateWithTransform().GetComponent<CharacterController>();
		var normalScale = clone.transform.localScale;
		clone.transform.localScale = Vector2.zero;
		clone.transform.localPosition += _placementOffset.xy0();
		if (clone is EnemyController enemy) enemy._isStillGrowing = true;
		clone.gameObject.SetActive(true);
		clone.transform.DOScale(normalScale, _buildupDuration_seconds).OnComplete(() => { if (clone is EnemyController enemy) enemy._isStillGrowing = false; });
	}
}