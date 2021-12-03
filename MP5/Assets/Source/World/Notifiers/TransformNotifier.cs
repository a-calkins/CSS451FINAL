using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformNotifier : Notifier<TransformNotifier.Transform>
{
    public class Transform
    {
        public readonly Vector3 vector;
        public readonly Quaternion quaternion;

        public Transform(Vector3 v, Quaternion q)
        {
            vector = v;
            quaternion = q;
        }
    }
}
