using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public GameManager gameManager;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFInish;
    public float maxSpeed; // ���⼭ maxspeed �����߱⶧���� ��Ʈ�ѹٿ��� ���ǵ� :0 ���� �ӵ� ���� ����
    public float jumpPower; // ���⼭ jumpPower �����߱⶧���� ��Ʈ�ѹٿ��� �����Ŀ� :0 ���� �ӵ� ���� ����
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;
    Animator anim;
    AudioSource audioSource;
    // Start is called before the first frame update

    // mobile key var
    int left_Value;
    int right_Value;
    int jump_Value;
    bool left_Down;
    bool right_Down;
    bool jump_Down;
    bool left_Up;
    bool right_Up;
    bool jump_Up;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        audioSource = GetComponent<AudioSource>();
    }


    private void Update() // ĳ���� �ȹи��� ��ũ��Ʈ ���⶧ speed
    {
        // �÷��̾� ����

       // if (Input.GetButtonDown("Jump") || jump_Down || jump_Down && !anim.GetBool("isJumping"))
        if (Input.GetButtonDown("Jump") || jump_Down && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            PlaySound("JUMP");
        }

        // stop speed
        if (Input.GetButtonUp("Horizontal") || right_Up || left_Up)
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }
        // ������ȯ
        if (Input.GetButton("Horizontal") || right_Down || left_Down)
            spriteRenderer.flipX = Input.GetAxisRaw("Horizontal") + right_Value + left_Value == -1;

        // �ִϸ��̼� �ȴٰ� �ٴٰ� 
        if (Mathf.Abs(rigid.velocity.x) < 0.3) // �����Լ� Mathf ���밪 abs ���밪�� 0.3 ���� ������ �ϱ⸦ ����
            anim.SetBool("isWalking", false);
        else
            anim.SetBool("isWalking", true);
        // ����� �ʱ�ȭ
        left_Down = false;
        right_Down = false;
        left_Up = false;
        right_Up = false;
        jump_Up = false;
        jump_Down = false;
    }





    // Update is called once per frame
    void FixedUpdate() // �������� Ű�Է�
    {
        // Ű���� ��Ʈ�ѿ� ���� �̵�(�¿�)
        float h = Input.GetAxisRaw("Horizontal")+ right_Value+left_Value;

        rigid.AddForce(Vector2.right * h * 4, ForceMode2D.Impulse);

        if (rigid.velocity.x > maxSpeed) // ������ ���ǵ� ����
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < maxSpeed * (-1)) // ���� ���ǵ� ����
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);

        //Landing Platform
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));

            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.5f)
                    anim.SetBool("isJumping", false);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            //attack
            if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
                PlaySound("ATTACK");
            }
            else // damaged
                OnDamaged(collision.transform.position);
        } // ���ô� ������ spike �±׸� �޾���� enemy�� �ٸ��� ��� 
        if (collision.gameObject.tag == "Spike")
            OnDamaged(collision.transform.position);

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Item") // ���� �±� item �Ծ����� ���� �������
        {
            bool isBronze = collision.gameObject.name.Contains("Bronze");
            bool isSilver = collision.gameObject.name.Contains("Silver");
            bool isGold = collision.gameObject.name.Contains("Gold");

            if (isBronze)
                gameManager.stagePoint += 50;
            if (isSilver)
                gameManager.stagePoint += 100;
            if (isGold)
                gameManager.stagePoint += 300;

            // itme ������� �ϱ�
            gameManager.stagePoint += 100;
            collision.gameObject.SetActive(false);
            PlaySound("ITEM");
        }
        else if (collision.gameObject.tag == "Finish")
        {
            gameManager.NextStage(); // ���� ����������
            PlaySound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        //piont
        gameManager.stagePoint += 100;
        //Reaction Force
        rigid.AddForce(Vector2.up * 10, ForceMode2D.Impulse);

        //Enemy DIe
        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();
        enemyMove.OnDamaged();
    }


    void OnDamaged(Vector2 targetPos)
    {
        // health down
        gameManager.HealthDown();
        gameObject.layer = 11;
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1) * 7, ForceMode2D.Impulse);
        Invoke("OffDamaged", 3);


    }

    void OffDamaged()
    {
        gameObject.layer = 10;
        spriteRenderer.color = new Color(1, 1, 1, 1);
    }

    public void OnDie()
    {
        //Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.4f);
        //Sprite Flip Y
        spriteRenderer.flipY = true;
        //Collider Disable
        capsuleCollider.enabled = false;
        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        //�ȵ�������
        PlaySound("DIE");
    }
    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }
    void PlaySound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                break;
            case "FINISH":
                audioSource.clip = audioFInish;
                break;

        }
        audioSource.Play();
    }
    public void ButtonDown(string type)
    {
        switch (type)
        {
            case "J":
                jump_Value = -1;
                jump_Down = true;
                break;
            case "L":
                left_Value = -1;
                left_Down = true;
                break;
            case "R":
                right_Value = 1;
                right_Down = true;
                break;

        }
    }
    public void ButtonUp(string type)
    {
        switch (type)
        {
            case "J":
                jump_Value = 0;
                jump_Up = true;
                break;
            case "L":
                left_Value = 0;
                left_Up = true;
                break;
            case "R":
                right_Value = 0;
                right_Up = true;
                break;

        }
    }
}