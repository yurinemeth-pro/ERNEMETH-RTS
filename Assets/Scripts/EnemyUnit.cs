using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    private float nextDecisionTime;
    private Vector2 erraticOffset;
    
    [Header("Patrulha")]
    public float patrolRadius = 3f;

    [Header("Estímulo (por enquanto, um alvo fixo)")]
    public Transform stimulus; // depois vira o campo de feromônio; por ora, um ponto fixo

    [Header("Movimento")]
    public float maxSpeed = 3f;
    public float attractionStrength = 1f; // quão forte é puxado pelo estímulo

    [Header("Caos individual")]
    public float jitterStrength = 0.5f;
    public float chaosResistance;

    private Vector2 velocity;
    private float spasmTimer;

    void Start()
    {
        nextDecisionTime = Time.time + Random.Range(0.2f, 1f);
        chaosResistance = Random.Range(0.8f, 1.2f);
        spasmTimer = Random.Range(1f, 3f);
    }

    void Update()
    {
        if (stimulus == null) return;

        Vector2 toStimulus = (Vector2)stimulus.position - (Vector2)transform.position;
        float distance = toStimulus.magnitude;

        Vector2 desiredDirection;

        if (distance > patrolRadius)
        {
            // Longe: vai direto na direção do estímulo
            desiredDirection = toStimulus.normalized;
        }
        else
        {
            // Perto: mira num ponto tangente (perpendicular à linha até o estímulo)
            // Isso faz a unidade "circular" em vez de tentar pousar no centro
            Vector2 tangent = new Vector2(-toStimulus.y, toStimulus.x).normalized;
            desiredDirection = tangent;
        }

        // Ruído orgânico (igual antes)
        Vector2 wander = erraticOffset * jitterStrength;

        // Decisão errática: em intervalos IRREGULARES (não periódicos), muda bruscamente
        if (Time.time >= nextDecisionTime)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            erraticOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            nextDecisionTime = Time.time + Random.Range(0.15f, 0.8f); // próximo giro em tempo aleatório
        }

        Vector2 finalDirection = (desiredDirection + wander).normalized;
        Vector2 desiredVelocity = finalDirection * maxSpeed;

        velocity = Vector2.Lerp(velocity, desiredVelocity, Time.deltaTime * 6f);
        transform.position += (Vector3)(velocity * Time.deltaTime);
    }
}