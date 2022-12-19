using UnityEngine;
using System.Collections;

public class Rocket : MonoBehaviour
{
	public GameObject explosion;		// Prefab of explosion effect.
    public string IgnoreTag;


    void Start ()
	{
		// Destroy the rocket after 2 seconds if it doesn't get destroyed before then.
		Destroy(gameObject, 2);
	}


	private void OnTriggerEnter2D (Collider2D col)
	{
        if (!col.CompareTag(IgnoreTag))
        {
            // NEW (Removed all logic)
            if (col.tag == "BombPickup")
            {
                // ... find the Bomb script and call the Explode function.
                col.gameObject.GetComponent<Bomb>().Explode();

                // Destroy the bomb crate.
                Destroy(col.transform.root.gameObject);

                // Destroy the rocket.
                Destroy(gameObject);
            }
            else
            {
                OnExplode();
                Destroy(gameObject);
            }
        }
	}

	private void OnExplode()
	{
        var explosionCircle = new GameObject("Explosion");
        explosionCircle.transform.position = transform.position;
        explosionCircle.tag = "ExplosionFX";
        Destroy(explosionCircle, 0.5f);
        var explosionRadius = explosionCircle.AddComponent<CircleCollider2D>();
        explosionRadius.radius = 2.5f;

		var randomRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

		Instantiate(explosion, transform.position, randomRotation);
	}
}
