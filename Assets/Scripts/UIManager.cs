using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Static UI helper class.
/// </summary>
public static class UIManager
{
	/// <summary>
	/// Sets an image based slider to a value.
	/// </summary>
	/// <param name="sliderImage">The slider to update.</param>
	/// <param name="value">The value to update the slider to.</param>
    public static void SetSlider(RawImage sliderImage, float value)
	{
		// Extract size and UV information...
		Vector2 sizeDelta = sliderImage.rectTransform.sizeDelta;
		Rect uvRect = sliderImage.uvRect;

		// Set related values...
		sizeDelta.y = 250 * value;
		uvRect.height = 20 * value;

		// Update...
		sliderImage.rectTransform.sizeDelta = sizeDelta;
		sliderImage.uvRect = uvRect;
	}
}
