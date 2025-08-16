using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMPlayer : MonoBehaviour
{
    public static BGMPlayer Instance;
    private AudioSource audioSource;

    void Awake()
    {
        // �������ʵ���������ظ���
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // ����Ϊȫ��Ψһʵ��
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
            audioSource.Stop();  // ��ֹ�ۻ�����
            audioSource.Play();
        }
    }
}
