using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Boid : MonoBehaviour
{
    private Rigidbody rb;
    private FlockManager manager;
    private Vector3 velocity;

    // Buffers reutilizados a cada frame para evitar qualquer alocação no GC
    private readonly Collider[] neighborColliders = new Collider[128];
    private readonly Boid[] gridNeighborBuffer = new Boid[128];

    private int boidLayerMask;

    public bool IsGizmoLeader { get; set; }
    public Vector3 Velocity => velocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        int boidLayer = LayerMask.NameToLayer("Boid");
        if (boidLayer >= 0)
        {
            boidLayerMask = 1 << boidLayer;
        }
        else
        {
            boidLayerMask = ~0;
        }
    }

    private void Start()
    {
        manager = FlockManager.Instance;

        float initSpeed = manager != null ? (manager.minSpeed + manager.maxSpeed) * 0.5f : 5.0f;
        velocity = transform.forward * initSpeed;
        if (velocity.sqrMagnitude < 0.001f)
        {
            velocity = Random.insideUnitSphere.normalized * initSpeed;
        }

        ApplyVelocityToRigidbody();
    }

    private void Update()
    {
        if (manager == null)
        {
            manager = FlockManager.Instance;
            if (manager == null) return;
        }

        Vector3 force = ComputeForce();
        ApplyPhysicsSafety(force);
    }

    // Calcula a soma de todas as forças que regem o comportamento do boid
    public Vector3 ComputeForce()
    {
        Vector3 totalForce = Vector3.zero;

        // Regras de bando (Separação, Alinhamento e Coesão)
        if (manager.useSpatialGrid && manager.Grid != null)
        {
            totalForce += ComputeFlockForcesSpatialGrid();
        }
        else
        {
            totalForce += ComputeFlockForcesPhysics();
        }

        // Desvio de obstáculos com SphereCast e Vector3.Reflect
        totalForce += ComputeObstacleAvoidance();

        // Força de retorno para manter os boids dentro do raio central do mundo
        totalForce += ComputeBoundaryForce();

        // Fuga do predador com peso elevado
        totalForce += ComputePredatorEvade();

        return totalForce;
    }

    // Cálculo das três regras de Reynolds usando Physics.OverlapSphereNonAlloc
    private Vector3 ComputeFlockForcesPhysics()
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            manager.perceptionRadius,
            neighborColliders,
            boidLayerMask
        );

        Vector3 separationForce = Vector3.zero;
        Vector3 alignmentSum = Vector3.zero;
        Vector3 centerOfMassSum = Vector3.zero;
        int validNeighbors = 0;

        float separationDistSqr = manager.separationDistance * manager.separationDistance;
        Vector3 currentPos = transform.position;

        for (int i = 0; i < count; i++)
        {
            Collider col = neighborColliders[i];
            if (col == null || col.gameObject == gameObject) continue;

            Boid neighbor = col.GetComponent<Boid>();
            if (neighbor == null) continue;

            Vector3 diff = currentPos - neighbor.transform.position;
            float distSqr = diff.sqrMagnitude;

            if (distSqr < 0.0001f) continue;

            float dist = Mathf.Sqrt(distSqr);

            // Separação proporcional a 1/d² (quanto mais próximo, mais forte o empurrão)
            if (distSqr < separationDistSqr)
            {
                float safeDist = Mathf.Max(dist, 0.05f);
                Vector3 pushDir = diff / dist;
                float intensity = 1.0f / (safeDist * safeDist);
                separationForce += pushDir * intensity;
            }

            // Alinhamento (soma das velocidades para calcular a média)
            alignmentSum += neighbor.Velocity;

            // Coesão (soma das posições para achar o centro de massa)
            centerOfMassSum += neighbor.transform.position;

            validNeighbors++;
        }

        Vector3 combinedForce = Vector3.zero;

        if (validNeighbors > 0)
        {
            // Separação ponderada
            combinedForce += separationForce * manager.separationWeight;

            // Alinhamento ponderado em direção à velocidade média dos vizinhos
            Vector3 avgVelocity = alignmentSum / validNeighbors;
            Vector3 alignmentForce = avgVelocity - velocity;
            combinedForce += alignmentForce * manager.alignmentWeight;

            // Coesão ponderada em direção ao centro de massa local
            Vector3 centerOfMass = centerOfMassSum / validNeighbors;
            Vector3 cohesionDir = centerOfMass - currentPos;
            if (cohesionDir.sqrMagnitude > 0.0001f)
            {
                Vector3 desiredVelocity = cohesionDir.normalized * manager.maxSpeed;
                Vector3 cohesionForce = desiredVelocity - velocity;
                combinedForce += cohesionForce * manager.cohesionWeight;
            }
        }

        return combinedForce;
    }

    // Cálculo das três regras usando Grid Espacial (Desafio Opcional para 500+ boids)
    private Vector3 ComputeFlockForcesSpatialGrid()
    {
        int count = manager.Grid.GetNeighborsNonAlloc(
            transform.position,
            manager.perceptionRadius,
            gridNeighborBuffer
        );

        Vector3 separationForce = Vector3.zero;
        Vector3 alignmentSum = Vector3.zero;
        Vector3 centerOfMassSum = Vector3.zero;
        int validNeighbors = 0;

        float separationDistSqr = manager.separationDistance * manager.separationDistance;
        Vector3 currentPos = transform.position;

        for (int i = 0; i < count; i++)
        {
            Boid neighbor = gridNeighborBuffer[i];
            if (neighbor == null || neighbor == this) continue;

            Vector3 diff = currentPos - neighbor.transform.position;
            float distSqr = diff.sqrMagnitude;
            if (distSqr < 0.0001f) continue;

            float dist = Mathf.Sqrt(distSqr);

            if (distSqr < separationDistSqr)
            {
                float safeDist = Mathf.Max(dist, 0.05f);
                Vector3 pushDir = diff / dist;
                float intensity = 1.0f / (safeDist * safeDist);
                separationForce += pushDir * intensity;
            }

            alignmentSum += neighbor.Velocity;
            centerOfMassSum += neighbor.transform.position;
            validNeighbors++;
        }

        Vector3 combinedForce = Vector3.zero;
        if (validNeighbors > 0)
        {
            combinedForce += separationForce * manager.separationWeight;

            Vector3 avgVelocity = alignmentSum / validNeighbors;
            Vector3 alignmentForce = avgVelocity - velocity;
            combinedForce += alignmentForce * manager.alignmentWeight;

            Vector3 centerOfMass = centerOfMassSum / validNeighbors;
            Vector3 cohesionDir = centerOfMass - currentPos;
            if (cohesionDir.sqrMagnitude > 0.0001f)
            {
                Vector3 desiredVelocity = cohesionDir.normalized * manager.maxSpeed;
                Vector3 cohesionForce = desiredVelocity - velocity;
                combinedForce += cohesionForce * manager.cohesionWeight;
            }
        }

        return combinedForce;
    }

    // Desvio de obstáculos usando SphereCast e Vector3.Reflect
    private Vector3 ComputeObstacleAvoidance()
    {
        if (velocity.sqrMagnitude < 0.001f) return Vector3.zero;

        Ray ray = new Ray(transform.position, velocity.normalized);
        if (Physics.SphereCast(ray, manager.obstacleProbeRadius, out RaycastHit hit, manager.obstacleProbeDistance, manager.obstacleLayer))
        {
            Vector3 reflectDir = Vector3.Reflect(velocity.normalized, hit.normal);
            Vector3 avoidDir = (reflectDir + hit.normal).normalized;

            float distanceRatio = 1.0f - Mathf.Clamp01(hit.distance / manager.obstacleProbeDistance);
            return avoidDir * (distanceRatio * manager.obstacleAvoidanceWeight * manager.maxSpeed);
        }

        return Vector3.zero;
    }

    // Limites do mundo: zero força dentro do raio, força de retorno progressiva fora dele
    private Vector3 ComputeBoundaryForce()
    {
        Vector3 center = manager.transform.position;
        Vector3 offset = transform.position - center;
        float distFromCenter = offset.magnitude;

        if (distFromCenter > manager.boundaryRadius)
        {
            float excess = distFromCenter - manager.boundaryRadius;
            Vector3 returnDir = -offset / distFromCenter;
            return returnDir * (excess * manager.boundaryWeight);
        }

        return Vector3.zero;
    }

    // Fuga do predador quando ele se aproxima do raio de pânico
    private Vector3 ComputePredatorEvade()
    {
        if (manager.predator == null) return Vector3.zero;

        Vector3 toPredator = manager.predator.position - transform.position;
        float distToPredator = toPredator.magnitude;

        if (distToPredator < manager.predatorPanicRadius && distToPredator > 0.001f)
        {
            Vector3 fleeDir = -toPredator / distToPredator;
            float panicFactor = 1.0f - (distToPredator / manager.predatorPanicRadius);
            return fleeDir * (panicFactor * manager.predatorEvadeWeight * manager.maxSpeed);
        }

        return Vector3.zero;
    }

    // Aplica limites de segurança numérica: maxForce, maxSpeed, LookRotation e proteção anti-NaN
    private void ApplyPhysicsSafety(Vector3 force)
    {
        // Proteção contra NaN na força calculada
        if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsNaN(force.z))
        {
            force = Vector3.zero;
        }

        // Limita a força final por maxForce
        force = Vector3.ClampMagnitude(force, manager.maxForce);

        // Integração da aceleração na velocidade
        velocity += force * Time.deltaTime;

        // Proteção contra NaN na velocidade
        if (float.IsNaN(velocity.x) || float.IsNaN(velocity.y) || float.IsNaN(velocity.z))
        {
            velocity = transform.forward * manager.minSpeed;
        }

        // Limita a velocidade máxima por maxSpeed
        velocity = Vector3.ClampMagnitude(velocity, manager.maxSpeed);

        // Mantém uma velocidade mínima para evitar boids paralisados
        if (velocity.sqrMagnitude < manager.minSpeed * manager.minSpeed)
        {
            velocity = (velocity.sqrMagnitude > 0.0001f ? velocity.normalized : transform.forward) * manager.minSpeed;
        }

        ApplyVelocityToRigidbody();

        // Usa LookRotation apenas quando a velocidade for maior que 0.01
        if (velocity.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * manager.rotationSpeed);
        }
    }

    private void ApplyVelocityToRigidbody()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = velocity;
#else
        rb.velocity = velocity;
#endif
    }

    private void OnDrawGizmos()
    {
        // Desenha a área de percepção do boid para visualização durante o Play Mode
        if (IsGizmoLeader || (FlockManager.Instance != null && FlockManager.Instance.drawAllGizmos))
        {
            float radius = FlockManager.Instance != null ? FlockManager.Instance.perceptionRadius : 2.5f;
            Gizmos.color = new Color(0.2f, 0.8f, 1.0f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, radius);

            float sepRadius = FlockManager.Instance != null ? FlockManager.Instance.separationDistance : 1.2f;
            Gizmos.color = new Color(1.0f, 0.4f, 0.2f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, sepRadius);
        }
    }

    private void OnDrawGizmosSelected()
    {
        float radius = FlockManager.Instance != null ? FlockManager.Instance.perceptionRadius : 2.5f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
