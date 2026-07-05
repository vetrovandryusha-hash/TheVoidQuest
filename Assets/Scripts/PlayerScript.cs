using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    private float verticalMoveRotation;
    [SerializeField] float speed = 5f;
    [SerializeField] private float mouseSensivity;
    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float currentStamina;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool canRun = true;
    [SerializeField] private bool freezeActive = false;
    [SerializeField] private Image staminaImage;
    [SerializeField] private Image healthImage;
    [SerializeField] private Image freezeImage;
    [SerializeField] private GameObject bloodObject;
    [SerializeField] private GameObject cameraP;
    [SerializeField] public bool drawOutline = false;
    public GameObject targetedObject;
    public GameObject objOther;
    public GameObject obj;
    Vector3 objPos;
    [Header("Lamp")]
    [SerializeField] private GameObject lamp;
    [SerializeField] private GameObject battery;
    [SerializeField] private GameObject lampLight;
    [SerializeField] private Image batteryImage;
    [SerializeField] private int countLamp = 0;
    [SerializeField] private bool isLampActive = false;
    [SerializeField] private bool canLampLight = true;
    [SerializeField] public float currentBattery;
    [SerializeField] public float maxBattery = 100f;
    [SerializeField] public int countBattery;
    [SerializeField] private Text countBatteryText;
    [SerializeField] private GameObject batteryItemFrame;
    [SerializeField] private GameObject lampTrigger;

    [Header("BlackScreen")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 5.0f; // Время затемнения в секундах

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        currentStamina = maxStamina;
        currentHealth = maxHealth;
        currentBattery = maxBattery;
        Time.timeScale = 1;
    }
    private void Update()
    {
        LookCamera();
        staminaChange();
        healthImage.fillAmount = currentHealth / maxHealth;
        lampActive();
        mouseRaycast();
        healthChecker();
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
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && freezeActive != true)
        {
            speed = 10f;
            currentStamina -= 0.15f;
        }
        if (currentStamina <= 0)
        {
            currentStamina += 0f;
            speed = 5f;
        }
        if(currentStamina < 100 && !Input.GetKey(KeyCode.LeftShift) && freezeActive != true)
        {
            speed = 5f;
            currentStamina += 0.05f;
        }
        if (freezeActive == true) 
        {
            speed = 3f;
        }
        staminaImage.fillAmount = currentStamina / maxStamina;
    }
    
    private void lampActive()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && countLamp % 2 == 0)
        {
            lamp.SetActive(true);
            battery.SetActive(true);
            batteryItemFrame.SetActive(true);
            isLampActive = true;
            
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            countLamp++;
        }
       
        if (Input.GetKeyDown(KeyCode.Alpha1) && countLamp % 2 > 0)
        {
            lamp.SetActive(false);
            battery.SetActive(false);
            batteryItemFrame.SetActive(false);
            isLampActive = false;
        }
        if (isLampActive)
        {
            currentBattery -= 0.02f;
            
        }
        if(currentBattery <= 0)
        {
            lampLight.SetActive(false);
        }
        else if(currentBattery > 0)
        {
            lampLight.SetActive(true);
        }
        if(countBattery > 0 && currentBattery <= 0)
        {
            currentBattery = 100f;
            countBattery--;
        }
        if(currentBattery > 0 && isLampActive)
        {
            lampTrigger.SetActive(true);
        }
        else
        {
            lampTrigger.SetActive(false);
        }
            batteryImage.fillAmount = currentBattery / maxBattery;
        countBatteryText.text = countBattery.ToString();
    }
    private void mouseRaycast()
    {
        Ray ray = cameraP.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit))
        {
            obj = hit.transform.gameObject;
            objOther = hit.transform.gameObject;
            objPos = hit.transform.position;
            if (obj.CompareTag("Battery"))
            {
                targetedObject = obj;
            }
            else 
            {
                targetedObject = objOther;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HandPush"))
        {
            currentHealth -= 40f;
            StartCoroutine("freezeTimer");
            StartCoroutine("bloodTimer");
        }
    }
    private void healthChecker()
    {
        if(currentHealth <= 0)
        {
            StartCoroutine(FadeRoutine(0f, 1f));
            
        }
    }
    IEnumerator freezeTimer()
    {
        freezeImage.gameObject.SetActive(true);
        freezeActive = true;
        yield return new WaitForSeconds(7f);
        freezeActive = false;
        freezeImage.gameObject.SetActive(false);
    }
    IEnumerator bloodTimer()
    {
        bloodObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        bloodObject.SetActive(false);
    }
    private IEnumerator FadeRoutine(float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Плавно интерполируем альфа-канал между начальным и конечным значением
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);

            color.a = newAlpha;
            fadeImage.color = color;

            //yield return new WaitForSeconds(0.02f);
            yield return null;
        }

        // Финальная фиксация точного значения альфа-канала
        color.a = targetAlpha;
        fadeImage.color = color;
        SceneManager.LoadScene(0);
    }
}
