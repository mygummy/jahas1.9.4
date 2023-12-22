using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // SceneManager 사용을 위한 네임스페이스 추가
using System.Collections;

namespace Com.MyCompany.JAHAS
{
    public class OpeningControl : MonoBehaviour
    {
        public Image centralImage;
        public Text narrationText;
        public Sprite[] imageArray;
        public string[] textArray;
        public float delayTime = 5.0f;
        public float fadeDuration = 1.0f;

        private int currentIndex = 0;

        public void SkipToLauncher()
        {
            // 코루틴을 중단하고 씬을 즉시 로드합니다.
            StopAllCoroutines();
            SceneManager.LoadScene("Launcher");
        }
        private void Start()
        {
            StartCoroutine(ChangeSceneContent());
        }

        private IEnumerator ChangeSceneContent()
        {
            while (currentIndex < imageArray.Length)
            {
                if (currentIndex != 4)
                {
                    yield return FadeOut(fadeDuration);
                }

                centralImage.sprite = imageArray[currentIndex];
                narrationText.text = textArray[currentIndex];
                currentIndex++;

                if (currentIndex != 5)
                {
                    yield return FadeIn(fadeDuration);
                }

                yield return new WaitForSeconds(delayTime);
            }

            // 모든 이미지 재생이 끝나면 Launcher 씬 로드
            SceneManager.LoadScene("Launcher");
        }

        private IEnumerator FadeIn(float duration)
        {
            float startTime = Time.time;
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                centralImage.color = new Color(1, 1, 1, t);
                narrationText.color = new Color(1, 1, 1, t);
                yield return null;
            }
        }

        private IEnumerator FadeOut(float duration)
        {
            float startTime = Time.time;
            while (Time.time < startTime + duration)
            {
                float t = 1 - (Time.time - startTime) / duration;
                centralImage.color = new Color(1, 1, 1, t);
                narrationText.color = new Color(1, 1, 1, t);
                yield return null;
            }
        }
    }
}
