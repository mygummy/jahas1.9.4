using Photon.Pun.Demo.PunBasics;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SeekerMoveRoom : MonoBehaviourPunCallbacks
{
    public bl_Joystick js;
    public float speed;

    private Animator seekerani;
    void Start()
    {
        Debug.Log("����Ϸ�");
        CameraController _cameraController = this.gameObject.GetComponent<CameraController>();
        if (_cameraController != null)
        {
            if (photonView.IsMine)
            {
                _cameraController.OnStartFollowing();
            }
        }
        else
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
        }
        js = GameObject.Find("Canvas/Joystick").GetComponent<bl_Joystick>();
        seekerani = GetComponent<Animator>();
    }
    public void Update()
    {
        if (photonView.IsMine)
        {
            // ��ƽ�� �����ִ� ������ �������ش�.
            Vector3 dir = new Vector3(js.Horizontal, js.Vertical, 0);

            // Vector�� ������ ���������� ũ�⸦ 1�� ���δ�. ���̰� ����ȭ ���� ������ 0���� ����.
            dir.Normalize();
            if (dir.sqrMagnitude > 0)
            {
                seekerani.SetBool("isMove", true);

                photonView.RPC("Flip", RpcTarget.All, dir.x);

            }
            else
            {
                seekerani.SetBool("isMove", false);
            }

            // ������Ʈ�� ��ġ�� dir �������� �̵���Ų��.
            transform.position += dir * speed * Time.deltaTime;
        }
    }

    [PunRPC]
    void Flip(float x)
    {
        if (x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

        else if (x > 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
    }
}