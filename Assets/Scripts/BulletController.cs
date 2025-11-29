using DG.Tweening;
using MarkusSecundus.Utils.Physics;
using MarkusSecundus.Utils.Primitives;
using UnityEditor.U2D.Animation;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    [field: SerializeField] float Damage { get; set; } = 1f;

    [SerializeField] CharacterController _toIgnore;

    Vector3 _startPosition;
    [SerializeField] float _maxDistanceTraveled = 10f;
    [SerializeField] Rigidbody2D _rb;
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
	}
    [SerializeField] EffectsConfig _effects;

    private void OnCollisionEnter2D(Collision2D collision) => _onHit2D(collision.collider);
	private void OnTriggerEnter2D(Collider2D collision)=> _onHit2D(collision);

    void _onHit2D(Collider2D collider)
	{
		if (_isDead) return;

		var character = collider.GetComponent<CharacterController>();
		if (character && (character != _toIgnore))
		{
            //Debug.Log($"Did hit: {character}", this);
			character.DoDamage(Damage);
			_doDie();
		}
	}

	bool _isDead = false;
    void _doDie()
    {
        _isDead = true;
        if(_effects.BulletSprite)
            _effects.BulletSprite.DOColor(new Color(0, 0, 0, 0), _effects.DeathFadeDuration).OnComplete(()=>Destroy(gameObject));
    }
}
