using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    private Queue<ICardEffect> _effectQueue = new Queue<ICardEffect>();
    private bool _isProcessing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 효과를 큐에 추가하는 메서드
    public void AddEffect(ICardEffect effect)
    {
        _effectQueue.Enqueue(effect);
        if (!_isProcessing) StartCoroutine(ProcessQueue());
    }

    // 큐를 순차적으로 실행하는 코루틴
    private IEnumerator ProcessQueue()
    {
        _isProcessing = true;
        InputManager.Instance.UpdateCurrentState(InputState.Processing);
        System.Action<GameOverType> onGameOver = (GameOverType type) => { _effectQueue.Clear(); };
        BattleFlowManager.Instance.OnGameOver += onGameOver;

        try
        {
            while (_effectQueue.Count > 0)
            {
                // 게임 오버 시 즉시 중단
                if (BattleFlowManager.Instance.IsGameOver) yield break;

                ICardEffect effect = _effectQueue.Dequeue();
                yield return StartCoroutine(effect.Execute());
                HandManager.Instance.AlignCards();
            }
        }
        finally
        {
            // 성공/실패/중단 여부와 상관없이 무조건 실행
            _isProcessing = false;
            InputManager.Instance.UpdateCurrentState(InputState.Idle);
            BattleFlowManager.Instance.OnGameOver -= onGameOver;
        }

    }
}