using System.Collections.Generic;
using UnityEngine;

public class VelocityHandler
{

    private List<VelocityWithTimeStep> velocityListMyo = new List<VelocityWithTimeStep>();
    private List<VelocityWithTimeStep> velocityListLeft = new List<VelocityWithTimeStep>();
    private List<VelocityWithTimeStep> velocityListRight = new List<VelocityWithTimeStep>();

    private float timeFrame;

    public VelocityHandler(float timeFrame)
    {
        this.timeFrame = timeFrame;
    }

    public void UpdateLists()
    {
        float timeStemp = Time.time;
        Vector3 angularVelocity;

        if (HandManager.MyoHand.TryGetAngularVelocity(out angularVelocity))
        {
            velocityListMyo.Add(new VelocityWithTimeStep(angularVelocity.magnitude, timeStemp));
        }
        if (HandManager.LeftHand.TryGetAngularVelocity(out angularVelocity))
        {
            velocityListLeft.Add(new VelocityWithTimeStep(angularVelocity.magnitude, timeStemp));
        }
        if (HandManager.RightHand.TryGetAngularVelocity(out angularVelocity))
        {
            velocityListRight.Add(new VelocityWithTimeStep(angularVelocity.magnitude, timeStemp));
        }

        velocityListMyo.RemoveAll(TooOld);
        velocityListLeft.RemoveAll(TooOld);
        velocityListRight.RemoveAll(TooOld);
    }
    bool TooOld(VelocityWithTimeStep velocity)
    {
        if (velocity.timeStemp < Time.time - timeFrame)
        {
            return true;
        }
        return false;
    }

    public float FindTimeStepWithMinVel()
    {
        switch (HandManager.CurrentHand.device)
        {
            case RayInputDevice.Myo:
                return FindTimeStepWithMinVel(velocityListMyo);
            case RayInputDevice.ControllerLeft:
                return FindTimeStepWithMinVel(velocityListLeft);
            case RayInputDevice.ControllerRight:
                return FindTimeStepWithMinVel(velocityListRight);
        }
        return -1;
    }

    private float FindTimeStepWithMinVel(List<VelocityWithTimeStep> velocityList)
    {
        float min = float.MaxValue;
        float timeStemp = Time.time;
        // find minimum velocity
        // return directly the current minimum when a high velocity value occurs
        for(int i = velocityList.Count - 1; i >= 0; i--)
        {
            VelocityWithTimeStep velocity = velocityList[i];
            if (velocity.velocity < min)
            {
                min = velocity.velocity;
                timeStemp = velocity.timeStemp;
            } else if(velocity.velocity>0.5f)
            {
                return timeStemp;
            }
        }
        return timeStemp;
    }

    public bool VelocityWasOverThSinceTimeStemp(float timeStemp)
    {
        switch (HandManager.CurrentHand.device)
        {
            case RayInputDevice.Myo:
                return VelocityWasOverThresholdSinceTimeStemp(velocityListMyo, timeStemp);
            case RayInputDevice.ControllerLeft:
                return VelocityWasOverThresholdSinceTimeStemp(velocityListLeft, timeStemp);
            case RayInputDevice.ControllerRight:
                return VelocityWasOverThresholdSinceTimeStemp(velocityListRight, timeStemp);
        }
        return false;
    }

    private bool VelocityWasOverThresholdSinceTimeStemp(List<VelocityWithTimeStep> velocityList, float timeStemp)
    {
        for (int i = velocityList.Count - 1; i >= 0; i--)
        {
            VelocityWithTimeStep velocity = velocityList[i];
            if (velocity.velocity > 0.5f)
            {
                return true;
            }
            if(velocity.timeStemp<timeStemp)
            {
                return false;
            }
        }
        return false;
    }

    public class VelocityWithTimeStep
    {

        public float velocity;
        public float timeStemp;

        public VelocityWithTimeStep(float velocity, float timeStemp)
        {
            this.velocity = velocity;
            this.timeStemp = timeStemp;
        }
    }

    public List<VelocityWithTimeStep> VelocityListMyo
    {
        get
        {
            return velocityListMyo;
        }
    }

    public List<VelocityWithTimeStep> VelocityListLeft
    {
        get
        {
            return velocityListLeft;
        }
    }

    public List<VelocityWithTimeStep> VelocityListRight
    {
        get
        {
            return velocityListRight;
        }
    }
}