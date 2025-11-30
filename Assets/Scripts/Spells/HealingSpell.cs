
using UnityEngine;

public class HealingSpell : AbstractSpell
{
	[SerializeField] float HealAmount = 3f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		character.DoHeal(HealAmount);
	}
}