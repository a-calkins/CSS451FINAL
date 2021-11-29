using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformNotifier : Notifier<TransformNotifier.Transform>
{
    public class Transform
    {
        Vector3 vector;
        Quaternion quaternion;

        public Transform(Vector3 v, Quaternion q)
        {
            vector = v;
            quaternion = q;
        }
    }
}
