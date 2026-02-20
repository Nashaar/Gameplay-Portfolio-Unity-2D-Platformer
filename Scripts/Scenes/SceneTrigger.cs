using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneTriggerName;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(!collision.CompareTag("Player"))
        {
            return;
        }
        SaveSystem.saveInstance.CheckCheckpoints(true, sceneTriggerName);
        SaveSystem.saveInstance.resetCp = true;

        if(SceneController.sceneInstance == null)
        {
            Debug.LogWarning("SceneController instance not found!");
            return;
        }
        SceneController.sceneInstance.LoadScene(
            sceneTriggerName, 
            SceneController.SceneLoadReason.NextLevel
        );
    }
}
