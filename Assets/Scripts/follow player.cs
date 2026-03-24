using UnityEngine;

public class followplayer : MonoBehaviour
{
    public float X;
    public float Y;
    public float Z;
    public GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.transform.position + new Vector3(X, Y, Z); 
    }
}
