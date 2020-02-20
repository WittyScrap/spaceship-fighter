using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityAsync;
using System.Threading.Tasks;
using System;

/// <summary>
/// The warp drive module for the spaceship.
/// </summary>
[RequireComponent(typeof(Player))]
public class WarpDrive : MonoBehaviour
{
	#region Class private data - private:

	/*
	 * This is class data, it is inacccessible from the outside and should
	 * only be used as storage, no operation should be performed directly
	 * on it. Use properties for general use.
	 */

	private float		_warpBuildup		= 0.0f;
	private bool		_isWarping			= false;
	private Material	_warpTunnelMaterial = null;
	private Player		_spaceship			= null;
	private bool		_coolingDown		= false;
	private FlareLayer  _mainFlareLayer		= null;
	private bool		_mustDrop			= false;

	#endregion

	#region Serialised class data - private:

	/*
	 * Note:
	 * This data is serialised through Unity, as with the
	 * private data, it is not meant to be accessed directly.
	 * Use one of the properties provided.
	 * 
	 */

	[Space, Header("Input properties"), Space]

	[SerializeField] KeyCode _warpButton = KeyCode.J;
	[SerializeField] float	 _warpCharge = 5.0f;
	[SerializeField] float	 _warpDecay  = .1f;

	[Space, Header("Spaceship settings"), Space]

	[SerializeField] float _warpCountdown	= 4.0f;
	[SerializeField] float _warpLaunchTime	= 1.0f;
	[SerializeField] float _warpDropTime	= 3.0f;
	[SerializeField] float _warpDuration	= 10.0f;

	[Space]

	[SerializeField] [Range(0, 1)] float _jumpSplit = 0.5f;
	[SerializeField] [Range(0, 1)] float _dropSplit = 0.5f;

	[Space]

	[SerializeField] bool _condenseJump = true;
	[SerializeField] bool _condenseDrop = true;
	[SerializeField] bool _infiniteWarp = false;

	[Space]

	[SerializeField] bool _mustFullspeed	= true;
	[SerializeField] bool _mustAlign		= true;
	[SerializeField] bool _forceCooldown	= true;

	[Space, Header("Drive properties"), Space]

	[SerializeField] float _dynamicWarpDistance		= 10000;
	[SerializeField] float _backdropWarpDistance	= 10000;
	[SerializeField] float _warpFlareIntensity		= .75f;

	[Space, Header("Object references"), Space]

	[SerializeField] GameObject			_warpTunnel		 = null;
	[SerializeField] GameObject			_starField		 = null;
	[SerializeField] LensFlare			_warpTunnelFlare = null;

	[Space]

	[SerializeField] Follower			_cameraFollower	 = null;
	[SerializeField] SkyboxManager		_skyboxManager	 = null;

	[Space]

	[SerializeField] SystemGenerator	_systemGenerator = null;
	[SerializeField] Transform			_dynamicSystem   = null;

	[Space, Header("User interface"), Space]

	[SerializeField] RawImage			_buildupSlider   = null;

	[Space, Header("Audio systems"), Space]

	[SerializeField] ThrusterManager	_warpDriveAudioManager = null;

	#endregion

	#region Internal class properties - private:

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
	/// Whether or not the spaceship should be at full speed before
	/// the warp drive can begin being engaged.
	/// </summary>
	private bool FullspeedRequired {
		get
		{
			return _mustFullspeed;
		}
	}

	/// <summary>
	/// Whether or not the spaceship should be aligned with its before 
	/// velocity before the warp drive can begin being engaged.
	/// </summary>
	private bool AlignmentRequired {
		get
		{
			return _mustAlign;
		}
	}

	/// <summary>
	/// The button used to buildup the warp drive.
	/// </summary>
	private KeyCode JumpButton {
		get
		{
			return _warpButton;
		}
	}

	/// <summary>
	/// How charged the warp drive is.
	/// </summary>
	private float WarpBuildup {
		get
		{
			return _warpBuildup;
		}
		set
		{
			_warpBuildup = Mathf.Clamp(value, 0, 2);
		}
	}

	/// <summary>
	/// Warp charge clamped between 0 and 1.
	/// </summary>
	private float ClampedBuildup {
		get
		{
			return Mathf.Clamp01(_warpBuildup);
		}
	}

	/// <summary>
	/// The material used by the warp tunnel.
	/// </summary>
	private Material WarpTunnelMaterial {
		get
		{
			if (!_warpTunnelMaterial && Application.isPlaying)
			{
				MeshRenderer tunnelRenderer = _warpTunnel.GetComponent<MeshRenderer>();
				Material instancedMaterial = tunnelRenderer.material;

				_warpTunnelMaterial = instancedMaterial;
			}
			return _warpTunnelMaterial;
		}
	}

	/// <summary>
	/// How cut-off the warp tunnel should be (0 - visible, 1 - invisible).
	/// </summary>
	private float WarpTunnelFade {
		set
		{
			WarpTunnelMaterial.SetFloat("_Cutoff", 1 - value);
			_warpTunnelFlare.brightness = Utilities.InverseCircleInterpolate(1 - value) * _warpFlareIntensity;
		}
	}
	
	/// <summary>
	/// Whether or not the star field particles should be visible.
	/// </summary>
	private bool ShowStarfield {
		set
		{
			_starField.SetActive(value);
		}
	}

	/// <summary>
	/// Sets the skybox to be either enabled or disabled, and does
	/// so with the camera as well.
	/// </summary>
	private bool SkyboxEnabled {
		set
		{
			_skyboxManager.UpdatePosition = value;
		}
	}

	/// <summary>
	/// Whether or not the 2D and 3D skyboxes should be visible.
	/// </summary>
	private bool SkyboxVisible {
		set
		{
			_skyboxManager.CameraEnabled = value;
		}
	}

	/// <summary>
	/// The duration of the hyperspace jump, in milliseconds.
	/// </summary>
	private int HyperspaceTime {
		get
		{
			return (int)(_warpDuration * 1000.0f);
		}
	}

	/// <summary>
	/// Whether or not the sun object is enabled.
	/// </summary>
	private bool SunEnabled {
		set
		{
			RenderSettings.sun.enabled = value;
		}
	}

	/// <summary>
	/// Sets whether or not the main camera can see flares.
	/// </summary>
	private bool RenderCameraFlareLayerEnabled {
		get
		{
			if (!_mainFlareLayer)
			{
				_mainFlareLayer = Camera.main.GetComponent<FlareLayer>();
			}
			return _mainFlareLayer.enabled;
		}
		set
		{
			if (!_mainFlareLayer)
			{
				_mainFlareLayer = Camera.main.GetComponent<FlareLayer>();
			}
			_mainFlareLayer.enabled = value;
		}
	}

	/// <summary>
	/// Whether or not the ship is currently in cooldown.
	/// </summary>
	private bool CoolingDown {
		get
		{
			return _coolingDown;
		}
		set
		{
			_coolingDown = value;
			UISystem.SetLock(value, _buildupSlider);
		}
	}

	/// <summary>
	/// How much the jump time should be split.
	/// </summary>
	private float JumpSplit {
		get
		{
			if (!_condenseJump)
			{
				return _jumpSplit;
			}
			else
			{
				return 1;
			}
		}
	}

	/// <summary>
	/// The inverse of the jump split.
	/// </summary>
	private float InverseJumpSplit {
		get
		{
			if (!_condenseJump)
			{
				return 1 - _jumpSplit;
			}
			else
			{
				return 1;
			}
		}
	}

	/// <summary>
	/// How much the drop time should be split.
	/// </summary>
	private float DropSplit {
		get
		{
			if (!_condenseDrop)
			{
				return _dropSplit;
			}
			else
			{
				return 1;
			}
		}
	}

	/// <summary>
	/// The inverse of the drop split.
	/// </summary>
	private float InverseDropSplit {
		get
		{
			if (!_condenseJump)
			{
				return 1 - _dropSplit;
			}
			else
			{
				return 1;
			}
		}
	}


	#endregion

	#region Data manipulators - private:

	/// <summary>
	/// Checks that all warp conditions are met.
	/// </summary>
	/// <returns>True if the ship can begin warping, false otherwise.</returns>
	private bool CheckWarpConditions()
	{
		Vector3 velocity = Spaceship.Thrusters.Velocity;
		float speedRate  = Spaceship.Thrusters.SpeedRate;

		// Here we are using 0.99 instead of 1.0 to work 
		// around floating point inaccuracies.
		if (FullspeedRequired && speedRate < 0.99f)
		{
			return false;
		}

		if (!AlignmentRequired)
		{
			return true;
		}

		Vector3 forward = transform.forward;
		velocity.Normalize();

		// Checks how similar the two vectors are.
		float forwardDotVelocity = Vector3.Dot(forward, velocity);

		// Again using 0.99 rather than 1.0 to work
		// around floating point inaccuracies.
		return forwardDotVelocity > 0.99f;
	}

	/// <summary>
	/// Builds up the warp drive.
	/// </summary>
	private void BuildupWarp()
	{
		WarpBuildup += Time.deltaTime / _warpCharge;
		_warpDriveAudioManager.Thrust = ClampedBuildup;
	}

	/// <summary>
	/// Decays the warp drive's buildup.
	/// </summary>
	private void DecayWarp()
	{
		WarpBuildup -= Time.deltaTime * _warpDecay;
		_warpDriveAudioManager.Thrust = ClampedBuildup;
	}

	/// <summary>
	/// Updates any UI elements.
	/// </summary>
	private void UpdateUI()
	{
		UIManager.SetSlider(_buildupSlider, ClampedBuildup);
	}

	/// <summary>
	/// Resets the position of the player to the origin.
	/// </summary>
	private void RecenterPlayer()
	{
		transform.position = Vector3.zero;
		_cameraFollower.Snap();
	}

	/// <summary>
	/// Sets up all elements that should change between in-jump and
	/// in-space.
	/// </summary>
	/// <param name="state">True for in-jump, false for in-space.</param>
	private void SetJumpState(bool state)
	{
		Spaceship.Thrusters.Locked = state;

		// We need to hide the starfield as the camera
		// shake would reveal the ship is not moving.
		ShowStarfield = !state;

		Warping = state;
	}

	/// <summary>
	/// Sets whether or not the dynamic system contents should be visible.
	/// </summary>
	/// <param name="visible">The visibility state.</param>
	private void SetDynamicSystemVisible(bool visible)
	{
		_dynamicSystem.gameObject.SetActive(visible);
	}


	/// <summary>
	/// Sets up the spaceship to be ready for launch.
	/// </summary>
	private void PrepareJump()
	{
		OnPreparingJump?.Invoke(this, null);

		UISystem.Hide();
		SetJumpState(true);

		// Set thrusters to zero
		Spaceship.Thrusters.ThrustPower = 0.0f;
		Spaceship.Thrusters.SpeedLinesVelocity = Spaceship.Thrusters.SpeedLimit * 2;
		Spaceship.Thrusters.StopAllRotation(true);

		OnJumpPrepared?.Invoke(this, null);
	}

	/// <summary>
	/// Moves the system's contents quickly out of the way.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task WarpOut()
	{
		Spaceship.Thrusters.ThrustPower = 1.0f;
		SkyboxEnabled = false;

		Vector3 dynamicWarpPoint = -transform.forward * _dynamicWarpDistance;
		Vector3 backdropWarpPoint = transform.forward * _backdropWarpDistance;

		float allocatedTime = _warpLaunchTime * JumpSplit;

		for (float alpha = 0.0f; alpha <= 1.0f; alpha += Time.deltaTime / allocatedTime)
		{
			_dynamicSystem.position = Vector3.Lerp
			(
				Vector3.zero,
				dynamicWarpPoint,
				Utilities.InverseCircleInterpolate(alpha)
			);

			_skyboxManager.Position = Vector3.Lerp
			(
				Vector3.zero,
				backdropWarpPoint,
				Utilities.InverseCircleInterpolate(alpha, 8)
			);

			// All done, hold until the next frame.
			await Await.NextUpdate();
		}

		SetDynamicSystemVisible(false);
	}

	/// <summary>
	/// Enters the warp tunnel.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task EnterTunnel()
	{
		RenderCameraFlareLayerEnabled = true;
		Spaceship.Thrusters.PhysicsEnabled = false;

		float allocatedTime = _warpLaunchTime * InverseJumpSplit;

		for (float alpha = 0.0f; alpha <= 1.0f; alpha += Time.deltaTime / allocatedTime)
		{
			WarpTunnelFade = 1 - alpha;

			Spaceship.Thrusters.ShakePowerAndStrength = Mathf.Lerp(allocatedTime, 1, alpha);
			Spaceship.Thrusters.ShakePowerAndStrength = Mathf.Lerp(allocatedTime, 1, alpha);

			// All done, hold until the next frame.
			await Await.NextUpdate();
		}

		// Floating point inaccuracies combined with time deltas not being exact fractions of seconds
		// mean this value might never fully reach its destination, so it will be forced to do so here.
		WarpTunnelFade = 0;
		SunEnabled = false;

		Spaceship.Thrusters.ShowSpeedlines = false;
	}

	/// <summary>
	/// Warp that will end after a given countdown OR after receiving
	/// a MUST DROP instruction.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task TimedWarp()
	{
		for (float t = 0; t < _warpDuration && !_mustDrop; t += Time.deltaTime)
		{
			OnHyperspaceUpdate?.Invoke(this, null);

			await Await.NextUpdate();
		}

		_mustDrop = false;
	}

	/// <summary>
	/// Warp that will only end after receiving a MUST DROP instruction.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task InfiniteWarp()
	{
		while (!_mustDrop)
		{
			OnHyperspaceUpdate?.Invoke(this, null);

			await Await.NextUpdate();
		}

		_mustDrop = false;
	}

	/// <summary>
	/// The hyperspace update routine.
	/// </summary>
	/// <returns></returns>
	private async Task HyperspaceRoutine()
	{
		if (_infiniteWarp)
		{
			await InfiniteWarp();
		}
		else
		{
			await TimedWarp();
		}
	}

	/// <summary>
	/// Hyperspace worker.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task Hyperspace()
	{
		RecenterPlayer();

		Task systemLoading = _systemGenerator.LoadNext();
		Task hyperspaceRoutine = HyperspaceRoutine();

		await Task.WhenAll(systemLoading, hyperspaceRoutine);
	}

	/// <summary>
	/// Exits the warp tunnel.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task ExitTunnel()
	{
		SunEnabled = true;
		Spaceship.Thrusters.ShowSpeedlines = true;

		float allocatedTime = _warpDropTime * InverseDropSplit;

		for (float alpha = 0.0f; alpha <= 1.0f; alpha += Time.deltaTime / allocatedTime)
		{
			WarpTunnelFade = alpha;

			Spaceship.Thrusters.ShakePowerAndStrength = Mathf.Lerp(1, allocatedTime, alpha);
			Spaceship.Thrusters.ShakePowerAndStrength = Mathf.Lerp(1, allocatedTime, alpha);

			// All done, hold until the next frame.
			await Await.NextUpdate();
		}

		// Floating point inaccuracies combined with time deltas not being exact fractions of seconds
		// mean this value might never fully reach its destination, so it will be forced to do so here.
		WarpTunnelFade = 1;
		RenderCameraFlareLayerEnabled = false;
	}

	/// <summary>
	/// Moves the system's contents quickly back in view.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task WarpIn()
	{
		SetDynamicSystemVisible(true);

		Vector3 dynamicStartPoint = transform.forward * _dynamicWarpDistance;
		Vector3 backdropStartPoint = -_skyboxManager.Position;

		float allocatedTime = _warpDropTime * DropSplit;
		float unavailableTime = _warpDropTime * InverseDropSplit;

		for (float alpha = 0.0f; alpha <= 1.0f; alpha += Time.deltaTime / allocatedTime)
		{
			_dynamicSystem.position = Vector3.Lerp
			(
				dynamicStartPoint,
				Vector3.zero,
				Utilities.CircleInterpolate(alpha)
			);

			_skyboxManager.Position = Vector3.Lerp
			(
				backdropStartPoint,
				Vector3.zero,
				Utilities.CircleInterpolate(alpha, 8)
			);

			if (!_condenseDrop)
			{
				Spaceship.Thrusters.ShakePower = Mathf.Lerp(unavailableTime, 0, alpha);
				Spaceship.Thrusters.ThrustPower = Mathf.Lerp(unavailableTime, 0, alpha);
			}

			// All done, hold until the next frame.
			await Await.NextUpdate();
		}

		SkyboxEnabled = true;
	}

	/// <summary>
	/// Restores all altered elements from the jump.
	/// </summary>
	private void RestoreJump()
	{
		OnRestoringJump?.Invoke(this, null);

		UISystem.Show();
		SetJumpState(false);

		Spaceship.Thrusters.PhysicsEnabled = true;
		Spaceship.Thrusters.SpeedLinesVelocity = 0;
		WarpBuildup = 1;

		if (_forceCooldown)
		{
			CoolingDown = true;
		}

		OnJumpRestored?.Invoke(this, null);
	}

	/// <summary>
	/// Will perform the necessary tasks to enter hyperspace.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task EnterHyperspace()
	{
		OnEnteringHyperspace?.Invoke(this, null);

		if (!_condenseJump)
		{
			await WarpOut();
			await EnterTunnel();
		}
		else
		{
			Task warpOut = WarpOut();
			Task enterTunnel = EnterTunnel();

			await Task.WhenAll(warpOut, enterTunnel);
		}

		OnHyperspaceEntered?.Invoke(this, null);
	}

	/// <summary>
	/// Will perform the necessary tasks to exit hyperspace.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task ExitHyperspace()
	{
		OnExitingHyperspace?.Invoke(this, null);

		if (!_condenseDrop)
		{
			await ExitTunnel();
			await WarpIn();
		}
		else
		{
			Task exitTunnel = ExitTunnel();
			Task warpIn = WarpIn();

			await Task.WhenAll(exitTunnel, warpIn);
		}

		OnHyperspaceLeft?.Invoke(this, null);
	}

	/// <summary>
	/// Performs the countdown procedure, but only updates
	/// itself every second that ticks down.
	/// </summary>
	private async void CountdownPerSecond()
	{
		int timeLeft = Mathf.CeilToInt(_warpCountdown);

		while (timeLeft-- > 0)
		{
			OnCountdownTick?.Invoke(this, new WarpCountdownArgs(timeLeft));

			await Task.Delay(1000);
		}
	}

	/// <summary>
	/// Performs the countdown procedure before jumping.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private async Task LaunchCountdown()
	{
		CountdownPerSecond();

		for (float t = 0; t < _warpCountdown; t += Time.deltaTime)
		{
			OnCountdownUpdate?.Invoke(this, new WarpCountdownArgs(_warpCountdown - t));

			await Await.NextUpdate();
		}

		OnCountdownFinished?.Invoke(this, EventArgs.Empty);
	}

	#endregion

	#region Class properties - public:

	/// <summary>
	/// Whether or not this spaceship is in warp mode.
	/// </summary>
	public bool Warping {
		get
		{
			return _isWarping;
		}
		private set
		{
			_isWarping = value;
		}
	}

	/// <summary>
	/// The follower for the main camera.
	/// </summary>
	public Follower CameraFollower {
		get
		{
			return _cameraFollower;
		}
	}

	#endregion

	#region Event dispatchers - public:

	#region EventArgs implmentation - public:

	/// <summary>
	/// Countdown argument list for warpdrive events.
	/// </summary>
	public class WarpCountdownArgs : EventArgs
	{
		/// <summary>
		/// How much time is left for the countdown.
		/// </summary>
		public float CountdownLeft { get; }

		/// <summary>
		/// Sets the property's value.
		/// </summary>
		/// <param name="countdownLeft">The amount of seconds left before warp.</param>
		public WarpCountdownArgs(float countdownLeft)
		{
			CountdownLeft = countdownLeft;
		}
	}

	#endregion

	/// <summary>
	/// Event called BEFORE the jump has been prepared.
	/// </summary>
	public event EventHandler OnPreparingJump;

	/// <summary>
	/// Event called AFTER the jump has been prepared.
	/// </summary>
	public event EventHandler OnJumpPrepared;

	/// <summary>
	/// Event called on every clock tick of the countdown phase.
	/// </summary>
	public event EventHandler<WarpCountdownArgs> OnCountdownTick;

	/// <summary>
	/// Event called on every frame tick during the countdown phase.
	/// </summary>
	public event EventHandler<WarpCountdownArgs> OnCountdownUpdate;

	/// <summary>
	/// Event called when the countdown has reached 0.
	/// </summary>
	public event EventHandler OnCountdownFinished;

	/// <summary>
	/// Event called as hyperspace is being entered.
	/// </summary>
	public event EventHandler OnEnteringHyperspace;

	/// <summary>
	/// Event called after hyperspace was entered.
	/// </summary>
	public event EventHandler OnHyperspaceEntered;

	/// <summary>
	/// Event called every frame spent in hyperspace.
	/// </summary>
	public event EventHandler OnHyperspaceUpdate;

	/// <summary>
	/// Event called as hyperspace is being left.
	/// </summary>
	public event EventHandler OnExitingHyperspace;

	/// <summary>
	/// Event called after hyperspace has been left.
	/// </summary>
	public event EventHandler OnHyperspaceLeft;

	/// <summary>
	/// Event called BEFORE the jump has been restored.
	/// </summary>
	public event EventHandler OnRestoringJump;

	/// <summary>
	/// Event called AFTER the jump has been restored.
	/// </summary>
	public event EventHandler OnJumpRestored;


	#endregion

	/// <summary>
	/// Performs the hyperspace jump.
	/// </summary>
	public async void Jump()
	{
		if (Warping)
		{
			return;
		}

		PrepareJump();

		await LaunchCountdown();
		await EnterHyperspace();
		await Hyperspace();
		await ExitHyperspace();

		RestoreJump();
	}

	/// <summary>
	/// Drops out of hyperspace, if warping.
	/// </summary>
	public void Drop()
	{
		if (!Warping)
		{
			return;
		}

		_mustDrop = true;
	}

	/// <summary>
	/// Handles input management and updating
	/// certain aspects of the class' functionality.
	/// </summary>
	private void Update()
	{
		if (Warping)
		{
			return;
		}

		if (Input.GetKey(JumpButton) && !CoolingDown)
		{
			BuildupWarp();
		}
		else
		{
			DecayWarp();
		}

		UpdateUI();

		if (WarpBuildup >= 1.0f && CheckWarpConditions())
		{
			Jump();
		}

		if (CoolingDown && WarpBuildup == 0)
		{
			CoolingDown = false;
		}
	}
}
