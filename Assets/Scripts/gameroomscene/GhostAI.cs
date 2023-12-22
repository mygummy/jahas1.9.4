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

        private float lerpSpeed = 0.1f; // 부드럽게 움직이기 위한 보간 속도

        private void Start()
        {
            if (photonView.IsMine)
            {
                // 랜덤 시간 설정
                SetRandomPeriod();
            }

            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // 자기 자신(유령)만
            if (photonView.IsMine)
            {
                // 상태 변경을 위한 시간 검사
                if (Time.time > nextActionTime)
                {
                    nextActionTime += period;
                    ToggleMovement(); // 움직임 상태 토글
                    SetRandomDirection(); // 랜덤한 방향 설정
                    SetRandomPeriod(); // 다음 행동까지의 시간을 랜덤하게 설정
                }

                // 움직이는 상태일 경우
                if (isMoving)
                {
                    // 현재 방향에서 목표 방향까지 부드럽게 이동
                    currentDirection = Vector2.Lerp(currentDirection, direction, lerpSpeed);
                    rb.velocity = currentDirection * moveSpeed;

                    // flipX 설정 추가
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
                    rb.velocity = Vector2.zero; // 속도를 0으로 설정

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
            // 80% 확률로 움직이고, 20% 확률로 멈춤
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
            period = Random.Range(1f, 3f); // 1초에서 3초 사이의 랜덤한 시간 설정
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