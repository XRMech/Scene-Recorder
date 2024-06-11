// using UnityEngine;
// using UnityEngine.InputSystem;
// using UnityEngine.XR.Interaction.Toolkit;
//
// public class CharacterMovement : MonoBehaviour
// {
//     public float speed = 2.0f;
//     private CharacterController characterController;
//     [SerializeField] private InputAction inputActions;
//
//     private void Awake()
//     {
//         characterController = GetComponent<CharacterController>();
//         
//     }
//
//     private void OnEnable()
//     {
//         inputActions.XRLeftHand.Enable();
//         inputActions.XRRightHand.Enable();
//     }
//
//     private void OnDisable()
//     {
//         inputActions.XRLeftHand.Disable();
//         inputActions.XRRightHand.Disable();
//     }
//
//     private void Update()
//     {
//         // Get thumbstick input
//         Vector2 leftThumbstick = inputActions.XRLeftHand.Thumbstick.ReadValue<Vector2>();
//         Vector2 rightThumbstick = inputActions.XRRightHand.Thumbstick.ReadValue<Vector2>();
//
//         // Use left thumbstick for movement
//         Vector3 direction = new Vector3(leftThumbstick.x, 0, leftThumbstick.y);
//         direction = Camera.main.transform.TransformDirection(direction);
//         direction.y = 0;  // Ignore vertical movement
//         characterController.Move(direction * speed * Time.deltaTime);
//
//         // Use right thumbstick for rotation
//         if (rightThumbstick.sqrMagnitude > 0.01f)
//         {
//             float rotationSpeed = 100.0f;
//             Vector3 rotation = new Vector3(0, rightThumbstick.x, 0);
//             transform.Rotate(rotation * rotationSpeed * Time.deltaTime);
//         }
//     }
// }