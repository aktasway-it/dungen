using UnityEngine;
using System.Collections;

public interface ISerializable 
{
	JSONObject Serialize();
	void Deserialize(JSONObject saveData);
}
