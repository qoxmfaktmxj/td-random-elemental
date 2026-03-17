using UnityEngine;

namespace TdRandomElemental.Character
{
    [DefaultExecutionOrder(-150)]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputRouter))]
    [RequireComponent(typeof(PlayerDashController))]
    public sealed class PlayerMover : MonoBehaviour
    {
        [Min(0.1f)]
        [SerializeField] private float moveSpeed = 6f;

        [Min(0.1f)]
        [SerializeField] private float acceleration = 20f;

        [Min(0.1f)]
        [SerializeField] private float rotationSharpness = 16f;

        [Min(0f)]
        [SerializeField] private float groundedStickForce = 2f;

        [Min(0f)]
        [SerializeField] private float gravity = 20f;

        private CharacterController characterController;
        private PlayerInputRouter inputRouter;
        private PlayerDashController dashController;
        private Vector3 planarVelocity;
        private float verticalVelocity;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputRouter = GetComponent<PlayerInputRouter>();
            dashController = GetComponent<PlayerDashController>();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Vector3 movementDirection = inputRouter.WorldMoveDirection;

            if (!dashController.IsDashing)
            {
                Vector3 targetVelocity = movementDirection * moveSpeed;
                planarVelocity = Vector3.MoveTowards(planarVelocity, targetVelocity, acceleration * deltaTime);
            }
            else
            {
                planarVelocity = dashController.CurrentVelocity;
            }

            UpdateVerticalVelocity(deltaTime);
            MoveCharacter(deltaTime);
            RotateCharacter(movementDirection);
        }

        private void UpdateVerticalVelocity(float deltaTime)
        {
            if (characterController.isGrounded && verticalVelocity <= 0f)
            {
                verticalVelocity = -groundedStickForce;
                return;
            }

            verticalVelocity -= gravity * deltaTime;
        }

        private void MoveCharacter(float deltaTime)
        {
            Vector3 frameVelocity = planarVelocity;
            frameVelocity.y = verticalVelocity;
            characterController.Move(frameVelocity * deltaTime);
        }

        private void RotateCharacter(Vector3 movementDirection)
        {
            Vector3 facingDirection = dashController.IsDashing
                ? dashController.CurrentVelocity.normalized
                : movementDirection;

            facingDirection.y = 0f;
            if (facingDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(facingDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSharpness * Time.deltaTime);
        }
    }
}
