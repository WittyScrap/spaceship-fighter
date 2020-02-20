using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Audio manager for the spaceship.
/// </summary>
[RequireComponent(typeof(WarpDrive))]
public class AudioSystem : MonoBehaviour
{
	#region Class private data - private:

	/*
	 * This is class data, it is inacccessible from the outside and should
	 * only be used as storage, no operation should be performed directly
	 * on it. Use properties for general use.
	 */

	private WarpDrive _warpDrive = null;

	#endregion

	#region Serialised class data - private:

	/*
	 * Note:
	 * This data is serialised through Unity, as with the
	 * private data, it is not meant to be accessed directly.
	 * Use one of the properties provided.
	 * 
	 */

	[Space, Header("Warp Drive - Sound manager"), Space]

	[SerializeField] AudioSource _warpOutSound = null;
	[SerializeField] AudioSource _hyperspaceSound = null;
	[SerializeField] AudioSource _warpInSound = null;

	[Space]

	[SerializeField] AudioSource _countdownTick = null;

	#endregion

	/// <summary>
	/// Saves the warp drive, sets all necessary event methods up.
	/// </summary>
	private void Awake()
	{
		_warpDrive = GetComponent<WarpDrive>();
		
		// ... Set up all events... //

		_warpDrive.OnHyperspaceEntered	+= _warpDrive_OnHyperspaceEntered;
		_warpDrive.OnExitingHyperspace	+= _warpDrive_OnExitingHyperspace;
		_warpDrive.OnJumpPrepared		+= _warpDrive_OnJumpPrepared;
		_warpDrive.OnCountdownTick		+= _warpDrive_OnCountdownTick;
	}

	/// <summary>
	/// Launch countdown ticking.
	/// </summary>
	private void _warpDrive_OnCountdownTick(object sender, WarpDrive.WarpCountdownArgs e)
	{
		_countdownTick.Play();
	}

	/// <summary>
	/// A jump is ready to start.
	/// </summary>
	private void _warpDrive_OnJumpPrepared(object sender, EventArgs e)
	{
		_warpOutSound.Play();
	}

	/// <summary>
	/// Entering hyperspace event.
	/// </summary>
	private void _warpDrive_OnHyperspaceEntered(object source, EventArgs args)
	{
		_hyperspaceSound.Play();
	}

	/// <summary>
	/// Leaving hyperspace event.
	/// </summary>
	private void _warpDrive_OnExitingHyperspace(object source, EventArgs args)
	{
		_hyperspaceSound.Stop();
		_warpInSound.Play();
	}
}
