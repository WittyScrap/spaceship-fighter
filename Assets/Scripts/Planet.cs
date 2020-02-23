using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Generates a planet randomly.
/// </summary>
public class Planet : MonoBehaviour
{
	/// <summary>
	/// The radius of the planet to generate.
	/// </summary>
	[SerializeField, Space, Header("Properties")]
	private float _radius = 100.0f;

	/// <summary>
	/// The amount of subdivisions to make for the planet.
	/// </summary>
	[SerializeField, Min(0)]
	private int _steps = 5;

	/// <summary>
	/// The rotation of the planet.
	/// </summary>
	[SerializeField]
	private Vector3 _rotation = Vector3.zero;

	/// <summary>
	/// Whether or not the planet should generate right away.
	/// </summary>
	[SerializeField]
	private bool _loadOnStart = false;

	/// <summary>
	/// The seed to render the planet.
	/// </summary>
	[SerializeField]
	private int _seed = 0;

	/// <summary>
	/// The material to render the planet with.
	/// </summary>
	[SerializeField, Space, Header("Data"), Space]
	private Material _groundMaterial = null;

	/// <summary>
	/// The material for the planet's atmosphere.
	/// </summary>
	[SerializeField]
	private Material _atmosphereMaterial = null;

	/// <summary>
	/// A constant reference to the sun light's transform.
	/// </summary>
	[SerializeField, Space, Header("Atmospheric data"), Space]
	private Transform _sun = null;

	/// <summary>
	/// The amount of HDR exposure.
	/// </summary>
	[SerializeField]
	private float _hdrExposure = 0.8f;

	/// <summary>
	/// The wawe length of the sun light.
	/// </summary>
	[SerializeField]
	private Vector3 _waveLength = new Vector3(0.65f, 0.57f, 0.475f);

	/// <summary>
	/// Sun brightness constant.
	/// </summary>
	[SerializeField, Tooltip("Sun brightness constant.")]
	private float _ESun = 20.0f;

	/// <summary>
	/// Rayleigh scattering constant.
	/// </summary>
	[SerializeField, Tooltip("Rayleigh scattering constant.")]
	private float _kr = 0.0025f;

	/// <summary>
	/// Mie scattering constant.
	/// </summary>
	[SerializeField, Tooltip("Mie scattering constant")]
	private float _km = 0.0010f;

	/// <summary>
	/// Mie phase asymmetry factor, must be between 0.999 and -0.999.
	/// </summary>
	[SerializeField, Tooltip("Mie phase asymmetry factor."), Range(-.999f, .999f)]
	private float _g = -0.990f;

	/// <summary>
	/// Difference between inner and outer radius, must be 2.5%.
	/// </summary>
	[ReadOnly, SerializeField, Space]
	private float _outerScaleFactor = 1.025f;

	/// <summary>
	/// The scale depth (i.e. the altitude at which the atmosphere's average density is found).
	/// </summary>
	[ReadOnly, SerializeField]
	private float _scaleDepth = 0.25f;

	/// <summary>
	/// The atmospheric thickness.
	/// </summary>
	private float AtmosphericThickness => OuterRadius - _radius;

	/// <summary>
	/// The radius of the atmosphere.
	/// </summary>
	private float OuterRadius => _outerScaleFactor * _radius;

	/// <summary>
	/// The mesh renderer for the ground.
	/// </summary>
	private MeshRenderer _groundRenderer = null;

	/// <summary>
	/// The mesh renderer for the atmosphere.
	/// </summary>
	private MeshRenderer _atmosphereRenderer = null;

	/// <summary>
	/// The mesh filter for the ground.
	/// </summary>
	private MeshFilter _groundFilter = null;

	/// <summary>
	/// The mesh filter for the atmosphere.
	/// </summary>
	private MeshFilter _atmosphereFilter = null;

	/// <summary>
	/// The generated ground mesh.
	/// </summary>
	private Mesh _groundMesh = null;

	/// <summary>
	/// The generated atmosphere mesh.
	/// </summary>
	private Mesh _atmosphereMesh = null;

	/// <summary>
	/// Whether or not the planet has finished loading in.
	/// </summary>
	public bool IsLoaded { get; private set; } = false;

	/// <summary>
	/// The material for the ground.
	/// </summary>
	public Material GroundMaterial {
		get => _groundMaterial;
		set => _groundMaterial = value;
	}

	/// <summary>
	/// The material for the atmosphere.
	/// </summary>
	public Material AtmosphereMaterial {
		get => _atmosphereMaterial;
		set => _atmosphereMaterial = value;
	}

	/// <summary>
	/// The radius of the planet.
	/// </summary>
	public float Radius {
		get => _radius;
		set => _radius = value;
	}

	/// <summary>
	/// The number of subdivisions for this planet.
	/// </summary>
	public int Steps {
		get => _steps;
		set => _steps = value;
	}

	/// <summary>
	/// The planet's rotation.
	/// </summary>
	public Vector3 Rotation {
		get => _rotation;
		set => _rotation = value;
	}

	/// <summary>
	/// The seed to render the planet with.
	/// </summary>
	public int Seed {
		get => _seed;
		set => _seed = value;
	}

	/// <summary>
	/// Sets the sun's transform.
	/// </summary>
	/// <param name="sun">The transform to use for the sun.</param>
	public void SetSun(Transform sun)
	{
		_sun = sun;
	}

	/// <summary>
	/// Creates the necessary components.
	/// </summary>
	private void CreateComponents()
	{
		GameObject ground = new GameObject("Terrain");
		GameObject atmosphere = new GameObject("Atmosphere");

		ground.transform.SetParent(transform);
		atmosphere.transform.SetParent(transform);

		ground.layer = gameObject.layer;
		atmosphere.layer = gameObject.layer;

		ground.transform.localPosition = Vector3.zero;
		atmosphere.transform.localPosition = Vector3.zero;

		_groundRenderer = ground.AddComponent<MeshRenderer>();
		_atmosphereRenderer = atmosphere.AddComponent<MeshRenderer>();

		_groundFilter = ground.AddComponent<MeshFilter>();
		_atmosphereFilter = atmosphere.AddComponent<MeshFilter>();

		// Create a collider to occlude sun flare
		SphereCollider planetCollider = gameObject.AddComponent<SphereCollider>();
		planetCollider.radius = _radius;
		planetCollider.isTrigger = true;
	}

	/// <summary>
	/// Applies the result of all generated data to the created components.
	/// </summary>
	private void ApplyComponents()
	{
		_groundFilter.sharedMesh = _groundMesh;
		_atmosphereFilter.sharedMesh = _atmosphereMesh;

		_groundMaterial = Instantiate(_groundMaterial);
		_atmosphereMaterial = Instantiate(_atmosphereMaterial);

		_groundRenderer.sharedMaterial = _groundMaterial;
		_atmosphereRenderer.sharedMaterial = _atmosphereMaterial;

		UpdateMaterial(_groundMaterial);
		UpdateMaterial(_atmosphereMaterial);

		System.Random seeder = new System.Random(_seed);

		_groundMaterial.SetVector("_Seed", new Vector4((float)seeder.NextDouble() * 100f, (float)seeder.NextDouble() * 100f, -(float)seeder.NextDouble() * 100f, 0));

		_groundMaterial.SetFloat("_NoiseScaleA", (float)seeder.NextDouble() * 5f);
		_groundMaterial.SetFloat("_NoiseScaleB", (float)seeder.NextDouble() * 10f);
		_groundMaterial.SetFloat("_NoiseScaleC", (float)seeder.NextDouble() * 25f);
		_groundMaterial.SetFloat("_NoiseScaleD", (float)seeder.NextDouble() * 50f);
		_groundMaterial.SetFloat("_NoiseScaleE", (float)seeder.NextDouble() * 500f);

		_groundMaterial.SetColor("_LandColor", new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f)));
		_groundMaterial.SetColor("_Mountain", new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f)));
		_groundMaterial.SetColor("_SeaColor", new Color(Random.Range(0, 1.0f), Random.Range(0, 1.0f), Random.Range(0, 1.0f)));

		_groundMaterial.SetFloat("_SeaLevel", (float)seeder.NextDouble());
	}

	/// <summary>
	/// Loads this planet.
	/// </summary>
	public async Task Load()
	{
		CreateComponents();
		await GenerateAsync(_steps);
		ApplyComponents();
	}
	
	/// <summary>
	/// Initialises the given planet material.
	/// </summary>
	/// <param name="material">The material to initialise.</param>
	private void UpdateMaterial(Material material)
	{
		Vector3 invWaveLength4 = new Vector3(1.0f / Mathf.Pow(_waveLength.x, 4.0f), 1.0f / Mathf.Pow(_waveLength.y, 4.0f), 1.0f / Mathf.Pow(_waveLength.z, 4.0f));
		float scale = 1.0f / AtmosphericThickness;

		// Hungarian notation, yuck.
		material.SetVector("v3LightPos", _sun.forward * -1.0f);
		material.SetVector("v3InvWavelength", invWaveLength4);
		material.SetFloat("fOuterRadius", OuterRadius);
		material.SetFloat("fOuterRadius2", OuterRadius * OuterRadius);
		material.SetFloat("fInnerRadius", _radius);
		material.SetFloat("fInnerRadius2", _radius * _radius);
		material.SetFloat("fKrESun", _kr * _ESun);
		material.SetFloat("fKmESun", _km * _ESun);
		material.SetFloat("fKr4PI", _kr * 4.0f * Mathf.PI);
		material.SetFloat("fKm4PI", _km * 4.0f * Mathf.PI);
		material.SetFloat("fScale", scale);
		material.SetFloat("fScaleDepth", _scaleDepth);
		material.SetFloat("fScaleOverScaleDepth", scale / _scaleDepth);
		material.SetFloat("fHdrExposure", _hdrExposure);
		material.SetFloat("g", _g);
		material.SetFloat("g2", _g * _g);
		material.SetVector("v3LightPos", _sun.forward * -1.0f);
		material.SetVector("v3Translate", transform.position);
	}

	/// <summary>
	/// Generates the planet.
	/// </summary>
	private async void Start()
	{
		if (_loadOnStart)
		{
			await Load();
		}
	}

	/// <summary>
	/// Generates the planet without blocking the execution flow.
	/// </summary>
	/// <returns>All the tasks currently processing the planet generation.</returns>
	public async Task GenerateAsync(int steps)
	{
		_groundMesh = GenerateCube(_radius);
		_atmosphereMesh = GenerateCube(OuterRadius);

		AsyncMesh asyncGround = _groundMesh;
		AsyncMesh asyncAtmosphere = _atmosphereMesh;

		Task groundTask = Task.Run(() => MakeSpheroid(asyncGround, steps, _radius));
		Task atmosphereTask = Task.Run(() => MakeSpheroid(asyncAtmosphere, steps, OuterRadius));

		await Task.WhenAll(groundTask, atmosphereTask);

		MapMesh(asyncGround, _groundMesh);
		MapMesh(asyncAtmosphere, _atmosphereMesh);
	}

	/// <summary>
	/// Maps an asyncmesh into a renderable mesh.
	/// </summary>
	private void MapMesh(AsyncMesh source, Mesh destination)
	{
		destination.vertices = source.Vertices;
		destination.normals = source.Normals;
		destination.triangles = source.Triangles;
		destination.uv = source.UVs;
	}

	/// <summary>
	/// Generates a single spheroid shape.
	/// </summary>
	/// <param name="steps">The number of subdivisions.</param>
	private void MakeSpheroid(AsyncMesh output, int steps, float radius)
	{
		MeshSubdivider subdivider = new MeshSubdivider();

		while (steps-- > 0)
		{
			subdivider.Subdivide(output);
		}

		Spherify(output, radius);
	}

	/// <summary>
	/// Transforms a cube into a sphere.
	/// </summary>
	private void Spherify(AsyncMesh sourceMesh, float radius)
	{
		Vector3[] vertices = sourceMesh.Vertices;
		Vector3[] normals = sourceMesh.Normals;

		for (int i = 0; i < vertices.Length; ++i)
		{
			vertices[i] = vertices[i].normalized * radius;
			normals[i] = vertices[i].normalized;
		}
		sourceMesh.Vertices = vertices;
		sourceMesh.Normals = normals;
	}

	/// <summary>
	/// Generates the baseline cube.
	/// </summary>
	private Mesh GenerateCube(float radius)
	{
		Mesh output = new Mesh
		{
			vertices = new Vector3[]
			{
				new Vector3(-radius, radius, -radius),
				new Vector3(-radius, -radius, -radius),
				new Vector3(radius, radius, -radius),
				new Vector3(radius, -radius, -radius),

				new Vector3(-radius, -radius, radius),
				new Vector3(radius, -radius, radius),
				new Vector3(-radius, radius, radius),
				new Vector3(radius, radius, radius),

				new Vector3(-radius, radius, -radius),
				new Vector3(radius, radius, -radius),

				new Vector3(-radius, radius, -radius),
				new Vector3(-radius, radius, radius),

				new Vector3(radius, radius, -radius),
				new Vector3(radius, radius, radius),
			},
			triangles = new int[]
			{
				0, 2, 1, // front
				1, 2, 3,

				4, 5, 6, // back
				5, 7, 6,

				6, 7, 8, //top
				7, 9 ,8, 

				1, 3, 4, //bottom
				3, 5, 4,

				1, 11,10,// left
				1, 4, 11,

				3, 12, 5,//right
				5, 12, 13
			},
			uv = new Vector2[] {
				new Vector2(0, 0.66f),
				new Vector2(0.25f, 0.66f),
				new Vector2(0, 0.33f),
				new Vector2(0.25f, 0.33f),

				new Vector2(0.5f, 0.66f),
				new Vector2(0.5f, 0.33f),
				new Vector2(0.75f, 0.66f),
				new Vector2(0.75f, 0.33f),

				new Vector2(1, 0.66f),
				new Vector2(1, 0.33f),

				new Vector2(0.25f, 1),
				new Vector2(0.5f, 1),

				new Vector2(0.25f, 0),
				new Vector2(0.5f, 0),
			}
		};

		Rotate(output, Quaternion.Euler(_rotation));
		output.RecalculateNormals();
		return output;
	}

	/// <summary>
	/// Rotates all the vertices in the mesh by the quaternion.
	/// </summary>
	private void Rotate(Mesh mesh, Quaternion rotation)
	{
		Matrix4x4 rotationMatrix = Matrix4x4.Rotate(rotation);
		Vector3[] vertices = mesh.vertices;

		for (int i = 0; i < vertices.Length; ++i)
		{
			vertices[i] = rotationMatrix * vertices[i];
		}

		mesh.vertices = vertices;
	}

	/// <summary>
	/// Draw sphere gizmos
	/// </summary>
	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere(transform.position, _radius);
	}

	/// <summary>
	/// Updates the materials.
	/// </summary>
	private void Update()
	{
		UpdateMaterial(_groundMaterial);
		UpdateMaterial(_atmosphereMaterial);
	}
}