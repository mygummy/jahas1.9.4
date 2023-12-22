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
            if (photonView != null && photonView.IsMine) // ������ ������Ʈ�� ó��
            {
                Ready readyScript = player.GetComponent<Ready>();
                if (readyScript != null)
                {
                    readyScript.OnClickReadyButton();

                    if (SceneManager.GetActiveScene().name == "WaitingRoom")
                    {
                        // Button�� ��Ȱ��ȭ
                        readyButton.interactable = false;

                        // 1�� �Ŀ� �ٽ� Ȱ��ȭ
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
