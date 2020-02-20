using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple thruster manager that manages
/// particle systems and effects in relation to
/// thrust being applied in a given direction.
/// </summary>
public class ThrusterManager : MonoBehaviour
{
	/// <summary>
	/// Simple name tag to more easily differenciate between
	/// managers in the inspector.
	/// </summary>
	[SerializeField]
	private string _name = "Thruster manager";

	/// <summary>
	/// The particle systems to alter.
	/// </summary>
	[SerializeField, Space, Header("Particle systems"), Space]
	private ParticleSystem[] _particleSystems = null;

	/// <summary>
	/// The limit that can be reached for the particle's lifetime.
	/// </summary>
	[SerializeField]
	private float _lifetimeLimit = .1f;

	/// <summary>
	/// The sound effect emitter for the thrusters.
	/// </summary>
	[SerializeField, Space, Header("Audio management"), Space]
	private AudioSource _thrusterSound = null;

	/// <summary>
	/// The low target for the pitch related to this thruster.
	/// </summary>
	[SerializeField, Range(0, 3)]
	private float _pitchBase = 0.0f;

	/// <summary>
	/// The target pitch to reach for this thruster.
	/// </summary>
	[SerializeField, Range(0, 3)]
	private float _pitchTarget = 3.0f;

	/// <summary>
	/// The amount of thrust to apply.
	/// </summary>
	[SerializeField, Space, Space]
	private float _thrust = 0;

	/// <summary>
	/// Applies a certain amount of thrust to the system.
	/// </summary>
	public float Thrust {
		get
		{
			return _thrust;
		}
		set
		{
			value = Mathf.Clamp01(value);
			_thrust = value;
			ParticleSystem.MainModule main;
			foreach (ParticleSystem particleSystem in _particleSystems)
			{
				main = particleSystem.main;
				main.startLifetimeMultiplier = value * _lifetimeLimit;
				main.startSizeMultiplier = value * 0.5f;
			}
			if (_thrusterSound)
			{
				_thrusterSound.pitch = Mathf.Lerp(_pitchBase, _pitchTarget, value);
			}
		}
	}

	/// <summary>
	/// This manager's name.
	/// </summary>
	public string Name => _name;

	/// <summary>
	/// Resets the thrust value.
	/// </summary>
	private void Awake()
	{
		Thrust = _thrust;	
	}
}
