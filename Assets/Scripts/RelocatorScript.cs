using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelocatorScript : MonoBehaviour
{
    public Vector3 relocationPosition;

    private void OnTriggerEnter(Collider other)
    {
        var pc = other.GetComponent<PlayerController>();
        if (pc != null)
        {
            StartCoroutine(LocationResetter(pc));
        }
        else
        {
            other.transform.position = relocationPosition;
        }
    }

    IEnumerator LocationResetter(PlayerController pc)
    {
        pc.enabled = false;
        pc.transform.position = relocationPosition;
        yield return new WaitForSeconds(0.01f);
        pc.enabled = true;
    }
}
