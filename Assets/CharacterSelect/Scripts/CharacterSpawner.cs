using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    public CharacterDatabase characterDatabase;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        foreach (var client in MatchplayNetworkServer.Instance.ClientData)
        {
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null)
            {
                //var spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                //var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                //characterInstance.SpawnAsPlayerObject(client.Value.clientId);

                var spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);

                Color32 color;
                // TODO this is ridiculously bad code
                switch (character.DisplayName.ToLower())
                {
                    case "red":
                        color = Color.red;
                        break;
                    case "blue":
                        color = Color.blue;
                        break;
                    case "green":
                        color = Color.green;
                        break;
                    case "magenta":
                        color = Color.magenta;
                        break;
                    case "cyan":
                        color = Color.cyan;
                        break;
                    case "yellow":
                        color = Color.yellow;
                        break;
                    default:
                        Debug.LogWarning($"Nieznany kolor: {character.DisplayName}");
                        color = Color.white;
                        break;
                }
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
                var playerData = characterInstance.gameObject.GetComponent<PlayerData>();
                playerData.Init((int)client.Value.clientId, color, character.DisplayName);
			}
        }
    }
}
