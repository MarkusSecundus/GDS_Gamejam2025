using DG.Tweening;
using MarkusSecundus.Utils.Behaviors.Cosmetics;
using MarkusSecundus.Utils.Behaviors.GameObjects;
using MarkusSecundus.Utils.Behaviors.GUI;
using MarkusSecundus.Utils.Datastructs;
using MarkusSecundus.Utils.Extensions;
using MarkusSecundus.Utils.Physics;
using MarkusSecundus.Utils.Primitives;
using MarkusSecundus.Utils.Randomness;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public enum WeaponType
{
	Ranged, Mellee
}

public abstract class CharacterController : MonoBehaviourPun, IPunObservable
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

	[SerializeField] protected UnityEvent OnDie;

	[SerializeField] protected TopBarController _spellNameDisplay;

	[SerializeField] int ScoreForKilling = 100;

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



	protected System.Random _rand;

	protected virtual void Start()
	{
		if (HP < 0f) HP = MaxHP;
		_rigidbody = GetComponent<Rigidbody2D>();
		_audioPlayer = GetComponent<AudioSource>();

		var randomSeed = (int)this.photonView.InstantiationData[0];
		_rand = new System.Random(randomSeed);
		this.name = this.name + "_" + randomSeed.ToString();

		transform.position += new Vector3(_rand.NextFloat(-2f, 2f), _rand.NextFloat(-2f, 2f), 0f);
	}


	protected virtual void Update()
	{
		if (IsDead) return;
		if ((! _isAttackNetworked()) || this.photonView.IsMine)
		{
			_rotatable.rotation = _getLookRotation().AsRotation2D();

			var targetVelocity = _getTargetMovement().xy0() * _movementSpeed;
			_rigidbody.SteerToVelocity(targetVelocity, _maxVelocityChange);

			if (_isShootCommand()) _doShoot();
			else if (_isSidearmCommand()) _doSidearm();
		}
	}

	private double _nextAllowedShootTimestamp = float.NegativeInfinity;
	private void _doShoot()
	{
		if (Time.timeAsDouble < _nextAllowedShootTimestamp) return;
		_nextAllowedShootTimestamp = Time.timeAsDouble + _shootCooldown_seconds;

		var originalGunPosition = _effects.GunObject.transform.localPosition.xy();
		_effects.GunObject.DOLocalMove(originalGunPosition + _effects.GunKnockback, _effects.GunKnockbackBuildup).OnComplete(() =>
		{
			if (_isAttackNetworked())
				photonView.RPC(nameof(_rpcPerformShootProjectile), RpcTarget.All);
			else
				_rpcPerformShootProjectile();

			_effects.GunObject.DOLocalMove(originalGunPosition, _effects.GunKnockbackEnd).SetDelay(_effects.GunKnockbackSustain).SetLink(_effects.GunObject.gameObject);
		}).SetLink(_effects.GunObject.gameObject);
	}

	protected virtual bool _isAttackNetworked() => true;

	[PunRPC]
	public void _rpcPerformShootProjectile()
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
		if (_isAttackNetworked())
			photonView.RPC(nameof(_rpcPerformSidearm), RpcTarget.All);
		else
			_rpcPerformSidearm();
	}

	[PunRPC]
	public void _rpcPerformSidearm()
	{
		if (_effects.GunObject) _effects.GunObject.gameObject.SetActive(false);
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


	protected PlayerController _lastDamagePlayer = null;

	public virtual void DoDamage(float damage, Object tag)
	{
		if (!IsDead)
		{
			var tagPlayer = tag?.GetComponentInParent<PlayerController>();
			if (tagPlayer) _lastDamagePlayer = tagPlayer;
			else if (_mustBeFinishedOffByPlayerToAwardScore()) _lastDamagePlayer = null;
		}

		if(PhotonNetwork.IsMasterClient)
			photonView.RPC(nameof(_rpcDoDamage), RpcTarget.All, HP - damage);
	}

	protected virtual bool _mustBeFinishedOffByPlayerToAwardScore() => false;

	[PunRPC]
	public void _rpcDoDamage(float newHP)
	{
		if (IsDead) return;

		HP = newHP;
		_effects.OnHPChange.Invoke($"{HP}");
		if (IsDead)
			DoDie(_effects.HurtColor);
		else
			_hurtAnimation(_effects.HurtColor, _effects.HurtBlinkBuildup, _effects.HurtBlinkSustain, _effects.HurtBlinkEnd, _sounds.HurtSound);
	}

	public void DoHeal(float hp)
	{
		if(PhotonNetwork.IsMasterClient)
			photonView.RPC(nameof(_rpcDoHeal), RpcTarget.All, Mathf.Min(HP + hp, MaxHP));
	}
	[PunRPC]
	public void _rpcDoHeal(float newHP)
	{
		if (IsDead) return;
		DoHealForceNoAnimation(newHP);
		_hurtAnimation(_effects.HealColor, _effects.HealBlinkBuildup, _effects.HealBlinkSustain, _effects.HealBlinkEnd, _sounds.HealSound);
	}

	protected void DoHealForceNoAnimation(float newHP)
	{
		HP = newHP;
		_effects.OnHPChange.Invoke($"{HP}");

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

		public Dictionary<Component, Color> OgColors = new();
		public Vector3? OgLocalScale = null;
	}
	[SerializeField] public EffectDetails _effects;

	bool _isEffectInProgress = false;
	

	
	public void DoDie(Color dieColor)
	{
		Debug.Log($"Dies: {this}", this);

		if (_lastDamagePlayer && ScoreForKilling != 0)
		{
			_lastDamagePlayer.AddScore(ScoreForKilling);
		}
		
		if(_sounds.DieSound) _audioPlayer.PlayOneShot(_sounds.DieSound);

		_effects.OgLocalScale ??= transform.localScale;

		_isEffectInProgress = true;
		foreach (var spr in _effects.Sprites)
		{
			spr.DOColor(dieColor, _effects.DeathColorBuildup).SetLink(spr.gameObject);
		}
		OnDie?.Invoke();
		transform.DOScale(0f, _effects.DeathEffectDuration).OnComplete(() =>
		{
			_doDestroySelf();
		}).SetLink(transform.gameObject);
	}

	protected virtual void _doDestroySelf()
	{
		Destroy(gameObject, 0.5f);
	}


	private void _hurtAnimation(Color hurtColor, float buildup, float sustain, float end, AudioClip sound)
	{
		if(sound) _audioPlayer.PlayOneShot(sound);
		if (_isEffectInProgress) return;

		foreach (var spr in _effects.Sprites)
		{
			var ogColor = _effects.OgColors.SetIfNotPresent(spr, spr.color);
			spr.DOColor(hurtColor, buildup).OnComplete(() =>
			{
				if (!spr) return;
				spr.DOColor(ogColor, end).SetDelay(sustain).OnComplete(
					() => {
						_isEffectInProgress = false;
					}
				).SetLink(spr.gameObject);
			}).SetLink(spr.gameObject);
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		return;
		if (stream.IsWriting)
		{
			stream.SendNext(this.HP);
		}
		else
		{
			float requestedHP = (float)stream.ReceiveNext();
			if(requestedHP >= 0 && requestedHP < HP)
			{
				DoDamage(HP - requestedHP, this);
			}
		}
	}
}









public class PlayerController : CharacterController
{
	[SerializeField] Transform _spellRoot;

	[SerializeField] CinemachineCamera _camera;
	[SerializeField] SpriteRenderer[] _spritesToColor;

	public int Score => IsDead ? 0 : _currentScore;

	int _currentScore = 0;

	AbstractSpell[] _allSpells;
	int _currentSpellIdx = 0;


	InputAction moveAction;
	InputAction lookAction;
	InputAction staffAction;
	InputAction swordAction;

	public Color MainColor { get; private set; }

	protected override void Start()
	{
		base.Start();
		DontDestroyOnLoad(gameObject);

		_effects.OgLocalScale = this.transform.localScale;


		_allSpells = _spellRoot.GetComponentsInChildren<AbstractSpell>(true);
		_rand.Shuffle<AbstractSpell>(_allSpells);
		bool isFirstColor = true;
		foreach(var spr in _spritesToColor)
		{
			spr.color = Color.HSVToRGB(_rand.NextFloat(), _rand.NextFloat(0.5f, 1.0f), 1.0f);
			if(Op.post_assign(ref isFirstColor, false))
				MainColor = spr.color;
		}

		moveAction = InputSystem.actions.FindAction("Move");
		lookAction = InputSystem.actions.FindAction("Look");
		staffAction = InputSystem.actions.FindAction("Staff");
		swordAction = InputSystem.actions.FindAction("Sword");

		_setupPlayerGUI();

		TagSearchable.FindByTag<LeaderboardManager>("Leaderboard").DoUpdateLeaderboard();
	}

	public void AddScore(int scoreToAdd)
	{
		if(PhotonNetwork.IsMasterClient)
			photonView.RPC(nameof(_rpcSetScore), RpcTarget.All, new object[] { _currentScore + scoreToAdd });
	}


	[PunRPC]
	public void _rpcSetScore(int scoreToAdd)
	{
		_currentScore = scoreToAdd;

		TagSearchable.FindByTag<LeaderboardManager>("Leaderboard").DoUpdateLeaderboard();
	}

	protected override bool _mustBeFinishedOffByPlayerToAwardScore() => true;

	void _setupPlayerGUI()
	{
		if (!this.photonView.IsMine) return;

		_camera.gameObject.SetActive(true);
		OnDie.AddListener(GameObject.FindWithTag("LossFader").GetComponent<FadeEffect>().FadeIn);
		_spellNameDisplay = GameObject.FindWithTag("TopBar").GetComponent<TopBarController>();
		var hpBar = TagSearchable.FindByTag<TMProFormatter>("HpBar");
		_effects.OnHPChange.AddListener(hpBar.SetTextWithStringArgument);
	}

	protected override Rigidbody2D _getProjectile()
	{
		var ret = _allSpells[_currentSpellIdx];
		++_currentSpellIdx;
		if (_currentSpellIdx >= _allSpells.Length) _currentSpellIdx = 0;
		return ret.GetComponent<Rigidbody2D>();
	}

	protected override bool _isAttackNetworked() => true;

	Vector3? _lastMousePosition;
	Vector2? _lastLookValue;
	bool _isGamepadMode = false;
    protected override float _getLookRotation()
    {
		Vector3 currentMousePosition = Input.mousePosition;
		Vector2 currentLookValue = lookAction.ReadValue<Vector2>();
		if(currentMousePosition != _lastMousePosition)
		{
			_lastMousePosition = currentMousePosition;
			_isGamepadMode = false;
			Cursor.visible = true;
		}
		else if(currentLookValue != _lastLookValue)
		{
			_lastLookValue = currentLookValue;
			_isGamepadMode = true;
			Cursor.visible = false;
		}

		Vector2 lookDirection;
		if (!_isGamepadMode)
		{
			var worldCursorPos = Camera.main.ScreenToWorldPoint(currentMousePosition);
			lookDirection = worldCursorPos - _rotatable.position;
		}
		else
		{
			lookDirection = lookAction.ReadValue<Vector2>();
		}
		return Mathf.Rad2Deg * Mathf.Atan2(lookDirection.y, lookDirection.x);

	}
    protected override Vector2 _getTargetMovement()
    {
		return moveAction.ReadValue<Vector2>();
	}
	protected override bool _isShootCommand()
		=> staffAction.WasPressedThisFrame();


	protected override bool _isSidearmCommand()
		=> swordAction.WasPressedThisFrame();


	[SerializeField] float _respawnSeconds = 5.0f;
	protected override void _doDestroySelf()
	{
		int score = this._currentScore;
		this._currentScore = 0;
		foreach (var spr in _effects.Sprites)
		{
			spr.color = _effects.OgColors[spr];
		}
		TMProFormatter printout = this.photonView.IsMine ? GameObject.FindWithTag("LossFader").GetComponentInChildren<TMProFormatter>() : null;


		StartCoroutine(respawnCountdown());
		IEnumerator respawnCountdown()
		{
			double respawnTime = Time.timeAsDouble + _respawnSeconds;
			while (true)
			{
				double timeUntilRespawn = respawnTime - Time.timeAsDouble;
				if (printout) printout.SetTextWithVarargs(timeUntilRespawn, score);

				if (Time.timeAsDouble >= respawnTime)
					break;

				yield return null;
			}
			if(photonView.IsMine)
				GameObject.FindWithTag("LossFader").GetComponent<FadeEffect>().FadeOut();
			this.DoHealForceNoAnimation(10f);
			this.transform.position = GameObject.FindWithTag("PlayerSpawn").transform.position;
			this.transform.DOScale(_effects.OgLocalScale.Value, 1.0f).SetLink(gameObject);
			TagSearchable.FindByTag<LeaderboardManager>("Leaderboard").DoUpdateLeaderboard();
		}
	}
}
