using System;

namespace Newtonsoft.Json
{
	/// <summary>
	/// Instructs the <see cref="T:Newtonsoft.Json.JsonSerializer" /> how to serialize the object.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
	public abstract class JsonContainerAttribute : Attribute
	{
		internal bool? _isReference;

		/// <summary>
		/// Gets or sets the id.
		/// </summary>
		/// <value>The id.</value>
		public string Id
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the title.
		/// </summary>
		/// <value>The title.</value>
		public string Title
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>The description.</value>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value that indicates whether to preserve object reference data.
		/// </summary>
		/// <value>
		/// 	<c>true</c> to keep object reference; otherwise, <c>false</c>. The default is <c>false</c>.
		/// </value>
		public bool IsReference
		{
			get
			{
				return _isReference ?? false;
			}
			set
			{
				_isReference = value;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonContainerAttribute" /> class.
		/// </summary>
		protected JsonContainerAttribute()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonContainerAttribute" /> class with the specified container Id.
		/// </summary>
		/// <param name="id">The container Id.</param>
		protected JsonContainerAttribute(string id)
		{
			Id = id;
		}
	}
}
