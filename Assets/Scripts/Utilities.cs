using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static helper class for mathematics and general utilities.
/// </summary>
public static class Utilities
{
	/// <summary>
	/// Passes the alpha through circular interpolation and returns the appropriate result.
	/// Circular interpolation runs an alpha value (0-1) through a circle equation to calculate
	/// a smoother gradient with a variant slope. This function uses the UPPER-LEFT quadrant
	/// of a circle for the interpolation.
	/// </summary>
	/// <param name="alpha">The alpha value.</param>
	/// <returns>Circularly interpolated alpha value.</returns>
	public static float CircleInterpolate(float alpha, float pow = 2)
	{
		return Mathf.Sqrt(1 - Mathf.Pow(alpha - 1, pow));
	}

	/// <summary>
	/// Inverse version of the circle interpolation function.
	/// Circular interpolation runs an alpha value (0-1) through a circle equation to calculate
	/// a smoother gradient with a variant slope. This function uses the LOWER-RIGHT quadrant
	/// of a circle for the interpolation.
	/// </summary>
	/// <param name="alpha">The alpha value.</param>
	/// <returns>Circularly interpolated alpha value.</returns>
	public static float InverseCircleInterpolate(float alpha, float pow = 2)
	{
		return 1 - Mathf.Sqrt(1 - Mathf.Pow(alpha, pow));
	}
}
