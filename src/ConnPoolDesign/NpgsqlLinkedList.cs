
//	LinkedList
// ------------------------------------------------------------------
//	Description
//	History
//		18.06.2002 ups - created.
//		19.06.2002 usp - test: OK.

using System;

namespace Npgsql
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
	/// for maintaining a double linked list: mNext and mPrev. The ListItem
	/// itself does not contain any useful information, so a caller
	/// should derive a useful class from ListItem.
	/// <br/>
	/// The nested class <i>Enumerator</i> provides a means for iterating
	/// through the list in a forward only manner with foreach().
	/// <seealso cref="RevLinkedList.cs"/>
	/// </remarks>
	/// <status>OK</status>

	public class LinkedList
	{
		/// <value>Points to the beginnig of the list</value>

		internal ListItem mHead;

		/// <value>stores the number of list items</value>

		protected int mCount;

		/// <summary>
		/// Retrieves the number of items stored in the list. Read Only.
		/// </summary>
		/// <status>OK</status>

		public int Count
		{
			get { return this.mCount; }
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
			Item.mNext = this.mHead;
			Item.mPrev = null;
			if ( this.mHead != null )
			{
				this.mHead.mPrev = Item;
			}
			this.mHead = Item;
			this.mCount++;
		}

		/// <summary>
		/// Inserts a list item at a specific position.
		/// </summary>
		/// <param name="Item">the item to be inserted</param>
		/// <param name="Before">specifies the insert position</param>
		/// <status>OK</status>

		public virtual void Add( ListItem Item, ListItem Before )
		{
			if ( Before == null || Before.mPrev == null )
			{
				this.Add( Item ); // Item becomes new head
			}
			else // insert between two items
			{
				if ( Item == null ) return;
				Item.mPrev = Before.mPrev;
				Item.mNext = Before;
				Before.mPrev = Before.mPrev.mNext = Item;
				this.mCount++;
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
			if ( Item.mNext != null ) Item.mNext.mPrev = Item.mPrev;
			if ( Item.mPrev == null ) this.mHead = Item.mNext;
			else Item.mPrev.mNext = Item.mNext;
			this.mCount--;
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
			this.mHead = null;
			this.mCount = 0;
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
			internal ListItem mNext;
			internal ListItem mPrev;
		}

		/// ---------------------------------------------------------

		/// <summary>
		/// Implements the enumerator for the linked list.
		/// </summary>
		/// <status>OK</status>

		public class Enumerator
		{
			/// <value>References the list to enumerate</value>

			protected LinkedList mList;

			/// <value>References the current list item.</value>

			protected ListItem mRover;

			/// <summary>
			/// Constructor, initializes the enumerator with the list
			/// to enumerate
			/// </summary>
			/// <param name="List">The list to be enumerated</param>
			/// <status>OK</status>

			public Enumerator( LinkedList List )
			{
				this.mList = List;
			}

			/// <summary>
			/// Advances the enumerator to the next list item, if there is
			/// one.
			/// </summary>
			/// <returns>false if there is no successor</returns>
			/// <status>OK</status>

			public bool MoveNext()
			{
				if ( mRover == null ) // point to list head
				{
					// activate list head if there is one...
					mRover = this.mList.mHead;
					return mRover != null;
				}
				if ( mRover.mNext == null ) return false;
				mRover = mRover.mNext;
				return true; // operation successful
			}

			/// <summary>
			/// Makes the rover point outside the list. A subsequent
			/// MoveNext() will then return the first list item.
			/// </summary>
			/// <status>test</status>

			public void Reset()
			{
				this.mRover = null; // point outside the list
			}

			/// <summary>
			/// Returns the current list item. Read only.
			/// </summary>
			/// <status>OK</status>

			public ListItem Current
			{
				get { return this.mRover; }
			}
		}
	}
}
