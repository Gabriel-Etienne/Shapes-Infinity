using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputTest : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dashAction;

    private Vector2 movement;
    public Vector2 Movement => movement;

    public event Action OnDashPerformed;

    private void OnEnable()
    {
        moveAction.action.Enable();
        dashAction.action.Enable();

        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;

        dashAction.action.performed += OnDash;
        dashAction.action.canceled += OnDash;
    }

    private void OnDisable()
    {
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;

        dashAction.action.performed -= OnDash;
        dashAction.action.canceled -= OnDash;

        moveAction.action.Disable();
        dashAction.action.Disable();
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnDashPerformed?.Invoke();
            Debug.Log("Dash pressed !");
        }

        if (context.canceled)
        {
            Debug.Log("Dash released !");
        }
    }
}