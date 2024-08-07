using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TP2 Joaquin Lopez
public abstract class Entity : MonoBehaviour, IDamageable
{
    public int life;
    public int damageAttack;

    public int visionRange; //Rango en el cual la entidad mirar� al jugador
    public int actionRange;
    public float rotationSpeed; //Velocidad de rotaci�n de la entidad

    public Vector3 directionTarget; //Distancia que ve al Player
    public Transform Player; //Para saber donde est� el Player siempre
    //Para tener el transform de la entidad, basta con colocar transfrom.position.

    public LayerMask detectableLayers;

    protected abstract void Attack();
    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        bool playerInRange = Vector3.Distance(transform.position, Player.position) < visionRange; //Distancia entre el enemigo y el jugador. 
        
        if(playerInRange == true)
        {
            print("Jugador dentro del rango");
            Vector3 directionToPlayer = (Player.position-transform.position).normalized; //Calcula la direcci�n. 
            directionToPlayer.y = 0;

            Debug.DrawRay(transform.position, directionToPlayer * visionRange, Color.red); 

            if(Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit,visionRange,detectableLayers)) //Rayito real si ve o no al player.
            {
                if (hit.transform.CompareTag("Player"))
                {
                    print("Te veo");
                    LookPlayer(); 
                }
                else
                {
                    print("No te veo");
                }
            }

        }
        else
        {
            print("Jugador fuera del rango");
        }

                
    }

    public virtual void LookPlayer()
    {
        Vector3 directionToPlayer = (Player.position - transform.position).normalized; //Calcula la direcci�n. 
        directionToPlayer.y = 0;

        Quaternion desiredRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime); //Girar al jugador.

        Debug.DrawRay(transform.position, transform.forward * actionRange, Color.blue); //Esta vez, al entrar en el rango de acci�n, se puede ejecutar tal cosa, ej, disparar.

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, actionRange, detectableLayers)) //Rayito real si ve o no al player.
        {
            if (hit.transform.CompareTag("Player"))
            {
                print("Te veo");
            }
            else
            {
                print("No te veo");
            }
        }

    }

    public void TakeDamage(int damage)
    {

        life -= damage;

        if (life <= 1.5f)
        {
            Death();
        }
    }

    public virtual void Death()
    {
        Destroy(gameObject);
    }

}
