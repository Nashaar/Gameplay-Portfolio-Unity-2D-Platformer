using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public Transform layer;
    [Range(0f, 1f)] public float parallaxX = 0.5f;
    [Range(0f, 1f)] public float parallaxY = 0.1f;

    [HideInInspector] public Vector3 startPos;
}

public class BackgroundController : MonoBehaviour
{
    public float parallaxX = 0.5f;
    public float parallaxY = 0.1f;

    [SerializeField] private Transform player;
    [SerializeField] private ParallaxLayer[] layers;
    
    private Vector3 startPlayerPos;
    private bool initialized = false;

    void Awake()
    {
        if(SaveSystem.saveInstance != null)
        {
            SaveSystem.saveInstance.backgroundController = this;
        }
    }
    void Start()
    {
        if(player == null)
        {
            return;
        }

        startPlayerPos = player.position;

        foreach (var layer in layers)
        {
            if(layer.layer != null)
                layer.startPos = layer.layer.position;
        }
    }

    // LateUpdate is called once per frame
    void LateUpdate()
    {
        if(!initialized || player == null)
        {
            return;
        }

        Vector3 playerDelta = player.position - startPlayerPos;

        foreach (var layer in layers)
        {
            if(layer.layer == null)
            {
                continue;
            }

            layer.layer.position = new Vector3(
                layer.startPos.x + playerDelta.x * layer.parallaxX,
                layer.startPos.y + playerDelta.y * layer.parallaxY,
                layer.startPos.z
            );
        }
    }

    public void Recenter()
    {
        if(player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if(player == null)
            {
                return;
            }
        }

        startPlayerPos = player.position;

        foreach (var layer in layers)
        {
            if(layer.layer != null)
            {
                layer.startPos = layer.layer.position;
            }
        }

        initialized = true;
    }
}
