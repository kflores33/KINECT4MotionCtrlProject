using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public void onClicked()
    {
        SceneManager.LoadScene("MainScene");
    }
}