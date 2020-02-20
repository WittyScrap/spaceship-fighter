using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshair : MonoBehaviour
{
	/// <summary>
	/// The player to use as reference point for where to project this crosshair.
	/// </summary>
	[SerializeField]
	private Transform _player = null;

	/// <summary>
	/// The camera to use for the projection.
	/// </summary>
	[SerializeField]
	private Camera _camera = null;

	/// <summary>
	/// The distance of the crosshair.
	/// </summary>
	[Space, SerializeField]
	private float _distance = 10000;

    // Update is called once per frame
    void Update()
    {
		Vector3 worldPosition = _player.position + _player.forward * _distance;
		Vector2 screenPosition = _camera.WorldToScreenPoint(worldPosition);

		GetComponent<RectTransform>().anchoredPosition = screenPosition;
    }
}
