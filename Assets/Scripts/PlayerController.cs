using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private Transform cameraTransform;

    [SerializeField]
    private float playerSpeed = 2.0f;
    [SerializeField]
    private float jumpHeight = 1.0f;
    [SerializeField]
    private float gravityValue = -9.81f;
    [SerializeField]
    private float rotationSpeed = 7f;
    [SerializeField]
    private Transform barrelTransform;
    [SerializeField]
    private Transform bulletParent;
    [SerializeField]
    private float bulletHitMissDistance = 25f;
    [SerializeField]
    private Transform gunTransform;
    [SerializeField]
    private GameObject muzzleVfxPrefab;
    [SerializeField]
    private GameObject hitVfxPrefab;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction shootAction;

    public int bulletCount = 10;
    public int currentBulletCount;
    public float reloadTime = 2f;
    public float currentReloadTime = 0;
    public LayerMask layerMask;

    [SerializeField]
    private AudioSource shootSFX;
    [SerializeField]
    private AudioSource reloadSFX;
    [SerializeField]
    private AudioSource gameOverSFX;


    private void Awake()
    {
        instance = this;
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        cameraTransform = Camera.main.transform;
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        shootAction = playerInput.actions["Shoot"];
        shootAction.performed += ShootGun;

        Cursor.lockState = CursorLockMode.Locked;
        currentBulletCount = bulletCount;
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");
        shootAction.Enable();
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable");
        shootAction.Disable();
    }

    private void ShootGun(InputAction.CallbackContext ctx)
    {
        if (currentBulletCount <= 0) return;
        RaycastHit hit;
        shootSFX.Play();
        var point = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity, layerMask) ? hit.point : cameraTransform.position + cameraTransform.forward * bulletHitMissDistance;
        Vector3 aimDir = (point - barrelTransform.position).normalized;
        if (Physics.Raycast(barrelTransform.position, aimDir, out hit, Mathf.Infinity, layerMask))
        {
            hit.transform.GetComponent<HealthScript>()?.SetDamage(25);
            if (muzzleVfxPrefab != null)
            {
                var muzzleVFX = Instantiate(muzzleVfxPrefab, barrelTransform.position, Quaternion.identity);
                muzzleVFX.transform.forward = barrelTransform.forward;
                var ps = muzzleVFX.GetComponent<ParticleSystem>();
                if (ps != null)
                    Destroy(muzzleVFX, ps.main.duration);
                else
                {
                    var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(muzzleVFX, psChild.main.duration);
                }
                var trail = muzzleVFX.GetComponent<LineRenderer>();
                trail.SetPosition(0, barrelTransform.position);
                trail.SetPosition(1, hit.point);
                Destroy(trail, 0.1f);
            }

            if (hitVfxPrefab != null)
            {
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                var hitVFX = Instantiate(hitVfxPrefab, hit.point, rot);
                var ps = hitVFX.GetComponent<ParticleSystem>();
                if (ps == null)
                {
                    var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                    Destroy(hitVFX, psChild.main.duration);
                }
                else
                    Destroy(hitVFX, ps.main.duration);
            }
        }
        currentBulletCount -= 1;
        if (currentBulletCount <= 0)
        {
            reloadSFX.Play();
            currentReloadTime = reloadTime;
        }
        GameManager.instance.SetBulletCountText(currentReloadTime > 0 ? "Reloading..." : currentBulletCount + " / " + bulletCount);
    }

    public void AddBullets(int bullets)
    {
        currentBulletCount += bullets;
        GameManager.instance.SetBulletCountText(currentBulletCount + " / " + bulletCount);
    }

    void Update()
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0;
        controller.Move(move * Time.deltaTime * playerSpeed);

        // Changes the height position of the player..
        if (jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (currentBulletCount == 0 && currentReloadTime <= 0)
        {
            currentBulletCount = bulletCount;
            DiceRollScript.instance.RollDice();
            GameManager.instance.SetBulletCountText(currentBulletCount + " / " + bulletCount);
        }
        if (currentReloadTime > 0)
        {
            currentReloadTime -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity, layerMask))
        {
            gunTransform.LookAt(hit.point);
        }
    }

    private void OnDestroy()
    {
        shootAction.performed -= ShootGun;
        if (GameManager.instance.isApplicationQuitting) return;
        gameOverSFX.Play();
        var timer = 1f;
        DOTween.To(() => timer, x => timer = x, 0, timer).OnComplete(() =>
        {
            if (GameManager.instance.isApplicationQuitting) return;
            Cursor.lockState = CursorLockMode.None;
            RestartManager.instance.LoadScene("RestartScene");
        });
    }
}