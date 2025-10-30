using System;
using System.Collections;
using UnityEngine;
using Util.Extensions;

public static class CoroutineExtensions
    {
        private static CoroutineHolder coroutineHolder;

        private static CoroutineHolder Holder
        {
            get
            {
                if (!coroutineHolder)
                {
                    coroutineHolder = new GameObject("CoroutineHolder").AddComponent<CoroutineHolder>();
                }

                return coroutineHolder;
            }
        }

        public static MonoBehaviour CoroutineHolder => Holder;

        public static void StartCoroutine(IEnumerator coroutine)
        {
            Holder.StartCoroutine(coroutine);
        }

        public static void StopCoroutine(IEnumerator coroutine)
        {
            if (coroutine == null) return;
            Holder.StopCoroutine(coroutine);
        }

        public static void BeginCoroutine(this MonoBehaviour mono, ExpandedCoroutine coroutine)
        {
            mono.StartCoroutine(coroutine?.Coroutine);
        }

        public static ExpandedCoroutine BeginCoroutine(this MonoBehaviour mono, IEnumerator coroutine, Action onEnd)
        {
            ExpandedCoroutine expandedCoroutine = new(coroutine, onEnd);
            mono.BeginCoroutine(expandedCoroutine);
            return expandedCoroutine;
        }

        public static void EndCoroutine(this MonoBehaviour mono, ExpandedCoroutine coroutine)
        {
            if (coroutine is { running: true })
                mono.StopCoroutine(coroutine?.Coroutine);
            coroutine?.EndCoroutine();
        }

        public static void WaitAFrame(Action onDone) => StartCoroutine(FrameDelay(onDone));

        public static IEnumerator FrameDelay(Action onDone)
        {
            yield return null;
            onDone?.Invoke();
        }
    }

    public class ExpandedCoroutine
    {
        private IEnumerator _coroutine;
        public event Action onEndCoroutine;

        private IEnumerator currentCoroutine;
        public bool running;

        public ExpandedCoroutine(IEnumerator coroutine, Action OnEnd = null)
        {
            _coroutine = coroutine;
            onEndCoroutine += OnEnd;
        }

        public IEnumerator Coroutine
        {
            get
            {
                if (currentCoroutine == null) currentCoroutine = CoroutineEnder();
                return currentCoroutine;
            }
        }

        private IEnumerator CoroutineEnder()
        {
            running = true;
            yield return _coroutine;
            EndCoroutine();
        }
        
        public void EndCoroutine()
        {
            if(!running) return;
            running = false;
            currentCoroutine = null;
            onEndCoroutine?.Invoke();
        }
    }

    internal class CoroutineHolder : MonoBehaviour { }

    public class CoroutineSequence : ISequence
    {
        private ExpandedCoroutine expandedCoroutine;
        private MonoBehaviour monoBehaviour;
        public event Action OnFinished;

        public CoroutineSequence(ExpandedCoroutine coroutine)
        {
            monoBehaviour = CoroutineExtensions.CoroutineHolder;
            expandedCoroutine = coroutine;
            expandedCoroutine.onEndCoroutine += OnCoroutineFinished;
        }

        public CoroutineSequence(ExpandedCoroutine coroutine, MonoBehaviour target)
        {
            monoBehaviour = target;
            expandedCoroutine = coroutine;
            expandedCoroutine.onEndCoroutine += OnCoroutineFinished;
        }

        public void Begin() => monoBehaviour.BeginCoroutine(expandedCoroutine);

        public void End() => monoBehaviour.EndCoroutine(expandedCoroutine);

        private void OnCoroutineFinished() => OnFinished?.Invoke();
    }