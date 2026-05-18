using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct WeightedAudioClip
{
    public AudioClip clip;
    [Min(0.001f)] public float weight;
}

[Serializable]
public class GameAudioEventDefinition
{
    public GameAudioEventId eventId = GameAudioEventId.None;
    public GameAudioBus bus = GameAudioBus.SFX;
    public bool spatialize3D = false;

    [Header("Playback")]
    [Range(0f, 1f)] public float baseVolume = 1f;
    [Min(0f)] public float cooldownSeconds = 0f;
    [Min(1)] public int maxSimultaneous = 8;

    [Header("Variations")]
    public List<WeightedAudioClip> variations = new List<WeightedAudioClip>();

    public AudioClip PickClip()
    {
        if (variations == null || variations.Count == 0)
        {
            return null;
        }

        float totalWeight = 0f;
        for (int i = 0; i < variations.Count; i++)
        {
            if (variations[i].clip == null)
            {
                continue;
            }

            totalWeight += Mathf.Max(0.001f, variations[i].weight);
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < variations.Count; i++)
        {
            WeightedAudioClip entry = variations[i];
            if (entry.clip == null)
            {
                continue;
            }

            cumulative += Mathf.Max(0.001f, entry.weight);
            if (roll <= cumulative)
            {
                return entry.clip;
            }
        }

        return variations[variations.Count - 1].clip;
    }
}

[CreateAssetMenu(menuName = "DeterministicRoulette/Audio Event Bank", fileName = "GameAudioEventBank")]
public class GameAudioEventBank : ScriptableObject
{
    [SerializeField] private List<GameAudioEventDefinition> events = new List<GameAudioEventDefinition>();

    private Dictionary<GameAudioEventId, GameAudioEventDefinition> byId;

    public bool TryGet(GameAudioEventId eventId, out GameAudioEventDefinition definition)
    {
        BuildIndexIfNeeded();
        return byId.TryGetValue(eventId, out definition);
    }

    private void BuildIndexIfNeeded()
    {
        if (byId != null)
        {
            return;
        }

        byId = new Dictionary<GameAudioEventId, GameAudioEventDefinition>();
        for (int i = 0; i < events.Count; i++)
        {
            GameAudioEventDefinition definition = events[i];
            if (definition == null || definition.eventId == GameAudioEventId.None)
            {
                continue;
            }

            byId[definition.eventId] = definition;
        }
    }
}
