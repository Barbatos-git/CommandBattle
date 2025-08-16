using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Linq;
using UnityEngine.InputSystem.Controls;

public class ControllerBindingUI : MonoBehaviour
{
    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;
    public TextMeshProUGUI continueText;

    private bool player1Bound = false;
    private bool player2Bound = false;
    private bool waitingToContinue = false;

    void Start()
    {
        player1Text.text = "Player 1\nPlease press any key to start";
        player2Text.text = "Player 2\nPlease press any key to start";
        if (continueText != null) continueText.gameObject.SetActive(false); // 初期非表示
    }

    void Update()
    {
        // バインド解除機能: F1 で P1 のバインドを解除、
        // F2 で P2 のバインドを解除
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            UnbindPlayer(1);
            return;
        }
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            UnbindPlayer(2);
            return;
        }

        // バインディングが完了し、
        // プレイヤーがキャラクター選択画面に入るのを確認するのを待っています
        if (player1Bound && player2Bound)
        {
            if (!waitingToContinue)
            {
                waitingToContinue = true;
                if (continueText != null)
                {
                    continueText.text = "All players ready!\nPress any key to continue";
                    continueText.gameObject.SetActive(true);
                }
            }
            else
            {
                // 続行するには、プレイヤーが F1/F2 以外のキーを押すまで待ちます。
                foreach (var device in InputSystem.devices)
                {
                    if (!device.allControls.Any(c => c is ButtonControl))
                        continue;

                    var pressedBtn = device.allControls
                        .Where(c => c is ButtonControl bc && bc.wasPressedThisFrame)
                        .Cast<ButtonControl>()
                        .FirstOrDefault();

                    if (pressedBtn != null)
                    {
                        if (!IsKey(pressedBtn, Key.F1) && !IsKey(pressedBtn, Key.F2))
                        {
                            SceneManager.sceneLoaded += OnGameSceneLoaded;
                            //SceneManager.LoadScene("GameScene");
                            SceneManager.LoadScene("CharacterSetupScene");
                            //SceneManager.LoadScene("QTETest");
                            return;
                        }
                    }
                }
            }

            return;
        }

        foreach (var device in InputSystem.devices)
        {
            if (!device.allControls.Any(c => c is ButtonControl))
                continue;

            if (!player1Bound && device.allControls.Any(c => c is ButtonControl btn && btn.wasPressedThisFrame))
            {
                BindPlayer(1, device);
                return;
            }

            if (player1Bound && !player2Bound && device.allControls.Any(c => c is ButtonControl btn && btn.wasPressedThisFrame))
            {
                if (device != TurnManager.Instance.player1Input.device)
                {
                    BindPlayer(2, device);
                    return;
                }
            }
        }
    }

    void BindPlayer(int player, InputDevice device)
    {
        string msg = "<color=green>success</color>";

        if ((player == 1 && TurnManager.Instance.player2Input.device == device) ||
        (player == 2 && TurnManager.Instance.player1Input.device == device))
        {
            Debug.LogWarning($"❗ デバイス {device.displayName} はすでに他のプレイヤーにバインドされています！");
            return;
        }

        if (player == 1)
        {
            TurnManager.Instance.player1Input.device = device;
            TurnManager.Instance.player1Input.controlScheme = device is Gamepad ? ControlScheme.Gamepad : ControlScheme.Keyboard;
            player1Bound = true;
            player1Text.text = $"P1: {device.displayName}\n{msg}";
        }
        else
        {
            TurnManager.Instance.player2Input.device = device;
            TurnManager.Instance.player2Input.controlScheme = device is Gamepad ? ControlScheme.Gamepad : ControlScheme.Keyboard;
            player2Bound = true;
            player2Text.text = $"P2: {device.displayName}\n{msg}";
        }

        //if (player1Bound && player2Bound)
        //{
        //    SceneManager.sceneLoaded += OnGameSceneLoaded;
        //    //SceneManager.LoadScene("GameScene");
        //    SceneManager.LoadScene("CharacterSetupScene");
        //}
    }

    private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene")
        {
            TurnManager.Instance?.BeginGame();
        }

        SceneManager.sceneLoaded -= OnGameSceneLoaded;
    }

    void UnbindPlayer(int player)
    {
        if (player == 1 && player1Bound)
        {
            player1Bound = false;
            TurnManager.Instance.player1Input.device = null;
            player1Text.text = "Player 1\nPlease press any key to start";
            Debug.Log("プレイヤー1のアンバウンドに成功");
        }
        else if (player == 2 && player2Bound)
        {
            player2Bound = false;
            TurnManager.Instance.player2Input.device = null;
            player2Text.text = "Player 2\nPlease press any key to start";
            Debug.Log("プレイヤー2のアンバウンドに成功");
        }

        if (continueText != null) continueText.gameObject.SetActive(false);
        waitingToContinue = false;
    }

    // 現在のButtonControlがキーボードのキーであるかどうかを判定します
    private bool IsKey(ButtonControl btn, Key key)
    {
        if (Keyboard.current == null || !(btn.device is Keyboard)) return false;

        var keyControl = Keyboard.current[key];
        return keyControl != null && btn.name == keyControl.name;
    }
}
