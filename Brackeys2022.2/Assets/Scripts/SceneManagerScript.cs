using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneManagerScript : MonoBehaviour
{
    public Animator transition;

    //private static FMOD.Studio.EventInstance Music;

    //[SerializeField] private AudioSource click;

    public void LevelNav(string levelName)
    {
        //Time.timeScale = 1f;
        //if (soundOn)
        //click.Play();
        StartCoroutine(LoadLevel(levelName));
    }
    public void SceneNav(string levelName)
    {
        //Time.timeScale = 1f;
        //if (soundOn)
        //click.Play();
        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/Confirm");
        StartCoroutine(LoadLevel(levelName));
    }
    public void FirstLevel()
    {
        //FMODUnity.RuntimeManager.PlayOneShot("event:/UI/Confirm");
        StartCoroutine(LoadLevel("Level1"));
    }

    void Start()
    {
        //Music = FMODUnity.RuntimeManager.CreateInstance("event:/OST/Music");
        //Music.start();
        //Music.release();
    }

    public void SettingSelect()
    {
        //Progress("Misc");
        StartCoroutine(LoadLevel("Settings"));
    }

    public void MainMenuSelect()
    {
        //Progress("Misc");
        StartCoroutine(LoadLevel("StartMenu"));
    }

    public void Quit()
    {
        Application.Quit();
    }

    IEnumerator LoadLevel(string LevelName)
    {
        transition.SetBool("Start", true);
        yield return new WaitForSeconds(0.2f);
        FMODUnity.RuntimeManager.PlayOneShot("event:/UI/Scene_Transition");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(LevelName);
    }

    public void Progress(string LevelName)
    {
        //Music.setParameterByNameWithLabel("Level Type", LevelName);
    }

    private void OnDestroy()
    {
        //Music.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
