using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneHandler : MonoBehaviour

{

    public void StartBlend()
    {
       print("Clicked");
       SceneManager.LoadScene("Default"); 
    }

    public void StartBunch()
    {
      SceneManager.LoadScene("Farm");
    }

    public void CloseGame()
    {
         Application.Quit(); 
    }


}