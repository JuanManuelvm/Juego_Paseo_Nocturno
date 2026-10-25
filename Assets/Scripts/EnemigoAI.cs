using UnityEngine;
using UnityEngine.AI;

public class EnemigoAI : MonoBehaviour
{
    public Transform jugador;
    private NavMeshAgent agente;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Solo actualiza la ruta si el jugador existe y hay NavMesh horneado
        if (jugador != null && agente.isOnNavMesh)
        {
            agente.SetDestination(jugador.position);
        }
    }
}