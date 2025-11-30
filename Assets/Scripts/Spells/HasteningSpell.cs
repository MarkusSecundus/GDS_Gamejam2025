
using MarkusSecundus.Utils.Extensions;
using UnityEngine;

public class HasteningSpell : AbstractSpell
{
	[SerializeField] float Duration_seconds = 5f;

	[SerializeField] float MovementMultiplier = 2f;
	[SerializeField] float ShootMultiplier = 2f;
	[SerializeField] float CosmeticMultiplier = 2f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		this.ApplyParticles(character);

		if(MovementMultiplier != 1f)
			character._movementSpeed *= MovementMultiplier;
		if (ShootMultiplier != 1f)
		{
			character._shootCooldown_seconds *= ShootMultiplier;
			character._effects.GunKnockbackBuildup *= CosmeticMultiplier;
			character._effects.GunKnockbackSustain *= CosmeticMultiplier;
			character._effects.GunKnockbackEnd *= CosmeticMultiplier;
		}

		character.InvokeWithDelay(() => {
			if (MovementMultiplier != 1f)
				character._movementSpeed /= MovementMultiplier;
			if (ShootMultiplier != 1f)
			{
				character._shootCooldown_seconds /= ShootMultiplier;
				character._effects.GunKnockbackBuildup /= CosmeticMultiplier;
				character._effects.GunKnockbackSustain /= CosmeticMultiplier;
				character._effects.GunKnockbackEnd /= CosmeticMultiplier;
			}
			RemoveParticles(character);
		}, Duration_seconds);
	}
}
