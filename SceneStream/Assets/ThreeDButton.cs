using UnityEngine;
using UnityEngine.Events;

public class ThreeDButton : MonoBehaviour
{
    public float pressDepth = 0.2f; // Depth at which the button will trigger the action
    public UnityEvent onPress; // Action to trigger when the button is pressed

    private Vector3 initialPosition;
    private bool isPressed;

    private void Start()
    {
        initialPosition = transform.localPosition;
        isPressed = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            isPressed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            isPressed = false;
            ResetButton();
        }
    }

    private void Update()
    {
        if (isPressed)
        {
            Vector3 pressDirection = -transform.up; // Assuming the button presses down along the local Y axis
            float distance = Vector3.Distance(initialPosition, transform.localPosition);

            if (distance < pressDepth)
            {
                transform.localPosition += pressDirection * Time.deltaTime * 0.1f; // Move the button down
            }
            else
            {
                onPress.Invoke(); // Trigger the action
                isPressed = false;
            }
        }
    }

    private void ResetButton()
    {
        transform.localPosition = initialPosition;
    }
}