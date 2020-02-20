using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple camera shake implementation.
/// </summary>
public class ShakeSystem : MonoBehaviour
{
	/// <summary>
	/// The amount the camera can shake by, in units.
	/// </summary>
	[SerializeField, Space, Header("Properties")]
	private float _shakeAmount = .1f;

	/// <summary>
	/// How fast the camera shake should be.
	/// </summary>
	[SerializeField]
	private float _shakeStrength = 10.0f;

	/// <summary>
	/// The power to give to the shaking system.
	/// </summary>
	[SerializeField, Range(0, 1)]
	private float _power = 0;

	/// <summary>
	/// A constant offset to apply to all transformations.
	/// </summary>
	[SerializeField, Space]
	private Vector3 _offset = Vector3.zero;

	/// <summary>
	/// Whether or not the system should offset across the X axis.
	/// </summary>
	[SerializeField, Space, Header("Toggles"), Space]
	private bool _offsetOnX = true;

	/// <summary>
	/// Whether or not the system should offset across the Y axis.
	/// </summary>
	[SerializeField]
	private bool _offsetOnY = true;

	/// <summary>
	/// Whether or not the system should rotate around the Z axis.
	/// </summary>
	[SerializeField]
	private bool _rotateOnZ = true;

	// Noise seeds (offsets).
	private int _perlinXnoise = 0;
	private int _perlinYnoise = 0;
	private int _perlinZnoise = 0;

	/// <summary>
	/// Cached transform.
	/// </summary>
	private Transform _transform;

	/// <summary>
	/// The power of the shaking, from 0 to 1.
	/// </summary>
	public float Power {
		get => _power;
		set => _power = Mathf.Clamp01(value);
	}

	/// <summary>
	/// The shake amount.
	/// </summary>
	public float Amount {
		get => _shakeAmount;
		set => _shakeAmount = value;
	}

	/// <summary>
	/// THe strenght of the shake.
	/// </summary>
	public float Strength {
		get => _shakeStrength;
		set => _shakeStrength = value;
	}

	/// <summary>
	/// A constant offset to apply to all transformations.
	/// </summary>
	public Vector3 Offset {
		get => _offset;
		set => _offset = value;
	}

	/// <summary>
	/// Generate the seeds for the X and Y perlin noises.
	/// </summary>
	private void Awake()
	{
		Random.InitState((int)System.DateTime.Now.Ticks);

		_perlinXnoise = Random.Range(0, 100);
		_perlinYnoise = Random.Range(0, 100);
		_perlinZnoise = Random.Range(0, 100);

		_transform = transform;
	}

	/// <summary>
	/// Manage noise.
	/// </summary>
	void Update()
    {
		float shakeBy = Power * _shakeAmount;
		Vector3 shakeNoise = new Vector3
		{
			x = _offsetOnX ? (Mathf.PerlinNoise(Time.time * _shakeStrength + _perlinXnoise, 0) * 2 - 1) * shakeBy : 0,
			y = _offsetOnY ? (Mathf.PerlinNoise(0, Time.time * _shakeStrength + _perlinYnoise) * 2 - 1) * shakeBy : 0,
			z = 0
		};

		if (_rotateOnZ)
		{
			shakeNoise.z = (Mathf.PerlinNoise(Time.time * _shakeStrength + shakeNoise.x, Time.time * _shakeStrength + shakeNoise.y) * 2 - 1) * shakeBy;
		}

		Vector3 eulerAngles = _transform.localEulerAngles;
		eulerAngles.z = shakeNoise.z;

		_transform.localPosition = _offset + Vector3.Lerp(_transform.localPosition, (Vector2)shakeNoise, 0.5f);
		_transform.localEulerAngles = eulerAngles;
    }
}
