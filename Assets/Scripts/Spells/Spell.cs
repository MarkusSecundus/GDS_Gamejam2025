using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using UnityEngine;

public class HealingSpell : AbstractProjectileController
{
	[SerializeField] float HealAmount = 3f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		character.DoHeal(HealAmount);
	}
}


public class HasteningSpell : AbstractProjectileController
{
	[SerializeField] float Multiplier = 2f;
	[SerializeField] float Duration_seconds = 5f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		character._movementSpeed *= Multiplier;
		character.InvokeWithDelay(() => { character._movementSpeed /= Multiplier; }, Duration_seconds);
	}
}


public class RateOfFireSpell : AbstractProjectileController
{
	[SerializeField] float Multiplier = 2f;
	[SerializeField] float CosmeticMultiplier = 2f;
	[SerializeField] float Duration_seconds = 5f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		character._shootCooldown_seconds		*= Multiplier;
		character._effects.GunKnockbackBuildup	*= CosmeticMultiplier;
		character._effects.GunKnockbackSustain	*= CosmeticMultiplier;
		character._effects.GunKnockbackEnd		*= CosmeticMultiplier;
		
		character.InvokeWithDelay(() => {
			character._shootCooldown_seconds		/= Multiplier;
			character._effects.GunKnockbackBuildup	/= CosmeticMultiplier;
			character._effects.GunKnockbackSustain	/= CosmeticMultiplier;
			character._effects.GunKnockbackEnd		/= CosmeticMultiplier;
		}, Duration_seconds);
	}
}

public class CloneSpell : AbstractProjectileController
{
	protected override void DamageTheCharacter(CharacterController character)
	{
		character.gameObject.InstantiateWithTransform();
	}
}

public class DrainHealthSpell : AbstractProjectileController
{
	[SerializeField] CharacterController _caster;
	protected override void DamageTheCharacter(CharacterController character)
	{
		var drainedHP = character.HP;
		character.DoDamage(drainedHP);
		_caster.DoHeal(drainedHP);
	}
}

public class ChangeSizeSpell : AbstractProjectileController
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
