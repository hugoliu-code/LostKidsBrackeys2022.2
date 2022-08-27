using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnLoad : MonoBehaviour
{
    [SerializeField] private string title;
    
    void Start()
    {
        Destroy(GameObject.Find(title));
    }
}
