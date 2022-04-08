namespace Utils
{
	public interface ISerializable 
	{
		JSONObject Serialize();
		void Deserialize(JSONObject saveData);
	}
}
