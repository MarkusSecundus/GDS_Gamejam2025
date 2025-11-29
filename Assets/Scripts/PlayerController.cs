using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Physics;
using MarkusSecundus.Utils.Primitives;
using UnityEngine;


public abstract class CharacterController : MonoBehaviour
{
	[field: SerializeField] public float HP { get; private set; } = -1f;
	[field: SerializeField] public float MaxHP { get; private set; }

	public bool IsDead => HP <= 0f;

	[field: SerializeField] protected Transform _rotatable { get; private set; }
	[field: SerializeField] protected float _movementSpeed { get; private set; } = 1f;
	[field: SerializeField] protected float _maxVelocityChange { get; private set; } = 1f;

	[field: SerializeField] protected Rigidbody2D _projectile { get; private set; }
	[field: SerializeField] protected float _shootForce {  get; private set; } = 1f;
	[field: SerializeField] protected float _shootCooldown_seconds { get; private set; } = 0.3f;

	Rigidbody2D _rigidbody;
	protected virtual void Start()
	{
		if (HP < 0f) HP = MaxHP;
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	protected virtual void Update()
	{
		_rotatable.rotation = _getLookRotation().AsRotation2D();

		var targetVelocity = _getTargetMovement().xy0() * _movementSpeed;
		_rigidbody.SteerToVelocity(targetVelocity, _maxVelocityChange);

		if (_isShootCommand()) _doShoot();
	}

	private double _nextAllowedShootTimestamp = float.NegativeInfinity;
	private void _doShoot()
	{
		if (Time.timeAsDouble < _nextAllowedShootTimestamp) return;

		_nextAllowedShootTimestamp = Time.timeAsDouble + _shootCooldown_seconds;

		var newProjectile = _projectile.gameObject.InstantiateWithTransform(true, true, true, false).GetComponent<Rigidbody2D>();
		var shootDirection = (newProjectile.position - transform.position.xy()).normalized;
		newProjectile.AddForce(shootDirection * _shootForce, ForceMode2D.Impulse);
	}

	protected abstract float _getLookRotation();
	protected abstract Vector2 _getTargetMovement();

	protected abstract bool _isShootCommand();

	public void DoDamage(float damage)
	{
		if (IsDead) return;
		HP -= damage;
		if (IsDead)
			_doDie();
		else
			_hurtAnimation();
	}

	[System.Serializable]
	public class EffectDetails
	{
		public SpriteRenderer[] Sprites;
		public Color HurtColor = Color.red;

		public float DeathEffectDuration = 1f;
		public float DeathColorBuildup = 0.5f;

		public float HurtBlinkBuildup = 0.1f;
		public float HurtBlinkSustain = 0.1f;
		public float HurtBlinkEnd = 0.1f;
	}
	[SerializeField] EffectDetails _effects;

	bool _isEffectInProgress = false;
	private void _doDie()
	{
		Debug.Log($"Dies: {this}", this);
		_isEffectInProgress = true;
		foreach (var spr in _effects.Sprites)
		{
			spr.DOColor(_effects.HurtColor, _effects.DeathColorBuildup);
		}
		transform.DOScale(0f, _effects.DeathEffectDuration).OnComplete(() =>
		{
			Destroy(gameObject);
		});
	}


	private void _hurtAnimation()
	{
		if (_isEffectInProgress) return;

		foreach (var spr in _effects.Sprites)
		{
			spr.DOColor(_effects.HurtColor, _effects.HurtBlinkBuildup).OnComplete(() =>
			{
				if (!spr) return;
				spr.DOColor(Color.white, _effects.HurtBlinkEnd).SetDelay(_effects.HurtBlinkSustain).OnComplete(
					() => {
						_isEffectInProgress = false;
					}
				);
			});
		}
	}
}



public class PlayerController : CharacterController
{

    protected override float _getLookRotation()
    {
        var worldCursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var lookDirection = worldCursorPos - _rotatable.position;
        return Mathf.Rad2Deg* Mathf.Atan2(lookDirection.y, lookDirection.x);
    }
    protected override Vector2 _getTargetMovement()
    {
        Vector2 ret = Vector3.zero;
        ret.x += Input.GetAxis("Horizontal");
        ret.y += Input.GetAxis("Vertical");
        return ret;
	}
	protected override bool _isShootCommand()
		=> Input.GetKeyDown(KeyCode.Mouse0);
}
