using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityAsync;

/// <summary>
/// Player spaceship.
/// </summary>
public class Player : MonoBehaviour
{
	#region Spaceship modules - private:

	/*
	 * These fields are used exclusively for storage and WILL be null
	 * if accessed directly and not through their relative properties.
	 * 
	 */

	private ThrustersModule _spaceshipThrusters = null;
	private WarpDrive		_spaceshipWarpdrive = null;

	#endregion

	#region Spaceship module accessors - public:

	/// <summary>
	/// The thrusters linked to this spaceship.
	/// </summary>
	public ThrustersModule Thrusters {
		get
		{
			if (!Application.isPlaying)
			{
				return null;
			}

			if (!_spaceshipThrusters)
			{
				_spaceshipThrusters = GetComponent<ThrustersModule>();
			}

			return _spaceshipThrusters;
		}
	}

	/// <summary>
	/// The warp drive linked to this spaceship.
	/// </summary>
	public WarpDrive Warpdrive {
		get
		{
			if (!Application.isPlaying)
			{
				return null;
			}

			if (!_spaceshipWarpdrive)
			{
				_spaceshipWarpdrive = GetComponent<WarpDrive>();
			}

			return _spaceshipWarpdrive;
		}
	}

	#endregion

	/// <summary>
	/// Caches the rigidbody and checks for the
	/// existence of the movement field.
	/// </summary>
	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
}
