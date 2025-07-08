using System;
using System.Collections.Generic;

public class WeightedRoulette<T>
{
    private struct Entry
    {
        public T Item;
        public float Weight;
        public float AccumulatedWeight;
    }

    private List<Entry> entries = new List<Entry>();
    private float totalWeight = 0f;
    private Random rng;

    public WeightedRoulette(int? seed = null)
    {
        rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public void Add(T item, float weight)
    {
        if (weight <= 0f) return;
        totalWeight += weight;
        entries.Add(new Entry { Item = item, Weight = weight, AccumulatedWeight = totalWeight });
    }

    public void Clear()
    {
        entries.Clear();
        totalWeight = 0f;
    }

    public void SetWeight(T item, float weight)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(entries[i].Item, item))
            {
                float oldWeight = entries[i].Weight;
                totalWeight += weight - oldWeight;
                entries[i] = new Entry { Item = item, Weight = weight, AccumulatedWeight = 0f };
            }
        }
        float accum = 0f;
        for (int i = 0; i < entries.Count; i++)
        {
            accum += entries[i].Weight;
            entries[i] = new Entry { Item = entries[i].Item, Weight = entries[i].Weight, AccumulatedWeight = accum };
        }
    }

    public T Roll()
    {
        if (entries.Count == 0 || totalWeight <= 0f)
            throw new InvalidOperationException("No hay elementos en la ruleta.");

        float value = (float)(rng.NextDouble() * totalWeight);

        foreach (var entry in entries)
        {
            if (value < entry.AccumulatedWeight)
                return entry.Item;
        }

        return entries[entries.Count - 1].Item;
    }
}
