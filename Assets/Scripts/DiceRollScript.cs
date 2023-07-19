using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using DG.Tweening;

public class DiceRollScript : MonoBehaviour
{
    public static DiceRollScript instance;

    public Transform diceTransform;
    public DiceData[] diceData;

    public Side[] sides;

    public List<Side> currentSides = new List<Side>();
    public AudioSource rollSFX;
    public AudioSource rollDoneSFX;

    private void Awake()
    {
        instance = this;
    }

    public void InitializeDice()
    {
        if (!diceTransform) return;
        currentSides.Clear();
        var meshRenderer = diceTransform.GetComponent<MeshRenderer>();
        var sidesList = sides.ToList();
        Material[] currentlyAssignedMaterials = meshRenderer.materials;
        for (int i = 0; i < diceData.Length; i++)
        {
            var sideIndex = Random.Range(0, sidesList.Count);
            currentSides.Add(sidesList[sideIndex]);
            currentlyAssignedMaterials[diceData[i].index] = sidesList[sideIndex].material;
            sidesList.RemoveAt(sideIndex);
        }
        meshRenderer.materials = currentlyAssignedMaterials;
    }

    public void RollDice()
    {
        if (!diceTransform) return;
        var sideIndex = Random.Range(0, currentSides.Count);
        Vector3 euler = diceTransform.eulerAngles;
        var newEuler = new Vector3(euler.x + Random.Range(2, 4) * 360,
        euler.y + Random.Range(2, 4) * 360,
        euler.z + Random.Range(2, 4) * 360);
        diceTransform.rotation = Quaternion.Euler(newEuler);
        rollSFX.Play();
        DOTween.To(() => newEuler, x => diceTransform.localRotation = Quaternion.Euler(x), diceData[sideIndex].rotation, 5).OnComplete(() =>
        {
            rollSFX.Stop();
            rollDoneSFX.Play();
            currentSides[sideIndex].action?.Invoke();
        });
    }
}

[System.Serializable]
public class Side
{
    public Material material;
    public UnityEvent action;
}

[System.Serializable]
public class DiceData
{
    public int index;
    public Vector3 rotation;
}

