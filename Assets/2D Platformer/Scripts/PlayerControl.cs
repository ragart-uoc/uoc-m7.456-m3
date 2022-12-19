using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the player is currently facing.
	[HideInInspector]
	public bool jump;				// Condition for whether the player should jump.

	public float moveForce = 365f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.
	public AudioClip[] jumpClips;			// Array of clips for when the player jumps.
	public float jumpForce = 1000f;			// Amount of force added when the player jumps.
	public AudioClip[] taunts;				// Array of clips for when the player taunts.
	public float tauntProbability = 50f;	// Chance of a taunt happening.
	public float tauntDelay = 1f;			// Delay for when the taunt should happen.

	private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	private bool grounded = false;			// Whether or not the player is grounded.
	private Animator anim;					// Reference to the player's animator component.

    // NEW
    protected float tilt;
    private readonly List<KeyCode> actions = new List<KeyCode>();
    private Transform pivot;

    // NEW
    public Gun gun;
    public bool IAmAnEnemy;

    [HideInInspector]
    public bool HasTurn;
    // NEW

	private void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		anim = GetComponent<Animator>();

        // NEW
        pivot = transform.Find("Pivot");
    }

	private void Update()
	{
		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));

        // NEW
        if (!HasTurn)
            return;

        // NEW
        if (!IAmAnEnemy)
        {
            // If the jump button is pressed and the player is grounded then the player should jump.
            // NEW
            // if (Input.GetButtonDown("Jump") && grounded)
            if (Input.GetKeyDown(KeyCode.Space) && grounded)
                jump = true;

            // NEW
            if (Input.GetMouseButton(0))
                gun.FireDown();
            else if (Input.GetMouseButtonUp(0))
                gun.FireUp();
            // NEW

            // NEW
            ActualizarAccionTeclado(KeyCode.LeftArrow);
            ActualizarAccionTeclado(KeyCode.RightArrow);
            ActualizarAccionTeclado(KeyCode.UpArrow);
            ActualizarAccionTeclado(KeyCode.DownArrow);
            ActualizarAccionTeclado(KeyCode.W);
            ActualizarAccionTeclado(KeyCode.S);
            // NEW
        }
    }

    private void FixedUpdate()
	{
        //NEW
        if (!HasTurn)
            return;

        // Cache the horizontal input.
        var h = Input.GetAxis("Horizontal");

        // NEW
        if (h == 0)
        {
            if (actions.Contains(KeyCode.LeftArrow))
                h = -1; // anim.GetFloat("Speed") - 0.1f;

            if (actions.Contains(KeyCode.RightArrow))
                h = 1; //anim.GetFloat("Speed") + 0.1f;
        }
        if (actions.Contains(KeyCode.UpArrow) || actions.Contains(KeyCode.W))
            tilt += 1.0f;
        if (actions.Contains(KeyCode.DownArrow) || actions.Contains(KeyCode.S))
            tilt -= 1.0f;

        tilt = Mathf.Clamp(tilt, 0, 75);
        pivot.rotation = Quaternion.Euler(0, 0, facingRight? tilt : -tilt);

        //NEW
        if (!IAmAnEnemy)
        {
            // The Speed animator parameter is set to the absolute value of the horizontal input.
            anim.SetFloat("Speed", Mathf.Abs(h));

            // If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
            if (h * GetComponent<Rigidbody2D>().velocity.x < maxSpeed)
            {
                // ... add a force to the player.
                GetComponent<Rigidbody2D>().AddForce(Vector2.right * h * moveForce);
            }

            // If the player's horizontal velocity is greater than the maxSpeed...
            if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > maxSpeed)
                // ... set the player's velocity to the maxSpeed in the x axis.
                GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Sign(GetComponent<Rigidbody2D>().velocity.x) * maxSpeed, GetComponent<Rigidbody2D>().velocity.y);
        }

        //NEW
        if (!IAmAnEnemy)
        {
            // If the input is moving the player right and the player is facing left...
            if (h > 0 && !facingRight)
                // ... flip the player.
                Flip();
            // Otherwise if the input is moving the player left and the player is facing right...
            else if (h < 0 && facingRight)
                // ... flip the player.
                Flip();
        }

        //NEW
        if (!IAmAnEnemy)
        {
            // If the player should jump...
            if (jump)
            {
                // Set the Jump animator trigger parameter.
                anim.SetTrigger("Jump");

                // Play a random jump audio clip.
                int i = Random.Range(0, jumpClips.Length);
                //AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

                // Add a vertical force to the player.
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, jumpForce));

                // Make sure the player can't jump again until the jump conditions from Update are satisfied.
                jump = false;
            }
        }
	}

	protected void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
    }

	public IEnumerator Taunt()
	{
		// Check the random chance of taunting.
		float tauntChance = Random.Range(0f, 100f);
		if(tauntChance > tauntProbability)
		{
			// Wait for tauntDelay number of seconds.
			yield return new WaitForSeconds(tauntDelay);

			// If there is no clip currently playing.
			if(!GetComponent<AudioSource>().isPlaying)
			{
				// Choose a random, but different taunt.
				tauntIndex = TauntRandom();

				// Play the new taunt.
				GetComponent<AudioSource>().clip = taunts[tauntIndex];
				GetComponent<AudioSource>().Play();
			}
		}
	}

	int TauntRandom()
	{
		// Choose a random index of the taunts array.
		int i = Random.Range(0, taunts.Length);

		// If it's the same as the previous taunt...
		if(i == tauntIndex)
			// ... try another random taunt.
			return TauntRandom();
		else
			// Otherwise return this index.
			return i;
	}

    // NEW
    public void Jump()
    {
        jump = true;
    }

    // NEW
    #region InputControls
    private void ActualizarAccionDown(KeyCode code)
    {
        if (!actions.Contains(code))
            actions.Add(code);
    }

    private void ActualizarAccionUp(KeyCode code)
    {
        if (actions.Contains(code))
            actions.Remove(code);
    }

    private void ActualizarAccionTeclado(KeyCode code)
    {
        if (Input.GetKeyDown(code))
            ActualizarAccionDown(code);

        if (Input.GetKeyUp(code))
            ActualizarAccionUp(code);
    }

    #region Pressed Buttons
    public void MueveDerechaDown()
    {
        ActualizarAccionDown(KeyCode.RightArrow);
    }
    public void MueveIzquierdaDown()
    {
        ActualizarAccionDown(KeyCode.LeftArrow);
    }
    public void RotaDerechaDown()
    {
        ActualizarAccionDown(KeyCode.DownArrow);
    }
    public void RotaIzquierdaDown()
    {
        ActualizarAccionDown(KeyCode.UpArrow);
    }
    #endregion

    #region Released Buttons
    public void MueveDerechaUp()
    {
        ActualizarAccionUp(KeyCode.RightArrow);
    }
    public void MueveIzquierdaUp()
    {
        ActualizarAccionUp(KeyCode.LeftArrow);
    }
    public void RotaDerechaUp()
    {
        ActualizarAccionUp(KeyCode.DownArrow);
    }
    public void RotaIzquierdaUp()
    {
        ActualizarAccionUp(KeyCode.UpArrow);
    }
    #endregion
    #endregion

}
