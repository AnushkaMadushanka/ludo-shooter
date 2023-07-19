using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Transform player;

    public TextMeshProUGUI countdownText;
    public Slider healthBar;
    public TextMeshProUGUI bulletCountText;
    public TextMeshProUGUI waveDetailsText;

    private int enemiesRemaining;
    private HealthScript playerHealthScript;
    public bool isApplicationQuitting;

    public AudioSource countdownNormalSFX;
    public AudioSource countdownLastSFX;
    public AudioSource winSFX;

    public GameObject pauseCanvas;
    private InputAction pauseAction;


    void Awake()
    {
        instance = this;
        pauseAction = player.GetComponent<PlayerInput>().actions["Pause"];
        pauseAction.performed += PauseAction;
    }

    private void PauseAction(InputAction.CallbackContext ctx)
    {
        SetApplcationPause(Time.timeScale > 0);
    }

    private void Start()
    {
        playerHealthScript = PlayerController.instance.GetComponent<HealthScript>();
        StartCoroutine(StartWave());
    }

    private void OnEnable()
    {
        pauseAction.Enable();
    }

    private void OnDisable()
    {
        pauseAction.Disable();
    }

    public void SetBulletCountText(string text)
    {
        bulletCountText.SetText(text);
    }

    private void FixedUpdate()
    {
        healthBar.value = playerHealthScript.GetHealth() / 100;
    }

    public void RemoveEnemy()
    {
        enemiesRemaining -= 1;
        SetWaveData();
        if (enemiesRemaining <= 0)
        {
            waveDetailsText?.gameObject.SetActive(false);
            countdownText.gameObject.SetActive(true);
            countdownText.SetText("Wave " + (WaveSpawner.instance.currWave - 1) + " Completed");
            winSFX.Play();
            StartCoroutine(StartWave());
        }
    }

    private void SetWaveData()
    {
        waveDetailsText.SetText("Wave " + (WaveSpawner.instance.currWave - 1).ToString() + "\n" + enemiesRemaining.ToString() + " Enemies Left");
    }

    private IEnumerator StartWave()
    {
        var wait = new WaitForSeconds(1f);
        yield return wait;
        waveDetailsText.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(true);
        countdownText.SetText("Wave " + WaveSpawner.instance.currWave + " Starting In...");
        yield return wait;
        countdownText.SetText("3");
        countdownNormalSFX.Play();
        yield return wait;
        countdownText.SetText("2");
        countdownNormalSFX.Play();
        yield return wait;
        countdownText.SetText("1");
        countdownNormalSFX.Play();
        yield return wait;
        countdownText.SetText("Go");
        countdownLastSFX.Play();
        DiceRollScript.instance.InitializeDice();
        enemiesRemaining = WaveSpawner.instance.GenerateWave();
        WaveSpawner.instance.currWave += 1;
        waveDetailsText.gameObject.SetActive(true);
        SetWaveData();
        yield return wait;
        countdownText.gameObject.SetActive(false);
    }

    public void SlowDown()
    {
        Time.timeScale = 0.5f;
        var timer = 5f;
        DOTween.To(() => timer, x => timer = x, 0, timer).OnComplete(() =>
        {
            if (Time.timeScale > 0)
                Time.timeScale = 1f;
        });
    }

    public void DestroyAllEnemies()
    {
        var healthScripts = FindObjectsOfType<HealthScript>();
        foreach (var item in healthScripts)
        {
            if (item.transform != PlayerController.instance.transform)
            {
                item.SetDamage(10000);
            }
        }
    }

    public void FreezeAllEnemies()
    {
        var scripts = new List<MonoBehaviour>();
        scripts.AddRange(FindObjectsOfType<EnemyDiscController>());
        scripts.AddRange(FindObjectsOfType<EnemyPieceController>());
        foreach (var item in scripts)
        {
            item.enabled = false;
        }
        var timer = 3f;
        DOTween.To(() => timer, x => timer = x, 0, timer).OnComplete(() =>
        {
            var scripts = new List<MonoBehaviour>();
            scripts.AddRange(FindObjectsOfType<EnemyDiscController>());
            scripts.AddRange(FindObjectsOfType<EnemyPieceController>());
            foreach (var item in scripts)
            {
                item.enabled = true;
            }
        });
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    public void SetApplcationPause(bool state)
    {
        PlayerController.instance.enabled = !state;
        Debug.Log("Paused: " + state);
        Time.timeScale = !state ? 1f : 0f;
        pauseCanvas.SetActive(state);
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        isApplicationQuitting = true;
        pauseAction.performed -= PauseAction;
    }
}
