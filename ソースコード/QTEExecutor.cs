using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public enum QTERank
{
    Failed,
    Good,
    Great
}

public class QTEExecutor : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject qtePanel;
    //public TMP_Text sequenceText;
    //public TMP_Text instructionText;
    public TMP_Text keyIconText;
    public TMP_Text resultText;
    public Image timerImage;
    public Image mageImage;
    public Image mageBGImage;
    private ControlScheme currentScheme;
    public bool isover = false;

    [Header("数値設定")]
    public int ArcherNumber;
    public float ArcherTime;

    public int MageNumber;
    public float MageTime;

    public int AssinNumber;
    public float AssinTime;

    [Header("QTE Keys")]
    public KeyCode[] keyboardKeys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    private KeyCode[] gamepadKeys = { KeyCode.JoystickButton0, KeyCode.JoystickButton1, KeyCode.JoystickButton2, KeyCode.JoystickButton3 };

    private System.Random rng = new System.Random();

    public IEnumerator ExecuteQTE(QTEType type, UnitOwner owner, Action<QTERank> onComplete)
    {
        qtePanel.SetActive(true);
        isover = false;
        InputLock.IsLocked = true;
        
        if (timerImage != null)
            timerImage.fillAmount = 1f;

        resultText.text = "";
        //instructionText.text = "";
        keyIconText.text = "";

        currentScheme = (owner == UnitOwner.Player1)
           ? TurnManager.Instance.player1Input.controlScheme
           : TurnManager.Instance.player2Input.controlScheme;

        KeyCode[] pool = (currentScheme == ControlScheme.Gamepad) ? gamepadKeys : keyboardKeys;

        QTERank resultRank = QTERank.Failed;

        switch (type)
        {
            case QTEType.Archer:
                if (timerImage != null) timerImage.enabled = true;
                yield return StartCoroutine(ArcherQTE(ArcherNumber, ArcherTime, pool, rank => resultRank = rank));
                break;
            case QTEType.Mage:
                if (mageImage != null) mageImage.enabled = true;
                if (mageBGImage != null) mageBGImage.enabled = true;
                yield return StartCoroutine(MageQTE(MageNumber, MageTime, pool, rank => resultRank = rank));
                break;
            case QTEType.Assassin:
                if (timerImage != null) timerImage.enabled = true;
                yield return StartCoroutine(AssassinQTE(AssinNumber, AssinTime, 0.5f, pool, rank => resultRank = rank));
                break;
        }

        if (timerImage != null)
            timerImage.fillAmount = 0f;      

        qtePanel.SetActive(false);

        yield return StartCoroutine(ShowResultText(resultRank));

        isover = true;
        InputLock.IsLocked = false;
        onComplete?.Invoke(resultRank);
    }

    private IEnumerator ArcherQTE(int targetCount, float timeLimit, KeyCode[] pool, Action<QTERank> onComplete)
    {
        if (mageImage != null) mageImage.enabled = false;
        if (mageBGImage != null) mageBGImage.enabled = false;

        KeyCode key = GetRandomKey(pool);
        //instructionText.text = "連打せよ！";
        keyIconText.text = GetKeyDisplayName(key);

        int count = 0;
        float timer = 0f;

        while (timer < timeLimit)
        {
            if (Input.GetKeyDown(key)) count++;
            timer += Time.deltaTime;

            if (timerImage != null)
                timerImage.fillAmount = Mathf.Clamp01(1f - (timer / timeLimit));

            yield return null;
        }

        QTERank rank = count switch
        {
            >= 8 => QTERank.Great,
            >= 4 => QTERank.Good,
            _ => QTERank.Failed
        };

        onComplete?.Invoke(rank);
    }

    private IEnumerator MageQTE(int length, float timeLimit, KeyCode[] pool, Action<QTERank> onComplete)
    {
        timerImage.enabled = false;

        DOTween.Kill("MageQTE");
        DOTween.Kill("MageRotate");

        if (mageImage != null)
        {
            mageImage.color = new Color(1f, 1f, 1f, 1f);
            mageImage.transform.localScale = Vector3.one * 0.5f;
            mageImage.transform.localEulerAngles = Vector3.zero;

            Sequence mageSequence = DOTween.Sequence().SetId("MageQTE");

            // 1. 先放大 & 显现
            mageSequence.Append(mageImage.DOFade(1f, 0.3f));
            mageSequence.Join(mageImage.transform.DOScale(2f, 0.3f));

            // 2. 顺时针旋转 360度，持续 timeLimit 秒
            mageSequence.Append(
                mageImage.transform
                .DORotate(new Vector3(0, 0, -180f), timeLimit-1.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear));

            // 3. 最后缩小 & 渐隐
            mageSequence.Append(
                mageImage.transform.DOScale(0.3f, 1.5f).SetEase(Ease.InQuad)
            );
            mageSequence.Join(
                mageImage.DOFade(0f, 1.5f).SetEase(Ease.OutQuad)
            );

            // 自动关闭
            mageSequence.OnComplete(() =>
            {
                mageImage.enabled = false;
                mageBGImage.enabled = false;
            });
        }

        KeyCode[] sequence = GetRandomKeySequence(length, pool);
        int index = 0;
        float timer = 0f;

        //instructionText.text = "順番に押して！";
        //sequenceText.text = "";

        //foreach (var key in sequence)
        //sequenceText.text += key.ToString() + " ";

        keyIconText.text = GetKeyDisplayName(sequence[index]);

        while (timer < timeLimit && index < sequence.Length)
        {
            if (Input.GetKeyDown(sequence[index]))
            {
                index++;
                //sequenceText.text = "";
                //for (int i = index; i < sequence.Length; i++)
                //    sequenceText.text += sequence[i].ToString() + " ";

                if (index < sequence.Length)
                    keyIconText.text = GetKeyDisplayName(sequence[index]);
                else
                    keyIconText.text = "";
            }
            timer += Time.deltaTime;

            yield return null;
        }

        DOTween.Kill("MageQTE");

        QTERank rank = index switch
        {
            5 => QTERank.Great,
            3 or 4 => QTERank.Good,
            _ => QTERank.Failed
        };

        onComplete?.Invoke(rank);
    }

    private IEnumerator AssassinQTE(int rounds, float initialTime, float decay, KeyCode[] pool, Action<QTERank> onComplete)
    {
        if (mageImage != null) mageImage.enabled = false;
        if (mageBGImage != null) mageBGImage.enabled = false;
        //instructionText.text = "素早く押せ！";

        int successCount = 0;

        for (int i = 0; i < rounds; i++)
        {
            KeyCode key = GetRandomKey(pool);
            float timeLimit = Mathf.Max(0.5f, initialTime - i * decay);
            float timer = 0f;

            keyIconText.text = GetKeyDisplayName(key);
            //sequenceText.text = $"残り時間: {timeLimit:F1}s";

            if (timerImage != null)
                timerImage.fillAmount = 1f;

            while (timer < timeLimit)
            {
                if (Input.GetKeyDown(key))
                {
                    successCount++;
                    break;
                }
                timer += Time.deltaTime;

                if (timerImage != null)
                    timerImage.fillAmount = Mathf.Clamp01(1f - (timer / timeLimit));

                //sequenceText.text = $"残り時間: {Mathf.Max(0f, timeLimit - timer):F1}s";
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);
        }

        keyIconText.text = "";

        QTERank rank = successCount switch
        {
            3 => QTERank.Great,
            2 => QTERank.Good,
            _ => QTERank.Failed
        };

        onComplete?.Invoke(rank);
    }

    private IEnumerator ShowResultText(QTERank rank)
    {
        string result = rank switch
        {
            QTERank.Failed => "FAILED",
            QTERank.Good => "GOOD!",
            QTERank.Great => "GREAT!!",
            _ => "?"
        };

        Color displayColor = rank switch
        {
            QTERank.Failed => new Color(1f, 0.2f, 0.2f, 0),
            QTERank.Good => new Color(1f, 1f, 0.3f, 0),
            QTERank.Great => new Color(0.3f, 1f, 0.3f, 0),
            _ => Color.white
        };

        resultText.text = result;
        displayColor.a = 0f;
        resultText.color = displayColor;

        resultText.transform.localScale = Vector3.zero;

        Sequence s = DOTween.Sequence();
        s.Append(resultText.transform.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
        s.Join(resultText.DOFade(1f, 0.2f));
        s.AppendInterval(1f);
        s.Append(resultText.DOFade(0f, 0.4f));
        s.Join(resultText.transform.DOScale(0.8f, 0.4f));
        s.OnComplete(() => resultText.text = "");

        yield return s.WaitForCompletion();
    }

    private KeyCode GetRandomKey(KeyCode[] pool)
    {
        return pool[rng.Next(pool.Length)];
    }

    private KeyCode[] GetRandomKeySequence(int length, KeyCode[] pool)
    {
        KeyCode[] seq = new KeyCode[length];
        for (int i = 0; i < length; i++)
            seq[i] = pool[rng.Next(pool.Length)];
        return seq;
    }

    private string GetKeyDisplayName(KeyCode key)
    {
        if (currentScheme == ControlScheme.Gamepad)
        {
            return key switch
            {
                KeyCode.JoystickButton0 => "A",
                KeyCode.JoystickButton1 => "B",
                KeyCode.JoystickButton2 => "X",
                KeyCode.JoystickButton3 => "Y",
                KeyCode.JoystickButton4 => "LB",
                KeyCode.JoystickButton5 => "RB",
                KeyCode.JoystickButton6 => "Back",
                KeyCode.JoystickButton7 => "Start",
                KeyCode.JoystickButton8 => "LS",
                KeyCode.JoystickButton9 => "RS",
                _ => "?"
            };
        }
        else
        {
            return key.ToString(); // 键盘时显示 W/A/S/D
        }
    }
}
