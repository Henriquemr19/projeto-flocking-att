using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    public static FlockManager Instance { get; private set; }

    [Header("Configuração do Bando")]
    [SerializeField] private int boidCount = 150;
    [SerializeField] private GameObject boidPrefab;
    [SerializeField] private float spawnRadius = 8.0f;

    [Header("Pesos das Regras")]
    [Range(0f, 10f)] public float separationWeight = 2.5f;
    [Range(0f, 10f)] public float alignmentWeight = 1.8f;
    [Range(0f, 10f)] public float cohesionWeight = 1.2f;

    [Header("Raios e Distâncias")]
    [Range(0.2f, 10f)] public float perceptionRadius = 2.5f;
    [Range(0.2f, 5f)] public float separationDistance = 1.2f;

    [Header("Limites Dinâmicos")]
    public float maxSpeed = 8.0f;
    public float minSpeed = 2.0f;
    public float maxForce = 15.0f;
    public float rotationSpeed = 6.0f;

    [Header("Limites do Mundo")]
    public float boundaryRadius = 22.0f;
    public float boundaryWeight = 6.0f;

    [Header("Desvio de Obstáculos")]
    public LayerMask obstacleLayer;
    public float obstacleProbeDistance = 4.5f;
    public float obstacleProbeRadius = 0.6f;
    public float obstacleAvoidanceWeight = 18.0f;

    [Header("Predador")]
    public Transform predator;
    public float predatorPanicRadius = 7.0f;
    public float predatorEvadeWeight = 22.0f;

    [Header("Grid Espacial (Otimização)")]
    public bool useSpatialGrid = false;
    public float gridCellSize = 2.0f;

    [Header("Gizmos")]
    public bool drawAllGizmos = false;

    private readonly List<Boid> boids = new List<Boid>(512);
    private SpatialGrid spatialGrid;

    public int BoidsCount => boids.Count;
    public Vector3 FlockCenterOfMass { get; private set; }
    public float PredatorPanicRadius => predatorPanicRadius;
    public SpatialGrid Grid => spatialGrid;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        spatialGrid = new SpatialGrid(gridCellSize);
    }

    private void Start()
    {
        SpawnFlock();
    }

    private void SpawnFlock()
    {
        if (boidPrefab == null)
        {
            Debug.LogError("Prefab do Boid não atribuído no FlockManager.");
            return;
        }

        for (int i = 0; i < boidCount; i++)
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * spawnRadius;
            Quaternion randomRot = Random.rotation;

            GameObject go = Instantiate(boidPrefab, randomPos, randomRot, transform);
            Boid boid = go.GetComponent<Boid>();

            if (boid != null)
            {
                if (i == 0)
                {
                    boid.IsGizmoLeader = true;
                }
                boids.Add(boid);
            }
        }
    }

    private void Update()
    {
        // Se ativado, reconstrói o grid espacial com as novas posições dos boids
        if (useSpatialGrid)
        {
            spatialGrid.Clear();
            int count = boids.Count;
            for (int i = 0; i < count; i++)
            {
                spatialGrid.Insert(boids[i]);
            }
        }

        // Calcula o centro de massa do grupo para o predador poder persegui-lo
        if (boids.Count > 0)
        {
            Vector3 center = Vector3.zero;
            int count = boids.Count;
            for (int i = 0; i < count; i++)
            {
                center += boids[i].transform.position;
            }
            FlockCenterOfMass = center / count;
        }
    }

    private void OnDrawGizmos()
    {
        // Desenha a esfera limite do mundo no editor
        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.15f);
        Gizmos.DrawWireSphere(transform.position, boundaryRadius);
    }
}
