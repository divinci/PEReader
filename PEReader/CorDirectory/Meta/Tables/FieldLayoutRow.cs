using System;

namespace AlphaOmega.Debug.CorDirectory.Meta.Tables
{
	/// <summary>
	/// A row in the FieldLayout table is created if the .field directive
	/// for the parent field has specified a field offset (§II.16)
	/// </summary>
	public class FieldLayoutRow : BaseMetaRow
	{
		/// <summary>A 4-byte constant</summary>
		public UInt32 Offset { get { return base.GetValue<UInt32>(0); } }

		/// <summary>An index into the Field table</summary>
		internal MetaCellPointer FieldI { get { return base.GetValue<MetaCellPointer>(1); } }

		/// <summary>Field table row</summary>
		public FieldRow Field
		{
			get { return new FieldRow() { Row = this.FieldI.TargetRow, }; }
		}
	}
}