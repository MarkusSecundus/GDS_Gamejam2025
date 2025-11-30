
using DG.Tweening;
using UnityEngine;

public class ChangeSizeSpell : AbstractSpell
{
	[SerializeField] float Multiplier = 2f;
	[SerializeField] float Buildup_seconds = 2f;
	[SerializeField] float Sustain_seconds = 10f;
	[SerializeField] float End_seconds = 2f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		character.transform.DOScale(character.transform.localScale * Multiplier, Buildup_seconds).OnComplete(() =>
		{
			character.transform.DOScale(character.transform.localScale / Multiplier, End_seconds).SetDelay(Sustain_seconds);
		});
	}
}