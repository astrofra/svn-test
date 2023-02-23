using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    public Material lens;
    public Material volumetric_plane;
    public Material volumetric_cone;
    private Light _light;
    //private bool _isOn = true;
    private int _flashlightState = 0;

    // Start is called before the first frame update
    void Start()
    {
        _light = GetComponentInChildren<Light>();
        ToggleFlashlight();
    }

    public void ToggleFlashlight()
    {
        _flashlightState += 1;
        if (_flashlightState > 3)
            _flashlightState = 0;

        switch(_flashlightState){
            case 0:
                LightOff();
                break;
            case 1:
                LightLow();
                break; 
            case 2:
                LightMedium();
                break;  
            case 3:
                LightStrong();
                break;  
        }
    }

    private void LightLow()
    {
        lens.EnableKeyword("_EMISSION");
        //_light.enabled = true;
        _light.intensity = 5.0f;
        volumetric_cone.SetFloat("_opacity", 0.3f);
        float test = volumetric_cone.GetFloat("opacity");
        volumetric_plane.SetFloat("_opacity", 0.3f);
    }

    private void LightMedium()
    {
        lens.EnableKeyword("_EMISSION");
        //_light.enabled = true;
        _light.intensity = 10.0f;
        volumetric_cone.SetFloat("_opacity", 0.6f);
        volumetric_plane.SetFloat("_opacity", 0.6f);
    }

    private void LightStrong()
    {
        lens.EnableKeyword("_EMISSION");
        //_light.enabled = true;
        _light.intensity = 15.0f;
        volumetric_cone.SetFloat("_opacity", 1.0f);
        volumetric_plane.SetFloat("_opacity", 1.0f);
    }

    private void LightOff()
    {
        lens.DisableKeyword("_EMISSION");
        //_light.enabled = false;
        _light.intensity = 0.0f;
        volumetric_cone.SetFloat("_opacity", 0.0f);
        volumetric_plane.SetFloat("_opacity", 0.0f);
    }
}
