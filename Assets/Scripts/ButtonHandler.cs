using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public Transform pushCylinderTransform;
    private GameObject pushCylinderAura;

    public PlaneGeneration planeGenerationScript;

    private bool isPressed = false;

    public void Start()
    {
        pushCylinderAura = GameObject.Find("PushCylinderAura");
        pushCylinderAura.SetActive(false);
    }

    public void Update()
    {
        if (isPressed) pushCylinderAura.SetActive(true);
    }
    public void Pressed() 
    {
        pushCylinderTransform.position -= new Vector3(0, 0.01f, 0);
        isPressed = true;

        planeGenerationScript.RNG();
        planeGenerationScript.Deform();
    }

    public void Unpressed()
    {
        pushCylinderTransform.position += new Vector3(0, 0.01f, 0);
        isPressed = false;
    }
}
