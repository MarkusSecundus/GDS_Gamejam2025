

using UnityEngine;

public class DrainHealthSpell : AbstractSpell
{
	[SerializeField] CharacterController _caster;
	[SerializeField] float _maxDamage = 4.0f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		var drainedHP = Mathf.Min(_maxDamage, character.HP);
		character.DoDamage(drainedHP, this);
		_caster.DoHeal(drainedHP);
	}
}