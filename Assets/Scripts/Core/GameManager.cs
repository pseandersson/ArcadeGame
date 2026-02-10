using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace EchoThief.Core
{
    /// <summary>
    /// Possible game states.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        LevelComplete
    }

    /// <summary>
    /// Singleton that manages high-level game state, level transitions, and win/lose flow.
    /// Persists across scenes via DontDestroyOnLoad.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>Fired when the game state changes. UI and other systems can subscribe.</summary>
        public static event Action<GameState> OnGameStateChanged;

        [Header("Level Settings")]
        [Tooltip("Scene names for each level, in order.")]
        [SerializeField] private string[] _levelSceneNames;

        private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState => _currentState;

        private int _currentLevelIndex = 0;
        public int CurrentLevelIndex => _currentLevelIndex;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Transition to a new game state.
        /// </summary>
        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            Debug.Log($"[GameManager] State changed to: {newState}");

            switch (newState)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f;
                    break;
                case GameState.LevelComplete:
                    Time.timeScale = 1f;
                    break;
            }

            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Start playing a specific level by index.
        /// </summary>
        public void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _levelSceneNames.Length)
            {
                Debug.LogError($"[GameManager] Invalid level index: {levelIndex}");
                return;
            }

            _currentLevelIndex = levelIndex;
            SceneManager.LoadScene(_levelSceneNames[levelIndex]);
            SetState(GameState.Playing);
        }

        /// <summary>
        /// Restart the current level.
        /// </summary>
        public void RestartLevel()
        {
            NoiseEventBus.ClearAll();
            LoadLevel(_currentLevelIndex);
        }

        /// <summary>
        /// Advance to the next level or return to main menu if all levels are done.
        /// </summary>
        public void NextLevel()
        {
            int next = _currentLevelIndex + 1;
            if (next < _levelSceneNames.Length)
            {
                LoadLevel(next);
            }
            else
            {
                Debug.Log("[GameManager] All levels complete!");
                LoadMainMenu();
            }
        }

        /// <summary>
        /// Load the main menu scene.
        /// </summary>
        public void LoadMainMenu()
        {
            NoiseEventBus.ClearAll();
            SceneManager.LoadScene("MainMenu");
            SetState(GameState.MainMenu);
        }

        /// <summary>
        /// Called when the player is caught by a guard.
        /// </summary>
        public void PlayerCaught()
        {
            SetState(GameState.GameOver);
        }

        /// <summary>
        /// Called when the player reaches the exit with the required artifacts.
        /// </summary>
        public void LevelCompleted()
        {
            SetState(GameState.LevelComplete);
        }

        /// <summary>
        /// Toggle pause state.
        /// </summary>
        public void TogglePause()
        {
            if (_currentState == GameState.Playing)
                SetState(GameState.Paused);
            else if (_currentState == GameState.Paused)
                SetState(GameState.Playing);
        }
    }
}
