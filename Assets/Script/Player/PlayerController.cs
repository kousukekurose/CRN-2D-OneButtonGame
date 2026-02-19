using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<Player>();
    }

    public void Onjump(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            player.OnJump();
        }

    }
}
