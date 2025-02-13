﻿using System;
using System.Collections.Generic;

namespace AlphaOmega.Debug.NTDirectory
{
	/// <summary>Import description class</summary>
	public struct ImportModule : IEnumerable<WinNT.IMAGE_IMPORT_BY_NAME>
	{
		private Import Directory { get; }

		/// <summary>Import directory header</summary>
		public WinNT.IMAGE_IMPORT_DESCRIPTOR Header { get; }

		/// <summary>Name of the imported module</summary>
		public String ModuleName
		{
			get
			{
				return this.Header.IsEmpty
					? null
					: this.Directory.Parent.Header.PtrToStringAnsi(this.Header.Name);//Marshal.PtrToStringAnsi(new IntPtr(this.Directory.Info.HModule.ToInt64() + this.Descriptor.Name));
			}
		}
		/// <summary>Create instance of import description class</summary>
		/// <param name="directory">Import directory</param>
		/// <param name="header">Header</param>
		/// <exception cref="ArgumentNullException">Directory is null</exception>
		public ImportModule(Import directory, WinNT.IMAGE_IMPORT_DESCRIPTOR header)
		{
			this.Directory = directory ?? throw new ArgumentNullException(nameof(directory));
			this.Header = header;
		}

		/// <summary>Get all import procedures from module</summary>
		/// <returns>Import procedures</returns>
		public IEnumerator<WinNT.IMAGE_IMPORT_BY_NAME> GetEnumerator()
		{
			if(!this.Header.IsEmpty)
			{
				UInt32 count = 0;
				UInt32 sizeOfUInt = sizeof(UInt32);
				while(true)
				{
					UInt32 position = this.Header.Characteristics == 0
						? this.Header.FirstThunk + sizeOfUInt * count++
						: this.Header.Characteristics + sizeOfUInt * count++;
					UInt32 thunkRef = this.Directory.Parent.Header.PtrToStructure<UInt32>(position);
					if(thunkRef == 0)
						break;

					WinNT.IMAGE_IMPORT_BY_NAME thunkImport = this.GetImageImport(position, true);
					yield return thunkImport;
				}
			}
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		/// <summary>Get Import Information</summary>
		/// <param name="offset">Shift in PE file</param>
		/// <param name="isRva">Get import by Relative Virtual Address address. Otherwise, the Virtual Address is passed</param>
		/// <returns>Structure</returns>
		private WinNT.IMAGE_IMPORT_BY_NAME GetImageImport(UInt32 offset, Boolean isRva)
		{
			PEHeader header = this.Directory.Parent.Header;
			UInt64 forwarderString;
			if(header.Is64Bit)
			{
				WinNT.IMAGE_THUNK_DATA64 thunk64 = header.PtrToStructure<WinNT.IMAGE_THUNK_DATA64>(offset);
				forwarderString = thunk64.ForwarderString;//TODO: Вот тут может быть ошибка
			} else
			{
				WinNT.IMAGE_THUNK_DATA32 thunk32 = header.PtrToStructure<WinNT.IMAGE_THUNK_DATA32>(offset);
				forwarderString = thunk32.ForwarderString;
			}

			WinNT.IMAGE_IMPORT_BY_NAME result;
			if((forwarderString & 0x80000000) != 0)
			{
				result = new WinNT.IMAGE_IMPORT_BY_NAME()
				{
					Hint = (UInt16)(forwarderString & 0x7FFFFFFF),
					Name = null,
				};
			} else if((forwarderString & 0x8000000000000000) != 0)
			{
				result = new WinNT.IMAGE_IMPORT_BY_NAME()
				{
					Hint = (UInt16)(forwarderString & 0x7FFFFFFFFFFFFFFF),
					Name = null,
				};
			} else
			{
				if(!isRva)
				{
					UInt64 imageBase=header.Is64Bit
						? header.HeaderNT64.OptionalHeader.ImageBase
						: header.HeaderNT32.OptionalHeader.ImageBase;

					forwarderString -= imageBase;
				}

				result = header.PtrToStructure<WinNT.IMAGE_IMPORT_BY_NAME>((UInt32)forwarderString);
			}
			return result;
		}
	}
}