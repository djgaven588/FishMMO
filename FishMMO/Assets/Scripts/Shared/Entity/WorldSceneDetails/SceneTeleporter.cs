using FishNet.Object;
using Server.Services;
using Server;
using UnityEngine;
using System;

public class SceneTeleporter : NetworkBehaviour
{
	private SceneServerSystem sceneServerSystem;

	public void Awake()
	{
		Debug.Log("On Start Network");

		if (sceneServerSystem == null)
		{
			Debug.Log("Getting scene server system");
			sceneServerSystem = ServerBehaviour.Get<SceneServerSystem>();
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other != null && other.gameObject != null && sceneServerSystem != null)
		{
			Character character = other.gameObject.GetComponent<Character>();
			if (character != null && !character.isTeleporting)
			{
				if (sceneServerSystem.worldSceneDetailsCache != null &&
					sceneServerSystem.worldSceneDetailsCache.scenes.TryGetValue(character.sceneName, out WorldSceneDetails details) &&
					details.teleporters.TryGetValue(gameObject.name, out SceneTeleporterDetails teleporter))
				{
					character.isTeleporting = true;

					// should we prevent players from moving to a different scene if they are in combat?
					/*if (character.DamageController.Attackers.Count > 0)
					{
						return;
					}*/

					// make the character immortal for teleport
					if (character.DamageController != null)
					{
						character.DamageController.immortal = true;
					}

					character.sceneName = teleporter.toScene;
					character.transform.SetPositionAndRotation(teleporter.toPosition, character.transform.rotation);// teleporter.toRotation);

					Debug.Log("[" + DateTime.UtcNow + "] " + character.characterName + " has been saved at: " + character.transform.position.ToString());

					// save the character with new scene and position
					using var dbContext = sceneServerSystem.Server.DbContextFactory.CreateDbContext();
					CharacterService.SaveCharacter(dbContext, character, false);
					dbContext.SaveChanges();

					// tell the client to reconnect to the world server for automatic re-entry
					character.Owner.Broadcast(new SceneWorldReconnectBroadcast()
					{
						address = sceneServerSystem.Server.relayAddress,
						port = sceneServerSystem.Server.relayPort,
					});

                    character.Owner.Disconnect(false);

                    //sceneServerSystem.ServerManager.Despawn(character.NetworkObject, DespawnType.Destroy);
                    //ServerManager.Despawn(character.NetworkObject, DespawnType.Pool);
                    //character.gameObject.SetActive(false);
                }
				else
				{
					// destination not found
					return;
				}
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other != null && other.gameObject != null)
		{
			Character character = other.gameObject.GetComponent<Character>();
			if (character != null)
			{
				character.isTeleporting = false;
			}
		}
	}
}