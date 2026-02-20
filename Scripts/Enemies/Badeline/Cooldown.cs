using UnityEngine;

[System.Serializable]
public class Cooldown
{
    public float cooldownTime;
    public bool isReady = true;
    private float timer = 0f;

    public bool UpdateTimer()
    {
        if(isReady) 
        {
            return false;
        }

        timer += Time.deltaTime;
        if(timer >= cooldownTime)
        {
            isReady = true;
            timer = 0f;
            return true;
        }
        return false;
    }

    public void Trigger()
    {
        isReady = false;
        timer = 0f;
    }
}