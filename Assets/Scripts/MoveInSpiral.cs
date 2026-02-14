using UnityEngine;

public class MoveInSpiral : MonoBehaviour
{
	[SerializeField] float AnglePerSecond_degree;
	[SerializeField] float DistancePerSecond;


	Vector3 _ogPosition;

	double _startTime;
	private void Start()
	{
		_ogPosition = transform.position;
		_startTime = Time.fixedTimeAsDouble;
	}

	private void FixedUpdate()
	{
		float t = (float)(Time.fixedTimeAsDouble - _startTime);
		float angle = AnglePerSecond_degree * t;
		Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
		float distance = DistancePerSecond * t;
		this.transform.position = _ogPosition + (direction * distance);
	}
}
