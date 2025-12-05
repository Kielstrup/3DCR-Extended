using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class ViewActivity : MonoBehaviour
{
    public string Id { get; private set; }
    public string Label { get; private set; }
    public string Description { get; private set; }
    public Color DisabledColor { get; private set; }
    public bool Disabled { get; private set; }
    private Color _buttonColor;
    
    private ButtonController _buttonController;
    private EffectsController _effectsController;
    private ProximityDetector _proximityDetector;
    private SceneryController _sceneryController;
    private ConstraintsController _constraintsController;
    private ActivityDetectionTrigger _activityDetectionTrigger;
    private event Action<ViewActivity> _activityMouseOver;
    private event Action<ViewActivity> _activityMouseExit;
    private event Action<ViewActivity> _onExecuted;
    private event Action<ViewActivity> _onExecuteRefused;
    private event Action<ViewActivity> _lockSelected;
    private event Action<bool> _simulatedExecution;
    private HashSet<Color> _disabledColors;
    private Color _pendingColor;
    private bool _cursorIsOneActivity;
    private bool _lockSignalRunning;
    private bool _keySignalRunning;
    private bool _isConditionOrMilestone;
    private string _sceneName;
    private Light _validityLight;
    private GameObject _validityCone;
    private Material _validityConeMaterial;
    private GameObject _validityCap;
    private Material _validityCapMaterial;
    [SerializeField]
    private float _validityConeScale = 2f;

    void Awake()
    {
        // Sub component controllers
        _buttonController = GetComponentInChildren<ButtonController>();
        _effectsController = GetComponentInChildren<EffectsController>();
        _proximityDetector = GetComponentInChildren<ProximityDetector>();
        _sceneryController = GetComponentInChildren<SceneryController>();
        _constraintsController = GetComponentInChildren<ConstraintsController>();
        _activityDetectionTrigger = GetComponentInChildren<ActivityDetectionTrigger>();

        // Subscribe to sub component events
        _buttonController.SubscribeToOnPressed(OnButtonPressed);
        _proximityDetector.SubscribeToIsTargetNearby(OnPlayerNearButton);
        _activityDetectionTrigger.SubscribeToOnMouseOver(OnActivityMouseOver);
        _activityDetectionTrigger.SubscribeToOnMouseExit(OnActivityMouseExit);
        _activityDetectionTrigger.SubscribeToOnMouseDown(OnActivityMouseDown);
        _activityDetectionTrigger.SubscribeToOnSimulatedMouseOver(OnActivitySimulatedMouseOver);
        _activityDetectionTrigger.SubscribeToOnSimulatedMouseDown(OnActivitySimulatedMouseDown);
        _activityDetectionTrigger.SubscribeToOnSimulatedMouseExit(OnActivitySimulatedMouseExit);
        _constraintsController.SubscribeToOnLockMouseDown(OnLockSelected);

        _pendingColor = Color.yellow;
        
        _lockSignalRunning = false;
        _keySignalRunning = false;

        _cursorIsOneActivity = false;
    }

    void Start()
    {
        _sceneName = GameSettings.SceneName;
        _effectsController.ToggleGlitterKey(false);
        _effectsController.ToggleGlitterLock(false);
        EnsureValidityLightExists();
    }

    private void OnActivitySimulatedMouseOver()
    {
        if (!_cursorIsOneActivity)
        {
            _cursorIsOneActivity = true;
            _simulatedExecution?.Invoke(true);
            OnActivityMouseOver();
            OnActivityMouseDown();
        }
    }

    private void OnActivitySimulatedMouseDown()
    {
    }

    private void OnActivitySimulatedMouseExit()
    {
        if (_cursorIsOneActivity)
        {
            _cursorIsOneActivity = false;
            _simulatedExecution?.Invoke(false);

        }
    }

    // Initialize object with activity data from graph
    internal void Initialize(string id, string label, string description, Color disabledColor)
    {
        Id = id;
        Label = label;
        Description = description;
        DisabledColor = disabledColor;
        _buttonColor = Color.white;
        _isConditionOrMilestone = disabledColor != Color.white;
        _constraintsController.ToggleKey(_isConditionOrMilestone);
        if(_isConditionOrMilestone)
        {
            _constraintsController.SetKeyColor(DisabledColor);
        }
    }

    // Public methods to set the visual state of the Activity
    internal void SetExecuted(bool isExecuted){

        // Toggle Glitter
        if(isExecuted)
        {
            _buttonColor = Color.green;
            _buttonController.SetPushButtonColor(_buttonColor);
            _effectsController.ChangeGlitterColor(Color.white);
            _effectsController.ToggleGlitterKey(false);
            _constraintsController.ToggleKey(false);
        }
        else
        {
            _buttonColor = Color.white;
            _buttonController.SetPushButtonColor(_buttonColor);
        }

        _effectsController.ToggleGlitter(!isExecuted);
        
        // Toggle animated elements on/off
        _sceneryController.ToggleAnimatedElements(isExecuted);
    }

    internal void SetPending(bool isPending)
    {
        _effectsController.ToggleSceneryLight(isPending);
        _effectsController.ToggleGodray(isPending);
        
        if(isPending){
        _effectsController.ToggleGlitter(true);
        _effectsController.ChangeSceneryLightColor(DisabledColor);
        _effectsController.TogglePulseOnSceneryLight(true);

        }   
    }

    internal void SetDisabled(HashSet<Color> colors)
    {
        _disabledColors = colors;

        bool isDisabled = false;
        if (_disabledColors.Count != 0){
            _constraintsController.ToggleLock(true);
            isDisabled = true;
            if (_lockSignalRunning)
            {
                LockSignal();
            }
            if (_disabledColors.Count == 1)
            {
                _constraintsController.StopLockColorCycle();
                _constraintsController.SetLockColor(_disabledColors.First());

            }
            else
            {
                _constraintsController.StartLockColorCycle(_disabledColors);
            }
        }
        else
        {
            _constraintsController.ToggleLock(false);
            _effectsController.ToggleGlitterLock(false);
            _lockSignalRunning = false;
        }
        // Modify this as needed when creating scenes.
        // // Toggle effects
        // switch(_sceneName)
        // {
        //     case "Rpg":
        //     case "Rpg_Abstract":
        //     case "Rpg_English":
        //     case "Rpg_English_Abstract":
        //         _effectsController.ToggleFog(isDisabled);
        //         _effectsController.ToggleDoorAndWalls(false);
        //         break;
        //     case "Office":
        //     case "Office_Abstract":
        //     case "Office_English":
        //     case "Office_English_Abstract":
        //         _effectsController.ToggleDoorAndWalls(isDisabled);
        //         _effectsController.ToggleFog(false);
        //         break;
        //     default:
        //         _effectsController.ToggleFog(isDisabled);
        //         _effectsController.ToggleDoorAndWalls(false);
        //         break;
        // }
        
        _effectsController.ToggleFog(isDisabled);
        _effectsController.ToggleDoorAndWalls(false);
        _buttonController.ToggleRotation(!isDisabled);

        // Set validity light color/visibility
        EnsureValidityLightExists();
        if (isDisabled)
        {
            _validityLight.color = Color.red;
            _validityLight.enabled = true;
            // Enable cone renderer and set its color to red
            if (_validityCone != null)
            {
                var mr = _validityCone.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    Color c = Color.red;
                    c.a = (_validityConeMaterial != null) ? _validityConeMaterial.color.a : 0.12f;
                    if (_validityConeMaterial != null) _validityConeMaterial.color = c;
                    mr.enabled = true;
                    // Cap color
                    if (_validityCapMaterial != null)
                    {
                        Color capCol = Color.red; capCol.a = 1f; _validityCapMaterial.color = capCol;
                    }
                    if (_validityCap != null)
                    {
                        var capMr = _validityCap.GetComponent<MeshRenderer>();
                        if (capMr != null) capMr.enabled = true;
                    }
                }
            }
        }
        else
        {
            _validityLight.color = Color.green;
            _validityLight.enabled = true;
            // Enable cone renderer and set its color to green
            if (_validityCone != null)
            {
                var mr = _validityCone.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    Color c = Color.green;
                    c.a = (_validityConeMaterial != null) ? _validityConeMaterial.color.a : 0.12f;
                    if (_validityConeMaterial != null) _validityConeMaterial.color = c;
                    mr.enabled = true;
                    // Cap color
                    if (_validityCapMaterial != null)
                    {
                        Color capCol = Color.green; capCol.a = 1f; _validityCapMaterial.color = capCol;
                    }
                    if (_validityCap != null)
                    {
                        var capMr = _validityCap.GetComponent<MeshRenderer>();
                        if (capMr != null) capMr.enabled = true;
                    }
                }
            }
        }

        // Set class variable
        Disabled = isDisabled;
    }

    internal void SetIncluded(bool isIncluded)
    {
        // Set opacity to reflect inclusion.
        _buttonController.SetOpaque(isIncluded);
        _sceneryController.SetOpaque(isIncluded);

        if(!isIncluded)
        {
            // Disable activity if excluded.
            Disabled = true;

            _buttonController.ToggleRotation(false);
            _constraintsController.ToggleKey(false);
            if (_keySignalRunning)
            {
                _keySignalRunning = false;
                _effectsController.ToggleGlitterKey(false);
            }
            // Hide validity light and cone when activity is excluded
            EnsureValidityLightExists();
            HideValidityVisuals();
        } else
        {
            _constraintsController.ToggleKey(_isConditionOrMilestone);
            // Show validity light when included
            EnsureValidityLightExists();
            _validityLight.enabled = true;
            _validityLight.color = Color.green;
            if (_validityCone != null)
            {
                var mr = _validityCone.GetComponent<MeshRenderer>();
                if (mr != null)
                {
                    Color c = Color.green;
                    c.a = (_validityConeMaterial != null) ? _validityConeMaterial.color.a : 0.12f;
                    if (_validityConeMaterial != null) _validityConeMaterial.color = c;
                    mr.enabled = true;
                }
            }
        }
    }

    /// <summary>
    /// Disables all validity visuals (light, translucent cone, and solid cap).
    /// Called when an activity is excluded or visibility should be hidden.
    /// </summary>
    private void HideValidityVisuals()
    {
        if (_validityLight != null)
        {
            _validityLight.enabled = false;
        }

        if (_validityCone != null)
        {
            var mr = _validityCone.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;
        }

        if (_validityCap != null)
        {
            var capMr = _validityCap.GetComponent<MeshRenderer>();
            if (capMr != null) capMr.enabled = false;
        }
    }

    /// <summary>
    /// Cleans up runtime-created validity visuals when the activity is destroyed.
    /// </summary>
    void OnDestroy()
    {
        if (_validityCap != null)
        {
            Destroy(_validityCap);
            _validityCap = null;
        }
        if (_validityCone != null)
        {
            Destroy(_validityCone);
            _validityCone = null;
        }
        if (_validityLight != null)
        {
            var go = _validityLight.gameObject;
            Destroy(go);
            _validityLight = null;
        }
    }

    /// <summary>
    /// Ensures a validity light (and cone/cap visuals) are created for this activity.
    /// Creates the light, cone mesh, and cap quad if they don't already exist.
    /// </summary>
    /// 
    /// 
    /// 
    private void EnsureValidityLightExists()
    {
        if (_validityLight != null) return;

        Transform parent = this.transform;
        GameObject lightObj = new GameObject("ValidityLight");
        lightObj.transform.SetParent(parent);
        lightObj.transform.localPosition = new Vector3(0f, 2f, 0f);
        lightObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Spot;
        // Increased range/intensity so the light and cone are visible from afar (minimap / BirdsEye)
        l.range = 30f;
        l.intensity = 6f;
        l.spotAngle = 80f;
        l.shadows = LightShadows.None;
        l.enabled = false;

        _validityLight = l;
        // Also create a translucent cone to visualize the full spotlight cone
        CreateValidityCone(l);
    }

    /// <summary>
    /// Creates a translucent cone mesh and solid cap quad at the top to visualize the spotlight.
    /// The cone extends upward from the activity button to a height based on the light's range and scale multiplier.
    /// </summary>
    /// <param name="lightSource">The spotlight light component to base cone dimensions on.</param>
    private void CreateValidityCone(Light lightSource)
    {
        if (_validityCone != null) return;

        // Make cone height proportional to the light's range so it's visible from far away
        float coneHeight = Mathf.Min(30f, lightSource.range) * _validityConeScale;
        // Narrow cone: radius is small relative to height for a tall waypoint look
        float coneRadius = Mathf.Max(0.5f, coneHeight * 0.06f);

        // Position the light at the top of the cone so the wide base is seen from above/minimap
        lightSource.transform.localPosition = new Vector3(0f, coneHeight, 0f);

        GameObject cone = new GameObject("ValidityCone");
        cone.transform.SetParent(this.transform);
        // Place apex at the button (local origin) and extend upward
        cone.transform.localPosition = Vector3.zero;
        cone.transform.localRotation = Quaternion.identity;

        MeshFilter mf = cone.AddComponent<MeshFilter>();
        MeshRenderer mr = cone.AddComponent<MeshRenderer>();

        mf.mesh = BuildConeMesh(coneRadius, coneHeight, 36);

        Shader shader = Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Legacy Shaders/Transparent/Diffuse") ?? Shader.Find("Sprites/Default");
        _validityConeMaterial = new Material(shader);
        Color c = Color.green;
        c.a = 0.12f; // more translucent so it's subtle but visible on minimap
        _validityConeMaterial.color = c;
        mr.material = _validityConeMaterial;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        // Ensure the cone is initially disabled; visibility controlled by SetDisabled/SetIncluded
        mr.enabled = false;

        // Create a solid quad at the top of the cone so the top reads as a solid color on the minimap
        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Quad);
        cap.name = "ValidityCap";
        cap.transform.SetParent(this.transform);
        cap.transform.localPosition = new Vector3(0f, coneHeight, 0f);
        // Quad default faces +Z; rotate so its normal points upward (towards +Y)
        cap.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        // Size the quad so it covers the cone base (diameter)
        float capSize = coneRadius * 2f;
        cap.transform.localScale = new Vector3(capSize, capSize, 1f);

        // Remove collider created by primitive
        var col = cap.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);

        MeshRenderer capMr = cap.GetComponent<MeshRenderer>();

        Shader capShader = Shader.Find("Unlit/Color") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Standard");
        _validityCapMaterial = new Material(capShader);
        Color capColor = Color.green; capColor.a = 1f;
        _validityCapMaterial.color = capColor;
        capMr.material = _validityCapMaterial;
        capMr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        capMr.receiveShadows = false;
        capMr.enabled = false;

        _validityCone = cone;
        _validityCap = cap;
    }

    private Mesh BuildDiskMesh(float radius, int segments)
    {
        return null;
    }

    /// <summary>
    /// Builds a procedural cone mesh with apex at origin and base at +height.
    /// Used for the translucent validity indicator cone.
    /// </summary>
    /// <param name="radius">Base radius of the cone.</param>
    /// <param name="height">Height of the cone from apex to base.</param>
    /// <param name="segments">Number of segments around the cone circumference.</param>
    /// <returns>A mesh representing the cone geometry.</returns>
    private Mesh BuildConeMesh(float radius, float height, int segments)
    {
        Mesh m = new Mesh();

        // Apex at origin (button), base circle at +height
        Vector3[] verts = new Vector3[segments + 2 + segments];
        int v = 0;
        // Apex
        verts[v++] = Vector3.zero;

        // Circle around +Y (upwards)
        for (int i = 0; i < segments; i++)
        {
            float ang = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(ang) * radius;
            float z = Mathf.Sin(ang) * radius;
            verts[v++] = new Vector3(x, height, z);
        }

        // center for base cap
        verts[v++] = new Vector3(0f, height, 0f);

        // duplicate circle for base (optional, keeps indexing simple)
        for (int i = 0; i < segments; i++)
        {
            float ang = (float)i / segments * Mathf.PI * 2f;
            float x = Mathf.Cos(ang) * radius;
            float z = Mathf.Sin(ang) * radius;
            verts[v++] = new Vector3(x, height, z);
        }

        List<int> tris = new List<int>();

        // Side triangles (apex, i+1, i)
        for (int i = 0; i < segments; i++)
        {
            int a = 0;
            int b = 1 + i;
            int c = 1 + ((i + 1) % segments);
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
        }

        int centerIndex = 1 + segments;
        int baseStart = centerIndex + 1;

        // Base cap triangles (center, baseStart + i, baseStart + i+1)
        for (int i = 0; i < segments; i++)
        {
            int b = baseStart + i;
            int c = baseStart + ((i + 1) % segments);
            tris.Add(centerIndex);
            tris.Add(b);
            tris.Add(c);
        }

        m.vertices = verts;
        m.SetTriangles(tris.ToArray(), 0);
        m.RecalculateNormals();
        return m;
    }
    // Allow subscribtion to Activity mouse events
    internal void SubscribeToActivityMouseOver(Action<ViewActivity> subscriber)
    {
        _activityMouseOver += subscriber;
    }
    internal void SubscribeToActivityMouseExit(Action<ViewActivity> subscriber)
    {
        _activityMouseExit += subscriber;
    }
    internal void SubscribeToLockSelected(Action<ViewActivity> subscriber)
    {
        _lockSelected += subscriber;
    }
    internal void SubscribeToSimulatedExecution(Action<bool> subscriber)
    {
        _simulatedExecution += subscriber;
    }
    

    // Forwards configuration data to Proximity Detector.
    internal void SetProximityDetectorTarget(int targetLayer)
    {
        _proximityDetector.SetTargetLayer(targetLayer);
    }

    // Allows View to subscribe to activity execution event
    internal void SubscribeToOnExecuted(Action<ViewActivity> subscriber){
        _onExecuted+=subscriber;
    }
    internal void SubscribeToOnExecuteRefused(Action<ViewActivity> subscriber){
        _onExecuteRefused+=subscriber;
    }

    // Forward MouseOver event to View
    internal void OnActivityMouseOver()
    { 
        _activityMouseOver?.Invoke(this);
    }

    // Forward MouseOver event to View
    internal void OnActivityMouseExit()
    {
        _activityMouseExit?.Invoke(this);
    }

    // Forward control to ButtonController On Mouse Down.
    internal void OnActivityMouseDown()
    {
        if(Disabled)
        {
            _buttonController.PressButtonRefuse(_effectsController);
            _onExecuteRefused?.Invoke(this);
        }
        else
        {
            _buttonController.PressButton();
        }
    }

    internal void OnLockSelected()
    {
        _lockSelected?.Invoke(this);
    }

    // Inform Button Controller on event from Proximity Detector
    internal void OnPlayerNearButton(bool playerNearButton)
    {
        if(!Disabled){
            _buttonController.TogglePushButtonAnimation(playerNearButton);

        }
    }
    /// <summary>
    /// Handles the event when a button is pressed (allowed activity execution).
    /// Triggers a green glitter burst at the button location.
    /// </summary>
    /// <param name="quickRotationDuration">Duration of the rotation animation; used for burst timing.</param>
    private void OnButtonPressed(float quickRotationDuration)
    {
        _onExecuted?.Invoke(this);
        // Ensure burst color is green for allowed presses
        _effectsController.ChangeGlitterBurstColor(Color.green);
        _effectsController.GlitterBurst(quickRotationDuration);
    }

    internal void SetStateAdded()
    {
    }

    internal void SetStateRemoved()
    {
    }
    internal void KeySignal()
    {
        _keySignalRunning = true;
        _effectsController.ToggleGlitterKey(true);
        _effectsController.ChangeGlitterKeyColor(DisabledColor);
    }

    internal void LockSignal()
    {
        _lockSignalRunning = true;
        _effectsController.ToggleGlitterLock(true);
        if(_disabledColors.Count > 1)
        {
            _effectsController.StartGlitterLockColorCycle(_disabledColors);
        }
        else
        {
            _effectsController.StopGlitterLockColorCycle();
            _effectsController.ChangeGlitterLockColor(_disabledColors.First());

        }
    }
    internal void DisableKeySignal()
    {
        _effectsController.ToggleGlitterKey(false);
    }

    internal void DisableLockSignal()
    {
        _lockSignalRunning = false;
         _effectsController.ToggleGlitterLock(false);
    }


}