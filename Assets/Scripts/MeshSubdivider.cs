using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Mesh subdividing utility.
/// </summary>
public class MeshSubdivider
{
	private List<Vector3> _vertices;
	private List<Vector3> _normals;
	private List<Vector2> _uvs;
	private List<int> _indices;

	private Dictionary<uint, int> _newVectices;

	/// <summary>
	/// Returns the next vertex in the subdivision.
	/// </summary>
	/// <param name="i1">The index for the first vertex.</param>
	/// <param name="i2">The index for the second vertex.</param>
	/// <returns>Newly generated vertex that sits between the two given vertices.</returns>
	int GetNewVertex(int i1, int i2)
	{
		// We have to test both directions since the edge
		// could be reversed in another triangle
		uint t1 = ((uint)i1 << 16) | (uint)i2;
		uint t2 = ((uint)i2 << 16) | (uint)i1;

		if (_newVectices.ContainsKey(t2))
		{
			return _newVectices[t2];
		}

		if (_newVectices.ContainsKey(t1))
		{
			return _newVectices[t1];
		}

		// generate vertex
		int newIndex = _vertices.Count;
		_newVectices.Add(t1, newIndex);

		// calculate new vertex
		_vertices.Add((_vertices[i1] + _vertices[i2]) * 0.5f);
		_normals.Add((_normals[i1] + _normals[i2]).normalized);
		_uvs.Add((_uvs[i1] + _uvs[i2]) * 0.5f);

		return newIndex;
	}

	/// <summary>
	/// Performs a mesh subdivision.
	/// </summary>
	/// <param name="mesh">The mesh to subdivide.</param>
	public void Subdivide(AsyncMesh mesh)
	{
		_newVectices = new Dictionary<uint, int>();

		_vertices = new List<Vector3>(mesh.Vertices);
		_normals = new List<Vector3>(mesh.Normals);
		_uvs = new List<Vector2>(mesh.UVs);
		_indices = new List<int>();

		int[] triangles = mesh.Triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			int i1 = triangles[i + 0];
			int i2 = triangles[i + 1];
			int i3 = triangles[i + 2];

			int a = GetNewVertex(i1, i2);
			int b = GetNewVertex(i2, i3);
			int c = GetNewVertex(i3, i1);

			_indices.Add(i1); _indices.Add(a); _indices.Add(c);
			_indices.Add(i2); _indices.Add(b); _indices.Add(a);
			_indices.Add(i3); _indices.Add(c); _indices.Add(b);
			_indices.Add(a);  _indices.Add(b); _indices.Add(c); // center triangle
		}

		mesh.Vertices = _vertices.ToArray();
		mesh.Normals = _normals.ToArray();
		mesh.Triangles = _indices.ToArray();
		mesh.UVs = _uvs.ToArray();
	}
}
