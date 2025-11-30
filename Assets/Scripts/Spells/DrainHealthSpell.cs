

using UnityEngine;

public class DrainHealthSpell : AbstractSpell
{
	[SerializeField] CharacterController _caster;
	protected override void DamageTheCharacter(CharacterController character)
	{
		var drainedHP = character.HP;
		character.DoDamage(drainedHP);
		_caster.DoHeal(drainedHP);
	}
}