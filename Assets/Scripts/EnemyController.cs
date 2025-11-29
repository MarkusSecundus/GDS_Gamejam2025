using MarkusSecundus.Utils.Behaviors.GameObjects;
using MarkusSecundus.Utils.Primitives;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Cinemachine.IInputAxisOwner.AxisDescriptor;

public class EnemyController : CharacterController
{
	[SerializeField] float _sufficientDistanceToPlayer = 10f;

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
		var dir = _getDirectionToPlayer().normalized;
		if (! _isDirectionObstructed(dir, 10f))
		{
			if (_player.transform.position.Distance(transform.position) <= _sufficientDistanceToPlayer)
			{
				return Vector2.zero;
			}
			lastDir = dir;
			return dir;
		}
		return _chooseLeastTerribleMovementDirection(dir);
	}


	Vector2 lastDir = default;
	float lastDist = 20f;
	Vector2 _chooseLeastTerribleMovementDirection(Vector2 dirToPlayer)
	{
		if(!_isDirectionObstructed(lastDir, lastDist))
		{
			return lastDir;
		}
		var opposite = -dirToPlayer;
		var left = new Vector2(dirToPlayer.y, -dirToPlayer.x);
		var right = -left;
		Span<Vector2> dirs = stackalloc Vector2[7] { left + dirToPlayer, right + dirToPlayer, left, right, left + opposite, right + opposite, opposite };
		Span<float> permittedDistances = stackalloc float[3] { 20f, 8f, 3f };
		foreach(var dist in permittedDistances)
		{
			foreach (var dir in dirs)
			{
				if(!_isDirectionObstructed(dir, dist))
				{
					lastDir = dir;
					lastDist = dist;
					return dir;
				}
			}
		}
		lastDir = Vector2.zero;
		return Vector2.zero;
	}


	protected override bool _isShootCommand()
	{
		return !_isDirectionObstructed(_getDirectionToPlayer());
	}


	List<RaycastHit2D> _raycastTemp = new();
	bool _isDirectionObstructed(Vector2 dir, float distance = float.PositiveInfinity)
	{
		_raycastTemp.Clear();
		Physics2D.Raycast(transform.position, dir, CreateLegacyFilter(-5, float.NegativeInfinity, float.PositiveInfinity), _raycastTemp, distance);
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
