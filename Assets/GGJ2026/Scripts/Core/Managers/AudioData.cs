using System.Collections.Generic;
using UnityEngine;

namespace GGJ2026.Core.Audio
{
    #region 列挙型

    /// <summary>
    /// BGM用の列挙型
    /// </summary>
    public enum BGMID
    {
        None = -1,
        Title,
        Battle,
    }

    /// <summary>
    /// SE用（共通SE）
    /// </summary>
    public enum SEID
    {
        None = -1,
        ButtonClick,
        Start,
        Attack,
        Damage
    }

    #endregion

    /// <summary>
    /// 共通音声データを管理するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "AudioData", menuName = "TechC/Audio/AudioData")]
    public class AudioData : ScriptableObject
    {
        [System.Serializable]
        public class BGMInfo
        {
            public BGMID id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1.0f;
            [Range(0f, 2f)] public float pitch = 1.0f;
            public bool loop = true;
            [Range(0f, 5f)] public float fadeInTime = 0.5f;
            [Range(0f, 5f)] public float fadeOutTime = 0.5f;
        }

        [System.Serializable]
        public class SEInfo
        {
            public SEID id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1.0f;
            [Range(0f, 2f)] public float pitch = 1.0f;
            public bool loop = false;
        }

        [Header("BGM設定")]
        public List<BGMInfo> bgmList = new();

        [Header("SE設定")]
        public List<SEInfo> seList = new();

        public BGMInfo GetBGM(BGMID id)
        {
            return bgmList.Find(bgm => bgm.id == id);
        }

        public SEInfo GetSE(SEID id)
        {
            return seList.Find(se => se.id == id);
        }
    }
}