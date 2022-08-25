using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathAnimationScript : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject DeathScreen;
    public void ActivateDeathScreen()
    {
        DeathScreen.SetActive(true);
    }
}
