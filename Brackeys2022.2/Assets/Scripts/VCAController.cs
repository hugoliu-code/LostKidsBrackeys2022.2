using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VCAController : MonoBehaviour
{
    private FMOD.Studio.VCA VcaController;
    [SerializeField] private string VcaName;

    private Slider slider;

    private 

    // Start is called before the first frame update
    void Start()
    {
        VcaController = FMODUnity.RuntimeManager.GetVCA("vca:/" + VcaName);
        slider = GetComponent<Slider>();
    }

    public void SetVolume(float volume)
    {
        VcaController.setVolume(volume);
    }
}
