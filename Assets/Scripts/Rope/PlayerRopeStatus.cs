using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RopeStatus
{
    None,
    ConnectedWithClimbAttacher,
    ConnectedWithAHeavierPlayer,
    ConnectedWithALighterPlayer
}

public class PlayerRopeStatus : MonoBehaviour
{
    public RopeStatus ropeStatus;

    private void Start()
    {
        ropeStatus = RopeStatus.None;
    }
}
