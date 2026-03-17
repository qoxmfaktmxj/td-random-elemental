using TdRandomElemental.Character;
using UnityEngine;

namespace TdRandomElemental.UI
{
    [DisallowMultipleComponent]
    public sealed class InteractPromptPresenter : MonoBehaviour
    {
        private PlayerInteractionSensor interactionSensor;

        public string CurrentPrompt
        {
            get
            {
                TryResolveSensor();
                return interactionSensor != null ? interactionSensor.CurrentPrompt : string.Empty;
            }
        }

        private void TryResolveSensor()
        {
            if (interactionSensor != null)
            {
                return;
            }

            interactionSensor = FindFirstObjectByType<PlayerInteractionSensor>();
        }
    }
}
