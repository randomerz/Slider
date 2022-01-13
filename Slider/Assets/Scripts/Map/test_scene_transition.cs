using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class test_scene_transition : MonoBehaviour
{
    public string sceneName;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.P)) {
            SGrid.current.SaveGrid();
            SceneManager.LoadScene(sceneName);
        }
    }
}
