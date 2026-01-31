using System.Collections;
using System.Collections.Generic;
using GGJ2026.Core.Audio;
using UnityEngine;

namespace GGJ2026.Core.Managers
{
    /// <summary>
    /// 音関連のマネージャー（BGM / 共通SEのみ）
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [SerializeField] private AudioData audioData;

        #region 音量設定
        [Range(0f, 1f)] public float masterVolume = 1.0f;
        [Range(0f, 1f)] public float bgmVolume = 1.0f;
        [Range(0f, 1f)] public float seVolume = 1.0f;
        #endregion

        private AudioSource bgmSource;
        private AudioSource bgmCrossSource;
        private List<AudioSource> seSources = new();

        [SerializeField] private int seSourceCount = 10;

        private BGMID currentBGM = BGMID.None;
        private bool isBgmFading = false;

        public override void Init()
        {
            base.Init();

            bgmSource = CreateSource("BGM_Source");
            bgmCrossSource = CreateSource("BGM_Cross_Source");

            CreateAudioSourcePool("SE_Source", seSourceCount, seSources);
        }

        private AudioSource CreateSource(string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(transform);
            var source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private void CreateAudioSourcePool(string name, int count, List<AudioSource> pool)
        {
            var parent = new GameObject($"{name}_Pool");
            parent.transform.SetParent(transform);

            for (int i = 0; i < count; i++)
            {
                var obj = new GameObject($"{name}_{i}");
                obj.transform.SetParent(parent.transform);
                var source = obj.AddComponent<AudioSource>();
                source.playOnAwake = false;
                pool.Add(source);
            }
        }

        #region BGM

        public void PlayBGM(BGMID id, bool crossFade = true)
        {
            if (currentBGM == id) return;

            AudioData.BGMInfo info = audioData.GetBGM(id);
            if (info == null || info.clip == null)
            {
                Debug.LogWarning($"[AudioManager] BGM {id} が見つかりません");
                return;
            }

            currentBGM = id;

            if (crossFade && bgmSource.isPlaying)
            {
                StartCoroutine(CrossFadeBGM(info));
                return;
            }

            bgmSource.clip = info.clip;
            bgmSource.pitch = info.pitch;
            bgmSource.loop = info.loop;
            bgmSource.volume = info.volume * bgmVolume * masterVolume;
            bgmSource.Play();
        }

        public void StopBGM(float fadeOut = 0.5f)
        {
            if (!bgmSource.isPlaying) return;

            StartCoroutine(FadeBGM(bgmSource, bgmSource.volume, 0f, fadeOut, true));
            currentBGM = BGMID.None;
        }

        private IEnumerator CrossFadeBGM(AudioData.BGMInfo next)
        {
            isBgmFading = true;

            bgmCrossSource.clip = next.clip;
            bgmCrossSource.volume = 0;
            bgmCrossSource.pitch = next.pitch;
            bgmCrossSource.loop = next.loop;
            bgmCrossSource.Play();

            float t = 0;
            float duration = Mathf.Max(next.fadeInTime, 0.5f);
            float targetVolume = next.volume * bgmVolume * masterVolume;

            while (t < duration)
            {
                t += Time.deltaTime;
                float rate = t / duration;

                bgmSource.volume = Mathf.Lerp(bgmSource.volume, 0, rate);
                bgmCrossSource.volume = Mathf.Lerp(0, targetVolume, rate);

                yield return null;
            }

            bgmSource.Stop();
            SwapBGMSources();
            isBgmFading = false;
        }

        private IEnumerator FadeBGM(AudioSource src, float from, float to, float time, bool stop)
        {
            float t = 0;
            while (t < time)
            {
                t += Time.deltaTime;
                src.volume = Mathf.Lerp(from, to, t / time);
                yield return null;
            }

            if (stop) src.Stop();
        }

        private void SwapBGMSources()
        {
            (bgmSource, bgmCrossSource) = (bgmCrossSource, bgmSource);
        }

        #endregion

        #region SE

        public AudioSource PlaySE(SEID id)
        {
            AudioData.SEInfo info = audioData.GetSE(id);
            if (info == null || info.clip == null)
            {
                Debug.LogWarning($"[AudioManager] SE {id} が見つかりません");
                return null;
            }

            AudioSource src = GetAvailableAudioSource(seSources);
            src.clip = info.clip;
            src.volume = info.volume * seVolume * masterVolume;
            src.pitch = info.pitch;
            src.loop = info.loop;
            src.Play();

            return src;
        }

        private AudioSource GetAvailableAudioSource(List<AudioSource> pool)
        {
            foreach (var src in pool)
            {
                if (!src.isPlaying) return src;
            }

            // 全部使用中なら一番古いのを再利用
            AudioSource oldest = pool[0];
            foreach (var src in pool)
            {
                if (src.time > oldest.time) oldest = src;
            }

            return oldest;
        }

        #endregion
    }
}