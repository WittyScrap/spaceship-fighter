using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mesh that can be manipulated in different threads.
/// </summary>
public class AsyncMesh
{
	public Vector3[] Vertices;
	public Vector3[] Normals;
	public Vector2[] UVs;
	public int[] Triangles;

	/// <summary>
	/// Casts a normal mesh into an async mesh.
	/// </summary>
	/// <param name="mesh"></param>
	public static implicit operator AsyncMesh(Mesh mesh)
	{
		AsyncMesh asyncMesh = new AsyncMesh();
		asyncMesh.Vertices = mesh.vertices;
		asyncMesh.Normals = mesh.normals;
		asyncMesh.Triangles = mesh.triangles;
		asyncMesh.UVs = mesh.uv;

		return asyncMesh;
	}

	/// <summary>
	/// Maps an asyncmesh into a renderable mesh.
	/// </summary>
	public static void Apply(AsyncMesh source, Mesh destination)
	{
		destination.vertices = source.Vertices;
		destination.normals = source.Normals;
		destination.triangles = source.Triangles;
		destination.uv = source.UVs;
	}

	/// <summary>
	/// Creates a completely empty asynchronous mesh.
	/// </summary>
	public AsyncMesh()
	{

	}
}
