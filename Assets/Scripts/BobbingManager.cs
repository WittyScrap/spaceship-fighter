using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies a relative offset to this object depending on movement input.
/// </summary>
[RequireComponent(typeof(ShakeSystem))]
public class BobbingManager : MonoBehaviour
{
	/// <summary>
	/// How powerful the bobbing should be.
	/// </summary>
	[SerializeField, Space, Header("Properties")]
	private Vector3 _power = Vector3.one;

	/// <summary>
	/// The axis to be used to pan forward (Z).
	/// </summary>
	[SerializeField, Space, Header("Axis"), Space]
	private string _forwardAxis = "Vertical";

	/// <summary>
	/// The axis to be used to pan laterally (X).
	/// </summary>
	[SerializeField]
	private string _lateralAxis = "Horizontal";

	/// <summary>
	/// The axis to be used to pan vertically (Y).
	/// </summary>
	[SerializeField]
	private string _verticalAxis = "Hover";

	/// <summary>
	/// The reference shake system.
	/// </summary>
	private ShakeSystem _shakeSystem = null;

	/// <summary>
	/// The offset to apply next.
	/// </summary>
	private Vector3 _nextOffset = Vector3.zero;

	/// <summary>
	/// Cache shake system.
	/// </summary>
	private void Awake()
	{
		_shakeSystem = GetComponent<ShakeSystem>();
	}

	/// <summary>
	/// Applies bobbing effect.
	/// </summary>
	void Update()
    {
		_nextOffset.x = Input.GetAxis(_lateralAxis) * _power.x;
		_nextOffset.y = Input.GetAxis(_verticalAxis) * _power.y;
		_nextOffset.z = Input.GetAxis(_forwardAxis) * _power.z;

		_shakeSystem.Offset = _nextOffset;
    }
}
