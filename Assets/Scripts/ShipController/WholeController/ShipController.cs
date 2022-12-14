using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
public class ShipController : MonoBehaviour
{
    public bool canDrive;
    public float forwardSpeed = 0, forwardSpeedSign = 0, forwardSpeedAbs = 0;
    public int numCornersSurface;
    public bool isBodySurface;
    public ShipStates shipState;
    public bool areAllCornersSurface;

    [Header("Other")] public Transform cogLow;
    public GameObject sceneViewFocusObject;

    public const float MaxSpeedBoost = 23.00f;

    Rigidbody _rb;
    GUIStyle _style;
    ShipGroundTrigger[] _sphereColliders;

    public enum ShipStates
    {
        Ground,
        Air,
        AllCornersSurface,
        SomeCornersSurface,
        BodySideGround,
        BodyGroundDead
    }

    void Start()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _rb.centerOfMass = cogLow.localPosition;
        _rb.maxAngularVelocity = 5.5f;

        _sphereColliders = GetComponentsInChildren<ShipGroundTrigger>();

        // GUI stuff
        _style = new GUIStyle();
        _style.normal.textColor = Color.red;
        _style.fontSize = 25;
        _style.fontStyle = FontStyle.Bold;

        // Lock scene view camera to the car
        // #if UNITY_EDITOR
        //         Selection.activeGameObject = sceneViewFocusObject;
        //         SceneView.lastActiveSceneView.FrameSelected(true);
        // #endif
    }

    void FixedUpdate()
    {
        UpdateShipState();
        UpdateShipVariables();
    }


    private void LateUpdate()
    {
        if (_rb.velocity.magnitude > MaxSpeedBoost)
        {
            _rb.velocity = _rb.velocity.normalized * MaxSpeedBoost;
        }
    }

    public void ResetShip(Vector3 position, Quaternion rotation, float boost = 33f)
    {
        _rb.transform.localPosition = position;
        _rb.rotation = rotation;
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        GetComponent<ShipDodging>().Reset();
        GetComponent<ShipBoosting>().boostAmount = 32f;
    }

    private void UpdateShipVariables()
    {
        forwardSpeed = Vector3.Dot(_rb.velocity, transform.forward);

        var vectorForwardSpeed = forwardSpeed * transform.forward;

        forwardSpeed = Math.Abs(forwardSpeed) > 0.02f ? (float)System.Math.Round(forwardSpeed, 2) : 0.0f;

        if (forwardSpeed == 0)
        {
            _rb.velocity -= vectorForwardSpeed;
        }
        forwardSpeedAbs = Mathf.Abs(forwardSpeed);
        forwardSpeedSign = Mathf.Sign(forwardSpeed);
    }

    void UpdateShipState()
    {
        numCornersSurface = _sphereColliders.Count(c => c.isTouchingSurface);

        areAllCornersSurface = numCornersSurface >= 3;

        // All corners are touching the ground
        if (areAllCornersSurface)
            shipState = ShipStates.AllCornersSurface;

        // Some wheels are touching the ground, but not the body
        if (!areAllCornersSurface && !isBodySurface)
            shipState = ShipStates.SomeCornersSurface;

        // We are lying on our side
        if (isBodySurface && !areAllCornersSurface)
            shipState = ShipStates.BodySideGround;

        // All wheels on the ground
        if (areAllCornersSurface && Vector3.Dot(Vector3.up, transform.up) > 0.95f)
            shipState = ShipStates.Ground;

        // He is dead Jimmy!
        if (isBodySurface && Vector3.Dot(Vector3.up, transform.up) < -0.95f)
            shipState = ShipStates.BodyGroundDead;

        // In the air
        if (!isBodySurface && numCornersSurface == 0)
            shipState = ShipStates.Air;

        canDrive = shipState == ShipStates.AllCornersSurface || shipState == ShipStates.Ground;
    }

    void DownForce()
    {
        if (shipState == ShipStates.AllCornersSurface || shipState == ShipStates.Ground)
            _rb.AddForce(-transform.up * 2, ForceMode.Acceleration);
    }

    # region GUI

    void OnGUI()
    {
        //GUI.Label(new Rect(10.0f, 40.0f, 150, 130), $"{forwardSpeed:F2} m/s {forwardSpeed * 100:F0} uu/s", _style);
        //GUI.Label(new Rect(30.0f, 40.0f, 150, 130), string.Format("turnRadius: {0:F2} m curvature: {1:F4}", turnRadius, curvature), style);
        //GUI.Label(new Rect(30.0f, 60.0f, 150, 130), $"car state: {carState.ToString()}", _style);
    }

    private void OnDrawGizmos()
    {
        // Draw CG
        if (_rb == null) return;
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(_rb.transform.TransformPoint(_rb.centerOfMass), 0.03f);
    }

    #endregion
}