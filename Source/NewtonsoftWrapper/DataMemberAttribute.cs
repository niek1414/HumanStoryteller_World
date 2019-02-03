// Decompiled with JetBrains decompiler
// Type: System.Runtime.Serialization.DataMemberAttribute
// Assembly: System.Runtime.Serialization, Version=3.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: 42113C28-8959-49B8-B74A-3E38A2A157E9
// Assembly location: C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\v3.0\System.Runtime.Serialization.dll

namespace System.Runtime.Serializationa
{
  /// <summary>When applied to the member of a type, specifies that the member is part of a data contract and is serializable by the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />. </summary>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  public sealed class DataMemberAttribute : Attribute
  {
    private int order = -1;
    private bool emitDefaultValue = true;
    private string name;
    private bool isNameSetExplicit;
    private bool isRequired;

    /// <summary>Gets or sets a data member name. </summary>
    /// <returns>The name of the data member. The default is the name of the target that the attribute is applied to. </returns>
    public string Name
    {
      get
      {
        return this.name;
      }
      set
      {
        this.name = value;
        this.isNameSetExplicit = true;
      }
    }

    internal bool IsNameSetExplicit
    {
      get
      {
        return this.isNameSetExplicit;
      }
    }

    /// <summary>Gets or sets the order of serialization and deserialization of a member.</summary>
    /// <returns>The numeric order of serialization or deserialization.</returns>
    public int Order
    {
      get
      {
        return this.order;
      }
      set
      {
        if (value < 0)
          throw new Exception();
        this.order = value;
      }
    }

    /// <summary>Gets or sets a value that instructs the serialization engine that the member must be present when reading or deserializing.</summary>
    /// <returns>true, if the member is required; otherwise, false.</returns>
    /// <exception cref="T:System.Runtime.Serialization.SerializationException">the member is not present.</exception>
    public bool IsRequired
    {
      get
      {
        return this.isRequired;
      }
      set
      {
        this.isRequired = value;
      }
    }

    /// <summary>Gets or sets a value that specifies whether to serialize the default value for a field or property being serialized. </summary>
    /// <returns>true if the default value for a member should be generated in the serialization stream; otherwise, false. The default is true.</returns>
    public bool EmitDefaultValue
    {
      get
      {
        return this.emitDefaultValue;
      }
      set
      {
        this.emitDefaultValue = value;
      }
    }
  }
}
