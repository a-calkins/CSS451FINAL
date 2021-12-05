using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformNotifier : Notifier<TransformNotifier.Transform>
{
    public class Transform
    {
        public readonly Vector3 translation = Vector3.zero;
        public readonly Vector3 rotation = Vector3.zero;
        public readonly Vector3 scale = Vector3.zero;

        public Transform(Vector3 v)
        {
            translation = v;
        }

        public Transform(Vector3 t, Vector3 r, Vector3 s)
        {
            translation = t;
            rotation = r;
            scale = s;
        }
    }
}
