using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


/// <summary>
/// Generates a solar system composed of a random arrangement
/// of planets and moons.
/// </summary>
public class SystemGenerator : MonoBehaviour
{
	/// <summary>
	/// Hierarchical solar system structure.
	/// </summary>
	private class SystemTree
	{
		/// <summary>
		/// The list of sub systems.
		/// </summary>
		private List<SystemTree> _subSystems = new List<SystemTree>();

		/// <summary>
		/// Sub systems.
		/// </summary>
		public SystemTree[] SubSystems => _subSystems.ToArray();

		/// <summary>
		/// This node's value.
		/// </summary>
		public Planet Node { get; set; } = null;

		/// <summary>
		/// The node's transform.
		/// </summary>
		public Transform NodeTransform => Node?.transform;

		/// <summary>
		/// Creates a new branch in the system tree.
		/// </summary>
		/// <returns>The generated branch.</returns>
		public SystemTree Branch(Planet node = null)
		{
			SystemTree nextBranch = new SystemTree();
			nextBranch.Node = node;

			_subSystems.Add(nextBranch);
			return nextBranch;
		}
	}

	/// <summary>
	/// The minimum number of planets.
	/// </summary>
	[SerializeField, Space, Header("System generation properties")]
	private int _minPlanets = 1;

	/// <summary>
	/// The maximum number of planets.
	/// </summary>
	[SerializeField]
	private int _maxPlanets = 5;

	/// <summary>
	/// The minimum number of moons per planet generated.
	/// </summary>
	[SerializeField]
	private int _minMoonsPerPlanet = 0;

	/// <summary>
	/// The maximum number of moons per planet generated.
	/// </summary>
	[SerializeField]
	private int _maxMoonsPerPlanet = 2;

	/// <summary>
	/// The minimum planet distance.
	/// </summary>
	[SerializeField]
	private float _minDistance = 2.0f;

	/// <summary>
	/// How far planets can be generated.
	/// </summary>
	[SerializeField]
	private float _maxDistance = 100.0f;

	/// <summary>
	/// The maximum distance between a moon and a planet.
	/// </summary>
	[SerializeField]
	private float _maxMoonDistance = 1.0f;

	[SerializeField, Space]
	private Transform _sun = null;

	/// <summary>
	/// Whether or not the position of the sun should be randomized.
	/// </summary>
	[SerializeField]
	private bool _randomizeSun = true;

	/// <summary>
	/// A list of all possible skyboxes.
	/// </summary>
	[SerializeField]
	private Material[] _skyboxes = null;

	/// <summary>
	/// The minimum radius for planets.
	/// </summary>
	[SerializeField, Space, Header("Planet generation properties"), Space]
	private float _minPlanetSize = 1.0f;

	/// <summary>
	/// The maximum radius for planets.
	/// </summary>
	[SerializeField]
	private float _maxPlanetSize = 5.0f;

	/// <summary>
	/// The minimum radius for moons.
	/// </summary>
	[SerializeField, Space]
	private float _minMoonSize = 0.1f;

	/// <summary>
	/// The maximum radius for moons.
	/// </summary>
	[SerializeField]
	private float _maxMoonSize = 0.2f;

	/// <summary>
	/// The number of subdivisions for planets and moons.
	/// </summary>
	[SerializeField]
	private int _detail = 6;

	/// <summary>
	/// The ground material to render planets with.
	/// </summary>
	[SerializeField, Space]
	private Material _planetGroundMaterial = null;

	/// <summary>
	/// The atmosphere material to render planets with.
	/// </summary>
	[SerializeField]
	private Material _planetAtmosphereMaterial = null;

	/// <summary>
	/// The root of the solar system.
	/// </summary>
	private Transform _systemRoot = null;

	/// <summary>
	/// The system's structure.
	/// </summary>
	private SystemTree _systemTree = null;

	/// <summary>
	/// The generator for the planet seeds.
	/// </summary>
	private System.Random _seedGenerator = null;

	/// <summary>
	/// Creates the root.
	/// </summary>
	private void CreateRoot()
	{
		GameObject systemRoot = new GameObject("Root");

		_systemRoot = systemRoot.transform;
		_systemRoot.SetParent(transform);

		_systemRoot.localPosition = Vector3.zero;
		_systemRoot.localRotation = Quaternion.identity;
		_systemRoot.localScale = Vector3.one;

		_systemTree = new SystemTree();
	}

	/// <summary>
	/// Loads the next solar system.
	/// </summary>
	public async Task LoadNext()
	{
		ClearSystem();
		CreateRoot();

		int planetCount = Random.Range(_minPlanets, _maxPlanets);
		_seedGenerator = new System.Random((int)System.DateTime.Now.Ticks);

		while (planetCount-- > 0)
		{
			await CreatePlanetAsync(
				_systemTree,
				_systemRoot, 
				_seedGenerator,
				Random.Range(_minPlanetSize, _maxPlanetSize), 
				Random.Range(_minMoonsPerPlanet, _maxMoonsPerPlanet), 
				_maxDistance, 
				true,
				_minDistance
			);
		}

		if (_skyboxes.Length > 0)
		{
			int skyboxIndex = Random.Range(0, _skyboxes.Length + 100) % _skyboxes.Length;
			RenderSettings.skybox = _skyboxes[skyboxIndex];
		}

		RandomizeSun();
	}

	/// <summary>
	/// Sets the sun in a random orientation.
	/// </summary>
	public void RandomizeSun()
	{
		if (_randomizeSun)
		{
			_sun.forward = Random.insideUnitSphere;
		}
	}

	/// <summary>
	/// Creates a planet asynchronously.
	/// </summary>
	/// <param name="node">The parent node.</param>
	/// <param name="parent">The parent transform.</param>
	/// <param name="radius">The radius of the planet.</param>
	/// <param name="moonsCount">The number of moons.</param>
	/// <param name="maxDistance">The maximum distance around the parent transform the planet can generate in.</param>
	/// <returns>An awaitable task</returns>
	private async Task CreatePlanetAsync(SystemTree node, Transform parent, System.Random seeder, float radius, int moonsCount, float maxDistance, bool hasAtmosphere, float minDistance = 0)
	{
		GameObject nextPlanet = new GameObject("AutoPlanet");
		nextPlanet.layer = LayerMask.NameToLayer("Backdrop");
		Transform planetTransform = nextPlanet.transform;

		planetTransform.SetParent(parent);
		planetTransform.localPosition = Random.insideUnitSphere * maxDistance;

		if (planetTransform.localPosition.sqrMagnitude < minDistance * minDistance)
		{
			planetTransform.localPosition = planetTransform.localPosition.normalized * minDistance;
		}

		Planet planetComponent = nextPlanet.AddComponent<Planet>();

		planetComponent.Radius = radius;
		planetComponent.Steps = _detail;
		planetComponent.GroundMaterial = _planetGroundMaterial;
		planetComponent.AtmosphereMaterial = _planetAtmosphereMaterial;
		planetComponent.SetSun(_sun);
		planetComponent.Rotation = new Vector3(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
		planetComponent.Seed = seeder.Next();
		planetComponent.HasAtmosphere = hasAtmosphere;

		await planetComponent.Load();

		SystemTree newRoot = node.Branch(planetComponent);

		while (moonsCount-- > 0)
		{
			float moonRadius = Random.Range(_minMoonSize, _maxMoonSize);
			float moonDistance = radius + moonRadius + _maxMoonDistance;

			await CreatePlanetAsync(
				newRoot,
				planetTransform,
				seeder,
				moonRadius,
				0,
				moonDistance,
				false,
				moonDistance
			);
		}
	}

	/// <summary>
	/// Clears the system and destroys the root.
	/// </summary>
	private void ClearSystem()
	{
		if (_systemRoot)
		{
			Destroy(_systemRoot.gameObject);
			_systemRoot = null;
		}
	}
}
