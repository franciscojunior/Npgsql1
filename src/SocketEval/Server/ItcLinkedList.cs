
//	LinkedList
// ------------------------------------------------------------------
//	Description
//	History
//		18.06.2002 usp - created.
//		19.06.2002 usp - test: OK.
//		28.06.2002 usp - revision for thread safety.

using System;

namespace ItcCodeLib
{
	/// <summary>
	/// Implements a basic linked list.
	/// </summary>
	/// <remarks>
	/// New Items are inserted in front of the list, so iterating
	/// through the list accesses the item in reverse insertion order.
	/// <b>Note</b> This class is intended for use with C# clients only.
	/// <br/>
	/// The LinkeList class is designed to handle <i>ListItem</i> objects.
	/// That is a nested class which implements the required properties
	/// for maintaining a double linked list: next and prev. The ListItem
	/// itself does not contain any useful information, so a caller
	/// should derive a useful class from ListItem.
	/// <br/>
	/// The nested class <i>Enumerator</i> provides a means for iterating
	/// through the list in a forward only manner with foreach().
	///	<br/>Thread safety: The Add() and Remove() methods are synchronized
	/// regarding the forward and bank links, the iterator MoveNext() 
	/// method is designed to work thread safe. However, it cannot be
	/// guaranteed that a list item remains
	/// <seealso cref="RevLinkedList.cs"/>
	/// </remarks>
	/// <status>OK</status>

	public class LinkedList
	{
		/// <value>Points to the beginnig of the list</value>

		internal ListItem head;

		/// <summary>
		/// Provides access to the list head. Read only.
		/// </summary>
		
		public ListItem Head 
		{
			get { return this.head; }
		}
		
		/// <value>stores the number of list items</value>

		protected int count;

		/// <summary>
		/// Retrieves the number of items stored in the list. Read Only.
		/// </summary>
		/// <status>OK</status>

		public int Count
		{
			get { return this.count; }
		}

		/// <summary>
		/// Inserts a new item at the list head. Not type safe, should
		/// be public overwritten by a derived class.
		/// </summary>
		/// <param name="Value">is stored in the new item.</param>
		/// <remarks>Should be overwritten for type safety.</remarks>
		/// <status>OK</status>

		public virtual void Add( ListItem Item )
		{
			if ( Item == null ) return;
			lock( this )
			{
				Item.next = this.head;
				Item.prev = null;
				if ( this.head != null )
				{
					this.head.prev = Item;
				}
				this.head = Item;
				this.count++;
			}
		}

		/// <summary>
		/// Inserts a list item at a specific position.
		/// </summary>
		/// <param name="Item">the item to be inserted</param>
		/// <param name="Before">specifies the insert position</param>
		/// <status>OK</status>

		public virtual void Add( ListItem Item, ListItem Before )
		{
			lock( this )
			{
				if ( Before == null || Before.prev == null )
				{
					this.Add( Item ); // Item becomes new head
				}
				else // insert between two items
				{
					if ( Item == null ) return;
					Item.prev = Before.prev;
					Item.next = Before;
					Before.prev = Before.prev.next = Item;
					this.count++;
				}
			}
		}

		/// <summary>
		/// Removes an Item from the list.
		/// </summary>
		/// <param name="Item">The item that will be removed.</param>
		/// <remarks>Should be overwritten for type safety.</remarks>
		/// <status>OK</status>

		public virtual void Remove( ListItem Item )
		{
			if ( Item == null ) return;
			lock( this )
			{
				if ( Item.next != null ) Item.next.prev = Item.prev;
				if ( Item.prev == null ) this.head = Item.next;
				else Item.prev.next = Item.next;
				this.count--;
			}
		}

		/// <summary>
		/// Clears the list in a fast mode.
		/// </summary>
		/// <remarks>The list is cleared by simple setting the
		/// head reference to null. If and only if no list item is
		/// referenced by the application, then the complete list is
		/// subject to garbage collection. If the application stores
		/// references to list items, then the list items are garbage
		/// collected when the application clears the last list item
		/// reference.</remarks>
		/// <status>OK</status>

		public virtual void Clear()
		{
			this.head = null;
			this.count = 0;
		}

		/// <summary>
		/// Provides a typesafe C# specific enumerator
		/// </summary>
		/// <returns>a ListEnumerator</returns>
		/// <remarks>The implementation is <b>not</b> virtual because
		/// the caller must be able to control (change) the GetEnumerator()
		/// access by type casting. See <see cref="BidiLinkedList.cs"/>
		/// BidiLinkedList class.</remarks>
		/// <status>OK</status>

		public Enumerator GetEnumerator()
		{
			return new Enumerator( this );
		}

		/// ---------------------------------------------------------

		/// <summary>
		/// Base class for deriving useful list items. Provides
		/// the variables for the forward/backward chaining.
		/// </summary>
		/// <status>OK</status>

		public class ListItem
		{
			/// <value>References to the chain neighbours</value>
			internal ListItem next;
			internal ListItem prev;
			
			/// <summary>
			/// Returns a reference the to successor. Read only.
			/// </summary>
			
			public ListItem Next
			{
				get { return this.next; }
			}
			
			/// <summary>
			/// Returns a reference the to predecessor. Read only.
			/// </summary>
			
			public ListItem Prev
			{
				get { return this.prev; }
			}
		}

		/// ---------------------------------------------------------

		/// <summary>
		/// Implements the enumerator for the linked list.
		/// </summary>
		/// <status>OK</status>

		public class Enumerator
		{
			/// <value>References the list to enumerate</value>

			protected LinkedList list;

			/// <value>References the current list item.</value>

			protected ListItem rover;

			/// <summary>
			/// Constructor, initializes the enumerator with the list
			/// to enumerate
			/// </summary>
			/// <param name="List">The list to be enumerated</param>
			/// <status>OK</status>

			public Enumerator( LinkedList List )
			{
				this.list = List;
			}

			/// <summary>
			/// Advances the enumerator to the next list item, if there is
			/// one.
			/// </summary>
			/// <returns>false if there is no successor</returns>
			/// <status>OK</status>
			/// <remarks>Modified for thread safety</remarks>

			public bool MoveNext()
			{
				if ( rover == null ) // points to list head
				{
					// activate list head if there is one...
					rover = this.list.head;
					return rover != null;
				}
				ListItem NextItem = rover.next;
				if ( NextItem == null ) return false;
				rover = NextItem;
				return true; // operation successful
			}

			/// <summary>
			/// Makes the rover point outside the list. A subsequent
			/// MoveNext() will then return the first list item.
			/// </summary>
			/// <status>test</status>

			public void Reset()
			{
				this.rover = null; // point outside the list
			}

			/// <summary>
			/// Returns the current list item. Read only.
			/// </summary>
			/// <status>OK</status>

			public ListItem Current
			{
				get { return this.rover; }
			}
		}
	}
}
