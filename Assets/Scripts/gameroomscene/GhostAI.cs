using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Com.MyCompany.JAHAS
{
    public class GhostAI : MonoBehaviourPun
    {
        private bool isMoving;
        private Vector2 direction;
        private Vector2 currentDirection;

        private Animator animator;
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        public float moveSpeed = 5.0f;
        private float nextActionTime = 0.0f;
        private float period;

        private float lerpSpeed = 0.1f; // �ε巴�� �����̱� ���� ���� �ӵ�

        private void Start()
        {
            if (photonView.IsMine)
            {
                // ���� �ð� ����
                SetRandomPeriod();
            }

            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // �ڱ� �ڽ�(����)��
            if (photonView.IsMine)
            {
                // ���� ������ ���� �ð� �˻�
                if (Time.time > nextActionTime)
                {
                    nextActionTime += period;
                    ToggleMovement(); // ������ ���� ���
                    SetRandomDirection(); // ������ ���� ����
                    SetRandomPeriod(); // ���� �ൿ������ �ð��� �����ϰ� ����
                }

                // �����̴� ������ ���
                if (isMoving)
                {
                    // ���� ���⿡�� ��ǥ ������� �ε巴�� �̵�
                    currentDirection = Vector2.Lerp(currentDirection, direction, lerpSpeed);
                    rb.velocity = currentDirection * moveSpeed;

                    // flipX ���� �߰�
                    if (currentDirection.x > 0f)
                    {
                        spriteRenderer.flipX = true;
                    }
                    else if (currentDirection.x < 0f)
                    {
                        spriteRenderer.flipX = false;
                    }

                    animator.SetBool("isMove", true);
                    if (animator != null && spriteRenderer != null)
                    {
                        photonView.RPC("UpdateAnimation", RpcTarget.All, true, spriteRenderer.flipX);
                    }

                }
                else
                {
                    rb.velocity = Vector2.zero; // �ӵ��� 0���� ����

                    animator.SetBool("isMove", false);
                    if (animator != null && spriteRenderer != null)
                    {
                        photonView.RPC("UpdateAnimation", RpcTarget.All, false, spriteRenderer.flipX);
                    }
                }
            }
        }

        private void ToggleMovement()
        {
            // 80% Ȯ���� �����̰�, 20% Ȯ���� ����
            isMoving = Random.Range(0f, 1f) < 0.8f;
        }

        private void SetRandomDirection()
        {
            if (isMoving)
            {
                direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            }
        }

        private void SetRandomPeriod()
        {
            period = Random.Range(1f, 3f); // 1�ʿ��� 3�� ������ ������ �ð� ����
        }


        [PunRPC]
        public void UpdateAnimation(bool isMoveState, bool flipX)
        {
            if (animator != null && spriteRenderer != null)
            {
                animator.SetBool("isMove", isMoveState);
                spriteRenderer.flipX = flipX;
            }
        }

    }
}