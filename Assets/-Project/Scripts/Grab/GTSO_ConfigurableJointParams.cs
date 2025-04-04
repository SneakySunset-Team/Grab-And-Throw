using Sirenix.OdinInspector;
using UnityEngine;


[CreateAssetMenu(fileName = "Joint Settings", menuName = "Scriptable Objects/JointSettings"), InlineEditor]
public class GTSO_ConfigurableJointParams : ScriptableObject
{
    public ConfigurableJointMotion XMotion = ConfigurableJointMotion.Locked;
    public ConfigurableJointMotion YMotion = ConfigurableJointMotion.Locked;
    public ConfigurableJointMotion ZMotion = ConfigurableJointMotion.Locked;

    public ConfigurableJointMotion AngularXMotion = ConfigurableJointMotion.Locked;
    public ConfigurableJointMotion AngularYMotion = ConfigurableJointMotion.Locked;
    public ConfigurableJointMotion AngularZMotion = ConfigurableJointMotion.Locked;

    public float LinearLimit = 0;
    public float LinearBounciness = 0;
    public float LinearContactDistance = 0;

    public float LinearSpring = 0;
    public float LinearDamperSpring = 0;

    public void InitializeJoint(ref ConfigurableJoint configurableJoint)
    {
        configurableJoint.xMotion = XMotion;
        configurableJoint.yMotion = YMotion;
        configurableJoint.zMotion = ZMotion;

        configurableJoint.angularXMotion = XMotion;
        configurableJoint.angularYMotion = YMotion;
        configurableJoint.angularZMotion = ZMotion;

        SoftJointLimit jointLimit = new SoftJointLimit();
        jointLimit.limit = LinearLimit;
        jointLimit.bounciness = LinearBounciness;
        jointLimit.contactDistance = LinearContactDistance;
        configurableJoint.linearLimit = jointLimit;

        SoftJointLimitSpring springLimit = new SoftJointLimitSpring();
        springLimit.spring = LinearSpring;
        springLimit.damper = LinearDamperSpring;
        configurableJoint.linearLimitSpring = springLimit;
    }
}
