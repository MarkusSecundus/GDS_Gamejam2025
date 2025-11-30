using DG.Tweening;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Physics;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Randomness;
using UnityEngine;
using UnityEngine.Events;


public enum WeaponType
{
	Ranged, Mellee
}

public abstract class CharacterController : MonoBehaviour
{
	[field: SerializeField] public float HP { get; private set; } = -1f;
	[field: SerializeField] public float MaxHP { get; private set; }

	public bool IsDead => HP <= 0f;

	[field: SerializeField] protected Transform _rotatable { get; private set; }
	[field: SerializeField] public float _movementSpeed { get; set; } = 1f;
	[field: SerializeField] protected float _maxVelocityChange { get; private set; } = 1f;

	[field: SerializeField] protected Rigidbody2D _projectile { get; private set; }
	[field: SerializeField] protected float _shootForce {  get; private set; } = 1f;
	[field: SerializeField] public float _shootCooldown_seconds { get; set; } = 0.3f;

	[SerializeField] protected WeaponType _favouriteWeapon = WeaponType.Ranged;

	[SerializeField] UnityEvent OnDie;

	[SerializeField] TopBarController _spellNameDisplay;

	protected AudioSource _audioPlayer;

	[System.Serializable]
	public class SoundEffects
	{
		public AudioClip HurtSound;
		public AudioClip HealSound;
		public AudioClip DieSound;
		public AudioClip SidearmSound;
	}
	[SerializeField] SoundEffects _sounds;

	Rigidbody2D _rigidbody;
	protected virtual void Start()
	{
		if (HP < 0f) HP = MaxHP;
		_rigidbody = GetComponent<Rigidbody2D>();
		_audioPlayer = GetComponent<AudioSource>();
	}

	protected virtual void Update()
	{
		_rotatable.rotation = _getLookRotation().AsRotation2D();

		var targetVelocity = _getTargetMovement().xy0() * _movementSpeed;
		_rigidbody.SteerToVelocity(targetVelocity, _maxVelocityChange);

		if (_isShootCommand()) _doShoot();
		else if (_isSidearmCommand()) _doSidearm();
	}

	private double _nextAllowedShootTimestamp = float.NegativeInfinity;
	private void _doShoot()
	{
		if (Time.timeAsDouble < _nextAllowedShootTimestamp) return;
		_nextAllowedShootTimestamp = Time.timeAsDouble + _shootCooldown_seconds;

		var originalGunPosition = _effects.GunObject.transform.localPosition.xy();
		_effects.GunObject.DOLocalMove(originalGunPosition + _effects.GunKnockback, _effects.GunKnockbackBuildup).OnComplete(() =>
		{
			var newProjectile = _getProjectile().gameObject.InstantiateWithTransform(true, true, true, false).GetComponent<Rigidbody2D>();
			var shootDirection = (newProjectile.position - transform.position.xy()).normalized;
			newProjectile.AddForce(shootDirection * _shootForce, ForceMode2D.Impulse);
			var projectile = newProjectile.GetComponent<AbstractProjectileController>();
			if (_spellNameDisplay && projectile is AbstractSpell spell)
			{
				_spellNameDisplay.ShowText(spell.SpellName);
			}
			if (projectile.CastSound) _audioPlayer.PlayOneShot(projectile.CastSound);

			_effects.GunObject.DOLocalMove(originalGunPosition, _effects.GunKnockbackEnd).SetDelay(_effects.GunKnockbackSustain);
		});
	}

	protected virtual Rigidbody2D _getProjectile()
	{
		return _projectile;
	}

	private void _doSidearm()
	{
		if (Time.timeAsDouble < _nextAllowedShootTimestamp) return;
		_nextAllowedShootTimestamp = Time.timeAsDouble + _shootCooldown_seconds;

		//Debug.Log($"Doing sidearm!", this);

		if(_effects.GunObject) _effects.GunObject.gameObject.SetActive(false);
		_effects.SidearmAnimation.gameObject.SetActive(true);
		_effects.SidearmAnimation.SetTrigger("DoAttack");
		_audioPlayer.PlayOneShot(_sounds.SidearmSound);
	}

	public void OnSwordAnimFinished()
	{
		if(_favouriteWeapon != WeaponType.Mellee)
		{
			if (_effects.GunObject) _effects.GunObject.gameObject.SetActive(true);
			_effects.SidearmAnimation.gameObject.SetActive(false);
		}
	}


	protected abstract float _getLookRotation();
	protected abstract Vector2 _getTargetMovement();

	protected abstract bool _isShootCommand();
	protected virtual bool _isSidearmCommand() => false;

	public void DoDamage(float damage)
	{
		if (IsDead) return;
		HP -= damage;
		_effects.OnHPChange.Invoke($"{HP}");
		if (IsDead)
			DoDie(_effects.HurtColor);
		else
			_hurtAnimation(_effects.HurtColor, _effects.HurtBlinkBuildup, _effects.HurtBlinkSustain, _effects.HurtBlinkEnd, _sounds.HurtSound);
	}

	public void DoHeal(float hp)
	{
		if (IsDead) return;
		HP = Mathf.Min(HP + hp, MaxHP);
		_effects.OnHPChange.Invoke($"{HP}");
		_hurtAnimation(_effects.HealColor, _effects.HealBlinkBuildup, _effects.HealBlinkSustain, _effects.HealBlinkEnd, _sounds.HealSound);

	}

	[System.Serializable]
	public class EffectDetails
	{
		public SpriteRenderer[] Sprites;
		public Color HurtColor = Color.red;
		public Color HealColor = Color.green;

		public float DeathEffectDuration = 1f;
		public float DeathColorBuildup = 0.5f;

		public float HurtBlinkBuildup = 0.1f;
		public float HurtBlinkSustain = 0.1f;
		public float HurtBlinkEnd = 0.1f;

		public float HealBlinkBuildup = 0.2f;
		public float HealBlinkSustain = 0.2f;
		public float HealBlinkEnd = 0.2f;

		public UnityEvent<string> OnHPChange;

		public Vector2 GunKnockback = new Vector2(0.3f, 0f);
		public Transform GunObject;
		public float GunKnockbackBuildup = 0.1f;
		public float GunKnockbackSustain = 0.1f;
		public float GunKnockbackEnd = 0.1f;

		public Animator SidearmAnimation;
	}
	[SerializeField] public EffectDetails _effects;

	bool _isEffectInProgress = false;
	

	
	public void DoDie(Color dieColor)
	{
		Debug.Log($"Dies: {this}", this);
		_audioPlayer.PlayOneShot(_sounds.DieSound);

		_isEffectInProgress = true;
		foreach (var spr in _effects.Sprites)
		{
			spr.DOColor(dieColor, _effects.DeathColorBuildup);
		}
		OnDie?.Invoke();
		transform.DOScale(0f, _effects.DeathEffectDuration).OnComplete(() =>
		{
			Destroy(gameObject);
		});
	}


	private void _hurtAnimation(Color hurtColor, float buildup, float sustain, float end, AudioClip sound)
	{
		if(sound) _audioPlayer.PlayOneShot(sound);
		if (_isEffectInProgress) return;

		foreach (var spr in _effects.Sprites)
		{
			spr.DOColor(hurtColor, buildup).OnComplete(() =>
			{
				if (!spr) return;
				spr.DOColor(Color.white, end).SetDelay(sustain).OnComplete(
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
	[SerializeField] Transform _spellRoot;

	AbstractSpell[] _allSpells;
	int _currentSpellIdx = 0;
	protected override void Start()
	{
		base.Start();
		_allSpells = _spellRoot.GetComponentsInChildren<AbstractSpell>(true);
		RandomHelpers.Rand.Shuffle<AbstractSpell>(_allSpells);
	}


	protected override Rigidbody2D _getProjectile()
	{
		var ret = _allSpells[_currentSpellIdx];
		++_currentSpellIdx;
		if (_currentSpellIdx >= _allSpells.Length) _currentSpellIdx = 0;
		return ret.GetComponent<Rigidbody2D>();
	}

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


	protected override bool _isSidearmCommand()
		=> Input.GetKeyDown(KeyCode.Mouse1);
}
