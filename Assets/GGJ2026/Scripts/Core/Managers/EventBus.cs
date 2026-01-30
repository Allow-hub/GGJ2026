using System;
using System.Collections.Generic;
using TechC.VBattle.Core.Extensions;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// バトル中に発生するイベントを Pub/Sub 方式で管理するイベントバス
    /// 任意の型をイベントとして扱える汎用実装
    /// 所有者はInGameManager
    /// </summary>
    public class EventBus
    {
        /// <summary>
        /// イベント型ごとのデリゲートディクショナリー
        /// key: イベントの型 (typeof(T))
        /// value: 登録されたデリゲート
        /// </summary>
        private Dictionary<Type, Delegate> _eventDictionary = new();

        /// <summary>
        /// 指定したイベント型に対してリスナーを登録する。
        /// 同じイベント型に複数のリスナーを追加登録できる。
        /// </summary>
        /// <typeparam name="T">購読するイベント型（任意の型を使用可能）</typeparam>
        /// <param name="listener">イベント発行時に呼び出されるコールバック。</param>
        public void Subscribe<T>(Action<T> listener)
        {
            var eventType = typeof(T);
            if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
                _eventDictionary[eventType] = Delegate.Combine(existingDelegate, listener);
            else
                _eventDictionary[eventType] = listener;
        }

        /// <summary>
        /// 指定したイベント型からリスナーを解除
        /// 該当リスナーが最後の1つの場合はイベント辞書からエントリごと削除される
        /// </summary>
        /// <typeparam name="T">解除対象のイベント型</typeparam>
        /// <param name="listener">登録解除するリスナー</param>
        public void Unsubscribe<T>(Action<T> listener)
        {
            var eventType = typeof(T);
            if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
            {
                var newDelegate = Delegate.Remove(existingDelegate, listener);
                if (newDelegate == null)
                    _eventDictionary.Remove(eventType);
                else
                    _eventDictionary[eventType] = newDelegate;
            }
        }

        /// <summary>
        /// 指定したイベントをすべての登録リスナーに対して発行
        /// 該当イベント型にリスナーが登録されていない場合は何も行わない
        /// </summary>
        /// <typeparam name="T">発行するイベントの型</typeparam>
        /// <param name="eventData">リスナーに渡されるイベントデータ</param>
        public void Publish<T>(T eventData)
        {
            var eventType = typeof(T);
            
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[EventBus] Publishing: {eventType.Name}");
            #endif
            
            if (_eventDictionary.TryGetValue(eventType, out var existingDelegate))
                (existingDelegate as Action<T>)?.Invoke(eventData);
        }

        /// <summary>
        /// すべてのイベントリスナーを解除し、イベント辞書を初期化
        /// </summary>
        public void Clear() => _eventDictionary.Clear();
    }
}