﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum JointType
{
    free,
    hinge
}

public enum Axis
{
    x,
    y,
    z
}

[Serializable]
public struct Joint
{
    public JointType type;

    public Transform transform;

    //Constraints for orientation in degrees, from -180 to 180
    public float phiMin, phiMax;

    //Axis constraint for hinge joints
    public Axis axis;
}

public class InverseKinematics : MonoBehaviour
{
    [SerializeField]
    private Transform origin;//Only for the purpose of constraining the rotation of the first joint
    
    [SerializeField]
    private List<Joint> joints;

    [SerializeField]
    private Transform target;

    private List<float> lengths;

    void Awake()
    {
        lengths = new List<float>();
        for (int i = 0; i < joints.Count - 1; ++i)
        {
            lengths.Add(Vector3.Distance(joints[i].transform.position, joints[i + 1].transform.position));
        }
    }

    void Update()
    {
        fabrik_solve();
    }

    private void fabrik_solve() {
        List<Vector3> joint_positions = new List<Vector3>();
        List<Quaternion> joint_rotations = new List<Quaternion>();
        foreach (Joint j in joints)
        {
            joint_positions.Add(j.transform.position);
            joint_rotations.Add(j.transform.rotation);
        }

        float tolerance = 0.1f * lengths.Sum();
        if (Math.Abs(Vector3.Distance(joint_positions[0], target.position)) >= lengths.Sum())
        {
            Vector3 dir = target.position - joint_positions[0];
            Joint j = joints[0];
            joint_rotations[0] = constrain_spin(dir, reorient(dir, origin.rotation), -j.phiMax, -j.phiMin);
            for (int i = 1; i < joints.Count; ++i)
            {
                joint_positions[i] = joint_positions[i - 1] + (lengths[i - 1] / dir.magnitude) * dir;
                j = joints[i];
                Quaternion q = constrain_spin(dir, joint_rotations[i - 1], j.phiMin, j.phiMax);
                dir = get_direction(joint_positions[i], target.position, q, j);
                joint_rotations[i] = reorient(dir, q);
            }
        } else
        {
            int num_loops = 0;
            float dif = float.PositiveInfinity;
            while (dif > tolerance)
            {
                //First pass: end to base
                joint_positions[lengths.Count] = target.position;
                joint_rotations[lengths.Count] = target.rotation;
                Joint j;
                Vector3 dir;
                for (int i = lengths.Count - 1; i > 0; --i)
                {
                    j = joints[i + 1];
                    //TODO: direction constraints from end to base? Implement after finishing get_direction maybe, too difficult rn
                    dir = joint_positions[i + 1] - joint_positions[i];
                    joint_positions[i] = joint_positions[i + 1] - (lengths[i] / dir.magnitude) * dir;
                    joint_rotations[i] = constrain_spin(dir, reorient(dir, joint_rotations[i + 1]), -j.phiMax, -j.phiMin);
                }

                //Second pass: base to end
                dir = joint_positions[1] - joint_positions[0];
                j = joints[0];
                joint_rotations[0] = constrain_spin(dir, reorient(dir, origin.rotation), -j.phiMax, -j.phiMin);
                Quaternion q;
                for (int i = 1; i < lengths.Count; ++i)
                {
                    joint_positions[i] = joint_positions[i - 1] + (lengths[i - 1] / dir.magnitude) * dir;
                    j = joints[i];
                    q = constrain_spin(dir, joint_rotations[i - 1], j.phiMin, j.phiMax);
                    dir = get_direction(joint_positions[i], joint_positions[i + 1], q, j);
                    joint_rotations[i] = reorient(dir, q);
                }
                
                //End effector
                joint_positions[lengths.Count] = joint_positions[lengths.Count - 1] + (lengths[lengths.Count - 1] / dir.magnitude) * dir;
                j = joints[lengths.Count];
                q = constrain_spin(dir, joint_rotations[lengths.Count - 1], j.phiMin, j.phiMax);
                dir = get_direction(joint_positions[lengths.Count], target.position, q, j);
                joint_rotations[lengths.Count] = reorient(dir, q);
                //TODO: implement a special type of constraint for the wrist/foot that tries to align with an object/the ground
                //TODO: stop the weird hand behavior that happens because the end effector overshoots the target

                dif = Math.Abs(Vector3.Distance(joint_positions[lengths.Count], target.position));
                ++num_loops;
                if (num_loops > 10)
                {
                    Debug.Log("IK took too long to converge!");
                    break;
                }
            }
        }

        for (int i = 0; i < joints.Count; ++i)
        {
            joints[i].transform.rotation = joint_rotations[i];
        }
    }

    //Reorients an existing global orientation to point in a specific direction
    private Quaternion reorient(Vector3 dir, Quaternion rot)
    {
        return Quaternion.FromToRotation(rot * Vector3.right, dir) * rot;
    }

    private Quaternion constrain_spin(Vector3 axis, Quaternion rot, float phiMin, float phiMax)
    {
        return Quaternion.AngleAxis(Mathf.Clamp(0f, phiMin, phiMax), axis) * rot;
    }

    //Constrains the direction vector between from and to, according to the angle constraints of the joint at from
    private Vector3 get_direction(Vector3 from, Vector3 to, Quaternion rot, Joint j)
    {
        if (j.type == JointType.hinge)
        {
            //n defines the plane of the joint
            Vector3 n = rot * ((j.axis == Axis.x) ? Vector3.right :
                               (j.axis == Axis.y) ? Vector3.up :
                                                    Vector3.forward);
            //TODO
            //return dir;
        }
        return to - from;
    }
}