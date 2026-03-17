using System;
using TdRandomElemental.Board;
using TdRandomElemental.Summoning;
using UnityEngine;

namespace TdRandomElemental.Character
{
    public interface IPlayerInteractable
    {
        Transform InteractionTransform { get; }
        string InteractionPrompt { get; }
        bool CanInteract(GameObject interactor);
        void Interact(GameObject interactor);
    }

    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(PlayerInputRouter))]
    public sealed class PlayerInteractionSensor : MonoBehaviour
    {
        private readonly Collider[] overlapBuffer = new Collider[16];

        [Min(0.1f)]
        [SerializeField] private float interactionRadius = 1.5f;

        [SerializeField] private LayerMask detectionMask = ~0;

        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

        private PlayerInputRouter inputRouter;

        public event Action<IPlayerInteractable> TargetChanged;

        public IPlayerInteractable CurrentTarget { get; private set; }
        public string CurrentPrompt => CurrentTarget?.InteractionPrompt ?? string.Empty;

        private void Awake()
        {
            inputRouter = GetComponent<PlayerInputRouter>();
        }

        private void Update()
        {
            RefreshTarget();

            if (inputRouter.ConsumeSellPressed())
            {
                TrySell();
            }

            if (!inputRouter.ConsumeInteractPressed())
            {
                return;
            }

            TryInteract();
        }

        public bool TryInteract()
        {
            if (CurrentTarget == null || !CurrentTarget.CanInteract(gameObject))
            {
                return false;
            }

            CurrentTarget.Interact(gameObject);
            return true;
        }

        public bool TrySell()
        {
            if (CurrentTarget is not TowerNode towerNode || TowerSellService.Instance == null)
            {
                return false;
            }

            return TowerSellService.Instance.TrySellNode(towerNode, out _, out _);
        }

        private void RefreshTarget()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(
                transform.position,
                interactionRadius,
                overlapBuffer,
                detectionMask,
                triggerInteraction);

            IPlayerInteractable bestTarget = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hitCollider = overlapBuffer[i];
                if (hitCollider == null)
                {
                    continue;
                }

                IPlayerInteractable candidate = FindInteractable(hitCollider);
                if (candidate == null || !candidate.CanInteract(gameObject))
                {
                    continue;
                }

                float candidateDistance = Vector3.SqrMagnitude(
                    GetInteractablePosition(candidate) - transform.position);

                if (candidateDistance >= bestDistance)
                {
                    continue;
                }

                bestDistance = candidateDistance;
                bestTarget = candidate;
            }

            if (ReferenceEquals(CurrentTarget, bestTarget))
            {
                return;
            }

            if (CurrentTarget is TowerNode currentNode)
            {
                currentNode.SetHighlighted(false);
            }

            CurrentTarget = bestTarget;

            if (CurrentTarget is TowerNode nextNode)
            {
                nextNode.SetHighlighted(true);
            }

            TargetChanged?.Invoke(CurrentTarget);
        }

        private static IPlayerInteractable FindInteractable(Collider hitCollider)
        {
            MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>(true);
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IPlayerInteractable interactable)
                {
                    return interactable;
                }
            }

            return null;
        }

        private static Vector3 GetInteractablePosition(IPlayerInteractable interactable)
        {
            return interactable.InteractionTransform != null
                ? interactable.InteractionTransform.position
                : Vector3.zero;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = CurrentTarget == null ? Color.cyan : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
