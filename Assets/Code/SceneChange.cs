using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneChange : MonoBehaviour
{
    AsyncOperation operation;
    public Slider slider;
    public GameObject sceneThing;
    // Start is called before the first frame update
    public void handlePlay(int scene)
    {
        operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene);
        sceneThing.SetActive(true);
        StartCoroutine(DoThing(scene));

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private IEnumerator DoThing(int scene)
    {
        while (!operation.isDone)
        {
            slider.value = operation.progress;
            yield return null;
        }
    }
}
