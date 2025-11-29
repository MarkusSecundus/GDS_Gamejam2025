using MarkusSecundus.Utils.Behaviors.GameObjects;
using MarkusSecundus.Utils.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Cinemachine.IInputAxisOwner.AxisDescriptor;

public class EnemyController : CharacterController
{
	[SerializeField] float _sufficientDistanceToPlayer = 10f;
	[SerializeField] Interval<float> _distanceToShoot = new Interval<float>(0f, 10f);

	PlayerController _player;

	AreaDamager _areaDamager;
	protected override void Start()
	{
		base.Start();

		_player = TagSearchable.FindByTag<PlayerController>("Player");
		_areaDamager = GetComponentInChildren<AreaDamager>();
	}

	Vector2 _getDirectionToPlayer() => (_player.transform.position - transform.position).xy();

	protected override float _getLookRotation()
	{
		var direction = lastDir == Vector2.zero ? _getDirectionToPlayer().normalized : lastDir;
		return Mathf.Rad2Deg * Mathf.Atan2(direction.y, direction.x);
	}

	protected override Vector2 _getTargetMovement()
	{
		var dir = _getDirectionToPlayer().normalized;
		if (_getNearestObstruction(dir, 10f) >= 2f)
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
		if(_getNearestObstruction(lastDir, lastDist) >= 3f)
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
				if(_getNearestObstruction(dir, dist) >= (dist*0.5f))
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


	bool _isInAttackRange()
	{
		var playerDir = _getDirectionToPlayer();
		return _getNearestObstruction(playerDir) > 9999f && _distanceToShoot.Contains(playerDir.magnitude);
	}

	protected override bool _isShootCommand() => _favouriteWeapon == WeaponType.Ranged && _isInAttackRange();

	protected override bool _isSidearmCommand()
	{
		if (! (_favouriteWeapon == WeaponType.Mellee && _isInAttackRange())) return false;
		return _areaDamager.TriggerInfo.GetActiveTriggers2D().Any(c => c.attachedRigidbody.gameObject == _player.gameObject);
	}


	List<RaycastHit2D> _raycastTemp = new();
	float _getNearestObstruction(Vector2 dir, float distance = float.PositiveInfinity)
	{
		_raycastTemp.Clear();
		Physics2D.Raycast(transform.position, dir, CreateLegacyFilter(-5, float.NegativeInfinity, float.PositiveInfinity), _raycastTemp, distance);
		var ret = float.PositiveInfinity;
		foreach (var hit in _raycastTemp)
		{
			if (hit.collider.attachedRigidbody && (
				hit.collider.attachedRigidbody.gameObject == this.gameObject 
				|| hit.collider.attachedRigidbody.gameObject == _player.gameObject
				|| hit.collider.GetComponent<BulletController>()
			))
				continue;
			ret = Mathf.Min(ret, hit.distance);
		}
		if (ret >= _getDirectionToPlayer().magnitude) ret = float.PositiveInfinity;
		return ret;

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
