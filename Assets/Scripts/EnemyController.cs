using MarkusSecundus.Utils.Behaviors.GameObjects;
using MarkusSecundus.Utils.Primitives;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Cinemachine.IInputAxisOwner.AxisDescriptor;

public class EnemyController : CharacterController
{
	PlayerController _player;
	protected override void Start()
	{
		base.Start();

		_player = TagSearchable.FindByTag<PlayerController>("Player");
	}

	Vector2 _getDirectionToPlayer() => (_player.transform.position - transform.position).xy();

	protected override float _getLookRotation()
	{
		var direction = _getDirectionToPlayer().normalized;
		return Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);
	}

	protected override Vector2 _getTargetMovement()
	{
		return _getDirectionToPlayer().normalized;
	}



	protected override bool _isShootCommand()
	{
		return !_isDirectionObstructed(_getDirectionToPlayer());
	}


	List<RaycastHit2D> _raycastTemp = new();
	bool _isDirectionObstructed(Vector2 dir)
	{
		_raycastTemp.Clear();
		Physics2D.Raycast(transform.position, dir, CreateLegacyFilter(-5, float.NegativeInfinity, float.PositiveInfinity), _raycastTemp);
		foreach (var hit in _raycastTemp)
		{
			if (hit.collider.attachedRigidbody.gameObject != _player.gameObject)
				return false;
		}
		return true;

	}


	internal static ContactFilter2D CreateLegacyFilter(int layerMask, float minDepth, float maxDepth)
	{
		ContactFilter2D result = default(ContactFilter2D);
		result.useTriggers = Physics2D.queriesHitTriggers;
		result.SetLayerMask(layerMask);
		result.SetDepth(minDepth, maxDepth);
		return result;
	}
}
