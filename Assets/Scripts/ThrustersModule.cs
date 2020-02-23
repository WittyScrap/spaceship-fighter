using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represets the spaceship's thrusters.
/// </summary>
[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(Player))]
public class ThrustersModule : MonoBehaviour
{
	#region Class private data - private:

	/*
	 * This is class data, it is inacccessible from the outside and should
	 * only be used as storage, no operation should be performed directly
	 * on it. Use properties for general use.
	 */

	private Rigidbody		_rigidbody			 = null;
	private Player			_spaceship			 = null;
	private Vector3			_nextForce			 = Vector3.zero;
	private Vector3			_nextTorque			 = Vector3.zero;
	private ParticleSystem  _speedLinesParticles = null;
	private bool			_isLocked			 = false;

	#endregion

	#region Serialised class data - private:

	/*
	 * Note:
	 * This data is serialised through Unity, as with the
	 * private data, it is not meant to be accessed directly.
	 * Use one of the properties provided.
	 * 
	 */

	// Separator header....
	[Space, Header("Input properties"), Space]

	[SerializeField] bool _invertX = true;
	[SerializeField] bool _invertY = false;
	[SerializeField] bool _invertZ = false;
	[SerializeField] bool _autoHeadlookOnLock = true;

	[Space, Header("Spaceship settings"), Space]

	[SerializeField] float _speedLimit		= 100.0f;
	[SerializeField] float _rotationLimit	= 1.0f;
	[SerializeField] float _idleShaking		= .1f;

	[Space]

	[SerializeField] ShakeSystem _cameraShake	= null;

	[Space, Header("Particle systems"), Space]

	[SerializeField] GameObject _speedLines		= null;

	[Space]

	[SerializeField] ThrusterManager _forward	= null;
	[SerializeField] ThrusterManager _backward	= null;
	[SerializeField] ThrusterManager _left		= null;
	[SerializeField] ThrusterManager _right		= null;
	[SerializeField] ThrusterManager _up		= null;
	[SerializeField] ThrusterManager _down		= null;

	[Space, Header("User Interface"), Space]

	[SerializeField] RawImage _velocitySlider	= null;

	#endregion

	#region Internal class properties - private:

	/// <summary>
	/// The spaceship's rigidbody.
	/// </summary>
	private Rigidbody Rigidbody {
		get
		{
			if (!_rigidbody)
			{
				_rigidbody = GetComponent<Rigidbody>();
			}
			return _rigidbody;
		}
	}

	/// <summary>
	/// The linked spaceship player.
	/// </summary>
	private Player Spaceship {
		get
		{
			if (!_spaceship)
			{
				_spaceship = GetComponent<Player>();
			}
			return _spaceship;
		}
	}

	/// <summary>
	/// The force to be applied to this spaceship on the next fixed tick.
	/// </summary>
	private Vector3 NextForce {
		get => _nextForce;
	}

	/// <summary>
	/// The torque to be applied to this spaceship on the next fixed tick.
	/// </summary>
	private Vector3 NextTorque {
		get => _nextTorque;
	}

	/// <summary>
	/// Whether or not the rotation around the red X (PITCH) axis should be inverted.
	/// </summary>
	private bool InvertX {
		get => _invertX;
	}

	/// <summary>
	/// Whether or not the rotation around the green Y (YAW) axis should be inverted.
	/// </summary>
	private bool InvertY {
		get => _invertY;
	}

	/// <summary>
	/// Whether or not the rotation around the blue Z (ROLL) axis should be inverted.
	/// </summary>
	private bool InvertZ {
		get => _invertZ;
	}

	/// <summary>
	/// The speed lines' particle system.
	/// </summary>
	private ParticleSystem SpeedLines {
		get
		{
			if (!_speedLinesParticles)
			{
				_speedLinesParticles = _speedLines.GetComponentInChildren<ParticleSystem>();
			}
			return _speedLinesParticles;
		}
	}

	/// <summary>
	/// The transform for the speed lines shell object.
	/// </summary>
	private Transform SpeedLinesTransform {
		get
		{
			return _speedLines.transform;
		}
	}

	#endregion

	#region Data manipulators - private:

	/// <summary>
	/// Records the requested force to be used in the next fixed frame tick.
	/// </summary>
	/// <param name="forceRequest">The requested force.</param>
	private void PrepareForce(Vector3 forceRequest)
	{
		_nextForce += forceRequest;
	}

	/// <summary>
	/// Records the requested torque to be used in the next fixed frame tick.
	/// </summary>
	/// <param name="torqueRequest">The requested torque.</param>
	private void PrepareTorque(Vector3 torqueRequest)
	{
		_nextTorque += torqueRequest;
	}

	/// <summary>
	/// Clears the force request vector.
	/// </summary>
	private void ClearForce()
	{
		_nextForce.x = 0;
		_nextForce.y = 0;
		_nextForce.z = 0;
	}

	/// <summary>
	/// Clears the torque request vector.
	/// </summary>
	private void ClearTorque()
	{
		_nextTorque.x = 0;
		_nextTorque.y = 0;
		_nextTorque.z = 0;
	}

	/// <summary>
	/// Sets up the thrust particles depending on the force being prepared.
	/// </summary>
	private void UpdateThrusters(Vector3 nextForce)
	{
		_forward.Thrust		=  nextForce.z;
		_backward.Thrust	= -nextForce.z;
		_left.Thrust		=  nextForce.x;
		_right.Thrust		= -nextForce.x;
		_down.Thrust		=  nextForce.y;
		_up.Thrust			= -nextForce.y;
	}

	/// <summary>
	/// Updates the speed component of the speedlines and their direction.
	/// </summary>
	private void UpdateSpeedlines()
	{
		float speed = Speed;
		SpeedLinesVelocity = speed;

		if (speed > 0.01f)
		{
			SpeedLinesTransform.forward = Velocity;
		}
	}

	/// <summary>
	/// Updates any UI elements.
	/// </summary>
	private void UpdateUI()
	{
		UIManager.SetSlider(_velocitySlider, Speed / _speedLimit);
	}

	/// <summary>
	/// Updates camera shake depending on velocity factors.
	/// </summary>
	private void UpdateShaking()
	{
		float speed = NextForce.magnitude;
		ShakePower = speed + Speed * _idleShaking;
	}

	#endregion

	#region Class properties - public:

	/// <summary>
	/// Whether or not speed lines should be visible.
	/// </summary>
	public bool ShowSpeedlines {
		get
		{
			return SpeedLines.gameObject.activeSelf;
		}
		set
		{
			SpeedLines.gameObject.SetActive(value);
		}
	}

	/// <summary>
	/// The speed of the speed lines particles.
	/// </summary>
	public float SpeedLinesVelocity {
		get
		{
			return SpeedLines.main.startSpeedMultiplier;
		}
		set
		{
			var mainComponent = SpeedLines.main;
			mainComponent.startSpeedMultiplier = value;
		}
	}

	/// <summary>
	/// Overrides the particle thruster power for the FORWARD
	/// thrusters. Using a negative value will set its positive
	/// equivalent for the BACKWARD thrusters.
	/// </summary>
	public float ThrustPower {
		get
		{
			return _forward.Thrust;
		}
		set
		{
			if (value > 0)
			{
				_forward.Thrust = value;
				_backward.Thrust = 0;
			}
			else
			{
				_backward.Thrust = -value;
				_forward.Thrust = 0;
			}
		}
	}

	/// <summary>
	/// The strength of the camera shake.
	/// </summary>
	public float ShakePower {
		get
		{
			return _cameraShake.Power;
		}
		set
		{
			value = Mathf.Clamp01(value);
			_cameraShake.Power = value;
		}
	}

	/// <summary>
	/// The power and strength of the camera shake.
	/// </summary>
	public float ShakePowerAndStrength 
	{
		get
		{
			return _cameraShake.Power;
		}
		set
		{
			value = Mathf.Clamp01(value);
			ShakePower = value;

			_cameraShake.Amount = Mathf.Lerp(1.0f, .25f, value);
			_cameraShake.Strength = Mathf.Lerp(0.5f, 50f, value);
		}
	}

	/// <summary>
	/// This spaceship's velocity.
	/// </summary>
	public Vector3 Velocity {
		get
		{
			return Rigidbody.velocity;
		}
		private set
		{
			Rigidbody.velocity = value;
		}
	}

	/// <summary>
	/// The speed of this spaceship.
	/// </summary>
	public float Speed {
		get
		{
			return Velocity.magnitude;
		}
	}

	/// <summary>
	/// How much of the speed limit has been reached so far.
	/// </summary>
	public float SpeedRate {
		get
		{
			return Speed / _speedLimit;
		}
	}

	/// <summary>
	/// The maximum speed for the spaceship.
	/// </summary>
	public float SpeedLimit {
		get
		{
			return _speedLimit;
		}
	}

	/// <summary>
	/// Whether or not this module should not take any input.
	/// </summary>
	public bool Locked {
		get
		{
			return _isLocked;
		}
		set
		{
			_isLocked = value;

			if (_autoHeadlookOnLock)
			{
				Spaceship.Warpdrive.CameraFollower.AllowHeadlook = !value;
			}
		}
	}

	/// <summary>
	/// Whether or not the Rigidbody associated with the spaceship is kinematic.
	/// </summary>
	public bool PhysicsEnabled {
		get
		{
			return !Rigidbody.isKinematic;
		}
		set
		{
			Rigidbody.isKinematic = !value;
		}
	}

	#endregion

	/// <summary>
	/// Stops all torque on the spaceship.
	/// </summary>
	/// <param name="align">Whether or not the spaceship should align to its velocity.</param>
	public void StopAllRotation(bool align)
	{
		Rigidbody.angularVelocity = Vector3.zero;

		if (align)
		{
			Rigidbody.rotation = Quaternion.LookRotation(Rigidbody.velocity, transform.up);
		}
	}

	/// <summary>
	/// Sets up the Rigidbody.
	/// </summary>
	private void Awake()
	{
		Rigidbody.maxAngularVelocity = _rotationLimit;
	}

	/// <summary>
	/// Handles input registration and updating critical components.
	/// </summary>
	private void Update()
	{
		if (Locked)
		{
			return;
		}

		Vector3 nextForce = GetForceInput();
		Vector3 nextTorque = GetTorqueInput();

		PrepareForce(nextForce);
		PrepareTorque(nextTorque);

		UpdateThrusters(nextForce);
		UpdateSpeedlines();

		if (Input.GetKey(KeyCode.LeftAlt))
		{
			ClearTorque();
		}

		UpdateUI();
		UpdateShaking();
	}

	/// <summary>
	/// Handles physics and velocity management.
	/// </summary>
	private void FixedUpdate()
	{
		if (Locked)
		{
			return;
		}

		Rigidbody.AddRelativeForce(NextForce);
		Rigidbody.AddRelativeTorque(NextTorque);

		Velocity = Vector3.ClampMagnitude(Velocity, _speedLimit);

		ClearForce();
		ClearTorque();
	}

	/// <summary>
	/// Calculates the force input vector based on which axis
	/// are currently set.
	/// </summary>
	/// <returns>Vector containing summary of current input.</returns>
	private Vector3 GetForceInput()
	{
		return new Vector3
		{
			x = Input.GetAxis("Horizontal"),
			y = Input.GetAxis("Hover"),
			z = Input.GetAxis("Vertical")
		};
	}

	/// <summary>
	/// Calculates the torque input vector based on which axis
	/// are currently set.
	/// </summary>
	/// <returns>Vector containing summary of current input.</returns>
	private Vector3 GetTorqueInput()
	{
		return new Vector3
		{
			x = Input.GetAxis("Mouse Y")	* (InvertX ? -1 : 1),
			y = Input.GetAxis("Mouse X")	* (InvertY ? -1 : 1),
			z = Input.GetAxis("Roll")		* (InvertZ ? -1 : 1)
		};
	}
}
