
using MarkusSecundus.Utils.Extensions;
using UnityEngine;

public class DamageSpell : AbstractSpell
{
	[SerializeField] float Damage;
	protected override void DamageTheCharacter(CharacterController character)
	{
		character.DoDamage(Damage, this);
	}
}