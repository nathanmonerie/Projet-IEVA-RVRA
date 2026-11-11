using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SelectorHandler : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Transform handle;
    [SerializeField] private GameObject label;

    [Space]

    [SerializeField] private float outputMin;
    [SerializeField] private float outputMax;

    private GameObject handleAura;
    private readonly float minLocalY = -0.25f;
    private readonly float maxLocalY = 0.25f;

    private bool isSelected = false;

    [HideInInspector] public float finalValue;

    public UnityEvent<float> onValueChanged;
    private float lastValue = -1f;

    private void Start()
    {
        handleAura = GameObject.Find("HandleAura");

        handleAura.SetActive(false);
        label.SetActive(false);
    }

    void Update()
    {
        if (handle == null) return;

        if (isSelected)
        {
            handleAura.SetActive(true);
            label.SetActive(true);
        }

        Vector3 AnchorRelativeposition = transform.InverseTransformPoint(handle.position);
        float currentLocalY = AnchorRelativeposition.y;

        float percentage = Mathf.InverseLerp(minLocalY, maxLocalY, currentLocalY);

        finalValue = Mathf.Lerp(outputMin, outputMax, percentage);

        if (!Mathf.Approximately(finalValue, lastValue))
        {
            lastValue = finalValue;
            onValueChanged?.Invoke(finalValue);
        }
    }

    public void Selection()
    {
        isSelected = true;
    }

    public void Unselection()
    {
        isSelected = false;
    }
}
