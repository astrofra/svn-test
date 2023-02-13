using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public Material lens;
    private Light _light;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponentInChildren<Light>();
    }

    // Update is called once per frame
    public void LightOn()
    {
        lens.EnableKeyword("_EMISSION");
        _light.enabled = true;
    }

    public void LightOff()
    {
        lens.DisableKeyword("_EMISSION");
        _light.enabled = false;
    }
}
