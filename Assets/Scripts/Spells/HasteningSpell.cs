
using MarkusSecundus.Utils.Extensions;
using UnityEngine;

public class HasteningSpell : AbstractSpell
{
	[SerializeField] float Multiplier = 2f;
	[SerializeField] float Duration_seconds = 5f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		character._movementSpeed *= Multiplier;
		character.InvokeWithDelay(() => { character._movementSpeed /= Multiplier; }, Duration_seconds);
	}
}
