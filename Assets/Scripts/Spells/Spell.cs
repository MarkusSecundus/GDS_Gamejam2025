using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using UnityEngine;

public abstract class AbstractSpell : AbstractProjectileController
{

}

public class HealingSpell : AbstractSpell
{
	[SerializeField] float HealAmount = 3f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		character.DoHeal(HealAmount);
	}
}


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


public class RateOfFireSpell : AbstractSpell
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

public class CloneSpell : AbstractSpell
{
	[SerializeField] float _buildupDuration_seconds = 3f;
	protected override void DamageTheCharacter(CharacterController character)
	{
		var clone = character.gameObject.InstantiateWithTransform();
		var normalScale = clone.transform.localScale;
		clone.transform.localScale = Vector2.zero;
		clone.transform.DOScale(normalScale, _buildupDuration_seconds);
	}
}

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
