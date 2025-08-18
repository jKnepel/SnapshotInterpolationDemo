using System.Linq;
using jKnepel.ProteusNet.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform), typeof(Rigidbody))]
	public class KatamariObject : NetworkBehaviour
	{
		#region attributes

		private Transform _attachedTo;
		private float _maxDistance;

		[SerializeField] private NetworkObject networkObject;
		[SerializeField] private Rigidbody rb;
		[SerializeField] private new MeshRenderer renderer;
		[SerializeField] private GameMenu gameMenu;
		[SerializeField] private float gravitationalPull = 3000;

		#endregion

		#region lifecycle

		private void Awake()
		{
			if (networkObject == null)
				networkObject = GetComponent<NetworkObject>();
			if (rb == null)
				rb = GetComponent<Rigidbody>();
			if (renderer == null)
				renderer = GetComponent<MeshRenderer>();
			if (gameMenu == null)
				gameMenu = FindFirstObjectByType<GameMenu>(FindObjectsInactive.Include);
		}

		private void FixedUpdate()
		{
			if (!_attachedTo || !ShouldReplicate)
				return;

			var distance = Vector3.Distance(transform.position, _attachedTo.position);
			var strength = Map(distance, _maxDistance, 0, 0, gravitationalPull);
			rb.AddForce(strength * Time.fixedDeltaTime * (_attachedTo.position - transform.position));
		}

		#endregion

		#region public methods

		public override void OnNetworkSpawned()
		{
			base.OnNetworkSpawned();
			UpdateColor();
		}

		public override void OnAuthorityChanged(uint _)
		{
			base.OnAuthorityChanged(_);
			UpdateColor();
		}

		public override void OnOwnershipChanged(uint prevClientID)
		{
			if (IsOwner)
			{
				gameMenu.NumberOfObjects++;
				return;
			}

			if (prevClientID == LocalClientID)
				gameMenu.NumberOfObjects--;
			
			_attachedTo = null;
			_maxDistance = 0;
		}

		public void Attach(uint id, Transform trf)
		{
			if (IsOwned)
				return;

			_attachedTo = trf;
			_maxDistance = _attachedTo.GetComponents<Collider>().First(x => x.isTrigger).bounds.size.x;
			
			if (IsServer)
				networkObject.AssignOwnership(id);
			else
				networkObject.RequestOwnership();
		}

		public void Detach(uint id)
		{
			if (OwnerID != id || !ShouldReplicate)
				return;
			
			if (IsServer)
				networkObject.RemoveOwnership();
			else
				networkObject.ReleaseOwnership();
		}

		#endregion

		#region private methods

		private void OnCollisionEnter(Collision other)
		{
			if (ShouldReplicate && IsAuthored && other.gameObject.TryGetComponent<KatamariObject>(out var obj) && !obj.IsOwned && obj.AuthorID != AuthorID)
			{
				if (IsServer)
					obj.AssignAuthority(AuthorID);
				else
					obj.RequestAuthority();
			}
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

		private static float Map(float value, float from1, float from2, float to1, float to2)
		{
			return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
		}

		#endregion
	}
