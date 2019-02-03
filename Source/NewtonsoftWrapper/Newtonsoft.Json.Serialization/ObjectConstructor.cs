namespace Newtonsoft.Json.Serialization
{
	/// <summary>
	/// Represents a method that constructs an object.
	/// </summary>
	public delegate object ObjectConstructor<T>(params object[] args);
}
