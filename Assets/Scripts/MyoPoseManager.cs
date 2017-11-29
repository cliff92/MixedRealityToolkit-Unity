using UnityEngine;
using Pose = Thalmic.Myo.Pose;
using Arm = Thalmic.Myo.Arm;

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

    private bool fist;
    private bool fistUp;
    private bool fistDown;

    private bool doubleTap;
    private bool doubleTapUp;
    private bool doubleTapDown;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Access the ThalmicMyo component attached to the Myo object.
        thalmicMyo = myo.GetComponent<ThalmicMyo>();
    }

    private void Update()
    {
        Reset();
        
        currentPose = thalmicMyo.pose;
        if (useMyo)
        {
            switch (currentPose)
            {
                case Pose.Rest:
                    break;
                case Pose.Fist:
                    fist = true;
                    break;
                case Pose.WaveIn:
                    break;
                case Pose.WaveOut:
                    break;
                case Pose.FingersSpread:
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
                        break;
                    case Pose.Fist:
                        fistDown = true;
                        break;
                    case Pose.WaveIn:
                        break;
                    case Pose.WaveOut:
                        break;
                    case Pose.FingersSpread:
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
                        break;
                    case Pose.Fist:
                        Vibrate();
                        fistUp = true;
                        break;
                    case Pose.WaveIn:
                        break;
                    case Pose.WaveOut:
                        break;
                    case Pose.FingersSpread:
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
    float normalizeAngle(float angle)
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
            return thalmicMyo.transform.localRotation;
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
            return fistDown;
        }
    }

    public bool Fist
    {
        get
        {
            return fist;
        }
    }

    public bool FistUp
    {
        get
        {
            return fistUp;
        }
    }

    public bool DoubleTap
    {
        get
        {
            return doubleTap;
        }
    }

    public bool DoubleTapUp
    {
        get
        {
            return doubleTapUp;
        }
    }

    public bool DoubleTapDown
    {
        get
        {
            return doubleTapDown;
        }
    }
}
