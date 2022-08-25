using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractIcon : MonoBehaviour
{
    [SerializeField] GameObject icon;
    private Vector2 startPosition;
    [SerializeField] float floatDistance;
    [SerializeField] float floatTime;
    private bool floatUp = false;
    private float nextFloatTime = 0;
    private float startFloatTime = 0;
    
    public void ShowIcon()
    {
        icon.SetActive(true);
    }
    public void HideIcon()
    {
        icon.SetActive(false);
    }
    private void Start()
    {
        startPosition = icon.transform.position;
    }
    private void Update()
    {
        if (Time.time > nextFloatTime)
        {
            startFloatTime = Time.time;
            nextFloatTime = Time.time + floatTime;
            floatUp = !floatUp;
        }
        float lerpFloat = 0f;
        if (nextFloatTime - startFloatTime != 0)
            lerpFloat = (Time.time - startFloatTime) / (nextFloatTime - startFloatTime);


        if (floatUp)
        {
            icon.transform.position = startPosition + new Vector2(0, Mathf.SmoothStep(0,floatDistance,lerpFloat));
        }
        else
        {
            icon.transform.position = startPosition + new Vector2(0, Mathf.SmoothStep(floatDistance, 0, lerpFloat));
        }
    }

}
