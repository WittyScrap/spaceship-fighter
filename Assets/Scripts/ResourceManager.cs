﻿using System.Collections;
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
    [SerializeField] private int _steps = 3;

    [Space]

    [SerializeField] private Gradient _plainRings;
    [SerializeField] private Gradient _icyRings;

    /// <summary>
    /// Simple rocky rings.
    /// </summary>
    public Texture2D PlainRings => FromGradient(_plainRings, _resolution, _steps);

    /// <summary>
    /// Icy rings, will contain a lot of water and resources.
    /// </summary>
    public Texture2D IcyRings => FromGradient(_icyRings, _resolution, _steps);

    #region Private

    private void Awake()
    {
        _instance = this;
    }

    private Texture2D FromGradient(Gradient source, int horizontalResolution, int repeats)
    {
        Texture2D o = new Texture2D(horizontalResolution * repeats, 1);

        for (int step = 0; step < repeats; ++step)
        {
            for (int i = horizontalResolution * step, t = 0; i < horizontalResolution * (step + 1); ++i, ++t)
            {
                o.SetPixel(i, 0, source.Evaluate((float)t / horizontalResolution));
            }
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