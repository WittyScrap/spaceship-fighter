using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Follows a given transform target.
/// </summary>
public class Follower : MonoBehaviour
{
	/// <summary>
	/// The target transform to follow.
	/// </summary>
	[SerializeField]
	private Transform _target = null;

	/// <summary>
	/// The source transform.
	/// </summary>
	private Transform _transform = null;

	/// <summary>
	/// Specifies how quickly this transform should follow the target, with 0
	/// meaning no movement and 1 meaning a perfectly quick, instant following.
	/// </summary>
	[SerializeField, Space, Header("Properties"), Space, Range(0, 1)]
	private float _ease = 0.5f;

	/// <summary>
	/// Specifies how quickly this transform should follow the target's rotation,
	/// with 0 meaning the rotation will never change and 1 meaning a perfectly
	/// quick, instant snapping to the target's rotation.
	/// </summary>
	[SerializeField, Range(0, 1)]
	private float _rotationEase = 0.5f;

	/// <summary>
	/// The offset, relative to the target, to keep from the target at all times.
	/// </summary>
	[SerializeField]
	private Vector3 _offset = Vector3.zero;

	/// <summary>
	/// Whether or not the follower should look at the target, or just mimic its
	/// rotation.
	/// </summary>
	[SerializeField]
	private bool _lookAt = false;

	/// <summary>
	/// The key to be used for freelook.
	/// </summary>
	[SerializeField, Space]
	private KeyCode _freeLook = KeyCode.LeftAlt;

	/// <summary>
	/// The speed for the freelook rotation mode.
	/// </summary>
	[SerializeField, Range(0, 10)]
	private float _freelookSpeed = 1.0f;

	/// <summary>
	/// How quickly the freelook mode should snap back to behind the ship.
	/// </summary>
	[SerializeField, Range(0, 10)]
	private float _snapBackSpeed = 2.0f;

	/// <summary>
	/// The total angle offset made by the freelook mode.
	/// </summary>
	private float _totalAngle = 0.0f;

	/// <summary>
	/// The starting offset value.
	/// </summary>
	private Vector3 _initialOffset = Vector3.zero;

	/// <summary>
	/// Whether or not to keep headlook on even without the headlook key being pressed down.
	/// </summary>
	public bool KeepHeadlook {
		get;
		set;
	}

	/// <summary>
	/// Whether or not headlook can be allowed.
	/// </summary>
	public bool AllowHeadlook {
		get;
		set;
	}

	/// <summary>
	/// Snaps the camera to the correct location.
	/// </summary>
	public void Snap()
	{
		_transform.position = _target.TransformDirection(_offset);
		_transform.rotation = Quaternion.LookRotation(GetLookDirection(), _target.up);
	}

	/// <summary>
	/// Caches the source transform.
	/// </summary>
	private void Start()
	{
		_transform = transform;
		_initialOffset = _offset;
	}

	/// <summary>
	/// Handles freelook mode.
	/// </summary>
	private void HandleFreelook()
	{
		if (AllowHeadlook && (Input.GetKey(_freeLook) || KeepHeadlook))
		{
			float angleAmount = Input.GetAxis("Mouse X") * _freelookSpeed;
			_totalAngle += angleAmount;

			_offset = Quaternion.Euler(0, angleAmount, 0) * _offset;
		}
		else if (_totalAngle != 0)
		{
			NormaliseAngle(ref _totalAngle);

			if (_totalAngle < 0)
			{
				float correction = _snapBackSpeed;
				_totalAngle += correction;

				if (_totalAngle > 0)
				{
					correction -= _totalAngle;
					_totalAngle = 0;
				}

				_offset = Quaternion.Euler(0, correction, 0) * _offset;
			}
			else if (_totalAngle > 0)
			{
				float correction = -_snapBackSpeed;
				_totalAngle += correction;

				if (_totalAngle < 0)
				{
					correction += _totalAngle;
					_totalAngle = 0;
				}

				_offset = Quaternion.Euler(0, correction, 0) * _offset;
			}
		}
		else
		{
			_offset = _initialOffset;
		}
	}

	/// <summary>
	/// Follows the target to keep its original distance from it.
	/// </summary>
	private void FixedUpdate()
    {
		Vector3 targetPosition  = _target.TransformPoint(_offset);
		Vector3 targetDirection = (targetPosition - _transform.position) * _ease;

		_transform.position += targetDirection;

		Vector3 targetForward = GetLookDirection() * _rotationEase;
		Vector3 targetFwdVector = (_transform.forward + targetForward).normalized;

		Vector3 targetUp = _target.up * _rotationEase;
		Vector3 targetUpVector = (_transform.up + targetUp).normalized;

		_transform.rotation = Quaternion.LookRotation(targetFwdVector, targetUpVector);

		HandleFreelook();
    }

	/// <summary>
	/// Normalises an angle within the -180/+180 range.
	/// </summary>
	/// <param name="angle">The angle to normalise.</param>
	private void NormaliseAngle(ref float angle)
	{
		// reduce the angle  
		angle = angle % 360;

		// force it to be the positive remainder, so that 0 <= angle < 360  
		angle = (angle + 360) % (360);

		// force into the minimum absolute value residue class, so that -180 < angle <= 180  
		if (angle > 180)
			angle -= 360;
	}

	/// <summary>
	/// Retrieves the view direction
	/// </summary>
	private Vector3 GetLookDirection()
	{
		if (_lookAt)
		{
			return (_target.position - _transform.position).normalized;
		}
		else
		{
			Vector3 localToTarget = -_offset;
			localToTarget.y = 0;
			localToTarget.Normalize();

			return _target.TransformDirection(localToTarget);
		}
	}
}
