using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour
{
    public void SceneChange(int index)
    {
        SceneManager.LoadScene(index);
    }
    public void SceneChange(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}