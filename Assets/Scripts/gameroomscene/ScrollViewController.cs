using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.JAHAS
{
    public class ScrollViewController : MonoBehaviour
    {
        public RectTransform scrollViewTransform;
        private bool isShown = false;
        private Vector2 hiddenPosition;
        private Vector2 shownPosition;

        private void Start()
        {
            hiddenPosition = scrollViewTransform.anchoredPosition;
            shownPosition = hiddenPosition + new Vector2(-250, 0); // ¿¹½Ã
        }
        private void Update()
        {
            Vector2 targetPosition = isShown ? shownPosition : hiddenPosition;
            scrollViewTransform.anchoredPosition = Vector2.Lerp(scrollViewTransform.anchoredPosition,targetPosition,Time.deltaTime * 5.0f);
        }

        public void OnMissionButtonClick()
        {
            isShown = !isShown;
        }
    }
}