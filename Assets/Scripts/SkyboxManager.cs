using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the internal 3D skybox.
/// </summary>
public class SkyboxManager : MonoBehaviour
{
	#region Serialised class data - private:

	/// <summary>
	/// The transform of the rendering camera.
	/// </summary>
	[SerializeField]
	private Transform _sourceCamera = null;

	/// <summary>
	/// How much slower the skybox camera should move
	/// relative to the real camera, with 0 being immobile
	/// and 1 being as fast as the original camera.
	/// </summary>
	[SerializeField, Range(0, 1)]
	private float _scaleFactor = 0.00001f;

	#endregion

	#region Internal class data - private:

	/// <summary>
	/// The internal transform, cached.
	/// </summary>
	private Transform _transform = null;

	/// <summary>
	/// A reference to the camera object itself.
	/// </summary>
	private Camera _cameraObject = null;

	#endregion

	#region Class properties - public:

	/// <summary>
	/// The position the camera is expected to be found in.
	/// </summary>
	public Vector3 ExpectedPosition {
		get
		{
			return _sourceCamera.position * _scaleFactor;
		}
	}

	/// <summary>
	/// The camera object related to this manager.
	/// </summary>
	public Camera CameraObject {
		get
		{
			if (!_cameraObject)
			{
				_cameraObject = GetComponent<Camera>();
			}
			return _cameraObject;
		}
	}

	/// <summary>
	/// The current local position of the camera.
	/// </summary>
	public Vector3 Position {
		get
		{
			return _transform.localPosition;
		}
		set
		{
			_transform.localPosition = value;
		}
	}

	/// <summary>
	/// The current forward vector of the camera.
	/// </summary>
	public Vector3 Forward {
		get
		{
			return _transform.forward;
		}
		set
		{
			_transform.forward = value;
		}
	}

	/// <summary>
	/// Whether or not the camera object is enabled.
	/// </summary>
	public bool CameraEnabled {
		get
		{
			return CameraObject.enabled;
		}
		set
		{
			CameraObject.enabled = value;
		}
	}

	/// <summary>
	/// Whether or not the skybox camera's position should be
	/// updated every frame, setting this to false will set the update
	/// method up to only update the camera's rotation.
	/// </summary>
	public bool UpdatePosition {
		get;
		set;
	}

	#endregion

	/// <summary>
	/// Cache transform.
	/// </summary>
	private void Start()
	{
		_transform = transform;
	}

	/// <summary>
	/// Place the camera in the appropriate location.
	/// </summary>
	private void Update()
	{
		if (UpdatePosition)
		{
			_transform.localPosition = ExpectedPosition;
		}

		_transform.localRotation = _sourceCamera.rotation;
	}
}
