using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiamondMind.Prototypes.Characters.TPS
{
    public class FootIK : MonoBehaviour
    {
        [Header("---------- Raycast Options ----------")]
        public bool drawGizmos;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float raycastMaxDistance = 1f;
        [SerializeField] private float raycastHeight = 0.5f;

        [Header("---------- Foot IK Settings ----------")]
        [Tooltip("Controls the overall influence of the foot IK system on the character's animation")]
        [Range(0, 1)][SerializeField] private float footIKBlendWeight = 1;
        [SerializeField] private bool enableFootIK = true;
        [Tooltip("The vertical position of the character's feet relative to the ground during foot placement calculations")]
        [SerializeField] private float footHeight = 0.02f;
        [SerializeField] private float maxStepHeight = 0.4f;
        [SerializeField] private float footHeightMultiplier = 0.6f;

        [Header("---------- Body Placement Settings ----------")]
        [Tooltip("The rate at which the character's body adjusts its height to match the terrain or foot placement changes")]
        [SerializeField] private float bodyHeightAdjustmentSpeed = 10f;
        [SerializeField] private float maxBodyAdjustmentHeight = 0.65f;

        #region ---------- Private Variables ----------
        Animator _animator;
        TPInput _tpInput;
        TPMovement _character;

        bool initialized;
        bool smoothIKTransition = true; // Whether the transition between IK and FK for the feet is smoothed or not
        bool isGrounded;
        bool useDynamicBodyPlacing;
        bool disableBodyPositioning;

        Vector3 smoothedLeftFootPosition, smoothedRightFootPosition; 
        Quaternion smoothedLeftFootRotation, smoothedRightFootRotation;
        float leftFootHeight, rightFootHeight; 
        RaycastHit leftFootGroundHit, rightFootGroundHit; 
        Transform rightFootPlaceBase, leftFootPlaceBase; 
        Transform leftFoot, rightFoot; 
        Transform leftFootForwardAnchor, rightFootForwardAnchor; 
        float animLeftFootPosY, animRightFootPosY; 
        float transitionIKtoFKWeight; // Weight for transition from IK to FK
        float leftFootHeightFromGround, rightFootHeightFromGround; 
        float leftFootRotationWeight, rightFootRotationWeight; 
        bool leftHit, rightHit; 

        RaycastHit hitGround; 
        float lastBodyPositionY;
        Vector3 newAnimationBodyPosition; 
        float bodyPositionOffset;
        float animation_Y_BodyPosition; // Real body position without changes
        float groundAngle;
#endregion

        private void Start()
        {
            _tpInput = GetComponent<TPInput>();
            _character = _tpInput._tPMovement;
            _animator = _character._animator;

            if (!initialized)
                Initialization();
        }

        #region ---------- Initialization ----------

        private void Initialization()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();

            // Retrieve references to the left and right foot bones
            leftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);

            /* Initialize smoothed foot positions and rotations
             * Adjust the smoothed foot's position slightly backward, which can help smooth out foot movements and improve stability, 
             * especially during animations or when navigating uneven terrain.
             */
            smoothedLeftFootPosition = leftFoot.position - transform.forward * 0.1f;
            smoothedRightFootPosition = rightFoot.position - transform.forward * 0.1f;
            smoothedLeftFootRotation = leftFoot.rotation;
            smoothedRightFootRotation = rightFoot.rotation;

            // Create GameObjects to serve as foot placement bases
            leftFootPlaceBase = CreateFootBase("Left Foot Position");
            rightFootPlaceBase = CreateFootBase("Right Foot Position");

            // Create GameObjects to serve as foot base transforms for height adjustments
            leftFootForwardAnchor  = CreateFootUpBase("Left Foot Forward Anchor", leftFoot);
            rightFootForwardAnchor = CreateFootUpBase("Right Foot Forward Anchor", rightFoot);
            initialized = true;
        }

        private Transform CreateFootBase(string name)
        {
            GameObject baseGo = new GameObject(name);
            baseGo.transform.position = Vector3.zero;
            baseGo.hideFlags = HideFlags.HideAndDontSave;
            return baseGo.transform;
        }

        private Transform CreateFootUpBase(string name, Transform parent)
        {
            GameObject baseGo = new GameObject(name);
            baseGo.transform.position = parent.position;
            baseGo.transform.SetParent(parent);
            baseGo.hideFlags = HideFlags.HideAndDontSave;
            return baseGo.transform;
        }

        #endregion

        private void Update()
        {
            if (!_character.isGrounded || _character.isJumping || _character.customAction || _character.inputMagnitude > 0.5f)
            {
                disableBodyPositioning = true;
                useDynamicBodyPlacing = false;
                smoothIKTransition = false;
            }
            else
            {
                smoothIKTransition = true;
                if (_character.isCrouching)
                    useDynamicBodyPlacing = false;
                else
                    useDynamicBodyPlacing = true;
            }
        }

        private void OnAnimatorIK()
        {
            UpdateFootIK();

            animation_Y_BodyPosition = _animator.bodyPosition.y;
            if (transitionIKtoFKWeight < 0.1f || footIKBlendWeight < 0.01f || rightFootGroundHit.point == Vector3.zero || rightFootGroundHit.point == Vector3.zero)
                return;

            if (enableFootIK == true)
            {
                //Get position before IK Correction
                animLeftFootPosY = transform.position.y - (leftFoot.position.y - footHeight);
                animRightFootPosY = transform.position.y - (rightFoot.position.y - footHeight);
                animLeftFootPosY = Mathf.Abs(animLeftFootPosY);
                animRightFootPosY = Mathf.Abs(animRightFootPosY);

                animLeftFootPosY = Mathf.Clamp(animLeftFootPosY, 0, 1);
                animRightFootPosY = Mathf.Clamp(animRightFootPosY, 0, 1);

                BodyPlacement();

                ApplyFootIK(AvatarIKGoal.LeftFoot, smoothedLeftFootPosition, leftHit, leftFootGroundHit, smoothedLeftFootRotation, leftFootRotationWeight);
                ApplyFootIK(AvatarIKGoal.RightFoot, smoothedRightFootPosition, rightHit, rightFootGroundHit, smoothedRightFootRotation, rightFootRotationWeight);

            }
        }


        #region ---------- Foot IK ----------
        private void UpdateFootIK()
        {
            UpdateFootHeight();

            Physics.SphereCast(leftFoot.position + transform.up * raycastHeight + leftFootForwardAnchor .forward * 0.12f, 0.1f, -transform.up, out leftFootGroundHit, raycastMaxDistance, groundLayers);
            Physics.SphereCast(rightFoot.position + transform.up * raycastHeight + rightFootForwardAnchor.forward * 0.12f, 0.1f, -transform.up, out rightFootGroundHit, raycastMaxDistance, groundLayers);

            UpdateSingleFootPlacement(leftFoot, leftFootPlaceBase, leftFootHeightFromGround, leftFootGroundHit, ref smoothedLeftFootPosition, ref leftHit);
            UpdateSingleFootPlacement(rightFoot, rightFootPlaceBase, rightFootHeightFromGround, rightFootGroundHit, ref smoothedRightFootPosition, ref rightHit);

            UpdateSingleFootRotation(leftFootGroundHit.normal, ref smoothedLeftFootRotation);
            UpdateSingleFootRotation(rightFootGroundHit.normal, ref smoothedRightFootRotation);

            leftFootRotationWeight = Mathf.Lerp(leftFootRotationWeight, leftFootHeightFromGround < 0.3f ? 1 : 0, 8 * Time.deltaTime);
            rightFootRotationWeight = Mathf.Lerp(rightFootRotationWeight, rightFootHeightFromGround < 0.3f ? 1 : 0, 8 * Time.deltaTime);

            transitionIKtoFKWeight = Mathf.Lerp(transitionIKtoFKWeight, smoothIKTransition ? 1 : 0, 5 * Time.deltaTime);
        }

        private void UpdateFootHeight()
        {
            leftFootHeightFromGround = footHeightMultiplier * animLeftFootPosY;
            rightFootHeightFromGround = footHeightMultiplier * animRightFootPosY;

            leftFootHeight = footHeight - Vector3.SignedAngle(leftFootForwardAnchor .up, transform.up, transform.right) / 500;
            rightFootHeight = footHeight - Vector3.SignedAngle(rightFootForwardAnchor.up, transform.up, transform.right) / 500;

            leftFootHeight = Mathf.Clamp(leftFootHeight, -0.2f, 0.2f);
            rightFootHeight = Mathf.Clamp(rightFootHeight, -0.2f, 0.2f);
        }

        private void UpdateSingleFootPlacement(Transform foot, Transform footPlaceBase, float footHeightFromGround, RaycastHit hitPlaceBase, ref Vector3 smoothedFootPosition, ref bool hit)
        {
            if (hitPlaceBase.point != Vector3.zero)
            {
                footPlaceBase.position = hitPlaceBase.point;
                footPlaceBase.rotation = Quaternion.FromToRotation(transform.up, hitPlaceBase.normal) * transform.rotation;
                hit = true;
            }
            else
            {
                footPlaceBase.position = foot.position;
                hit = false;
            }

            if (hit && hitPlaceBase.point.y < transform.position.y + maxStepHeight)
            {
                smoothedFootPosition = Vector3.Lerp(smoothedFootPosition, footPlaceBase.position + hitPlaceBase.normal * footHeightFromGround + transform.up * footHeight, 15 * Time.deltaTime);
            }
            else
            {
                smoothedFootPosition = Vector3.Lerp(smoothedFootPosition, transform.position + transform.up * footHeight + transform.up * footHeightFromGround, 15 * Time.deltaTime);
            }
        }

        private void UpdateSingleFootRotation(Vector3 hitNormal, ref Quaternion smoothedFootRotation)
        {
            Vector3 rotAxis = Vector3.Cross(Vector3.up, hitNormal);
            float angle = Vector3.Angle(Vector3.up, hitNormal);
            Quaternion rotation = Quaternion.AngleAxis(angle * footIKBlendWeight, rotAxis);
            smoothedFootRotation = Quaternion.Lerp(smoothedFootRotation, rotation, 20 * Time.deltaTime);
        }

        private void ApplyFootIK(AvatarIKGoal foot, Vector3 footPosition, bool hit, RaycastHit hitPlaceBase, Quaternion footRotation, float rotationWeight)
        {
            if (hit && hitPlaceBase.point.y < transform.position.y + raycastHeight)
            {
                Vector3 pos = new Vector3(foot == AvatarIKGoal.LeftFoot ? leftFoot.position.x : rightFoot.position.x, footPosition.y, foot == AvatarIKGoal.LeftFoot ? leftFoot.position.z : rightFoot.position.z);
                _animator.SetIKPosition(foot, pos);
                _animator.SetIKPositionWeight(foot, footIKBlendWeight * transitionIKtoFKWeight);

                _animator.SetIKRotationWeight(foot, footIKBlendWeight * transitionIKtoFKWeight * rotationWeight);
                _animator.SetIKRotation(foot, footRotation * _animator.GetIKRotation(foot));
            }
        }

        #endregion

        #region ---------- Body Positioning ----------
        private void BodyPlacement()
        {
            isGrounded = _tpInput._tPMovement.isGrounded;
            hitGround = _tpInput._tPMovement.GetGroundHit();
            groundAngle = _tpInput._tPMovement.GroundAngle();

            if (isGrounded && !IsInvoking(nameof(DisableBlock)) && disableBodyPositioning)
            {
                Invoke(nameof(DisableBlock), 0.5f);
            }

            if (useDynamicBodyPlacing && !disableBodyPositioning)
            {
                UpdateDynamicBodyPlacement();
            }
            else
            {
                if (isGrounded && !disableBodyPositioning)
                {
                    UpdateStaticBodyPlacement();
                }
            }
        }

        private void UpdateDynamicBodyPlacement()
        {
            if (leftFootGroundHit.point == Vector3.zero || rightFootGroundHit.point == Vector3.zero || lastBodyPositionY == 0)
            {
                lastBodyPositionY = animation_Y_BodyPosition;
                bodyPositionOffset = 0;
                newAnimationBodyPosition = _animator.bodyPosition;
                return;
            }

            float leftOffsetBodyPosition = leftFootGroundHit.point.y - transform.position.y - rightFootHeightFromGround / 2;
            float rightOffsetBodyPosition = rightFootGroundHit.point.y - transform.position.y - leftFootHeightFromGround / 2;

            bodyPositionOffset = Mathf.Clamp((leftOffsetBodyPosition < rightOffsetBodyPosition) ? leftOffsetBodyPosition : rightOffsetBodyPosition, -maxBodyAdjustmentHeight, 0);

            float force = bodyHeightAdjustmentSpeed + (groundAngle / 20);
            newAnimationBodyPosition = _animator.bodyPosition + transform.up * bodyPositionOffset;
            newAnimationBodyPosition.y = Mathf.Lerp(lastBodyPositionY, newAnimationBodyPosition.y, force * Time.deltaTime);

            float dist = Mathf.Abs(animation_Y_BodyPosition - lastBodyPositionY);
            if (dist < 1)
            {
                _animator.bodyPosition = newAnimationBodyPosition;
            }

            lastBodyPositionY = _animator.bodyPosition.y;
        }

        private void UpdateStaticBodyPlacement()
        {
            newAnimationBodyPosition = _animator.bodyPosition + transform.up * bodyPositionOffset;
            newAnimationBodyPosition.y = Mathf.Lerp(lastBodyPositionY, animation_Y_BodyPosition, bodyHeightAdjustmentSpeed * Time.deltaTime);
            _animator.bodyPosition = newAnimationBodyPosition;
            lastBodyPositionY = _animator.bodyPosition.y;
        }

        private void DisableBlock()
        {
            disableBodyPositioning = false;
            lastBodyPositionY = animation_Y_BodyPosition;
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            if (!drawGizmos)
                return;

            if (!initialized)
            {
                Initialization();
            }

            // Visualize left foot sphere cast
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(leftFoot.position + transform.up * raycastHeight + leftFootForwardAnchor.forward * 0.12f, 0.1f);

            // Visualize right foot sphere cast
            Gizmos.DrawWireSphere(rightFoot.position + transform.up * raycastHeight + rightFootForwardAnchor.forward * 0.12f, 0.1f);

            // Visualize raycast height
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftFoot.position + transform.up * raycastHeight + leftFootForwardAnchor.forward * 0.12f, leftFoot.position + transform.up * raycastHeight + leftFootForwardAnchor.forward * 0.12f - transform.up * raycastMaxDistance);
            Gizmos.DrawLine(rightFoot.position + transform.up * raycastHeight + rightFootForwardAnchor.forward * 0.12f, rightFoot.position + transform.up * raycastHeight + rightFootForwardAnchor.forward * 0.12f - transform.up * raycastMaxDistance);

            // Visualize max distance
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftFoot.position + transform.up * raycastHeight + leftFootForwardAnchor.forward * 0.12f, leftFoot.position + transform.up * raycastHeight + leftFootForwardAnchor.forward * 0.12f + transform.forward * raycastMaxDistance);
            Gizmos.DrawLine(rightFoot.position + transform.up * raycastHeight + rightFootForwardAnchor.forward * 0.12f, rightFoot.position + transform.up * raycastHeight + rightFootForwardAnchor.forward * 0.12f + transform.forward * raycastMaxDistance);
        }

    }
}
