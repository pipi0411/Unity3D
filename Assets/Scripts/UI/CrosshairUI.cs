using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair")]
    [SerializeField] private bool autoCreateCrosshair = true;
    [SerializeField] private RectTransform crosshairRoot;
    [SerializeField] private Color crosshairColor = Color.white;
    [SerializeField] private float lineLength = 10f;
    [SerializeField] private float lineThickness = 2f;
    [SerializeField] private float lineGap = 6f;
    [SerializeField] private bool showCenterDot = true;
    [SerializeField] private float centerDotSize = 3f;

    [Header("Visibility")]
    [SerializeField] private bool showOnlyWhenAiming;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        if (autoCreateCrosshair && crosshairRoot == null)
        {
            crosshairRoot = CreateCrosshairRoot();
            BuildCrosshairGraphics();
        }

        if (showOnlyWhenAiming && playerMovement == null)
        {
            playerMovement = FindAnyObjectByType<PlayerMovement>();
        }
    }

    private void Update()
    {
        if (crosshairRoot == null)
        {
            return;
        }

        if (!showOnlyWhenAiming)
        {
            crosshairRoot.gameObject.SetActive(true);
            return;
        }

        bool canShow = playerMovement != null && playerMovement.IsAiming;
        crosshairRoot.gameObject.SetActive(canShow);
    }

    private RectTransform CreateCrosshairRoot()
    {
        GameObject rootObject = new GameObject("Crosshair", typeof(RectTransform));
        RectTransform rect = rootObject.GetComponent<RectTransform>();
        rect.SetParent(transform, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(64f, 64f);
        return rect;
    }

    private void BuildCrosshairGraphics()
    {
        if (crosshairRoot == null)
        {
            return;
        }

        CreateLine("Top", new Vector2(lineThickness, lineLength), new Vector2(0f, lineGap + lineLength * 0.5f));
        CreateLine("Bottom", new Vector2(lineThickness, lineLength), new Vector2(0f, -(lineGap + lineLength * 0.5f)));
        CreateLine("Left", new Vector2(lineLength, lineThickness), new Vector2(-(lineGap + lineLength * 0.5f), 0f));
        CreateLine("Right", new Vector2(lineLength, lineThickness), new Vector2(lineGap + lineLength * 0.5f, 0f));

        if (showCenterDot)
        {
            CreateLine("CenterDot", new Vector2(centerDotSize, centerDotSize), Vector2.zero);
        }
    }

    private void CreateLine(string objectName, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject lineObject = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        RectTransform rect = lineObject.GetComponent<RectTransform>();
        Image image = lineObject.GetComponent<Image>();

        rect.SetParent(crosshairRoot, false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        image.color = crosshairColor;
        image.raycastTarget = false;
    }
}