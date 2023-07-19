using UnityEngine;

public class BulletController : MonoBehaviour
{

    public float bulletDamage = 25f;

    void Start()
    {
    }


    public void DoDamage(Collision col)
    {
        col.gameObject.GetComponent<HealthScript>()?.SetDamage(bulletDamage);
    }
    
}
