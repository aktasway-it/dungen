using UnityEngine;
using System.Collections;
using Random = System.Random;

public static class RandomUtility 
{
	private static Random rng = new Random();  

	public static void Create(int seed)
	{
		rng = new Random(seed);
	}

	public static int Range(int min, int max)
	{
		return rng.Next(min, max);
	}

	public static float Range(float min, float max)
	{
		return (float) rng.NextDouble() * (max - min) + min;
	}
}
