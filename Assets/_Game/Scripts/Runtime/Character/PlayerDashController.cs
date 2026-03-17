using UnityEngine;

namespace TdRandomElemental.Character
{
    [DefaultExecutionOrder(-200)]
    [RequireComponent(typeof(PlayerInputRouter))]
    public sealed class PlayerDashController : MonoBehaviour
    {
        [Min(0.1f)]
        [SerializeField] private float dashSpeed = 12f;

        [Min(0.01f)]
        [SerializeField] private float dashDuration = 0.18f;

        [Min(0f)]
        [SerializeField] private float dashCooldown = 0.6f;

        private PlayerInputRouter inputRouter;
        private float dashTimeRemaining;
        private float cooldownRemaining;
        private Vector3 dashDirection = Vector3.forward;

        public bool IsDashing => dashTimeRemaining > 0f;
        public Vector3 CurrentVelocity => IsDashing ? dashDirection * dashSpeed : Vector3.zero;

        private void Awake()
        {
            inputRouter = GetComponent<PlayerInputRouter>();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - deltaTime);

            if (dashTimeRemaining > 0f)
            {
                dashTimeRemaining = Mathf.Max(0f, dashTimeRemaining - deltaTime);
                return;
            }

            if (cooldownRemaining > 0f)
            {
                inputRouter.ConsumeDashPressed();
                return;
            }

            if (!inputRouter.ConsumeDashPressed())
            {
                return;
            }

            dashDirection = ResolveDashDirection();
            dashTimeRemaining = dashDuration;
            cooldownRemaining = dashCooldown;
        }

        private Vector3 ResolveDashDirection()
        {
            Vector3 movementDirection = inputRouter.WorldMoveDirection;
            if (movementDirection.sqrMagnitude > 0.001f)
            {
                return movementDirection.normalized;
            }

            Vector3 fallbackDirection = transform.forward;
            fallbackDirection.y = 0f;
            return fallbackDirection.sqrMagnitude > 0.001f
                ? fallbackDirection.normalized
                : Vector3.forward;
        }
    }
}
