using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages textures, materials, and other resources.
/// </summary>
public class ResourceManager : MonoBehaviour
{
    private static ResourceManager _instance = null;

    [Space, Header("Planetary Rings"), Space]

    [SerializeField] private int _resolution = 64;
    [SerializeField, Range(0, 1)] private float _chunkVariation = .1f;

    [Space]

    [SerializeField] private Gradient _plainRings;
    [SerializeField] private Gradient _icyRings;

    [Space]

    [SerializeField] private Material _ringsMaterial;

    /// <summary>
    /// Simple rocky rings.
    /// </summary>
    public Texture2D PlainRings => FromGradient(_plainRings, _resolution);

    /// <summary>
    /// Icy rings, will contain a lot of water and resources.
    /// </summary>
    public Texture2D IcyRings => FromGradient(_icyRings, _resolution);

    /// <summary>
    /// The material to be used to render rings.
    /// </summary>
    public Material RingsMaterial => _ringsMaterial;

    #region Private

    private void Awake()
    {
        _instance = this;
    }

    private int Chunk()
    {
        return Random.Range(5, 500);
    }

    private Texture2D FromGradient(Gradient source, int horizontalResolution)
    {
        Texture2D o = new Texture2D(horizontalResolution, 1);

        int chunkSize = Chunk();
        int chunkValue = 0;
        int chunk = 0;

		for (int i = 0; i < horizontalResolution; ++i)
		{
            chunk++;

            if (chunk >= chunkSize)
            {
                chunkSize = Chunk();
                chunk = 0;
				chunkValue = (chunkValue + 1) % source.colorKeys.Length;
			}

            float variation = (Random.value * 2 - 1) * _chunkVariation;
            Color value = source.colorKeys[chunkValue].color;

            value.r += variation;
            value.g += variation;
            value.b += variation;

            value.a = (value.r + value.g + value.b) / 3;

			o.SetPixel(i, 0, value);
		}

        o.Apply();
        return o;
    }

    #endregion

    /// <summary>
    /// Gets the current manager, if one exists.
    /// </summary>
    /// <returns>The current manager, if one exists. Returns null of no manager has been assigned.</returns>
    public static ResourceManager GetManager()
    {
        return _instance;
    }
}
