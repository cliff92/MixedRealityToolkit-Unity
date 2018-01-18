using UnityEngine;
using Pose = Thalmic.Myo.Pose;
using Arm = Thalmic.Myo.Arm;
using HoloToolkit.Unity.InputModule;

public class MyoPoseManager : MonoBehaviour
{
    public static MyoPoseManager Instance;

    public GameObject myo = null;
    private ThalmicMyo thalmicMyo;

    // The pose from the last update. This is used to determine if the pose has changed
    // so that actions are only performed upon making them rather than every frame during
    // which they are active.
    private Pose lastPose = Pose.Unknown;
    private Pose currentPose = Pose.Unknown;

    // A rotation that compensates for the Myo armband's orientation parallel to the ground, i.e. yaw.
    // Once set, the direction the Myo armband is facing becomes "forward" within the program.
    // Set by making the fingers spread pose or pressing "r".
    private Quaternion antiYaw = Quaternion.identity;

    // A reference angle representing how the armband is rotated about the wearer's arm, i.e. roll.
    // Set by making the fingers spread pose or pressing "r".
    private float referenceRoll = 0.0f;

    private float relativeRoll = 0.0f;

    private readonly FloatRollingStatistics velocityRollingStats = new FloatRollingStatistics();
    private int StoredSamples = 50;

    private bool rest;
    private bool restUp;
    private bool restDown;

    private bool fist;
    private bool fistUp;
    private bool fistDown;

    private bool doubleTap;
    private bool doubleTapUp;
    private bool doubleTapDown;

    private bool waveIn;
    private bool waveInUp;
    private bool waveInDown;

    private bool waveOut;
    private bool waveOutUp;
    private bool waveOutDown;

    private bool fingersSpread;
    private bool fingersSpreadUp;
    private bool fingersSpreadDown;

    private void Awake()
    {
        velocityRollingStats.Init(StoredSamples);
        Instance = this;
    }

    private void Start()
    {
        // Access the ThalmicMyo component attached to the Myo object.
        thalmicMyo = myo.GetComponent<ThalmicMyo>();
        InputManager.Instance.AddGlobalListener(gameObject);
    }

    private void Update()
    {
        Reset();
        currentPose = thalmicMyo.pose;
        UpdatePose();
        lastPose = currentPose;
        UpdateRotation();
    }

    private void UpdatePose()
    {
        // This is done to reduce movement induced gesture changes
        if ((velocityRollingStats.Average>10)
            && lastPose==Pose.FingersSpread)
        {
            if(currentPose != Pose.FingersSpread)
            {
                currentPose = Pose.FingersSpread;
                Logger.PoseCorrectionUsed();
            }
        }
        switch (currentPose)
        {
            case Pose.Rest:
                rest = true;
                break;
            case Pose.Fist:
                fist = true;
                break;
            case Pose.WaveIn:
                waveIn = true;
                break;
            case Pose.WaveOut:
                waveOut = true;
                break;
            case Pose.FingersSpread:
                fingersSpread = true;
                break;
            case Pose.DoubleTap:
                doubleTap = true;
                break;
            case Pose.Unknown:
                break;
        }
        
        if (currentPose != lastPose)
        {
            switch (currentPose)
            {
                case Pose.Rest:
                    restDown = true;
                    break;
                case Pose.Fist:
                    fistDown = true;
                    break;
                case Pose.WaveIn:
                    waveInDown = true;
                    break;
                case Pose.WaveOut:
                    waveOutDown = true;
                    break;
                case Pose.FingersSpread:
                    fingersSpreadDown = true;
                    break;
                case Pose.DoubleTap:
                    doubleTapDown = true;
                    break;
                case Pose.Unknown:
                    break;
            }
            switch (lastPose)
            {
                case Pose.Rest:
                    restUp = true;
                    break;
                case Pose.Fist:
                    fistUp = true;
                    break;
                case Pose.WaveIn:
                    waveInUp = true;
                    break;
                case Pose.WaveOut:
                    waveOutUp = true;
                    break;
                case Pose.FingersSpread:
                    fingersSpreadUp = true;
                    break;
                case Pose.DoubleTap:
                    doubleTapUp = true;
                    break;
                case Pose.Unknown:
                    break;
            }
        }
    }

    private void UpdateRotation()
    {
        if (ClickDown || ClickUp)
        {
            UpdateReference();
        }
        // Current zero roll vector and roll value.
        Vector3 zeroRoll = computeZeroRollVector(myo.transform.forward);
        float roll = rollFromZero(zeroRoll, myo.transform.forward, myo.transform.up);

        // The relative roll is simply how much the current roll has changed relative to the reference roll.
        // adjustAngle simply keeps the resultant value within -180 to 180 degrees.
        relativeRoll = NormalizeAngle(roll - referenceRoll);

        // antiRoll represents a rotation about the myo Armband's forward axis adjusting for reference roll.
        Quaternion antiRoll = Quaternion.AngleAxis(relativeRoll, myo.transform.forward);

        // Here the anti-roll and yaw rotations are applied to the myo Armband's forward direction to yield
        // the orientation of the joint.
        transform.rotation = antiYaw * antiRoll * Quaternion.LookRotation(myo.transform.forward);

        // The above calculations were done assuming the Myo armbands's +x direction, in its own coordinate system,
        // was facing toward the wearer's elbow. If the Myo armband is worn with its +x direction facing the other way,
        // the rotation needs to be updated to compensate.
        if (thalmicMyo.xDirection == Thalmic.Myo.XDirection.TowardWrist)
        {
            // Mirror the rotation around the XZ plane in Unity's coordinate system (XY plane in Myo's coordinate
            // system). This makes the rotation reflect the arm's orientation, rather than that of the Myo armband.
            transform.rotation = new Quaternion(transform.localRotation.x,
                                                -transform.localRotation.y,
                                                transform.localRotation.z,
                                                -transform.localRotation.w);
        }
    }

    private void Reset()
    {
        //fist
        fist = false;
        fistDown = false;
        fistUp = false;

        //doubleTap
        doubleTap = false;
        doubleTapDown = false;
        doubleTapUp = false;

        //waveIn
        waveIn = false;
        waveInDown = false;
        waveInUp = false;

        //waveOut
        waveOut = false;
        waveOutDown = false;
        waveOutUp = false;

        //fingersspread
        fingersSpread = false;
        fingersSpreadDown = false;
        fingersSpreadUp = false;

        //rest
        rest = false;
        restDown = false;
        restUp = false;
    }

    public void Vibrate(Thalmic.Myo.VibrationType vibrationType)
    {
        thalmicMyo.Vibrate(vibrationType);
    }

    public float RollFromZero()
    {
        // Current zero roll vector and roll value.
        Vector3 zeroRoll = computeZeroRollVector(myo.transform.forward);
        return rollFromZero(zeroRoll, myo.transform.forward, myo.transform.up);
    }

    // Compute the angle of rotation clockwise about the forward axis relative to the provided zero roll direction.
    // As the armband is rotated about the forward axis this value will change, regardless of which way the
    // forward vector of the Myo is pointing. The returned value will be between -180 and 180 degrees.
    private float rollFromZero(Vector3 zeroRoll, Vector3 forward, Vector3 up)
    {
        // The cosine of the angle between the up vector and the zero roll vector. Since both are
        // orthogonal to the forward vector, this tells us how far the Myo has been turned around the
        // forward axis relative to the zero roll vector, but we need to determine separately whether the
        // Myo has been rolled clockwise or counterclockwise.
        float cosine = Vector3.Dot(up, zeroRoll);

        // To determine the sign of the roll, we take the cross product of the up vector and the zero
        // roll vector. This cross product will either be the same or opposite direction as the forward
        // vector depending on whether up is clockwise or counter-clockwise from zero roll.
        // Thus the sign of the dot product of forward and it yields the sign of our roll value.
        Vector3 cp = Vector3.Cross(up, zeroRoll);
        float directionCosine = Vector3.Dot(forward, cp);
        float sign = directionCosine < 0.0f ? 1.0f : -1.0f;

        // Return the angle of roll (in degrees) from the cosine and the sign.
        return sign * Mathf.Rad2Deg * Mathf.Acos(cosine);
    }

    // Compute a vector that points perpendicular to the forward direction,
    // minimizing angular distance from world up (positive Y axis).
    // This represents the direction of no rotation about its forward axis.
    Vector3 computeZeroRollVector(Vector3 forward)
    {
        Vector3 antigravity = Vector3.up;
        Vector3 m = Vector3.Cross(myo.transform.forward, antigravity);
        Vector3 roll = Vector3.Cross(m, myo.transform.forward);

        return roll.normalized;
    }

    // Adjust the provided angle to be within a -180 to 180.
    public static float NormalizeAngle(float angle)
    {
        if (angle > 180.0f)
        {
            return angle - 360.0f;
        }
        if (angle < -180.0f)
        {
            return angle + 360.0f;
        }
        return angle;
    }

    // Update references. This anchors the joint on-screen such that it faces forward away
    // from the viewer when the Myo armband is oriented the way it is when these references are taken.
    public void UpdateReference()
    {
        // _antiYaw represents a rotation of the Myo armband about the Y axis (up) which aligns the forward
        // vector of the rotation with Z = 1 when the wearer's arm is pointing in the reference direction.
        antiYaw = Quaternion.FromToRotation(
            new Vector3(myo.transform.forward.x, 0, myo.transform.forward.z),
            new Vector3(0, 0, 1)
        );

        // _referenceRoll represents how many degrees the Myo armband is rotated clockwise
        // about its forward axis (when looking down the wearer's arm towards their hand) from the reference zero
        // roll direction. This direction is calculated and explained below. When this reference is
        // taken, the joint will be rotated about its forward axis such that it faces upwards when
        // the roll value matches the reference.
        Vector3 referenceZeroRoll = computeZeroRollVector(myo.transform.forward);
        referenceRoll = rollFromZero(referenceZeroRoll, myo.transform.forward, myo.transform.up);
    }

    public static Arm Arm
    {
        get
        {
            return Instance.thalmicMyo.arm;
        }
    }

    public static Quaternion Rotation
    {
        get
        {
            return Instance.transform.rotation;
        }
    }

    /// <summary>
    /// Velocity in radians
    /// </summary>
    public static Vector3 AngularVelocity
    {
        get
        {
            return Instance.thalmicMyo.gyroscope * Mathf.Deg2Rad;
        }
    }

    public static bool FistDown
    {
        get
        {
            return Instance.fistDown;
        }
    }

    public static bool Fist
    {
        get
        {
            return Instance.fist;
        }
    }

    public static bool FistUp
    {
        get
        {
            return Instance.fistUp;
        }
    }

    public static bool DoubleTap
    {
        get
        {
            return Instance.doubleTap;
        }
    }

    public static bool DoubleTapUp
    {
        get
        {
            return Instance.doubleTapUp;
        }
    }

    public static bool DoubleTapDown
    {
        get
        {
            return Instance.doubleTapDown;
        }
    }

    public static bool WaveIn
    {
        get
        {
            return Instance.waveIn;
        }
    }

    public static bool WaveInUp
    {
        get
        {
            return Instance.waveInUp;
        }
    }

    public static bool WaveInDown
    {
        get
        {
            return Instance.waveInDown;
        }
    }

    public static bool WaveOut
    {
        get
        {
            return Instance.waveOut;
        }
    }

    public static bool WaveOutUp
    {
        get
        {
            return Instance.waveOutUp;
        }
    }

    public static bool WaveOutDown
    {
        get
        {
            return Instance.waveOutDown;
        }
    }

    public static bool FingersSpread
    {
        get
        {
            return Instance.fingersSpread;
        }
    }

    public static bool FingersSpreadUp
    {
        get
        {
            return Instance.fingersSpreadUp;
        }
    }

    public static bool FingersSpreadDown
    {
        get
        {
            return Instance.fingersSpreadDown;
        }
    }

    public static bool Rest
    {
        get
        {
            return Instance.rest;
        }
    }

    public static bool RestUp
    {
        get
        {
            return Instance.restUp;
        }
    }

    public static bool RestDown
    {
        get
        {
            return Instance.restDown;
        }
    }

    public static bool Click
    {
        get
        {
            return FingersSpread;
        }
    }
    public static bool ClickUp
    {
        get
        {
            return FingersSpreadUp;
        }
    }
    public static bool ClickDown
    {
        get
        {
            return FingersSpreadDown;
        }
    }

    public static bool Jump
    {
        get
        {
            return false;
        }
    }

    public static float RelativeRoll
    {
        get
        {
            return Instance.relativeRoll;
        }
    }
}
