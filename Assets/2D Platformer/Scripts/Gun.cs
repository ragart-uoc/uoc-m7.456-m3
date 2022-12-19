using UnityEngine;

public class Gun : MonoBehaviour
{
	public Rigidbody2D Rocket;
	public float Speed = 20f;

	private Animator anim;

    public Sprite AttackBarSprite;

    private Transform attackBar;
    private float targetSpeed;

    private enum States
    {
        Down,
        Fire,
        Up
    }
    private States state = States.Up;

    public delegate void GunFired();
    public event GunFired gunFired;

    private void Awake()
    {
        anim = transform.root.gameObject.GetComponent<Animator>();

        var attackBarObject = new GameObject("Power");
        attackBar = attackBarObject.transform;
        attackBar.SetParent(transform);
        attackBar.localPosition = Vector3.zero;
        attackBar.localRotation = Quaternion.identity;
        attackBar.localScale = Vector3.up * 2 + Vector3.forward;

        var spriteRenderer = attackBarObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = AttackBarSprite;
        spriteRenderer.sortingLayerID = transform.root.GetComponentInChildren<SpriteRenderer>().sortingLayerID;
    }

    private void Update()
    {
        if (targetSpeed > 0)
        {
            state = States.Down;
            if (Speed >= targetSpeed)
                state = States.Fire;
        }

        switch (state)
        {
            case States.Down:
                Speed += Time.deltaTime * 30;
                attackBar.localScale += Vector3.right * 0.01f;
                attackBar.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.green, Color.red, attackBar.localScale.x);
                break;

            case States.Fire:
                Fire();
                state = States.Up;
                break;

            default:
                break;
        }
    }

    public void FireUp()
    {
        if (state == States.Down)
            state = States.Fire;
    }

    public void FireDown()
    {
        state = States.Down;
    }

    private void Fire()
    {
        anim.SetTrigger("Shoot");
        GetComponent<AudioSource>().Play();

        var bulletInstance = Instantiate(Rocket, transform.position, transform.rotation);

        if (transform.root.GetComponent<PlayerControl>().facingRight)
            bulletInstance.velocity = transform.right.normalized * Speed;
        else
            bulletInstance.velocity = new Vector2(-transform.right.x, -transform.right.y).normalized * Speed;

        bulletInstance.gameObject.tag = bulletInstance.GetComponentInChildren<Rocket>().IgnoreTag = transform.root.tag;

        targetSpeed = Speed = 0;
        attackBar.localScale = Vector3.up * 2 + Vector3.forward;

        if (gunFired != null)
            gunFired();
    }

    public void Fire(float speed)
    {
        targetSpeed = speed;
    }
}
