using UnityEngine;

public class AI : MonoBehaviour
{
    Rigidbody2D rigidBody;
    LayerMask floorMask;
    LayerMask obstacleRedMask;
    LayerMask obstacleBlueMask;

    GameManager gameManager;
    GeneticMLPNetwork network;

    bool onGround = true;
    bool isDead = false;
    float timerBeforeJumpAgain;
    float timeAlive;
    bool hasABlockAboveHim;

    float InvincibilityCD = -1f;

    [SerializeField] float jumpForceBase;
    [SerializeField] float jumpForceAdditionalBySpeed;


    public GeneticMLPNetwork Network { get { return network; } }
    public bool OnGround { get { return onGround; } }
    public bool IsDead { get { return isDead; } }
    public bool HasABlockAboveHim { get { return hasABlockAboveHim; } }
    public float TimeAlive { get { return timeAlive; } }


    // Start is called before the first frame update
    void Start()
    {
        timeAlive = 0f;

        rigidBody = GetComponent<Rigidbody2D>();
        gameManager = FindObjectOfType<GameManager>();

        floorMask = LayerMask.GetMask("Floor");
        obstacleRedMask = 11;
        obstacleBlueMask = 12;

        rigidBody.gravityScale *= 2f;

        network = GetComponent<GeneticMLPNetwork>();

        isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(InvincibilityCD > 0f)
            InvincibilityCD -= Time.deltaTime;

        if (timerBeforeJumpAgain > 0f)
            timerBeforeJumpAgain -= Time.deltaTime;

        RaycastBlockAbove();
    }

    public void RaycastBlockAbove()
    {
        RaycastHit2D hitObstacle1 = Physics2D.Raycast(transform.position - new Vector3(transform.localScale.x / 2f, 0, 0), transform.up, 10f, (1 << obstacleBlueMask | (1 << obstacleRedMask)));
        RaycastHit2D hitObstacle2 = Physics2D.Raycast(transform.position + new Vector3(transform.localScale.x / 2f, 0, 0), transform.up, 10f, (1 << obstacleBlueMask | (1 << obstacleRedMask)));
        if (hitObstacle1 || hitObstacle2)
            hasABlockAboveHim = true;
        else
            hasABlockAboveHim = false;

        Debug.DrawRay(transform.position - new Vector3(transform.localScale.x / 2f, 0, 0), transform.up * 2f, Color.green);
        Debug.DrawRay(transform.position + new Vector3(transform.localScale.x / 2f, 0, 0), transform.up * 2f, Color.green);
    }

    public void ResetAI()
    {
        gameObject.SetActive(true);
        rigidBody.gravityScale = 2f;
        isDead = false;
    }

    public void Jump()
    {
        //Inverse the gravity
        if (CanJump())
        {
            timerBeforeJumpAgain = 0.25f;
            rigidBody.AddForce(transform.up * (jumpForceBase + (gameManager.GetSpeedDifference() * jumpForceAdditionalBySpeed)));
            rigidBody.gravityScale *= -1;
            transform.eulerAngles = new Vector3(transform.rotation.eulerAngles.x + 180f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }

    public bool CanJump()
    {
        if (onGround && timerBeforeJumpAgain <= 0)
            return true;

        return false;
    }

    public bool IsOnBottomSide()
    {
        if (rigidBody.gravityScale > 0)
            return true;

        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == obstacleRedMask || collision.gameObject.layer == obstacleBlueMask)
        {
            if (InvincibilityCD > 0)
                return;

            Debug.Log("Dead");
            isDead = true;
            gameManager.AddDeadAI();
            timeAlive = gameManager.GameTime;
            gameObject.SetActive(false);
        }
        if (collision.gameObject.layer == floorMask)
        {
            onGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == floorMask)
        {
            onGround = false;
        }
    }

    public void SwitchColor(bool shouldBeRed)
    {
        if(shouldBeRed)
        {
            gameObject.layer = 10; //PlayerRed Layer
            GetComponent<SpriteRenderer>().color = Color.red;
        }
        else
        {
            gameObject.layer = 9; //PlayerBlue Layer
            GetComponent<SpriteRenderer>().color = Color.blue;
        }

        InvincibilityCD = 0.35f;

    }
}
