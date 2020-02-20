using System.Threading.Tasks;
using UnityAsync;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles universal UI operations.
/// </summary>
public class UISystem : MonoBehaviour
{
	#region Serialised class data - private:

	[Space, Header("UI Elements"), Space]

	[SerializeField] RectTransform		_upperUI   = null;
	[SerializeField] RectTransform		_lowerUI   = null;
	[SerializeField] TextMeshProUGUI	_warpLock  = null;
	[SerializeField] Image				_crosshair = null;

	#endregion

	#region Single instance management - private:

	/// <summary>
	/// Singleton.
	/// </summary>
	private static UISystem instance;

	/// <summary>
	/// Access to the singleton.
	/// </summary>
	private static UISystem Instance {
		get
		{
			if (!instance)
			{
				instance = FindObjectOfType<UISystem>();
			}
			return instance;
		}
	}

	#endregion

	/// <summary>
	/// Hides/shows the lower portion of the UI.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private static async Task SetLower(bool state)
	{
		RectTransform lowerUI = Instance._lowerUI;

		float startBottom = -(Screen.height / 2) * (state ? 0 : 1);
		float startTop = Screen.height / (state ? 2 : 1);

		float targetBottom = -(Screen.height / 2) * (state ? 1 : 0);
		float targetTop	=   Screen.height / (state ? 1 : 2);

		for (float alpha = 0; alpha <= 1.0f; alpha += Time.deltaTime * 2)
		{
			Vector2 lowerAnchor = lowerUI.offsetMin;
			Vector2 upperAnchor = lowerUI.offsetMax;

			upperAnchor.y = Mathf.Lerp(startTop, targetTop, alpha);
			lowerAnchor.y = Mathf.Lerp(startBottom, targetBottom, alpha);

			lowerUI.offsetMin = lowerAnchor;
			lowerUI.offsetMax = upperAnchor;

			// Wait one frame...
			await Await.NextUpdate();
		}
	}	

	/// <summary>
	/// Hides/shows the upper portion of the UI.
	/// </summary>
	/// <returns>An awaitable task.</returns>
	private static async Task SetUpper(bool state)
	{
		RectTransform upperUI = Instance._upperUI;

		float startBottom = Screen.height / (state ? 2 : 1);
		float startTop = (Screen.height / 2) * (state ? 0 : 1);

		float targetBottom = Screen.height / (state ? 1 : 2);
		float targetTop = (Screen.height / 2) * (state ? 1 : 0);

		for (float alpha = 0; alpha <= 1.0f; alpha += Time.deltaTime * 2)
		{
			Vector2 lowerAnchor = upperUI.offsetMin;
			Vector2 upperAnchor = upperUI.offsetMax;

			upperAnchor.y = Mathf.Lerp(startTop, targetTop, alpha);
			lowerAnchor.y = Mathf.Lerp(startBottom, targetBottom, alpha);

			upperUI.offsetMin = lowerAnchor;
			upperUI.offsetMax = upperAnchor;

			// Wait one frame...
			await Await.NextUpdate();
		}
	}

	/// <summary>
	/// Hides away the UI.
	/// </summary>
	public static async void Hide()
	{
		Instance._crosshair.enabled = false;

		Task hideLower = SetLower(true);
		Task hideUpper = SetUpper(true);

		await Task.WhenAll(hideLower, hideUpper);
	}

	/// <summary>
	/// Shows the UI.
	/// </summary>
	public static async void Show()
	{
		Task showLower = SetLower(false);
		Task showUpper = SetUpper(false);

		await Task.WhenAll(showLower, showUpper);

		Instance._crosshair.enabled = true;
	}

	/// <summary>
	/// Sets the graphics for the slider to locked or unlocked.
	/// </summary>
	public static void SetLock(bool lockState, RawImage slider)
	{
		Instance._warpLock.enabled = lockState;
		slider.color = lockState ? Color.red : Color.white;
	}
}
