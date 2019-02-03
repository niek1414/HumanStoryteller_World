using Newtonsoft.Json.Linq.ComponentModel;
using Newtonsoft.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Newtonsoft.Json.Linq
{
	/// <summary>
	/// Represents a token that can contain other tokens.
	/// </summary>
	public abstract class JContainer : JToken, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, ITypedList, IBindingList, IList, ICollection, IEnumerable
	{
		private JToken _content;

		private object _syncRoot;

		private bool _busy;

		internal JToken Content
		{
			get
			{
				return _content;
			}
			set
			{
				_content = value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this token has childen tokens.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
		/// </value>
		public override bool HasValues => _content != null;

		/// <summary>
		/// Get the first child token of this token.
		/// </summary>
		/// <value>
		/// A <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the first child token of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </value>
		public override JToken First
		{
			get
			{
				if (Last == null)
				{
					return null;
				}
				return Last._next;
			}
		}

		/// <summary>
		/// Get the last child token of this token.
		/// </summary>
		/// <value>
		/// A <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the last child token of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </value>
		public override JToken Last
		{
			[DebuggerStepThrough]
			get
			{
				return _content;
			}
		}

		JToken IList<JToken>.this[int index]
		{
			get
			{
				return GetItem(index);
			}
			set
			{
				SetItem(index, value);
			}
		}

		int ICollection<JToken>.Count
		{
			get
			{
				return CountItems();
			}
		}

		bool ICollection<JToken>.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		bool IList.IsFixedSize
		{
			get
			{
				return false;
			}
		}

		bool IList.IsReadOnly
		{
			get
			{
				return false;
			}
		}

		object IList.this[int index]
		{
			get
			{
				return GetItem(index);
			}
			set
			{
				SetItem(index, EnsureValue(value));
			}
		}

		int ICollection.Count
		{
			get
			{
				return CountItems();
			}
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null)
				{
					Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				}
				return _syncRoot;
			}
		}

		bool IBindingList.AllowEdit
		{
			get
			{
				return true;
			}
		}

		bool IBindingList.AllowNew
		{
			get
			{
				return true;
			}
		}

		bool IBindingList.AllowRemove
		{
			get
			{
				return true;
			}
		}

		bool IBindingList.IsSorted
		{
			get
			{
				return false;
			}
		}

		ListSortDirection IBindingList.SortDirection
		{
			get
			{
				return ListSortDirection.Ascending;
			}
		}

		PropertyDescriptor IBindingList.SortProperty
		{
			get
			{
				return null;
			}
		}

		bool IBindingList.SupportsChangeNotification
		{
			get
			{
				return true;
			}
		}

		bool IBindingList.SupportsSearching
		{
			get
			{
				return false;
			}
		}

		bool IBindingList.SupportsSorting
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Occurs when the list changes or an item in the list changes.
		/// </summary>
		public event ListChangedEventHandler ListChanged;

		/// <summary>
		/// Occurs before an item is added to the collection.
		/// </summary>
		public event AddingNewEventHandler AddingNew;

		internal JContainer()
		{
		}

		internal JContainer(JContainer other)
		{
			ValidationUtils.ArgumentNotNull(other, "c");
			JToken jToken = other.Last;
			if (jToken != null)
			{
				do
				{
					jToken = jToken._next;
					Add(jToken.CloneToken());
				}
				while (jToken != other.Last);
			}
		}

		internal void CheckReentrancy()
		{
			if (_busy)
			{
				throw new InvalidOperationException("Cannot change {0} during a collection change event.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:Newtonsoft.Json.Linq.JContainer.AddingNew" /> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.ComponentModel.AddingNewEventArgs" /> instance containing the event data.</param>
		protected virtual void OnAddingNew(AddingNewEventArgs e)
		{
			this.AddingNew?.Invoke(this, e);
		}

		/// <summary>
		/// Raises the <see cref="E:Newtonsoft.Json.Linq.JContainer.ListChanged" /> event.
		/// </summary>
		/// <param name="e">The <see cref="T:System.ComponentModel.ListChangedEventArgs" /> instance containing the event data.</param>
		protected virtual void OnListChanged(ListChangedEventArgs e)
		{
			ListChangedEventHandler listChanged = this.ListChanged;
			if (listChanged != null)
			{
				_busy = true;
				try
				{
					listChanged(this, e);
				}
				finally
				{
					_busy = false;
				}
			}
		}

		internal bool ContentsEqual(JContainer container)
		{
			JToken jToken = First;
			JToken jToken2 = container.First;
			if (jToken == jToken2)
			{
				return true;
			}
			while (true)
			{
				if (jToken == null && jToken2 == null)
				{
					return true;
				}
				if (jToken == null || jToken2 == null || !jToken.DeepEquals(jToken2))
				{
					break;
				}
				jToken = ((jToken != Last) ? jToken.Next : null);
				jToken2 = ((jToken2 != container.Last) ? jToken2.Next : null);
			}
			return false;
		}

		/// <summary>
		/// Returns a collection of the child tokens of this token, in document order.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the child tokens of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.
		/// </returns>
		public override JEnumerable<JToken> Children()
		{
			return new JEnumerable<JToken>(ChildrenInternal());
		}

		internal IEnumerable<JToken> ChildrenInternal()
		{
			JToken first = First;
			JToken current = first;
			if (current != null)
			{
				JToken next;
				do
				{
					yield return current;
					current = (next = current.Next);
				}
				while (next != null);
			}
		}

		/// <summary>
		/// Returns a collection of the child values of this token, in document order.
		/// </summary>
		/// <typeparam name="T">The type to convert the values to.</typeparam>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerable`1" /> containing the child values of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.
		/// </returns>
		public override IEnumerable<T> Values<T>()
		{
			return Children().Convert<JToken, T>();
		}

		/// <summary>
		/// Returns a collection of the descendant tokens for this token in document order.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> containing the descendant tokens of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.</returns>
		public IEnumerable<JToken> Descendants()
		{
			foreach (JToken item in Children())
			{
				yield return item;
				JContainer c = item as JContainer;
				if (c != null)
				{
					foreach (JToken item2 in c.Descendants())
					{
						yield return item2;
					}
				}
			}
		}

		internal bool IsMultiContent(object content)
		{
			if (content is IEnumerable && !(content is string) && !(content is JToken))
			{
				return !(content is byte[]);
			}
			return false;
		}

		internal virtual void AddItem(bool isLast, JToken previous, JToken item)
		{
			CheckReentrancy();
			ValidateToken(item, null);
			item = EnsureParentToken(item);
			JToken next = (previous != null) ? previous._next : item;
			item.Parent = this;
			item.Next = next;
			if (previous != null)
			{
				previous.Next = item;
			}
			if (isLast || previous == null)
			{
				_content = item;
			}
			if (this.ListChanged != null)
			{
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, IndexOfItem(item)));
			}
		}

		internal JToken EnsureParentToken(JToken item)
		{
			if (item.Parent != null)
			{
				item = item.CloneToken();
			}
			else
			{
				JContainer jContainer = this;
				while (jContainer.Parent != null)
				{
					jContainer = jContainer.Parent;
				}
				if (item == jContainer)
				{
					item = item.CloneToken();
				}
			}
			return item;
		}

		internal void AddInternal(bool isLast, JToken previous, object content)
		{
			if (IsMultiContent(content))
			{
				IEnumerable enumerable = (IEnumerable)content;
				JToken jToken = previous;
				foreach (object item2 in enumerable)
				{
					AddInternal(isLast, jToken, item2);
					jToken = ((jToken != null) ? jToken._next : Last);
				}
			}
			else
			{
				JToken item = CreateFromContent(content);
				AddItem(isLast, previous, item);
			}
		}

		internal int IndexOfItem(JToken item)
		{
			int num = 0;
			foreach (JToken item2 in Children())
			{
				if (item2 == item)
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		internal virtual void InsertItem(int index, JToken item)
		{
			if (index == 0)
			{
				AddFirst(item);
			}
			else
			{
				JToken item2 = GetItem(index);
				AddInternal(isLast: false, previous: item2.Previous, content: item);
			}
		}

		internal virtual void RemoveItemAt(int index)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException("index", "index is less than 0.");
			}
			CheckReentrancy();
			int num = 0;
			foreach (JToken item in Children())
			{
				if (index == num)
				{
					item.Remove();
					OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
					return;
				}
				num++;
			}
			throw new ArgumentOutOfRangeException("index", "index is equal to or greater than Count.");
		}

		internal virtual bool RemoveItem(JToken item)
		{
			if (item == null || item.Parent != this)
			{
				return false;
			}
			CheckReentrancy();
			JToken jToken = _content;
			int num = 0;
			while (jToken._next != item)
			{
				num++;
				jToken = jToken._next;
			}
			if (jToken == item)
			{
				_content = null;
			}
			else
			{
				if (_content == item)
				{
					_content = jToken;
				}
				jToken._next = item._next;
			}
			item.Parent = null;
			item.Next = null;
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, num));
			return true;
		}

		internal virtual JToken GetItem(int index)
		{
			return Children().ElementAt(index);
		}

		internal virtual void SetItem(int index, JToken item)
		{
			CheckReentrancy();
			JToken item2 = GetItem(index);
			item2.Replace(item);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
		}

		internal virtual void ClearItems()
		{
			CheckReentrancy();
			while (_content != null)
			{
				JToken content = _content;
				JToken next = content._next;
				if (content != _content || next != content._next)
				{
					throw new InvalidOperationException("This operation was corrupted by external code.");
				}
				if (next != content)
				{
					content._next = next._next;
				}
				else
				{
					_content = null;
				}
				next.Parent = null;
				next._next = null;
			}
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		internal virtual void ReplaceItem(JToken existing, JToken replacement)
		{
			if (existing != null && existing.Parent == this && !IsTokenUnchanged(existing, replacement))
			{
				CheckReentrancy();
				replacement = EnsureParentToken(replacement);
				ValidateToken(replacement, existing);
				JToken jToken = _content;
				int num = 0;
				while (jToken._next != existing)
				{
					num++;
					jToken = jToken._next;
				}
				if (jToken == existing)
				{
					_content = replacement;
					replacement._next = replacement;
				}
				else
				{
					if (_content == existing)
					{
						_content = replacement;
					}
					jToken._next = replacement;
					replacement._next = existing._next;
				}
				replacement.Parent = this;
				existing.Parent = null;
				existing.Next = null;
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, num));
			}
		}

		internal virtual bool ContainsItem(JToken item)
		{
			return IndexOfItem(item) != -1;
		}

		internal virtual void CopyItemsTo(Array array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0)
			{
				throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
			}
			if (arrayIndex >= array.Length)
			{
				throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
			}
			if (CountItems() > array.Length - arrayIndex)
			{
				throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
			}
			int num = 0;
			foreach (JToken item in Children())
			{
				array.SetValue(item, arrayIndex + num);
				num++;
			}
		}

		internal virtual int CountItems()
		{
			return Children().Count();
		}

		internal static bool IsTokenUnchanged(JToken currentValue, JToken newValue)
		{
			JValue jValue = currentValue as JValue;
			if (jValue != null)
			{
				if (jValue.Type == JTokenType.Null && newValue == null)
				{
					return true;
				}
				return jValue.Equals(newValue);
			}
			return false;
		}

		internal virtual void ValidateToken(JToken o, JToken existing)
		{
			ValidationUtils.ArgumentNotNull(o, "o");
			if (o.Type == JTokenType.Property)
			{
				throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
			}
		}

		/// <summary>
		/// Adds the specified content as children of this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="content">The content to be added.</param>
		public void Add(object content)
		{
			AddInternal(isLast: true, previous: Last, content: content);
		}

		/// <summary>
		/// Adds the specified content as the first children of this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <param name="content">The content to be added.</param>
		public void AddFirst(object content)
		{
			AddInternal(isLast: false, previous: Last, content: content);
		}

		internal JToken CreateFromContent(object content)
		{
			if (content is JToken)
			{
				return (JToken)content;
			}
			return new JValue(content);
		}

		/// <summary>
		/// Creates an <see cref="T:Newtonsoft.Json.JsonWriter" /> that can be used to add tokens to the <see cref="T:Newtonsoft.Json.Linq.JToken" />.
		/// </summary>
		/// <returns>An <see cref="T:Newtonsoft.Json.JsonWriter" /> that is ready to have content written to it.</returns>
		public JsonWriter CreateWriter()
		{
			return new JTokenWriter(this);
		}

		/// <summary>
		/// Replaces the children nodes of this token with the specified content.
		/// </summary>
		/// <param name="content">The content.</param>
		public void ReplaceAll(object content)
		{
			ClearItems();
			Add(content);
		}

		/// <summary>
		/// Removes the child nodes from this token.
		/// </summary>
		public void RemoveAll()
		{
			ClearItems();
		}

		internal void ReadContentFrom(JsonReader r)
		{
			ValidationUtils.ArgumentNotNull(r, "r");
			IJsonLineInfo lineInfo = r as IJsonLineInfo;
			JContainer jContainer = this;
			do
			{
				if (jContainer is JProperty && ((JProperty)jContainer).Value != null)
				{
					if (jContainer == this)
					{
						break;
					}
					jContainer = jContainer.Parent;
				}
				switch (r.TokenType)
				{
				case JsonToken.StartArray:
				{
					JArray jArray = new JArray();
					jArray.SetLineInfo(lineInfo);
					jContainer.Add(jArray);
					jContainer = jArray;
					break;
				}
				case JsonToken.EndArray:
					if (jContainer == this)
					{
						return;
					}
					jContainer = jContainer.Parent;
					break;
				case JsonToken.StartObject:
				{
					JObject jObject2 = new JObject();
					jObject2.SetLineInfo(lineInfo);
					jContainer.Add(jObject2);
					jContainer = jObject2;
					break;
				}
				case JsonToken.EndObject:
					if (jContainer == this)
					{
						return;
					}
					jContainer = jContainer.Parent;
					break;
				case JsonToken.StartConstructor:
				{
					JConstructor jConstructor = new JConstructor(r.Value.ToString());
					jConstructor.SetLineInfo(jConstructor);
					jContainer.Add(jConstructor);
					jContainer = jConstructor;
					break;
				}
				case JsonToken.EndConstructor:
					if (jContainer == this)
					{
						return;
					}
					jContainer = jContainer.Parent;
					break;
				case JsonToken.Integer:
				case JsonToken.Float:
				case JsonToken.String:
				case JsonToken.Boolean:
				case JsonToken.Date:
				case JsonToken.Bytes:
				{
					JValue jValue = new JValue(r.Value);
					jValue.SetLineInfo(lineInfo);
					jContainer.Add(jValue);
					break;
				}
				case JsonToken.Comment:
				{
					JValue jValue = JValue.CreateComment(r.Value.ToString());
					jValue.SetLineInfo(lineInfo);
					jContainer.Add(jValue);
					break;
				}
				case JsonToken.Null:
				{
					JValue jValue = new JValue(null, JTokenType.Null);
					jValue.SetLineInfo(lineInfo);
					jContainer.Add(jValue);
					break;
				}
				case JsonToken.Undefined:
				{
					JValue jValue = new JValue(null, JTokenType.Undefined);
					jValue.SetLineInfo(lineInfo);
					jContainer.Add(jValue);
					break;
				}
				case JsonToken.PropertyName:
				{
					string name = r.Value.ToString();
					JProperty jProperty = new JProperty(name);
					jProperty.SetLineInfo(lineInfo);
					JObject jObject = (JObject)jContainer;
					JProperty jProperty2 = jObject.Property(name);
					if (jProperty2 == null)
					{
						jContainer.Add(jProperty);
					}
					else
					{
						jProperty2.Replace(jProperty);
					}
					jContainer = jProperty;
					break;
				}
				default:
					throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, r.TokenType));
				case JsonToken.None:
					break;
				}
			}
			while (r.Read());
		}

		internal int ContentsHashCode()
		{
			int num = 0;
			foreach (JToken item in Children())
			{
				num ^= item.GetDeepHashCode();
			}
			return num;
		}

		string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
		{
			return string.Empty;
		}

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			JObject jObject = First as JObject;
			if (jObject != null)
			{
				JTypeDescriptor jTypeDescriptor = new JTypeDescriptor(jObject);
				return jTypeDescriptor.GetProperties();
			}
			return null;
		}

		int IList<JToken>.IndexOf(JToken item)
		{
			return IndexOfItem(item);
		}

		void IList<JToken>.Insert(int index, JToken item)
		{
			InsertItem(index, item);
		}

		void IList<JToken>.RemoveAt(int index)
		{
			RemoveItemAt(index);
		}

		void ICollection<JToken>.Add(JToken item)
		{
			Add(item);
		}

		void ICollection<JToken>.Clear()
		{
			ClearItems();
		}

		bool ICollection<JToken>.Contains(JToken item)
		{
			return ContainsItem(item);
		}

		void ICollection<JToken>.CopyTo(JToken[] array, int arrayIndex)
		{
			CopyItemsTo(array, arrayIndex);
		}

		bool ICollection<JToken>.Remove(JToken item)
		{
			return RemoveItem(item);
		}

		private JToken EnsureValue(object value)
		{
			if (value == null)
			{
				return null;
			}
			if (value is JToken)
			{
				return (JToken)value;
			}
			throw new ArgumentException("Argument is not a JToken.");
		}

		int IList.Add(object value)
		{
			Add(EnsureValue(value));
			return CountItems() - 1;
		}

		void IList.Clear()
		{
			ClearItems();
		}

		bool IList.Contains(object value)
		{
			return ContainsItem(EnsureValue(value));
		}

		int IList.IndexOf(object value)
		{
			return IndexOfItem(EnsureValue(value));
		}

		void IList.Insert(int index, object value)
		{
			InsertItem(index, EnsureValue(value));
		}

		void IList.Remove(object value)
		{
			RemoveItem(EnsureValue(value));
		}

		void IList.RemoveAt(int index)
		{
			RemoveItemAt(index);
		}

		void ICollection.CopyTo(Array array, int index)
		{
			CopyItemsTo(array, index);
		}

		void IBindingList.AddIndex(PropertyDescriptor property)
		{
		}

		object IBindingList.AddNew()
		{
			AddingNewEventArgs addingNewEventArgs = new AddingNewEventArgs();
			OnAddingNew(addingNewEventArgs);
			if (addingNewEventArgs.NewObject == null)
			{
				throw new Exception("Could not determine new value to add to '{0}'.".FormatWith(CultureInfo.InvariantCulture, GetType()));
			}
			if (!(addingNewEventArgs.NewObject is JToken))
			{
				throw new Exception("New item to be added to collection must be compatible with {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JToken)));
			}
			JToken jToken = (JToken)addingNewEventArgs.NewObject;
			Add(jToken);
			return jToken;
		}

		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotSupportedException();
		}

		int IBindingList.Find(PropertyDescriptor property, object key)
		{
			throw new NotSupportedException();
		}

		void IBindingList.RemoveIndex(PropertyDescriptor property)
		{
		}

		void IBindingList.RemoveSort()
		{
			throw new NotSupportedException();
		}
	}
}
