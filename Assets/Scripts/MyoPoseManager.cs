using UnityEngine;
using Pose = Thalmic.Myo.Pose;
using Arm = Thalmic.Myo.Arm;
using HoloToolkit.Unity.InputModule;
using UnityEngine.UI;

public class MyoPoseManager : MonoBehaviour
{
    public static MyoPoseManager Instance;

    public GameObject myo = null;
    public bool useMyo = false;
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

    public Text text;

    private void Awake()
    {
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

        if(Input.GetButtonUp("Switch"))
        {
            useMyo = !useMyo;
        }
        
        currentPose = thalmicMyo.pose;
        text.text = "Current Pose " + currentPose;
        if (useMyo)
        {
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
                        UpdateReference();
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
                        Vibrate();
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
                        Vibrate();
                        doubleTapUp = true;
                        break;
                    case Pose.Unknown:
                        break;
                }
            }
        }
        lastPose = currentPose;
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

    public void Vibrate()
    {
        thalmicMyo.Vibrate(Thalmic.Myo.VibrationType.Short);
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

    public Arm Arm
    {
        get
        {
            return thalmicMyo.arm;
        }
    }

    public Quaternion Rotation
    {
        get
        {
            // Current zero roll vector and roll value.
            Vector3 zeroRoll = computeZeroRollVector(myo.transform.forward);
            float roll = rollFromZero(zeroRoll, myo.transform.forward, myo.transform.up);

            // The relative roll is simply how much the current roll has changed relative to the reference roll.
            // adjustAngle simply keeps the resultant value within -180 to 180 degrees.
            float relativeRoll = NormalizeAngle(roll - referenceRoll);

            // antiRoll represents a rotation about the myo Armband's forward axis adjusting for reference roll.
            Quaternion antiRoll = Quaternion.AngleAxis(relativeRoll, myo.transform.forward);
            Quaternion rotation = Quaternion.identity;
            rotation = antiYaw * antiRoll * Quaternion.LookRotation(myo.transform.forward);
            return rotation;
        }
    }

    public Vector3 AngularVelocity
    {
        get
        {
            return thalmicMyo.gyroscope;
        }
    }

    public bool FistDown
    {
        get
        {
            if(useMyo)
                return fistDown;
            return false;
        }
    }

    public bool Fist
    {
        get
        {
            if (useMyo)
                return fist;
            return false;
        }
    }

    public bool FistUp
    {
        get
        {
            if (useMyo)
                return fistUp;
            return false;
        }
    }

    public bool DoubleTap
    {
        get
        {
            if (useMyo)
                return doubleTap;
            return false;
        }
    }

    public bool DoubleTapUp
    {
        get
        {
            if (useMyo)
                return doubleTapUp;
            return false;
        }
    }

    public bool DoubleTapDown
    {
        get
        {
            if (useMyo)
                return doubleTapDown;
            return false;
        }
    }

    public bool WaveIn
    {
        get
        {
            if (useMyo)
                return waveIn;
            return false;
        }
    }

    public bool WaveInUp
    {
        get
        {
            if (useMyo)
                return waveInUp;
            return false;
        }
    }

    public bool WaveInDown
    {
        get
        {
            if (useMyo)
                return waveInDown;
            return false;
        }
    }

    public bool WaveOut
    {
        get
        {
            if (useMyo)
                return waveOut;
            return false;
        }
    }

    public bool WaveOutUp
    {
        get
        {
            if (useMyo)
                return waveOutUp;
            return false;
        }
    }

    public bool WaveOutDown
    {
        get
        {
            if (useMyo)
                return waveOutDown;
            return false;
        }
    }

    public bool FingersSpread
    {
        get
        {
            if (useMyo)
                return fingersSpread;
            return false;
        }
    }

    public bool FingersSpreadUp
    {
        get
        {
            if (useMyo)
                return fingersSpreadUp;
            return false;
        }
    }

    public bool FingersSpreadDown
    {
        get
        {
            if (useMyo)
                return fingersSpreadDown;
            return false;
        }
    }

    public bool Rest
    {
        get
        {
            if (useMyo)
                return rest;
            return false;
        }
    }

    public bool RestUp
    {
        get
        {
            if (useMyo)
                return restUp;
            return false;
        }
    }

    public bool RestDown
    {
        get
        {
            if (useMyo)
                return restDown;
            return false;
        }
    }
}
