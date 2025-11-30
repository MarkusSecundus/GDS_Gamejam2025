using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Physics;
using MarkusSecundus.Utils.Primitives;
using UnityEditor.U2D.Animation;
using UnityEngine;

public abstract class AbstractProjectileController : MonoBehaviour
{

    [SerializeField] CharacterController _toIgnore;

    Vector3 _startPosition;
    [SerializeField] float _maxDistanceTraveled = 30f;
    Rigidbody2D _rb;
    [SerializeField] protected bool _canHitOtherBullets = true;
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _startPosition = transform.position;
    }

    void Update()
    {
        if (transform.position.DistanceSqr(_startPosition) > (_maxDistanceTraveled * _maxDistanceTraveled))
        {
            _doDie();
        }
        if (_isDead)
        {
            _rb.SteerToVelocity(Vector2.zero, _effects.DeathStopForce, ForceMode2D.Force);
        }
    }


	[System.Serializable]
	public class EffectsConfig
	{
        public SpriteRenderer BulletSprite;
        public float DeathFadeDuration = 0.2f;
        public float DeathStopForce = 0.2f;
        public ParticleSystem Particles;
	}
    [SerializeField] EffectsConfig _effects;

    private void OnCollisionEnter2D(Collision2D collision) => _onHit2D(collision.collider);
	private void OnTriggerEnter2D(Collider2D collision)=> _onHit2D(collision);


    protected abstract void DamageTheCharacter(CharacterController character);

    void _onHit2D(Collider2D collider)
	{
		if (_isDead) return;

		var character = collider.attachedRigidbody?.GetComponent<CharacterController>();
		if (character)
		{
            if (character == _toIgnore) return;
            DamageTheCharacter(character);
		}
        if ((!_canHitOtherBullets) && collider.attachedRigidbody?.GetComponent<BulletController>())
            return;
		_doDie();
	}

	bool _isDead = false;
    void _doDie()
    {
        _isDead = true;
        if (_effects.Particles)
            _effects.Particles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        
        if(_effects.BulletSprite)
            _effects.BulletSprite.DOColor(new Color(0, 0, 0, 0), _effects.DeathFadeDuration).OnComplete(()=>Destroy(gameObject));
        else
            this.InvokeWithDelay(()=>Destroy(gameObject), _effects.DeathFadeDuration);
    }
}

public class BulletController : AbstractProjectileController
{
	[field: SerializeField] float Damage { get; set; } = 1f;

	protected override void DamageTheCharacter(CharacterController character)
	{
		character.DoDamage(Damage);
	}
}
