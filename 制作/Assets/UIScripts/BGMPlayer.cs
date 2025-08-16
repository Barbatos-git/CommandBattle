using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMPlayer : MonoBehaviour
{
    public static BGMPlayer Instance;
    private AudioSource audioSource;

    void Awake()
    {
        // 如果已有实例，销毁重复的
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 设置为全局唯一实例
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StartScene")
        {
            audioSource.Stop();  // 防止累积播放
            audioSource.Play();
        }
    }
}
