namespace System.Runtime.Serializationa
{
  /// <summary>Specifies that the type defines or implements a data contract and is serializable by a serializer, such as the <see cref="T:System.Runtime.Serialization.DataContractSerializer" />. To make their type serializable, type authors must define a data contract for their type.</summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
  public sealed class DataContractAttribute : Attribute
  {
    private string name;
    private string ns;
    private bool isNameSetExplicit;
    private bool isNamespaceSetExplicit;
    private bool isReference;
    private bool isReferenceSetExplicit;

    /// <summary>Gets or sets a value that indicates whether to preserve object reference data.</summary>
    /// <returns>true to keep object reference data using standard XML; otherwise, false. The default is false.</returns>
    public bool IsReference
    {
      get
      {
        return this.isReference;
      }
      set
      {
        this.isReference = value;
        this.isReferenceSetExplicit = true;
      }
    }

    internal bool IsReferenceSetExplicit
    {
      get
      {
        return this.isReferenceSetExplicit;
      }
    }

    /// <summary>Gets or sets the namespace for the data contract for the type.</summary>
    /// <returns>The namespace of the contract. </returns>
    public string Namespace
    {
      get
      {
        return this.ns;
      }
      set
      {
        this.ns = value;
        this.isNamespaceSetExplicit = true;
      }
    }

    internal bool IsNamespaceSetExplicit
    {
      get
      {
        return this.isNamespaceSetExplicit;
      }
    }

    /// <summary>Gets or sets the name of the data contract for the type.</summary>
    /// <returns>The local name of a data contract. The default is the name of the class that the attribute is applied to. </returns>
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
  }
}
