using MarkusSecundus.Utils.Physics;
using MarkusSecundus.Utils.Primitives;
using UnityEngine;


public abstract class CharacterController : MonoBehaviour
{

	[field: SerializeField] protected Transform _rotatable { get; private set; }
	[field: SerializeField] protected float _movementSpeed { get; private set; } = 1f;
	[field: SerializeField] protected float _maxVelocityChange { get; private set; } = 1f;

	Rigidbody2D _rigidbody;
	void Start()
	{
		_rigidbody = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		_rotatable.rotation = _getLookRotation().AsRotation2D();

		var targetVelocity = _getTargetMovement().xy0() * _movementSpeed;
		_rigidbody.SteerToVelocity(targetVelocity, _maxVelocityChange);
	}

	protected abstract float _getLookRotation();
	protected abstract Vector2 _getTargetMovement();
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
}
