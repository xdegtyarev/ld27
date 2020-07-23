using UnityEngine;
using System;

public class Enemy : MonoBehaviour {
	
	public float size;
	public float hitPoints;
	public float attack;
	public float attackSpeed;
	public float speed;
	public float alertDistance;
	public tk2dAnimatedSprite sprite;

	float timeSinceLastAttack;
	readonly Vector3[] checks = new Vector3[] { Vector3.forward * 0.3f, Vector3.back * 0.3f, Vector3.left, Vector3.right };

	void Start(){

	}

	void Update() {
		var hits = Physics.OverlapSphere(transform.position, alertDistance);
		foreach (Collider hit in hits) {
			if (hit.tag == "Char") {
				move(hit.transform.position);
			}
		}
	}

	bool canWalk(Vector3 position) {
		var colliders = UnityEngine.Object.FindObjectsOfType(typeof(Collider));
		foreach(Vector3 check in checks) {
			bool contains = false;
			foreach(Collider collider in colliders) {
				if (collider.tag == "Wall" && collider.bounds.Contains(check * size + position)) {
					contains = true;
					break;
				}
			}

			if (!contains) {
				return false;
			}
		}
		return true;
	}

	void move(Vector3 target) {
		var direction = target - transform.position;
		var movement = direction.normalized * speed * Time.deltaTime;
		var newPosition = transform.position + movement;
	
		var sign = Math.Sign(movement.x);
		sprite.transform.localScale = new Vector3(sign == 0 ? 1 : sign, 1, 1);

		if (canWalk(newPosition)) {
			transform.position =
				(direction.sqrMagnitude > movement.sqrMagnitude)
					? newPosition
					: target;
		}
	}

	void OnTriggerStay(Collider collision) {
		var main = collision.GetComponent<MainCharacter>();
		if (main) {
			attackHero(main);
		}
	}

	void attackHero(MainCharacter main) {
		if (Time.time - timeSinceLastAttack >= attackSpeed) {
			timeSinceLastAttack = Time.time;
			main.takeDamage(attack);
		}
	}

	public void takeDamage(float amount) {
		hitPoints -= amount;
		if (hitPoints <= 0) {
			die();
		}
	}

	void die() {
	}

}
