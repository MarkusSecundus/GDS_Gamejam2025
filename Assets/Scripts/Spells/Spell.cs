using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using System.Collections.Generic;
using UnityEngine;

public enum SpellType
{
	Neutral, Good, Bad
}

public abstract class AbstractSpell : AbstractProjectileController
{
	[field: SerializeField] public SpellType Type { get; private set; }
	[field: SerializeField] ParticleSystem _victimParticles;
	public AbstractSpell()
	{
		this._canHitOtherBullets = false;
	}


	Dictionary<CharacterController, ParticleSystem> _particlesPerVictim;
	protected void ApplyParticles(CharacterController victim)
	{
		if (!_victimParticles) return;

		var newParticles = Instantiate(_victimParticles);
		newParticles.transform.SetParent(victim.transform, false);
		newParticles.transform.localPosition = Vector2.zero;
		newParticles.gameObject.SetActive(true);
		_particlesPerVictim ??= new();
		_particlesPerVictim[victim] = newParticles;
	}
	protected void RemoveParticles(CharacterController victim)
	{
		if (!_victimParticles) return;

		if (_particlesPerVictim?.TryGetValue(victim, out var particles) == true)
		{
			particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			victim.InvokeWithDelay(() => Destroy(particles.gameObject), 5f);
		}
	}
}






