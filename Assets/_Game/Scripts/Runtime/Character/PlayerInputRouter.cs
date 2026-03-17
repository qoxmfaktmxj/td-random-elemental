using TdRandomElemental.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TdRandomElemental.Character
{
    [DefaultExecutionOrder(-300)]
    public sealed class PlayerInputRouter : MonoBehaviour
    {
        private const string RuntimePlayerName = "__RuntimePlayer";

        [Header("Input")]
        [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode sellKey = KeyCode.Q;
        [SerializeField] private Transform cameraTransform;

        private Vector2 moveInput;
        private Vector3 worldMoveDirection;
        private bool dashPressed;
        private bool interactPressed;
        private bool sellPressed;

        public Vector2 MoveInput => moveInput;
        public Vector3 WorldMoveDirection => worldMoveDirection;

        private void Awake()
        {
            TryResolveCamera();
        }

        private void Update()
        {
            TryResolveCamera();
            ReadInput();
        }

        public bool ConsumeDashPressed()
        {
            bool result = dashPressed;
            dashPressed = false;
            return result;
        }

        public bool ConsumeInteractPressed()
        {
            bool result = interactPressed;
            interactPressed = false;
            return result;
        }

        public bool ConsumeSellPressed()
        {
            bool result = sellPressed;
            sellPressed = false;
            return result;
        }

        private void ReadInput()
        {
            moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"));

            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            worldMoveDirection = ResolveWorldMoveDirection(inputDirection);

            dashPressed |= Input.GetKeyDown(dashKey);
            interactPressed |= Input.GetKeyDown(interactKey);
            sellPressed |= Input.GetKeyDown(sellKey);
        }

        private Vector3 ResolveWorldMoveDirection(Vector3 inputDirection)
        {
            if (inputDirection.sqrMagnitude <= 0.001f)
            {
                return Vector3.zero;
            }

            if (cameraTransform == null)
            {
                return inputDirection.normalized;
            }

            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 worldDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
            return worldDirection.sqrMagnitude <= 0.001f
                ? Vector3.zero
                : worldDirection.normalized;
        }

        private void TryResolveCamera()
        {
            if (cameraTransform != null)
            {
                return;
            }

            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureRuntimePlayer()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.name != StartupConfig.MvpArenaSceneName)
            {
                return;
            }

            if (Object.FindFirstObjectByType<PlayerInputRouter>() != null)
            {
                return;
            }

            GameObject playerRoot = new GameObject(RuntimePlayerName);
            playerRoot.transform.position = new Vector3(0f, 0f, -6f);

            CharacterController controller = playerRoot.AddComponent<CharacterController>();
            controller.radius = 0.35f;
            controller.height = 2f;
            controller.center = new Vector3(0f, 1f, 0f);
            controller.stepOffset = 0.3f;

            playerRoot.AddComponent<PlayerInputRouter>();
            playerRoot.AddComponent<PlayerDashController>();
            playerRoot.AddComponent<PlayerInteractionSensor>();
            playerRoot.AddComponent<PlayerMover>();

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(playerRoot.transform, false);
            visual.transform.localPosition = new Vector3(0f, 1f, 0f);
            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Object.Destroy(visualCollider);
            }
        }
    }
}
