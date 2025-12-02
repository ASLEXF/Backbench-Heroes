using FS_SwingSystem;
using FS_ThirdPerson;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rope
{
    [System.Serializable]
    public struct ObstacleHitData
    {
        public bool forwardHitFound;
        public bool heightHitFound;
        public bool ledgeHitFound;
        public RaycastHit forwardHit;
        public RaycastHit heightHit;
        public RaycastHit ledgeHit;
        public bool hasSpaceToVault;
        public bool hasSpace;
    }

    [System.Serializable]
    public class SwingData
    {
        public Vector3 hookPosition;
        public bool hasLedge;
        public Vector3 directionToHook;
        public Vector3 forwardDirection;
        public float distance;
    }

    public class RopePointScanner : MonoBehaviour
    {
        [SerializeField] float heightRayLength = 4;
        [SerializeField] float obstacleCheckRange = 0.7f;
        public float ledgeHeightThreshold = 1f;

        private void Awake()
        {
            if (characterCapsuleCollider == null)
                characterCapsuleCollider = GetComponent<CapsuleCollider>();
        }

        private void Start()
        {
            if (characterCapsuleCollider != null)
                characterCapsuleHalfSize = new Vector3(characterCapsuleCollider.radius, characterCapsuleCollider.height / 2, characterCapsuleCollider.radius * 1.2f);
        }

        private void Update()
        {
            obstacleHitData = ObstacleCheck();
            swingData = GetSwingLedgeData(9f, 2f, transform, true);
        }

        #region Vault Ledge Detection

        [field: SerializeField] public LayerMask ObstacleLayer { get; set; } = 1;
        [SerializeField] ObstacleHitData obstacleHitData;

        [SerializeField] CapsuleCollider characterCapsuleCollider;
        private Vector3 characterCapsuleHalfSize;
        [SerializeField][Range(0.5f, 1)] float colliderScale = 0.8f;

        [SerializeField] float maxCanStandAngle = 45f;

        public ObstacleHitData ObstacleCheck(bool performHeightCheck = true, float forwardOriginOffset = 1f)
        {
            ObstacleHitData hitData = new ObstacleHitData();

            Vector3 forwardOrigin = transform.position + transform.forward * forwardOriginOffset;
            hitData.forwardHitFound = Physics.BoxCast(forwardOrigin, new Vector3(0.1f, 0.7f, 0.01f), transform.forward, out hitData.forwardHit, Quaternion.LookRotation(transform.forward), obstacleCheckRange, ObstacleLayer);
            Debug.DrawRay(forwardOrigin, transform.forward * obstacleCheckRange, (hitData.forwardHitFound) ? Color.red : Color.white);

            if (hitData.forwardHitFound && performHeightCheck)
            {
                Vector3 heightOrigin = hitData.forwardHit.point + Vector3.up * heightRayLength;
                //Gizmos.DrawSphere(hitData.forwardHit.point, 0.2f);

                Vector3 spaceCheckOrigin = transform.position + Vector3.up * heightRayLength;
                spaceCheckOrigin.y = heightOrigin.y;
                if (Physics.Raycast(spaceCheckOrigin, Vector3.down, out hitData.heightHit, heightRayLength, ObstacleLayer) && hitData.heightHit.point.y > hitData.forwardHit.point.y)
                    heightOrigin.y = hitData.heightHit.point.y;

                for (int i = 0; i < 4; ++i)
                {
                    hitData.heightHitFound = Physics.SphereCast(heightOrigin, 0.2f, Vector3.down, out hitData.heightHit, heightRayLength, ObstacleLayer);
                    //Gizmos.DrawSphere(heightOrigin, 0.2f);


                    if (hitData.heightHitFound && Vector3.Angle(Vector3.up, hitData.heightHit.normal) <= maxCanStandAngle)
                        break;

                    hitData.heightHitFound = false;
                    heightOrigin += transform.forward * 0.15f;
                }

                if (hitData.heightHitFound)
                {
                    // Check for space to stand
                    forwardOrigin = hitData.heightHit.point;
                    forwardOrigin.y = hitData.heightHit.point.y + 0.2f + colliderScale * 1.5f / 2;
                    hitData.hasSpace = !Physics.CheckBox(forwardOrigin, characterCapsuleHalfSize * colliderScale, Quaternion.LookRotation(transform.forward), ObstacleLayer);
                    // Check for space to vault
                    Vector3 spaceOrigin = hitData.heightHit.point + transform.forward * 0.5f + Vector3.up * 0.6f;
                    RaycastHit spaceHit;
                    hitData.hasSpaceToVault = Physics.SphereCast(spaceOrigin, 0.1f, Vector3.down, out spaceHit, 1f, ObstacleLayer);
                    // Check for ledge
                    Vector3 dir = hitData.heightHit.point - transform.position;
                    dir.y = 0;
                    heightOrigin = hitData.heightHit.point;
                    heightOrigin.y += 0.8f;
                    hitData.ledgeHit = hitData.heightHit;
                    int i = 1;
                    for (; i <= 6; ++i)
                    {
                        bool ledgeHitFound = Physics.CheckSphere(heightOrigin, 0.4f, ObstacleLayer);

                        if (!ledgeHitFound)
                        {
                            ledgeHitFound = Physics.SphereCast(heightOrigin, 0.3f, Vector3.down, out RaycastHit ledgeHit, 2f, ObstacleLayer);

                            if (ledgeHitFound && Mathf.Abs(ledgeHit.point.y - hitData.heightHit.point.y) < 0.4f)
                            {
                                hitData.ledgeHit = ledgeHit;
                                hitData.ledgeHitFound = true;
                            }
                            else
                                break;

                            heightOrigin += transform.forward * 0.4f;
                        }
                        else
                        {
                            hitData.ledgeHitFound = false;
                            break;
                        }
                    }
                    if (hitData.ledgeHitFound)
                        hitData.ledgeHitFound = !Physics.CheckSphere(hitData.ledgeHit.point + Vector3.up * 0.5f, 0.3f, ObstacleLayer) && i < 7;
                }
            }
            return hitData;
        }

        #endregion

        #region Swing Ledge Detection

        [field: SerializeField] public LayerMask LedgeLayer { get; set; } = 1;
        [SerializeField] SwingData swingData;

        [field: SerializeField] public LayerMask SwingLedgeLayer { get; set; } = 1;

        Vector3 s_pathCheckExtend = new Vector3(0.05f, .1f, 0.01f);

        float s_capsuleRadius = 1.5f;
        float s_minimumHookAngle = 5;

        public SwingData GetSwingLedgeData(float maxDistance, float minDistance, Transform ropeHoldPoint, bool debug)
        {
            List<Vector3> detectedHookPoints = new List<Vector3>();
            Vector3 direction = transform.forward + transform.up * 0.25f;
            Vector3 checkOrigin = transform.position + transform.up * 0.75f + direction.normalized * s_capsuleRadius * 2;
            bool hasLedge = false;
            float radius = s_capsuleRadius;
            float coveredDistance = 0f;
            float totalCastLength = 0f;
            float singleCastDist = 5f;

            Vector3 hookPos = Vector3.zero;
            Vector3 dir = Vector3.zero;
            float distancToHookPos = 0;
            var disToOrigin = Vector3.Distance(checkOrigin, ropeHoldPoint.position);

            for (float i = 0; totalCastLength < maxDistance - disToOrigin; i++)
            {
                var castLength = (i + 1) * singleCastDist + radius * 2;

                var hits = Physics.SphereCastAll(
                   checkOrigin + direction.normalized * coveredDistance,
                   radius,
                   direction,
                   castLength,
                   SwingLedgeLayer);

                hasLedge = hits.Length > 0;

                if (hasLedge)
                {
                    foreach (var hit in hits)
                    {
                        var swingLedge = hit.transform.gameObject.GetComponent<SwingLedge>();
                        if (swingLedge != null)
                        {
                            hookPos = swingLedge.transform.TransformPoint(swingLedge.hookPoint);
                            var o = transform.position + transform.up * .6f;
                            var distance = (o - hookPos).magnitude;
                            var hasPath = !Physics.BoxCast(
                                    o,
                                    s_pathCheckExtend,
                                    (hookPos - o).normalized,
                                    out RaycastHit pathHit,
                                    Quaternion.LookRotation(hookPos),
                                    distance - .5f,
                                    ObstacleLayer
                                );
                            distancToHookPos = (ropeHoldPoint.position - hookPos).magnitude;
                            hasLedge = hasPath && distancToHookPos > minDistance && distancToHookPos <= maxDistance;

                            if (hasLedge)
                                detectedHookPoints.Add(hookPos);

                            if (debug)
                            {
                                BoxCastDebug.DrawBoxCastBox(
                                       o,
                                       s_pathCheckExtend,
                                       Quaternion.LookRotation(hookPos),
                                       (hookPos - o).normalized,
                                       distance - .5f,
                                       Color.green
                                   );
                            }
                        }
                    }
                }

                coveredDistance += castLength - radius;
                totalCastLength += castLength + 2 * radius;

                radius += s_capsuleRadius * 1.5f;
            }

            if (detectedHookPoints.Count > 0)
            {
                hookPos = s_GetAccurateHookPosition(checkOrigin, direction, detectedHookPoints);
                var hp = hookPos;
                distancToHookPos = (ropeHoldPoint.position - hookPos).magnitude;
                hp.y = transform.position.y;
                dir = (hp - transform.position).normalized;
            }


            SwingData data = new SwingData()
            {
                hasLedge = detectedHookPoints.Count > 0,
                hookPosition = hookPos,
                forwardDirection = dir,
                directionToHook = (hookPos - transform.position).normalized,
                distance = distancToHookPos
            };


            return data;
        }

        Vector3 s_GetAccurateHookPosition(Vector3 origin, Vector3 direction, List<Vector3> hookPositions)
        {
            Vector3 hookPos = hookPositions.First();
            float previousAngle = 400;
            Vector3 bestPos = hookPos;
            foreach (var hp in hookPositions)
            {
                var dir = (hp - origin).normalized;
                var angle = Vector3.Angle(direction.normalized, dir);
                if (angle < s_minimumHookAngle)
                    return hp;
                else if (angle < previousAngle)
                    bestPos = hp;
                previousAngle = angle;
            }

            return bestPos;
        }

        #endregion
    }
}
