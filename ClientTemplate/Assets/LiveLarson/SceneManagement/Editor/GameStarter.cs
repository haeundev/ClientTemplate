#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiveLarson.SceneSystem.Editor
{
    [InitializeOnLoad]
    public static class GameStarter
    {
        private const string LobbyScene = "Assets/Scenes/Lobby.unity";
        private const string DownloadScene = "Assets/Scenes/Download.unity";
        private const string Loading = "Assets/Scenes/Loading.unity";
        private const string OfficeScene = "Assets/Scenes/Office.unity";
        private const string GameScene = "Assets/Scenes/Game.unity";

        static GameStarter()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                Debug.LogWarning("Is editor closed or crashed while playing?");
        }

        [MenuItem("LiveLarson/Start Game %&r", false, 999)] //%&
        private static void StartGame()
        {
            StartGame_impl(true);
        }

        private static void StartGame_impl(bool server, bool noLimit = false)
        {
            EditorSceneManager.OpenScene(LobbyScene);
            EditorApplication.EnterPlaymode();
        }
        
        [MenuItem("LiveLarson/1. Lobby", false)]
        private static void LoadLobbyScene()
        {
            OpenScene(LobbyScene);
        }
        
        [MenuItem("LiveLarson/2. Download", false)]
        private static void LoadDownloadScene()
        {
            OpenScene(DownloadScene);
        }
        
        [MenuItem("LiveLarson/3. Loading", false)]
        private static void LoadLoadingScene()
        {
            OpenScene(Loading);
        }

        [MenuItem("LiveLarson/4. Office", false)]
        private static void LoadOfficeScene()
        {
            OpenScene(OfficeScene);
        }

        [MenuItem("LiveLarson/5. Game", false)]
        private static void LoadGameScene()
        {
            OpenScene(GameScene);
        }

        private static void OpenScene(string scenePath)
        {
            if (SceneManager.GetActiveScene().isDirty) EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            EditorSceneManager.OpenScene(scenePath);
        }
    }
}
#endif