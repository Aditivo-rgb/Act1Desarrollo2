using UnityEngine;
using UnityEngine.AI;

public class Enemigo : MonoBehaviour, Danhable
{
    private NavMeshAgent agent;
    private Player target;//mi target tiene el script player
    private Animator anim;
    private float vidas = 100;

    [Header("Sistema de combate")]
    [SerializeField] private Transform puntoAtaque;
    [SerializeField] private float radioAtaque;
    [SerializeField] private float danhoAtaque = 20;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        target = FindObjectOfType<Player>();//le pido que cuando inicie localice cual es el objeto con player
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(target.transform.position);//encuentra el sitio donde está el player aunque se mueva
        
        //Distancia de ataque
        if(agent.remainingDistance <= agent.stoppingDistance)
        {
            EnfocarObjetivo();
            LanzarAtaque();
        }
    }

    private void LanzarAtaque()
    {
        agent.isStopped = true;
        anim.SetBool("attacking", true);
    }

    private void Atacar()
    {
        //Lanzo el overlap desde un punto de ataque y  bajo un radio de ataque y recojo todos los collider tocados
        Collider[] coliderTocados = Physics.OverlapSphere(puntoAtaque.position, radioAtaque);
        foreach (Collider coll in  coliderTocados)
        {
            if(coll.TryGetComponent(out Danhable danhable))
            {
                danhable.RecibirDahno(danhoAtaque);
            }
        }
    }

    private void EnfocarObjetivo()
    {
        //Si tienes un objetivo B y quieres sacar la dirección desde el punto A siempre restas destino - origen y normalizas
        Vector3 direccionAObjetivo = (target.transform.position - transform.position).normalized;
        //para que la direccion a la que gira no le haga tumbarse pongo a 0 el vector en y
        direccionAObjetivo.y = 0;
        //Con esa direccion saco el ángulo al que el personaje tiene que girarse
        Quaternion rotacionAObjetivo = Quaternion.LookRotation(direccionAObjetivo);
        transform.rotation = rotacionAObjetivo;
    }

    private void FinDeAtaque()
    {
            agent.isStopped = false;
            anim.SetBool("attacking", false);
    }

    public void RecibirDahno(float danho)
    {
        vidas -= danho;
        if (vidas <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(puntoAtaque.position, radioAtaque);
    }
}
