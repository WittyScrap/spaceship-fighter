using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// What type of material an asteroid belt/ring
/// is composed from.
/// </summary>
enum RockType
{ 
    RT_Rocky,
    RT_Icy,
    RT_Metallic
}

/// <summary>
/// A ring system that can contain rocky, metallic,
/// or icy planetary rings. This system is generated
/// asynchronously.
/// </summary>
public class RingSystem : MonoBehaviour
{
    [SerializeField] private float _innerRadius = 10f;
    [SerializeField] private float _outerRadius = 20f;
    [SerializeField] private int _resolution = 256;
    [SerializeField] private bool _generateOnStart = false;
    [SerializeField] private RockType _ringType = RockType.RT_Rocky;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private Mesh _generatedMesh;
    private AsyncMesh _asyncMesh;
    private Material _ringMaterial;
    private Texture2D _ringTexture;

    /// <summary>
    /// The radius at which the planetary rings begin (starting from the planet).
    /// </summary>
    public float InnerRadius { get => _innerRadius; set => _innerRadius = value; }

    /// <summary>
    /// The radius at which the planetary rings end (starting from the planet).
    /// </summary>
    public float OuterRadius { get => _outerRadius; set => _outerRadius = value; }


    #region Private
    
    private void GenerateComponents()
    {
        _meshFilter = gameObject.AddComponent<MeshFilter>();
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _generatedMesh = new Mesh();
        _ringMaterial = new Material(ResourceManager.GetManager().RingsMaterial);
    }

    private Vector3 GetVertex(int v, float radius)
	{
		float x = radius * Mathf.Sin((2 * Mathf.PI * v) / _resolution);
		float y = radius * Mathf.Cos((2 * Mathf.PI * v) / _resolution);

        return new Vector3(x, 0, y);
	}

    private (Vector3, Vector3) GetRingVertices(int v)
    {
        Vector3 inner = GetVertex(v, _innerRadius);
        Vector3 outer = GetVertex(v, _outerRadius);

        return (inner, outer);
    }

    private void AppendTriangle(List<int> triangles, int verticesCount, bool reversed)
	{
        int[] newCoordinates = new int[]
        {
            verticesCount - 2,
            verticesCount - 1,
            verticesCount,
            verticesCount - 1,
            verticesCount + 1,
            verticesCount
        };

        if (reversed)
        {
            triangles.AddRange(newCoordinates.Reverse());
        }
        else
        {
            triangles.AddRange(newCoordinates);
        }
	}

    private void PopulateMeshData(List<Vector3> vertices, List<Vector3> normals, List<int> triangles, List<Vector2> uvs, bool reversed, Vector3 normal)
	{
		for (int v = 0; v < _resolution; ++v)
		{
			(Vector3 inner, Vector3 outer) current = GetRingVertices(v);

			if (vertices.Count > 0)
			{
				AppendTriangle(triangles, vertices.Count, reversed);
			}

			vertices.Add(current.inner);
			vertices.Add(current.outer);

			float uv_v = (float)v / (_resolution + 1);

			uvs.Add(new Vector2(0, uv_v));
			uvs.Add(new Vector2(1, uv_v));

			normals.Add(normal);
			normals.Add(normal);
		}

		(Vector3 inner, Vector3 outer) last = GetRingVertices(0);

		AppendTriangle(triangles, vertices.Count, reversed);

		vertices.Add(last.inner);
		vertices.Add(last.outer);

		uvs.Add(new Vector2(0, 1));
		uvs.Add(new Vector2(1, 1));

		normals.Add(normal);
		normals.Add(normal);
	}

    private void TaskGenerateMesh()
	{
		_asyncMesh = new AsyncMesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

		PopulateMeshData(vertices, normals, triangles, uvs, false, Vector3.up);
		PopulateMeshData(vertices, normals, triangles, uvs, true, Vector3.down);
		PopulateMeshData(vertices, normals, triangles, uvs, false, Vector3.down);
		PopulateMeshData(vertices, normals, triangles, uvs, true, Vector3.up);

		_asyncMesh.Vertices = vertices.ToArray();
        _asyncMesh.Triangles = triangles.ToArray();
        _asyncMesh.UVs = uvs.ToArray();
        _asyncMesh.Normals = normals.ToArray();
	}

    private async Task GenerateMesh()
    {
        await Task.Run(TaskGenerateMesh);
    }

    private void ApplyComponents()
    {
        AsyncMesh.Apply(_asyncMesh, _generatedMesh);

		_meshFilter.sharedMesh = _generatedMesh;
        _meshRenderer.sharedMaterial = _ringMaterial;

        switch (_ringType)
        {
            case RockType.RT_Rocky:
            case RockType.RT_Metallic:
                _ringTexture = ResourceManager.GetManager().PlainRings;
                break;

            case RockType.RT_Icy:
                _ringTexture = ResourceManager.GetManager().IcyRings;
                break;
        }

        _ringMaterial.mainTexture = _ringTexture;
    }

    #endregion

    /// <summary>
    /// Begins the ring initialization.
    /// Will create the necessary component, use temporary
    /// asynchronous meshes to generate the geometry, apply
    /// them to Unity meshes, and refresh the materials and
    /// textures to the given parameters.
    /// </summary>
    public async Task Generate()
    {
        GenerateComponents();
        await GenerateMesh();
        ApplyComponents();
    }

    /// <summary>
    /// If required, generates the planetary rings.
    /// </summary>
    public async void Start()
    {
        if (_generateOnStart)
        {
            await Generate();
        }
    }
}
