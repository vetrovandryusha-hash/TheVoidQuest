using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    // Start is called before the first frame update
    private Rigidbody rb;
    [SerializeField] float speed = 5f;
    [SerializeField] private float mouseSensivity;
    [SerializeField] private GameObject cameraHolder;
    private float verticalMoveRotation;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.visible = false;
    }
    private void Update()
    {
        LookCamera();
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
}
