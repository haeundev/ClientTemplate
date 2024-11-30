using UnityEngine;

namespace LiveLarson.Util
{
    public class AppPausedHandler : MonoBehaviour
    {
        /// <summary>
        ///     Invoked when the application gains or loses focus.
        /// </summary>
        /// <param name="hasFocus">True if the application gains focus, false otherwise.</param>
        private void OnApplicationFocus(bool hasFocus)
        {
            HandlePauseState(!hasFocus);
        }

        /// <summary>
        ///     Invoked when the application is paused or resumed.
        /// </summary>
        /// <param name="isPaused">True if the application is paused, false otherwise.</param>
        private void OnApplicationPause(bool isPaused)
        {
            HandlePauseState(isPaused);
        }

        /// <summary>
        ///     Handles the application's paused state.
        /// </summary>
        /// <param name="isPaused">True if the application is paused, false otherwise.</param>
        private void HandlePauseState(bool isPaused)
        {
            if (isPaused)
                OnAppPaused();
            else
                OnAppResumed();
        }

        /// <summary>
        ///     Called when the application is paused.
        ///     Override or extend this method to add custom behavior.
        /// </summary>
        protected virtual void OnAppPaused()
        {
            Debug.Log("Application paused.");
            // Add custom pause logic here (e.g., save progress, mute sounds).
        }

        /// <summary>
        ///     Called when the application is resumed.
        ///     Override or extend this method to add custom behavior.
        /// </summary>
        protected virtual void OnAppResumed()
        {
            Debug.Log("Application resumed.");
            // Add custom resume logic here (e.g., resume audio, refresh state).
        }
    }
}