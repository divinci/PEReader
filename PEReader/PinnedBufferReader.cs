﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace AlphaOmega.Debug
{
	/// <summary>Reader from memory allocated bytes array</summary>
	[DefaultProperty("Length")]
	public class PinnedBufferReader : IDisposable
	{
		#region Fields
		private GCHandle _gcHandle;
		private IntPtr _gcPointer;
		private readonly Byte[] _bytes;
		#endregion Fields
		#region Properties
		/// <summary>Bytes array</summary>
		private Byte[] Bytes { get { return this._bytes; } }
		/// <summary>Allocated handle</summary>
		private IntPtr Handle { get { return this._gcPointer; } }
		/// <summary>Read byte from index</summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Byte this[UInt32 index]
		{
			get { return this.Bytes[index]; }
		}
		/// <summary>Length of the array</summary>
		public Int32 Length { get { return this.Bytes.Length; } }
		#endregion Properties
		/// <summary>Create instance of bytesreader class</summary>
		/// <param name="bytes">Bytes</param>
		public PinnedBufferReader(Byte[] bytes)
		{
			this._bytes = bytes;
			this._gcHandle = GCHandle.Alloc(this._bytes, GCHandleType.Pinned);

			// Get the address of the data array
			this._gcPointer = this._gcHandle.AddrOfPinnedObject();
		}

		/// <summary>Накладывание структуры на массив байт и увеличивание отступа на размер массива</summary>
		/// <typeparam name="T">Тип накладываемой структуры</typeparam>
		/// <param name="padding">Отступ от начала массива байт и отступ от начала массива + конца структуры</param>
		/// <returns>Наложенная структура с данными</returns>
		public T BytesToStructure<T>(ref UInt32 padding) where T : struct
		{
			Int32 length;
			T result = this.BytesToStructure<T>(padding, out length);
			padding += (UInt32)length;
			return result;
		}
		/// <summary>Накладывание структуры на массив байт</summary>
		/// <typeparam name="T">Тип накладываемой структуры</typeparam>
		/// <param name="padding">Отступ от начала массива байт</param>
		/// <returns>Наложенная структура с данными</returns>
		public T BytesToStructure<T>(UInt32 padding) where T : struct
		{
			Int32 length;
			return this.BytesToStructure<T>(padding, out length);
		}
		/// <summary>Накладывание структуры на массив байт</summary>
		/// <typeparam name="T">Тип накладываемой структуры</typeparam>
		/// <param name="padding">Отступ от начала массива байт</param>
		/// <param name="length">Размер полчившегося массива</param>
		/// <exception cref="T:ArgumentOutOfRangeException">padding+structure size is out of range of byte array</exception>
		/// <returns>Наложенная структура с данными</returns>
		public T BytesToStructure<T>(UInt32 padding, out Int32 length) where T : struct
		{
			length = Marshal.SizeOf(typeof(T));
			if(length + padding > this.Bytes.Length)
				throw new ArgumentOutOfRangeException("padding");

			IntPtr ptr;
			if(padding == 0)
				ptr = this.Handle;
			else
				ptr = new IntPtr(this._gcPointer.ToInt64() + padding);

			return (T)Marshal.PtrToStructure(ptr, typeof(T));
		}
		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="padding">Отступ от начала массива</param>
		/// <returns>Получаемая строка</returns>
		public String BytesToStringUni(UInt32 padding)
		{
			Int32 length;
			return this.BytesToStringUni(padding, out length);
		}
		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="padding">Отступ от начала массива, который после возврата станет отступом от конца строки</param>
		/// <returns>Получаемая строка</returns>
		public String BytesToStringUni(ref UInt32 padding)
		{
			Int32 length;
			String result = this.BytesToStringUni(padding, out length);
			padding += (UInt32)length;
			return result;
		}
		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="padding">Отступ от начала массива</param>
		/// <param name="length">Результатирующий размер строки</param>
		/// <exception cref="T:ArgumentOutOfRangeException">Bytes array is smaller than padding</exception>
		/// <returns>Получаемая строка</returns>
		public String BytesToStringUni(UInt32 padding, out Int32 length)
		{
			if(padding > this.Bytes.Length)
				throw new ArgumentOutOfRangeException("padding");

			IntPtr ptr;
			if(padding == 0)
				ptr = this.Handle;
			else
				ptr = new IntPtr(this._gcPointer.ToInt64() + padding);

			String result = Marshal.PtrToStringUni(ptr);
			length = (result.Length + 1) * Marshal.SystemDefaultCharSize;
			return result;
		}
		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="padding">Отступ от начала массива</param>
		/// <returns>Получаемая строка</returns>
		public String BytesToStringAnsi(UInt32 padding)
		{
			Int32 length;
			return this.BytesToStringAnsi(padding, out length);
		}
		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="padding">Отступ от начала массива, который после возврата станет отступом от конца строки</param>
		/// <returns>Получаемая строка</returns>
		public String BytesToStringAnsi(ref UInt32 padding)
		{
			Int32 length;
			String result = this.BytesToStringAnsi(padding, out length);
			padding += (UInt32)length;
			return result;
		}
		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="padding">Offset from the beginning of the array</param>
		/// <param name="length">Результатирующий размер строки</param>
		/// <exception cref="T:ArgumentOutOfRangeException">Bytes array is smaller than padding</exception>
		/// <returns>Получаемая строка</returns>
		public String BytesToStringAnsi(UInt32 padding, out Int32 length)
		{
			if(padding > this.Bytes.Length)
				throw new ArgumentOutOfRangeException("padding");

			IntPtr ptr;
			if(padding == 0)
				ptr = this.Handle;
			else
				ptr = new IntPtr(this._gcPointer.ToInt64() + padding);

			String result = Marshal.PtrToStringAnsi(ptr);
			length = (result.Length + 1);//ANSII length == 1
			return result;
		}
		/// <summary>Get bytes from data</summary>
		/// <param name="padding">Offset from the beginning of the array</param>
		/// <param name="length">Length</param>
		/// <exception cref="T:ArgumentOutOfRangeException">index>Index and length is larger than bytes array length</exception>
		/// <returns>Bytes from index</returns>
		public Byte[] GetBytes(UInt32 padding, UInt32 length)
		{
			if(padding + length > this.Bytes.Length)
				throw new ArgumentOutOfRangeException("padding");

			Byte[] result = new Byte[length];
			Array.Copy(this.Bytes, padding, result, 0, result.Length);
			return result;
		}
		/// <summary>Накладывание структуры на массив байт и увеличивание отступа на размер массива</summary>
		/// <typeparam name="T">Тип накладываемой структуры</typeparam>
		/// <param name="bytes">Массив байт на который наложить структуру</param>
		/// <param name="padding">Отступ от начала массива байт и отступ от начала массива + конца структуры</param>
		/// <returns>Наложенная структура с данными</returns>
		public static T BytesToStructure<T>(Byte[] bytes, ref UInt32 padding) where T : struct
		{
			Int32 length;
			T result = PinnedBufferReader.BytesToStructure<T>(bytes, padding, out length);
			padding += (UInt32)length;
			return result;
		}
		/// <summary>Накладывание структуры на массив байт</summary>
		/// <typeparam name="T">Тип накладываемой структуры</typeparam>
		/// <param name="bytes">Массив байт на который наложить структуру</param>
		/// <param name="padding">Отступ от начала массива байт</param>
		/// <returns>Наложенная структура с данными</returns>
		public static T BytesToStructure<T>(Byte[] bytes, UInt32 padding) where T : struct
		{
			Int32 length;
			return PinnedBufferReader.BytesToStructure<T>(bytes, padding, out length);
		}
		/// <summary>Накладывание структуры на массив байт</summary>
		/// <typeparam name="T">Тип накладываемой структуры</typeparam>
		/// <param name="bytes">Массив байт на который наложить структуру</param>
		/// <param name="padding">Отступ от начала массива байт</param>
		/// <param name="length">Размер полчившегося массива</param>
		/// <returns>Наложенная структура с данными</returns>
		public static T BytesToStructure<T>(Byte[] bytes, UInt32 padding, out Int32 length) where T : struct
		{
			using(PinnedBufferReader reader = new PinnedBufferReader(bytes))
				return reader.BytesToStructure<T>(padding, out length);
		}
		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="bytes">Массив байт</param>
		/// <param name="padding">Отступ от начала массива</param>
		/// <param name="length">Результатирующий размер строки</param>
		/// <returns>Получаемая строка</returns>
		public static String BytesToStringUni(Byte[] bytes, UInt32 padding, out Int32 length)
		{
			using(PinnedBufferReader reader = new PinnedBufferReader(bytes))
				return reader.BytesToStringUni(padding, out length);
		}
		/// <summary>Преобразование массива байт от отступа в строку</summary>
		/// <param name="bytes">Массив байт</param>
		/// <param name="padding">Отступ от начала массива</param>
		/// <param name="length">Результатирующий размер строки</param>
		/// <returns>Получаемая строка</returns>
		public static String BytesToStringAnsi(Byte[] bytes, UInt32 padding, out Int32 length)
		{
			using(PinnedBufferReader reader = new PinnedBufferReader(bytes))
				return reader.BytesToStringAnsi(padding, out length);
		}
		/// <summary>Преобразование структуры из памяти в массив байт</summary>
		/// <typeparam name="T">Структура, которую необходимо преобразовать</typeparam>
		/// <param name="structure">Структура для преобразования</param>
		/// <returns>Массив байт</returns>
		public static Byte[] StructureToArray<T>(T structure) where T : struct
		{
			Int32 length = Marshal.SizeOf(structure);
			Byte[] result = new Byte[length];

			IntPtr ptr = Marshal.AllocHGlobal(length);
			try
			{
				Marshal.StructureToPtr(structure, ptr, true);
				Marshal.Copy(ptr, result, 0, length);
			} finally
			{
				Marshal.FreeHGlobal(ptr);
			}
			return result;
		}
		/// <summary>Release allocated memory</summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>Release allocated memory</summary>
		/// <param name="disposing">Free managed resources</param>
		protected virtual void Dispose(Boolean disposing)
		{
			if(disposing && this._gcHandle.IsAllocated)
				this._gcHandle.Free();
		}
	}
}