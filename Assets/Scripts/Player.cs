using UnityEngine;
using UnityEngine.Rendering;


public class Player : MonoBehaviour, Danhable
{
    [SerializeField] private float velocidadMovimiento;
    [SerializeField] private float factorGravedad;
    [SerializeField] private float alturaDeSalto;
    [SerializeField] private Transform camara;
    [SerializeField] private InputManagerSO inputManager;
    [SerializeField] private Animator anim;
    [SerializeField] private ParticleSystem particles;

    [Header("Detección suelo")]
    [SerializeField] private Transform pies;
    [SerializeField] private float radioDeteccion;
    [SerializeField] private LayerMask queEsSuelo;
    private Enemigo enemigo;
    
    private CharacterController controller;

    private Vector3 direccionMovimiento;
    private Vector3 direccionInput;
    private Vector3 velocidadVertical;

   [Header ("Sistema de combate")]
   [SerializeField] private float vidas = 100f;
   [SerializeField] private float distanciaDisparo = 500;
   [SerializeField] private float danhoDisparo = 20;

    private AudioSource audioSource;


    private void OnEnable() //el player se suscribe a la llamada que ha hecho el input manager y crea su evento
    {
        inputManager.OnSaltar += Saltar;
        inputManager.OnMover += Mover;
        inputManager.OnDisparar += Disparar;
        inputManager.OnRecargar += Recargar;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        AplicarMovimiento();

        ActualizarMovimiento();

        ManejarVelocidadVertical();
    }

    private void Recargar()
    {
        anim.SetTrigger("reload");
    }

    private void Disparar()
    {
        anim.SetTrigger("shoot");
        audioSource.Play();
        particles.Play();
        //lo encapsulo dentro de un if porque me devuelve un bool
        //mira a ver si impactas en algo...
        if(Physics.Raycast(camara.position, camara.forward, out RaycastHit hitInfo, distanciaDisparo))//origen, dirección, distancia y qué hemos tocado (acepción 12)
        {
                //Saber si el gameObject con el que he colisionado tiene la interfaz buscada
                //mira a ver si ese algo es dañable
                if (hitInfo.transform.TryGetComponent (out Danhable sistemaDanho))
                {
                    if(!hitInfo.transform.CompareTag("Player"))
                    {
                        sistemaDanho.RecibirDahno(danhoDisparo);
                        Debug.Log("Estás dando");
                    }
                     
                   
                }
        }
                
            
        
    }

    //Solo cuando se actualice el input de movimiento
    private void Mover(Vector2 ctx)
    {
        direccionInput = new Vector3(ctx.x, 0, ctx.y);
        
    }

    private void Saltar()
    {
        if(EstoyEnSuelo())
        {
        //fórmula cinemática, tu valor en y es igual a la raiz cuadrada de -2 * la gravedad * la altura que alcances
        velocidadVertical.y = Mathf.Sqrt(-2 * factorGravedad * alturaDeSalto);
            //lo animo
            Debug.Log("Saltando");
        }
        
    }
        

    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

    private void AplicarMovimiento()
    {
        //Me muevo hacia el delante de la cámara según mi z y hacia el lado de la cámara según mi x (que puede ser + ó - según el imput)
        direccionMovimiento = camara.forward * direccionInput.z + camara.right * direccionInput.x;
        direccionMovimiento.y = 0;//porque esto es la gravedad
        controller.Move(direccionMovimiento * velocidadMovimiento * Time.deltaTime);//hacia donde * cuán rápido * el tiempo que dure el movimiento
    }

    private void ManejarVelocidadVertical()
    {
        //Si hemos aterrizado...
        if (EstoyEnSuelo() && velocidadVertical.y < 0)
        {
            //Entonces reseteo mi velocidad vertical
            velocidadVertical.y = 0;
        }
        AplicarGravedad();
    }

    private void ActualizarMovimiento()
    {
        if (direccionMovimiento.sqrMagnitude > 0)
        {
            //el bool va a depender de direccionMovimiento. si el vector del movimiento es mayor que cero me muevo, y por eso paso al estado true de moverme
            anim.SetBool("walking", true);
            RotarHaciaDestino();
        }
        else
        {
            anim.SetBool("walking", false);
        }
    }

    private void AplicarGravedad()
    {
        //A la velocidad vertical le sumo la gravedad multiplicada por el tiempo y eso se lo añado al controlador vertical del personaje por el tiempo también
        velocidadVertical.y += factorGravedad * Time.deltaTime;
        controller.Move(velocidadVertical * Time.deltaTime);
    }
    private bool EstoyEnSuelo()
    {
        return Physics.CheckSphere(pies.position, radioDeteccion, queEsSuelo);
    }
    
    private void RotarHaciaDestino()
    {
        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionMovimiento);
        transform.rotation = rotacionObjetivo;
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
        Gizmos.DrawSphere(pies.position, radioDeteccion); 
    }

}
