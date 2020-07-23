using UnityEngine;
using System;

public class MainCharacter : MonoBehaviour {

	public float size;
	public float hitPoints;
	public float damageTakenOnTripOut;
	public float speed;
	public float attack;
	public float attackSpeed;
	public float attackDistance;
	public tk2dAnimatedSprite sprite;

	float timeSinceLastMedication;
	float timeSinceLastAttack;
	bool isAttacking;
	Vector2 walkDirection;
	Vector2 attackDirection = -Vector2.up;

	readonly Vector3[] checks = new Vector3[] { Vector3.forward * 0.3f, Vector3.back * 0.3f, Vector3.left, Vector3.right };

	readonly System.Random random = new System.Random();

	void Start() {
		var index = random.Next(0, DungeonGenerator.instance.rooms.Count);
		var room = DungeonGenerator.instance.rooms[index];
		transform.position = room.transform.position;
	}

	void OnEnable() {
		timeSinceLastMedication = Time.time;
		GetComponent<LineRenderer>().enabled = false;

		Inputron.UpKeyDown += onUpKeyDown;
		Inputron.RightKeyDown += onRightKeyDown;
		Inputron.DownKeyDown += onDownKeyDown;
		Inputron.LeftKeyDown += onLeftKeyDown;
		Inputron.SpaceKeyDown += onSpaceKeyDown;
		Inputron.UpKeyUp += onUpKeyUp;
		Inputron.RightKeyUp += onRightKeyUp;
		Inputron.DownKeyUp += onDownKeyUp;
		Inputron.LeftKeyUp += onLeftKeyUp;
		Inputron.SpaceKeyUp += onSpaceKeyUp;
	}

	void onUpKeyDown() {
		walkDirection += Vector2.up;
		attackDirection = Vector2.up;
	}

	void onUpKeyUp() {
		walkDirection -= Vector2.up;
	}

	void onDownKeyDown() {
		walkDirection += -Vector2.up;
		attackDirection = -Vector2.up;
	}

	void onDownKeyUp() {
		walkDirection -= -Vector2.up;
	}

	void onLeftKeyDown() {
		walkDirection += -Vector2.right / 2;
		attackDirection = -Vector2.right;
	}

	void onLeftKeyUp() {
		walkDirection -= -Vector2.right / 2;
	}

	void onRightKeyDown() {
		walkDirection += Vector2.right / 2;
		attackDirection = Vector2.right;
	}

	void onRightKeyUp() {
		walkDirection -= Vector2.right / 2;
	}

	void onSpaceKeyDown() {
		isAttacking = true;
		GetComponent<LineRenderer>().enabled = true;
	}

	void onSpaceKeyUp() {
		isAttacking = false;
		GetComponent<LineRenderer>().enabled = false;
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

	void walk() {
		sprite.transform.localScale = new Vector3(-attackDirection.x - attackDirection.y, 1, 1);

		var movement = walkDirection * speed * Time.deltaTime;
		var newPosition = transform.position + new Vector3(movement.x, 0, movement.y);
		if (transform.position == newPosition) {
			sprite.Stop();
		} else if (!sprite.IsPlaying(sprite.CurrentClip)) {
			sprite.Play();
		}

		if (canWalk(newPosition))  {
			transform.position = newPosition;
		}
	}

	void Update() {
		walk();

		if (Time.time - timeSinceLastMedication >= 10) {
			tripOut();
		}

		if (isAttacking && Time.time - timeSinceLastAttack >= attackSpeed) {
			shoot();
		}
	}

	void shoot() {
		timeSinceLastAttack = Time.time;

		var randomDiff = new Vector2(random.Next((int) -attackDistance, (int) attackDistance),
		                             random.Next((int) -attackDistance, (int) attackDistance));
		var target = attackDirection * attackDistance + randomDiff * 0.05f;
		var hits = Physics.RaycastAll(transform.position, new Vector3(target.x, 0, target.y), attackDistance);
		foreach (RaycastHit hit in hits) {
			var enemy = hit.collider.GetComponent<Enemy>();
			if (enemy) {
				enemy.takeDamage(attack);
			}
		}

		var line = GetComponent<LineRenderer>();
		line.SetPosition(1, new Vector3(target.x, 0.65f, target.y));
	}

	void takeMedicine() {
		timeSinceLastMedication = Time.time;
	}

	void tripOut() {
		timeSinceLastMedication = Time.time;
		takeDamage(damageTakenOnTripOut);
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
