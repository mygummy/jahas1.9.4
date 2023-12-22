using Com.MyCompany.JAHAS;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class ReadyButtonController : MonoBehaviour
{
    public Button readyButton;

    private void Start()
    {
     }

    public void OnButtonClicked()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Seeker");
        foreach (GameObject player in players)
        {
            PhotonView photonView = player.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine) // 소유한 오브젝트만 처리
            {
                Ready readyScript = player.GetComponent<Ready>();
                if (readyScript != null)
                {
                    readyScript.OnClickReadyButton();

                    if (SceneManager.GetActiveScene().name == "WaitingRoom")
                    {
                        // Button을 비활성화
                        readyButton.interactable = false;

                        // 1초 후에 다시 활성화
                        Invoke("EnableButton", 1f);
                    }
                }
            }
        }
    }

    private void EnableButton()
    {
        readyButton.interactable = true;
    }
}
