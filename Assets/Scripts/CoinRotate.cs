using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CoinRotate : MonoBehaviour
{
    public float rotateSpeed = 200f;
    public float floatHeight = 0.5f;
    public float floatSpeed = 2f;

    [Header("Pickup")]
    [SerializeField, Min(1)] private int coinValue = 1;
    [SerializeField] private GameObject pickupFx;
    [SerializeField] private float pickupFxLifetime = 1f;

    [Header("Color & Glow")]
    public Color baseColor = Color.yellow;
    public Color emissionColor = Color.yellow;
    public float emissionIntensity = 2f;

    private Vector3 startPos;
    private Material mat;
    private bool isCollected;

    void Start()
    {
        startPos = transform.position;

        Collider coinCollider = GetComponent<Collider>();
        if (coinCollider != null)
        {
            coinCollider.isTrigger = true;
        }

        Renderer coinRenderer = GetComponent<Renderer>();
        if (coinRenderer == null)
        {
            return;
        }

        // Lấy material
        mat = coinRenderer.material;

        // Set màu
        mat.color = baseColor;

        // Bật emission
        mat.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        // quay
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);

        // lên xuống
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // hiệu ứng phát sáng nhấp nháy
        float emission = Mathf.PingPong(Time.time * 2f, emissionIntensity);
        Color finalColor = emissionColor * emission;

        if (mat != null)
        {
            mat.SetColor("_EmissionColor", finalColor);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isCollected)
        {
            return;
        }

        PlayerCoins playerCoins = other.GetComponentInParent<PlayerCoins>();
        if (playerCoins == null)
        {
            return;
        }

        isCollected = true;
        playerCoins.AddCoins(coinValue);

        if (pickupFx != null)
        {
            GameObject newFx = Instantiate(pickupFx, transform.position, Quaternion.identity);
            Destroy(newFx, Mathf.Max(0f, pickupFxLifetime));
        }

        Destroy(gameObject);
    }
}