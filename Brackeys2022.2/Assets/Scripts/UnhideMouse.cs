using UnityEngine;

public class UnhideMouse : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Cursor.visible = true;
    }

    private void OnEnable()
    {
        //Cursor.visible = true;
    }
}
