using UnityEngine;
using System.Collections;


public interface IVectorInt
{
	Vector2 ToVector2();
	Vector3 ToVector3();
}

[System.Serializable]
public struct Vector2Int : IVectorInt, ISerializable
{
	public int X;
	public int Y;

	public Vector2Int(int xValue, int yValue)
	{
		X = xValue;
		Y = yValue;
	}

	public Vector2Int(Vector2 vector)
	{
		X = Mathf.RoundToInt(vector.x);
		Y = Mathf.RoundToInt(vector.y);
	}

	public Vector2Int(Vector3 vector)
	{
		X = Mathf.RoundToInt(vector.x);
		Y = Mathf.RoundToInt(vector.y);
	}

	public Vector2 ToVector2()
	{
		return new Vector2(X, Y);
	}

	public Vector3 ToVector3()
	{
		return new Vector3(X, Y, 0);
	}

	public bool Equals (Vector2Int obj)
	{
		return X == obj.X && Y == obj.Y;
	}

	public static Vector2Int operator + (Vector2Int vectorA, Vector2Int vectorB)
	{
		return new Vector2Int(vectorA.X + vectorB.X, vectorA.Y + vectorB.Y);
	}

	public static Vector2Int operator - (Vector2Int vectorA, Vector2Int vectorB)
	{
		return new Vector2Int(vectorA.X - vectorB.X, vectorA.Y - vectorB.Y);
	}

    public static Vector2Int operator * (Vector2Int vectorA, int value)
    {
        return new Vector2Int(vectorA.X * value, vectorA.Y * value);
    }

	public override string ToString()
	{
		return "("+X+", "+Y+")";
	}

	public static float Distance(Vector2Int vectorA, Vector2Int vectorB)
    {
		return Mathf.Abs(vectorA.X - vectorB.X) + Mathf.Abs(vectorA.Y - vectorB.Y);
    }

    public static int SqrDistance(Vector2Int vectorA, Vector2Int vectorB)
    {
        return (int)(Mathf.Pow(vectorB.X - vectorA.X, 2) + Mathf.Pow(vectorB.Y - vectorA.Y, 2));
    }

	public JSONObject Serialize ()
	{
		JSONObject vectorJSON = JSONObject.obj;
		vectorJSON.AddField("x", X);
		vectorJSON.AddField("y", Y);

		return vectorJSON;
	}

	public void Deserialize (JSONObject vectorJSON)
	{
		X = (int) vectorJSON.GetField("x").f;
		Y = (int) vectorJSON.GetField("y").f;
	}
}

[System.Serializable]
public struct MinMax
{
    public int Min;
    public int Max;

    public MinMax(int min, int max)
    {
        Min = min;
        Max = max;
    }
}

public class VectorIntUtils
{
	public static int Distance(Vector2Int vectorA, Vector2Int vectorB)
	{
		return Mathf.Abs(vectorA.X - vectorB.X) + Mathf.Abs(vectorA.Y - vectorB.Y);
	}
}