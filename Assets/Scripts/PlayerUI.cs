using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Player exclusive UI elements manager.
/// </summary>
[RequireComponent(typeof(WarpDrive))]
public class PlayerUI : MonoBehaviour
{
	#region Class private data - private:

	/*
	 * This is class data, it is inacccessible from the outside and should
	 * only be used as storage, no operation should be performed directly
	 * on it. Use properties for general use.
	 */

	private WarpDrive _warpDrive = null;
	private string[] Animation = new string[] { "*", "**", "***" };
	private int AnimationFrame = 0;

	#endregion

	#region Serialised class data - private:

	/*
	 * Note:
	 * This data is serialised through Unity, as with the
	 * private data, it is not meant to be accessed directly.
	 * Use one of the properties provided.
	 * 
	 */

	[Space, Header("Warp Drive - UI manager"), Space]

	[SerializeField] TMPro.TextMeshProUGUI _countdownTick = null;

	#endregion

	/// <summary>
	/// Saves the warp drive instance and sets up events.
	/// </summary>
	private void Awake()
	{
		_warpDrive = GetComponent<WarpDrive>();
		_warpDrive.OnCountdownUpdate += _warpDrive_OnCountdownUpdate;
		_warpDrive.OnCountdownFinished += _warpDrive_OnCountdownFinished;
		_warpDrive.OnHyperspaceEntered += _warpDrive_OnHyperspaceEntered;
		_warpDrive.OnExitingHyperspace += _warpDrive_OnExitingHyperspace;
		_warpDrive.OnHyperspaceLeft += _warpDrive_OnHyperspaceLeft;
	}

	/// <summary>
	/// Sets countdown text to final frame.
	/// </summary>
	private void _warpDrive_OnCountdownFinished(object sender, System.EventArgs e)
	{
		_countdownTick.text = "entering hyperspace";
	}

	/// <summary>
	/// Advances spinning animation and returns current character.
	/// </summary>
	private string AdvanceAnimation()
	{
		string frame = Animation[AnimationFrame];
		AnimationFrame++;

		if (AnimationFrame >= Animation.Length)
		{
			AnimationFrame = 0;
		}

		return frame;
	}

	/// <summary>
	/// Updates the countdown timer.
	/// </summary>
	private void _warpDrive_OnCountdownUpdate(object sender, WarpDrive.WarpCountdownArgs e)
	{
		string spaces = GetSpaces(Mathf.FloorToInt(e.CountdownLeft));
		_countdownTick.text = $">>>> {spaces} {e.CountdownLeft.ToString("000.000")} {spaces} <<<<";
	}

	/// <summary>
	/// Hide the label.
	/// </summary>
	private void _warpDrive_OnHyperspaceLeft(object sender, System.EventArgs e)
	{
		_countdownTick.text = "";
	}

	/// <summary>
	/// Jump complete label.
	/// </summary>
	private void _warpDrive_OnExitingHyperspace(object sender, System.EventArgs e)
	{
		_countdownTick.text = "... beginning next phase ...";
	}

	/// <summary>
	/// Hyperspace label.
	/// </summary>
	private void _warpDrive_OnHyperspaceEntered(object sender, System.EventArgs e)
	{
		_countdownTick.text = "";
	}

	/// <summary>
	/// Returns the given amount of spaces in a string.
	/// </summary>
	/// <param name="count"></param>
	private string GetSpaces(int count)
	{
		return new string(' ', count);
	}

	///// <summary>
	///// Updates the countdown text.
	///// </summary>
	//private void _warpDrive_OnCountdownTick(object sender, WarpDrive.WarpCountdownArgs e)
	//{
	//	_countdownTick.text = ">>> " + GetSpaces((int)e.CountdownLeft) + (e.CountdownLeft == 0 ? "*" : e.CountdownLeft.ToString()) + GetSpaces((int)e.CountdownLeft) + " <<<";
	//}
}
