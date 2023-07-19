using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour
{
    public float health = 100f;

    public float GetHealth()
    {
        return health;
    }

    public void SetDamage(float damage)
    {
        health -= damage;
        Debug.Log(gameObject.name + " " + damage.ToString());
        if (health <= 0) Destroy(gameObject);
    }
}
