#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ABM
{
    class AssetBundleCoroutineEditor
    {
        private readonly Stack<IEnumerator> coroutines = new Stack<IEnumerator>();

        public static void Coroutine(IEnumerator coroutine)
        {
            new AssetBundleCoroutineEditor(coroutine).Start();
        }

        private AssetBundleCoroutineEditor(IEnumerator coroutine)
        {
            coroutines.Push(coroutine);
        }

        private void Start()
        {
            EditorApplication.update += Update;
        }

        private void Stop()
        {
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            try
            {
                var coroutine = coroutines.Peek();

                if (coroutine.MoveNext() == false)
                {
                    coroutines.Pop();
                }
                else if (coroutine.Current is IEnumerator)
                {
                    coroutines.Push((IEnumerator)coroutine.Current);
                }
                else if (coroutine.Current is AsyncOperation)
                {
                    coroutines.Push(new AsyncCustomYield((AsyncOperation)coroutine.Current));
                }

                if (coroutines.Count == 0)
                {
                    Stop();
                }
            }
            catch (Exception)
            {
                coroutines.Clear();
                Stop();
                throw;
            }
        }

        class AsyncCustomYield : CustomYieldInstruction
        {
            private AsyncOperation async;

            public AsyncCustomYield(AsyncOperation async)
            {
                this.async = async;
            }

            public override bool keepWaiting
            {
                get { return async.isDone == false; }
            }
        }
    }
}
#endif