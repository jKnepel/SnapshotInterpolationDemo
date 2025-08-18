using jKnepel.ProteusNet.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform), typeof(Rigidbody))]
public class KatamariPlayer : NetworkBehaviour
{
	#region attributes

	[SerializeField] private Rigidbody rb;
	[SerializeField] private new MeshRenderer renderer;
	[SerializeField] private float forceMult = 50000;
	[SerializeField] private new Camera camera;
	[SerializeField] private GameMenu gameMenu;
	[SerializeField] private Vector3 cameraOffset = new(0, 3, -5);
	[SerializeField] private float followSmooth = 5f;
	[SerializeField] private float rotateSmooth = 5f;

	#endregion

	#region lifecycle

	private void Awake()
	{
		if (rb == null)
			rb = GetComponent<Rigidbody>();
		if (camera == null && Camera.main != null)
			camera = Camera.main;
		if (gameMenu == null)
			gameMenu = FindFirstObjectByType<GameMenu>(FindObjectsInactive.Include);
	}

	private void FixedUpdate()
	{
		if (!ShouldReplicate)
			return;

		Vector2 input = new Vector2(
			Input.GetAxis("Horizontal") + gameMenu.MenuInput.x,
			Input.GetAxis("Vertical") + gameMenu.MenuInput.y
		);

		Vector3 camForward = camera.transform.forward;
		camForward.y = 0;
		camForward.Normalize();

		Vector3 camRight = camera.transform.right;
		camRight.y = 0;
		camRight.Normalize();

		Vector3 moveDir = (camForward * input.y + camRight * input.x).normalized;

		rb.AddForce(moveDir * (forceMult * Time.fixedDeltaTime), ForceMode.Force);
	}

	
	private Vector3 _lastForward = Vector3.forward;

	private void LateUpdate()
	{
		if (camera == null || !IsAuthor)
			return;

		Vector3 vel = rb.linearVelocity;
		vel.y = 0;

		if (vel.sqrMagnitude > 0.1f)
		{
			Vector3 velDir = vel.normalized;
			_lastForward = Vector3.Slerp(_lastForward, velDir, rotateSmooth * Time.deltaTime);
		}

		Quaternion targetRot = Quaternion.LookRotation(_lastForward, Vector3.up);

		Vector3 targetPos = transform.position + targetRot * cameraOffset;

		camera.transform.position = Vector3.Lerp(
			camera.transform.position,
			targetPos,
			followSmooth * Time.deltaTime
		);

		camera.transform.LookAt(transform.position);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!ShouldReplicate || !other.TryGetComponent<KatamariObject>(out var att))
			return;

		att.Attach(AuthorID, transform);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!ShouldReplicate || !other.TryGetComponent<KatamariObject>(out var att))
			return;

		att.Detach(AuthorID);
	}

	public override void OnNetworkSpawned()
	{
		base.OnNetworkSpawned();
		UpdateColor();
	}

	public override void OnAuthorityChanged(uint prevClientID)
	{
		base.OnAuthorityChanged(prevClientID);
		UpdateColor();
	}
	
	private void UpdateColor()
	{
		if (!IsAuthored)
		{
			renderer.material.color = Color.white;
			return;
		}
			
		if (IsAuthor)
		{
			renderer.material.color = NetworkManager.Client.UserColour;			
		}
		else
		{
			var client = IsServer
				? NetworkManager.Server.ConnectedClients[AuthorID]
				: NetworkManager.Client.ConnectedClients[AuthorID];
			renderer.material.color = client.UserColour;
		}
	}

	#endregion
}
