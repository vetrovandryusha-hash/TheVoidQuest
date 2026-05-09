using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    [SerializeField] float speed = 5f;
    [SerializeField] private float mouseSensivity;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float currentStamina;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool canRun = true;
    [SerializeField] private Image staminaImage;
    [SerializeField] private Image healthImage;
    [SerializeField] private Image freezeImage;
    [SerializeField] private GameObject bloodObject;
    private float verticalMoveRotation;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        currentStamina = maxStamina;
        currentHealth = maxHealth;
    }
    private void Update()
    {
        LookCamera();
        staminaChange();
        healthImage.fillAmount = currentHealth / maxHealth;

    }
    // Update is called once per frame
    void FixedUpdate()
    {
       
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveInput = transform.right * x + transform.forward * z;
        rb.velocity = new Vector3(moveInput.x * speed, rb.velocity.y, moveInput.z * speed);
    }
    private void LookCamera()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensivity);
        verticalMoveRotation += Input.GetAxisRaw("Mouse Y") * mouseSensivity;
        verticalMoveRotation = Mathf.Clamp(verticalMoveRotation, -75f, 75f);
        cameraHolder.transform.localEulerAngles = Vector3.left * verticalMoveRotation;
    }
    private void staminaChange()
    {
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0)
        {
            speed = 10f;
            currentStamina -= 0.15f;
        }
        if (currentStamina <= 0)
        {
            currentStamina += 0f;
            speed = 5f;
        }
        if(currentStamina < 100 && !Input.GetKey(KeyCode.LeftShift))
        {
            speed = 5f;
            currentStamina += 0.05f;
        }
        
        staminaImage.fillAmount = currentStamina / maxStamina;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HandPush")) {
            currentHealth -= 40f;
            StartCoroutine("freezeTimer");
            StartCoroutine("bloodTimer");
        }
    }
    IEnumerator freezeTimer()
    {
        freezeImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(7f);
        freezeImage.gameObject.SetActive(false);
    }
    IEnumerator bloodTimer()
    {
        bloodObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        bloodObject.SetActive(false);
    }
}
