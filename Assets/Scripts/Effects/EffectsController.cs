using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsController : MonoBehaviour
{
    private GameObject _fog;
    private GameObject _doorAndWalls;
    private GameObject _glitter;
    private GameObject _glitterKey;
    private GameObject _glitterLock;
    private GameObject _sceneryLight;
    private GameObject _glitterBurst;
    private GameObject _godray;
    private ParticleSystem _particleSystem;
    private ParticleSystem.EmissionModule _emissionModule;
    private float _initialEmissionRate;
    private Coroutine _resetBurstColorCoroutine;

    void Awake()
    {
        _fog = transform.Find(FileStrings.Fog).gameObject;
        _doorAndWalls = transform.Find(FileStrings.DoorAndWalls).gameObject;
        _glitter = transform.Find(FileStrings.Glitter).gameObject;
        _glitterKey = transform.Find(FileStrings.GlitterKey).gameObject;
        _glitterLock = transform.Find(FileStrings.GlitterLock).gameObject;
        _sceneryLight = transform.Find(FileStrings.SceneryLight).gameObject;
        _glitterBurst = transform.Find(FileStrings.GlitterBurst).gameObject;
        _godray = transform.Find(FileStrings.GodRay).gameObject;
        _particleSystem =_glitter.GetComponent<ParticleSystem>();
        _emissionModule = _particleSystem.emission;
        _initialEmissionRate = _emissionModule.rateOverTime.constant;
    }

    public void ToggleFog(bool isActive)
    {
        _fog.SetActive(isActive);
    }

    internal void ToggleDoorAndWalls(bool isActive)
    {
        _doorAndWalls.SetActive(isActive);
    }

    public void ToggleGlitter(bool isActive)
    {
        _glitter.SetActive(isActive);
    }

    internal void ToggleGlitterKey(bool isActive)
    {
        _glitterKey.SetActive(isActive);
    }

    internal void ToggleGlitterLock(bool isActive)
    {
        _glitterLock.SetActive(isActive);
    }

    public void ChangeGlitterColor(Color color)
    {
        ParticleSystem ps = _glitter.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = ps.main;
        mainModule.startColor = color;
    }

    internal void ChangeGlitterKeyColor(Color color)
    {
        ParticleSystem ps = _glitterKey.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = ps.main;
        mainModule.startColor = color;
    }

    internal void ChangeGlitterLockColor(Color color)
    {
        ParticleSystem ps = _glitterLock.GetComponent<ParticleSystem>();
        ParticleSystem.MainModule mainModule = ps.main;
        mainModule.startColor = color;
    }

    internal void ChangeSceneryLightColor(Color color)
    {
        _sceneryLight.GetComponent<Light>().color = color;
    }

    public void ToggleSceneryLight(bool isPending)
    {
        _sceneryLight.SetActive(isPending);
    }

    public void GlitterBurst(float duration)
    {
        // The burst particle system should already be configured with the right color
        // If we need to change it, update the burst prefab or add a ChangeGlitterBurstColor method
        Debug.Log("[EffectsController.GlitterBurst] Called. _glitterBurst is " + (_glitterBurst != null ? "set" : "NULL"));
        if (_glitterBurst != null)
        {
            Debug.Log("[EffectsController.GlitterBurst] _glitterBurst.activeInHierarchy: " + _glitterBurst.activeInHierarchy);
            ParticleSystem ps = _glitterBurst.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Debug.Log("[EffectsController.GlitterBurst] ParticleSystem found. isPlaying: " + ps.isPlaying + ", emission enabled: " + ps.emission.enabled);
                Debug.Log("[EffectsController.GlitterBurst] emission.rateOverTime: " + ps.emission.rateOverTime + ", duration: " + ps.main.duration);
                ps.Stop();
                ps.Clear();
                ps.Play();
                Debug.Log("[EffectsController.GlitterBurst] ParticleSystem.Stop().Clear().Play() called. isPlaying now: " + ps.isPlaying);
                
                // Stop any previous reset coroutine and start a new one
                if (_resetBurstColorCoroutine != null)
                {
                    StopCoroutine(_resetBurstColorCoroutine);
                }
                _resetBurstColorCoroutine = StartCoroutine(ResetGlitterBurstColorAfterDelay(duration));
            }
            else
            {
                Debug.LogError("[EffectsController.GlitterBurst] ParticleSystem component not found on _glitterBurst!");
            }
        }
        else
        {
            Debug.LogError("[EffectsController.GlitterBurst] _glitterBurst is NULL!");
        }
    }

    private System.Collections.IEnumerator ResetGlitterColorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        ChangeGlitterColor(Color.white);
        Debug.Log("[EffectsController.ResetGlitterColorAfterDelay] Glitter color reset to white.");
    }

    private System.Collections.IEnumerator ResetGlitterBurstColorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        ChangeGlitterBurstColor(Color.white);
        Debug.Log("[EffectsController.ResetGlitterBurstColorAfterDelay] Burst color reset to white.");
    }

    public void ChangeGlitterBurstColor(Color color)
    {
        Debug.Log("[EffectsController.ChangeGlitterBurstColor] Called with color " + color);
        if (_glitterBurst == null)
        {
            Debug.LogError("[EffectsController.ChangeGlitterBurstColor] _glitterBurst is NULL!");
            return;
        }
        ParticleSystem burstPs = _glitterBurst.GetComponent<ParticleSystem>();
        if (burstPs == null)
        {
            Debug.LogError("[EffectsController.ChangeGlitterBurstColor] ParticleSystem on _glitterBurst is NULL!");
            return;
        }
        var mainModule = burstPs.main;
        mainModule.startColor = color;
        // Note: MainModule is a struct, so this change won't persist to the ParticleSystem
        // We use the material's color property instead for a working solution
        Renderer renderer = _glitterBurst.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            mat.color = color;
            Debug.Log("[EffectsController.ChangeGlitterBurstColor] Material color set to " + color);
        }
        else
        {
            Debug.LogWarning("[EffectsController.ChangeGlitterBurstColor] Renderer not found on _glitterBurst.");
        }
    }

    internal void TogglePulseOnSceneryLight(bool isPulsating)
    {
        _sceneryLight.GetComponent<PulsatingLight>().TogglePulse(isPulsating);
    }

    internal void ToggleGodray(bool isActive)
    {
        GodRay godRay =_godray.GetComponent<GodRay>();
        if (godRay.GetIsActive() != isActive)
        {
            godRay.toggleActive(isActive);
        }
    }

    internal void ChangeGodrayColor(Color color)
    {
        GameObject godRay = transform.Find("GodRay/GodrayColumn").gameObject;
        Material godRayMaterial = godRay.GetComponent<Renderer>().material;
        godRayMaterial.color = color;
        godRayMaterial.EnableKeyword("_EMISSION");
        godRayMaterial.SetColor("_EmissionColor", color);
    }

    internal void SetGlitterRate(float v)
    {
        _emissionModule.rateOverTime = v;
    }

    internal void ResetGlitterRate()
    {
        _emissionModule.rateOverTime = _initialEmissionRate;
    }

    internal void StartGlitterColorCycle(HashSet<Color> colors)
    {
        _glitter.GetComponent<ParticleColorCycler>().StartCycle(colors);
    }

    internal void StopGlitterColorCycle()
    {
        _glitter.GetComponent<ParticleColorCycler>().StopCycle();
    }

    internal void StartGlitterLockColorCycle(HashSet<Color> colors)
    {
        _glitterLock.GetComponent<ParticleColorCycler>().StartCycle(colors);
    }

    internal void StopGlitterLockColorCycle()
    {
         _glitterLock.GetComponent<ParticleColorCycler>().StopCycle();
    }
}
