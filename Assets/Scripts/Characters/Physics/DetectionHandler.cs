using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Graveyard.CharacterSystem.Detections
{
    [Serializable]
    public struct DetectionHandler
    {
        public bool DebugMode;

        public List<DetectionObject> DetectionObjects;
        public List<RadialDetection> RadialDetections;

        #region Getter Methods
        public DetectionObject GetDetectionObject(string s)
        {
            return DetectionObjects.Find(d => d.DetectionName == s);
        }

        public RadialDetection GetRadialDetection(string s)
        {
            return RadialDetections.Find(r => r.DetectionName == s);
        }
        #endregion

        #region Radial raycast methods
        public Quaternion CalculateSegment(RadialDetection radialDetection)
        {
            radialDetection.AreDetectionsHit = new bool[radialDetection.RayAmount];
            radialDetection.RayCastHits = new RaycastHit[radialDetection.RayAmount];
            Quaternion segment = Quaternion.Euler(0, ((radialDetection.MaxAngleRange) / (radialDetection.RayAmount - 1)), 0);

            radialDetection.Segment = segment;
            return segment;
        }

        public void InitializeRadialDetection(string detectionName, out RadialDetection detection)
        {
            detection = GetRadialDetection(detectionName);
            CalculateSegment(detection);
        }

        public void InitializeRadialDetection(string detectionName, Vector3 position, out RadialDetection detection)
        {
            detection = GetRadialDetection(detectionName);
            detection.detectionObject.transform.localPosition = position;
            CalculateSegment(detection);
        }

        public void UpdateRadialDetection(Vector3 direction, ref RadialDetection radialDetection)
        {
            radialDetection.castDirection = direction;

            for (int i = 0; i < radialDetection.RayAmount; i++)
            {
                Vector3 angleDirection = Quaternion.AngleAxis(radialDetection.castDirection.y - (radialDetection.MaxAngleRange / 2) + (radialDetection.Segment.eulerAngles.y * i), radialDetection.detectionObject.transform.up) * radialDetection.castDirection;
                radialDetection.AreDetectionsHit[i] = Physics.Raycast(radialDetection.detectionObject.transform.position, angleDirection, out radialDetection.RayCastHits[i], radialDetection.length, radialDetection.layerMask, radialDetection.TriggerInteraction);

                if (DebugMode)
                    Debug.DrawRay(radialDetection.detectionObject.transform.position, angleDirection.normalized * radialDetection.length, Color.magenta);
            }

            radialDetection.hitInfo = Array.Find(radialDetection.RayCastHits, ray => ray.collider != null);
            radialDetection.IsObjectDetected = radialDetection.AreDetectionsHit.Any(x => x);
        }
        #endregion
    }

    [Serializable]
    public class DetectionObject
    {
        public string DetectionName;
        public GameObject detectionObject;

        [HideInInspector] public bool IsObjectDetected;

        public event Action<bool> OnValueChange;
        public void ResetOnValueChange() { OnValueChange = null; }

        [HideInInspector] public bool valueChanged = false;
        public void CheckValueChange(bool b)
        {
            if (b == valueChanged) return;

            valueChanged = b;

            OnValueChange?.Invoke(valueChanged);
        }

        public enum DetectionType { overlapSphere = 1, overlapCube = 2, rayCast = 3 };
        public DetectionType detectionType = DetectionType.overlapSphere;

        public LayerMask layerMask;
        public QueryTriggerInteraction TriggerInteraction;

        [HideInInspector]
        public Collider[] detectedCollisions;

        [Space(10)]
        [EnumHide("detectionType", 0, true)]
        public float radius;

        [EnumHide("detectionType", 1, true)]
        public Vector3 detectionBounds;

        [EnumHide("detectionType", 2, true)]
        public Vector3 castDirection;
        [EnumHide("detectionType", 2, true)]
        public float length;

        public RaycastHit hitInfo;
    }

    [Serializable]
    public class RadialDetection : DetectionObject
    {
        public float MaxAngleRange;
        public int RayAmount;

        [HideInInspector] public Quaternion Segment;

        public RaycastHit[] RayCastHits;
        [HideInInspector] public bool[] AreDetectionsHit;
    }
}