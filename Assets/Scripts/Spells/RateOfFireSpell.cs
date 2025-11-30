
using MarkusSecundus.Utils.Extensions;
using UnityEngine;

public class RateOfFireSpell : AbstractSpell
{
	[SerializeField] float Multiplier = 2f;
	[SerializeField] float CosmeticMultiplier = 2f;
	[SerializeField] float Duration_seconds = 5f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		character._shootCooldown_seconds *= Multiplier;
		character._effects.GunKnockbackBuildup *= CosmeticMultiplier;
		character._effects.GunKnockbackSustain *= CosmeticMultiplier;
		character._effects.GunKnockbackEnd *= CosmeticMultiplier;

		character.InvokeWithDelay(() => {
			character._shootCooldown_seconds /= Multiplier;
			character._effects.GunKnockbackBuildup /= CosmeticMultiplier;
			character._effects.GunKnockbackSustain /= CosmeticMultiplier;
			character._effects.GunKnockbackEnd /= CosmeticMultiplier;
		}, Duration_seconds);
	}
}