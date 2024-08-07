using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

//TP2 Joaquin Lopez
public class Player : MonoBehaviour, IDamageable
{
    [SerializeField] private int life;
    [SerializeField] private float _speed;
    [SerializeField] private float _jumpForce;
    private Rigidbody _rB;
    private MoveController _moveController;
    [SerializeField] public bool isGrounded;
    private bool isOnTrapPlatform;


    public int speedAssaultGoat;
    public int assaultDamage; // Da�o de la embestida
    private bool isAssaulting;

    private bool isCharging = false;
    private float chargingSpeed = 10.0f; // Velocidad m�xima que se puede cargar
    private float chargeTime = 2.0f; // Tiempo m�ximo de carga en segundos
    private float currentChargeTime = 0.0f; // Tiempo actual de carga
    private float shootForceMultiplier = 50.0f; // Multiplicador de la fuerza de disparo

    public GameObject shootBall;
    [SerializeField] Transform shootPoint;

    public ShootPlayer shootPlayer; //Variable de referencia del script de la bala. 

    public event EventHandler OnJump; //Evento de la plataforma trampa.
                                      //
    public GameplayCanvasManager gamePlayCanvas;
    private GameManager _gameManager;



    private void Awake()
    {
        _rB = GetComponent<Rigidbody>();
        _gameManager = FindAnyObjectByType<GameManager>();

        if (_gameManager == null)
        {
            Debug.LogError("GameManager no encontrado");
        }
    }

    void Start()
    {
        _moveController = new MoveController(transform, _speed, _jumpForce, _rB);

        
    }

    void Update()
    {
        if (!isCharging)
        {
            Walk();
        }

        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isOnTrapPlatform))
        {
            OnJump?.Invoke(this, EventArgs.Empty);
            Jump();
        }
        assaultGoat();
        shootRock();

        // Actualizar el cooldown de disparo
        if (currentShootCooldown > 0)
        {
            currentShootCooldown -= Time.deltaTime;
        }
    }

    private void Walk()
    {
        Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _moveController.Move(direction * _speed);
    }

    private void Jump()
    {
        _moveController.Jump(isGrounded || isOnTrapPlatform);
        isGrounded = false;
        isOnTrapPlatform = false;
    }

    public void ReciveLife(int value)
    {
        life += value;
        if (life > 10) life = 10;
    }

    public void TakeDamage(int value)
    {
        life -= value;
        _gameManager.LoseHP(value);
        
        if (life <= 0)
        {
            life = 0;
            Dead();
        }
    }


    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Entity enemy = collision.gameObject.GetComponent<Entity>();
            if (enemy != null && isAssaulting)
            {
                // Hacer da�o al enemigo
                enemy.TakeDamage(assaultDamage);
                isAssaulting = false; // Terminar la embestida
            }
            else
            {
                TakeDamage(collision.gameObject.GetComponent<Golem>().damageAttack); //Da�o de colisi�n chocando con el golem
            }
        }

        if (collision.gameObject.CompareTag("Suelo"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("TrapPlatform"))
        {
            isOnTrapPlatform = true;
        }

        if (collision.gameObject.CompareTag("Food"))
        {
            collision.gameObject.GetComponent<CollectableObject>().Collect();
        }

    }

    

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            isGrounded = false;
        }
        if (collision.gameObject.CompareTag("TrapPlatform"))
        {
            isOnTrapPlatform = false;
        }
    }
    private void Dead()
    {
        Time.timeScale = 0;
        gamePlayCanvas.onLose();
        
        Destroy(GetComponent<Player>(), 1);
    }

    //Augusto Cayo
    public void assaultGoat()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            isCharging = true;
            // Detener el movimiento del jugador al cargar la embestida
            _moveController.Move(Vector3.zero);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if (currentChargeTime < chargeTime)
            {
                currentChargeTime += Time.deltaTime;
            }
            else
            {
                currentChargeTime = chargeTime; // No permitir que el tiempo de carga exceda chargeTime
            }
        }
        else if (Input.GetKeyUp(KeyCode.Q))
        {
            if (isCharging)
            {             
                // Calcular la fuerza de disparo basada en el tiempo de carga
                float shootForce = currentChargeTime / chargeTime * chargingSpeed * shootForceMultiplier;

                // Aplicar fuerza hacia adelante al Rigidbody para disparar al jugador
                _rB.AddForce(transform.forward * shootForce, ForceMode.Impulse);

                // Reiniciar variables de carga
                isCharging = false;
                isAssaulting = true; // Indicar que la embestida est� en curso
                currentChargeTime = 0.0f;
            }
        }

    }


    private bool isShootingCharging = false;


    [SerializeField] private float shootCooldown = 0.5f; // Tiempo m�nimo entre cada disparo
    private float currentShootCooldown = 0.0f; // Tiempo restante antes del pr�ximo disparo

    //Augusto Cayo
    public void shootRock()
    {
        if (Input.GetMouseButtonDown(0) && currentShootCooldown <= 0)
        {
            // Iniciar la carga del disparo
            isShootingCharging = true;
            currentChargeTime = 0.0f;
        }

        if (Input.GetMouseButton(0) && isShootingCharging)
        {
            // Continuar cargando el disparo
            currentChargeTime += Time.deltaTime;
            if (currentChargeTime > chargeTime)
            {
                currentChargeTime = chargeTime; // Limitar el tiempo de carga
            }

        }

        if (Input.GetMouseButtonUp(0) && isShootingCharging)
        {
            // Calcular la fuerza del disparo
            float shootForce = (currentChargeTime / chargeTime) * shootForceMultiplier;

            // Instanciar la bala
            GameObject bullet = Instantiate(shootBall, shootPoint.position, shootPoint.rotation);
            Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
            bulletRb.velocity = shootPoint.forward * shootForce;

            ShootPlayer bulletScript = bullet.GetComponent<ShootPlayer>();
            if (currentChargeTime >= chargeTime)
            {
                bulletScript.damage *= 3; // Doble da�o si est� completamente cargado
            }

            // Reiniciar variables de carga
            isShootingCharging = false;
            currentChargeTime = 0.0f;

            // Aplicar el cooldown de disparo
            currentShootCooldown = shootCooldown;
        }

    }

}
