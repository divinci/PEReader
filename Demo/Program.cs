﻿using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using AlphaOmega.Debug.CorDirectory.Meta;
using AlphaOmega.Debug.CorDirectory.Meta.Tables;

namespace AlphaOmega.Debug
{
	class Program
	{
		static void Main(String[] args)
		{
#if NETCOREAPP
			//.NET Core Error: No data is available for encoding 1252. For information on defining a custom encoding, see the documentation for the Encoding.RegisterProvider method
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
			//String dll = @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\symsrv.yes";
			//String obj = @"C:\Visual Studio Projects\C++\DBaseTool\DBaseTool_src\Debug\TabPageSSL.obj";
			//String dll = @"C:\Visual Studio Projects\C++\DBaseTool\DBaseTool_U.exe";
			//String dll = @"C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe";
			//String dll = @"C:\Windows\Microsoft.NET\assembly\GAC_32\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll";
			//String dll = @"C:\Windows\SysWOW64\jscript.dll";
			//String dll = @"C:\WINDOWS\System32\Mpegc32f.dll";//TODO: Не получается прочитать PE файл через стандартный WinApi
			//String dll = @"C:\Visual Studio Projects\C++\SeQueL Explorer\bin\ManagedFlatbed.dll";
			//TODO: Не правильно читается MSDOS файл. (Вылетает с ошибкой при поптыке чтения по адресу e_lfanew)
			//String dll = @"C:\Program Files\HOMM_3.5\Data\zvs\h3blade.exe";
			//String dll = @"C:\WINDOWS\WinSxS\x86_Microsoft.Tools.VisualCPlusPlus.Runtime-Libraries_6595b64144ccf1df_6.0.9792.0_x-ww_08a6620a\atl.dll";

			/*using(FileStream stream = new FileStream(dll, FileMode.Open, FileAccess.Read))
			{
				using(BinaryReader reader = new BinaryReader(stream))
				{
					ImageHlp.IMAGE_DOS_HEADER dosHeader = PtrToStructure<ImageHlp.IMAGE_DOS_HEADER>(reader, 0);
					if(dosHeader.IsValid)
					{
						ImageHlp.IMAGE_NT_HEADERS32 hdr32 = PtrToStructure<ImageHlp.IMAGE_NT_HEADERS32>(reader, dosHeader.e_lfanew);
						ImageHlp.IMAGE_NT_HEADERS64 hdr64 = PtrToStructure<ImageHlp.IMAGE_NT_HEADERS64>(reader, dosHeader.e_lfanew);
					}
				}
			}
			return;*/

			/*DynamicDllLoader loader = new DynamicDllLoader();
			Byte[] bytes = File.ReadAllBytes(dll);
			loader.LoadLibrary(bytes);
			//Console.WriteLine(String.Format("Module count: {0}", loader.BuildImportTable()));
			//foreach(String procName in loader.GetProcedures())
			//{
			//	UInt32? procAddress = loader.GetProcAddress(procName);
			//	String text = String.Format("Proc: {0} Addr: 0x{1:X}", procName, procAddress);
			//	Console.WriteLine(text);
			//}
			return;*/

			/*PEFile file = new PEFile(dll);
			return;*/

			foreach(String dll in Directory.GetFiles(@"C:\Visual Studio Projects\C#", "*.*", SearchOption.AllDirectories))
			switch(Path.GetExtension(dll.ToLowerInvariant()))
				{
				case ".dll":
				case ".exe":
					try
					{
						//ReadObjInfo(obj);
						ReadPeInfo2(dll, true, false);

					} catch(Win32Exception exc)
					{
						Utils.ConsoleWriteError(exc, $"EXCEPTION IN FILE ({dll})", false);
						/*Console.WriteLine("Do yow want to continue? (Y/N)");
						switch(Console.ReadKey().KeyChar)
						{
						case 'y':
						case 'Y':
						default:
							break;
						case 'n':
						case 'N':
							return;
						}*/
					}
					break;
				}
			return;
		}

		static void ReadObjInfo(String obj)
		{
			using(ObjFile info = new ObjFile(StreamLoader.FromFile(obj)))
			{
				if(info.IsValid)//Проверка на валидность загруженного файла
				{
					Utils.ConsoleWriteMembers(info.FileHeader);

					foreach(var section in info.Sections)
						Utils.ConsoleWriteMembers(section);

					foreach(var symbol in info.Symbols)
					{
						Utils.ConsoleWriteMembers(symbol);
						/*if(symbol.Name.Short == 0 && symbol.Name.Long != 0)
							Console.WriteLine(info.StringTable[symbol.Name.Long]);*/
					}

					foreach(var str in info.StringTable)
						Console.Write(str);
				}
			}
		}

		static void ReadPeInfo2(String dll, Boolean showDllName, Boolean pauseOnDir)
		{
			if(showDllName)
				Console.WriteLine("Reading file: {0}", dll);

			using(PEFile info = new PEFile(dll, StreamLoader.FromFile(dll)))
			{
				if(info.Header.IsValid)//Проверка на валидность загруженного файла
				{
					WinNT.IMAGE_FILE_HEADER fileHeader = info.Header.Is64Bit
					? info.Header.HeaderNT64.FileHeader
					: info.Header.HeaderNT32.FileHeader;
					Utils.ConsoleWriteMembers(fileHeader);

					foreach(var section in info.Sections)
					{
						if(section.Header.Section != null && section.Description == null)
							Console.WriteLine($"Unknown section name: {section.Header.Section}");

						Utils.ConsoleWriteMembers(section);
					}

					if(info.Header.SymbolTable != null)
						Utils.ConsoleWriteMembers(info.Header.SymbolTable.Value);

					if(!info.Resource.IsEmpty)
					{
						Console.WriteLine("===Resources===");
						Int32 directoriesCount = 0;

						foreach(var dir in info.Resource)
						{
							directoriesCount++;
							//Console.WriteLine("dir: {0}", dir.NameAddress);
							Console.WriteLine("Resource dir: {0}", dir.Name);
							foreach(var dir1 in dir)
							{
								Console.WriteLine("----- {0}", dir1.Name);
								foreach(var dir2 in dir1)
								{
									Console.WriteLine("-------- {0}", dir2.Name);
									if(dir2.DirectoryEntry.IsDataEntry)
									{
										switch(dir.DirectoryEntry.NameType)
										{
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_VERSION:
											var version1 = new AlphaOmega.Debug.NTDirectory.Resources.ResourceVersion(dir2);
											var strFileInfo = version1.GetFileInfo();
											Utils.ConsoleWriteMembers(version1.FileInfo.Value);
											
											//WinNT.StringFileInfo fInfo = NativeMethods.BytesToStructure<WinNT.StringFileInfo>(bytesV, ptr);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_STRING:
											var strings = new AlphaOmega.Debug.NTDirectory.Resources.ResourceString(dir2);
											foreach(var entry in strings)
												Utils.ConsoleWriteMembers(entry);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ACCELERATOR:
											var acc = new AlphaOmega.Debug.NTDirectory.Resources.ResourceAccelerator(dir2).ToArray();
											String testAcc=String.Empty;
											foreach(var a in acc)
												Utils.ConsoleWriteMembers(a);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MANIFEST:
											Byte[] bytesM = dir2.GetData();//http://msdn.microsoft.com/ru-ru/library/eew13bza.aspx
											String xml = System.Text.Encoding.GetEncoding((Int32)dir2.DataEntry.Value.CodePage).GetString(bytesM);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MESSAGETABLE:
											var messageTable = new AlphaOmega.Debug.NTDirectory.Resources.ResourceMessageTable(dir2);
											foreach(var entry in messageTable)
												Utils.ConsoleWriteMembers(entry);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_MENU:
											var resMenu = new AlphaOmega.Debug.NTDirectory.Resources.ResourceMenu(dir2);
											foreach(var entry in resMenu.GetMenuTemplate())
											Utils.ConsoleWriteMembers(entry);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_TOOLBAR:
											var resToolbar = new AlphaOmega.Debug.NTDirectory.Resources.ResourceToolBar(dir2);
											Utils.ConsoleWriteMembers(resToolbar.Header);

											foreach(var entry in resToolbar.GetToolBarTemplate())
												Utils.ConsoleWriteMembers(entry);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONTDIR:
											var resFontDir = new AlphaOmega.Debug.NTDirectory.Resources.ResourceFontDir(dir2);
											foreach(var fontItem in resFontDir)
												Utils.ConsoleWriteMembers(fontItem);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_FONT:
											var resFont = new AlphaOmega.Debug.NTDirectory.Resources.ResourceFont(dir2);
											Utils.ConsoleWriteMembers(resFont.Font);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_BITMAP:
											//TODO:
											// http://www.vbdotnetforums.com/graphics-gdi/49563-i-need-help-converting-bitmap-dib-intptr.html
											// http://snipplr.com/view/36593/
											// http://www.codeproject.com/Articles/16268/DIB-to-System-Bitmap
											// http://ebersys.blogspot.com/2009/06/how-to-convert-dib-to-bitmap.html
											// http://hecgeek.blogspot.com/2007/04/converting-from-dib-to.html
											// http://objectmix.com/dotnet/101391-dib-bitmap-system-drawing-bitmap.html
											var resBitmap = new AlphaOmega.Debug.NTDirectory.Resources.ResourceBitmap(dir2);
											try
											{
												Utils.ConsoleWriteMembers(resBitmap.Header);
											} catch(ArgumentOutOfRangeException exc)
											{
												Utils.ConsoleWriteError(exc, "ArgumentOutOfRangeException (Corrupt bitmap)", true);
											}
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_ICON:
											var resIcon = new AlphaOmega.Debug.NTDirectory.Resources.ResourceIcon(dir2);
											/*WinGdi.ICONDIR icoHeader = new WinGdi.ICONDIR() { idReserved = 0, idType = 1, idCount = 1 };
											List<Byte> bytes = new List<Byte>();
											bytes.AddRange(PinnedBufferReader.StructureToArray<WinGdi.ICONDIR>(icoHeader));

											var resHeader=resIcon.Header;
											Byte[] payload = resIcon.Directory.GetData().Skip(Marshal.SizeOf(typeof(WinGdi.GRPICONDIRENTRY))).ToArray();
											WinGdi.ICONDIRENTRY icoEntry = new WinGdi.ICONDIRENTRY()
											{
												bWidth = resHeader.bWidth,
												bHeight = resHeader.bHeight,
												bColorCount = resHeader.bColorCount,
												bReserved = resHeader.bReserved,
												wPlanes = resHeader.wPlanes,
												wBitCount = resHeader.wBitCount,
												dwBytesInRes = (UInt32)payload.Length,
												dwImageOffset = (UInt32)(Marshal.SizeOf(typeof(WinGdi.ICONDIR)) + Marshal.SizeOf(typeof(WinGdi.ICONDIRENTRY))),
											};
											bytes.AddRange(PinnedBufferReader.StructureToArray<WinGdi.ICONDIRENTRY>(icoEntry));
											bytes.AddRange(payload);
											File.WriteAllBytes(@"C:\Visual Studio Projects\C++\DBaseTool\DBaseTool_src\res\RT_ICON.ico", bytes.ToArray());*/
											Utils.ConsoleWriteMembers(resIcon.Header);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DLGINIT:
											var dlgInit = new AlphaOmega.Debug.NTDirectory.Resources.ResourceDialogInit(dir2);
											foreach(var initData in dlgInit)
												Utils.ConsoleWriteMembers(initData);
											break;
										case WinNT.Resource.RESOURCE_DIRECTORY_TYPE.RT_DIALOG:
											var dialog = new AlphaOmega.Debug.NTDirectory.Resources.ResourceDialog(dir2);
											try
											{
												var template = dialog.GetDialogTemplate();
												foreach(var ctrl in template.Controls)
													if(ctrl.CX < 0 || ctrl.CY < 0 || ctrl.X < 0 || ctrl.Y < 0)
													{
														Console.WriteLine("???Invalid position? ({0}) CX: {1} CY: {2} X: {3} Y: {4}", template.Title, ctrl.CX, ctrl.CY, ctrl.X, ctrl.Y);
														Console.ReadKey();
													} else
														Utils.ConsoleWriteMembers(ctrl);
											} catch(IndexOutOfRangeException exc)
											{
												Utils.ConsoleWriteError(exc, "IndexOutOfRangeException (Corrupt dialog)", true);
											} catch(ArgumentException exc)
											{
												Utils.ConsoleWriteError(exc, "ArgumentException (Corrupt dialog)", true);
											}
											break;
										}
									}
								}
							}
						}
						Console.WriteLine("Total dirs: {0}", directoriesCount);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(info.ComDescriptor != null)
					{//.NET Framework
						if(info.ComDescriptor.Resources != null)
						{
							if(info.ComDescriptor.Resources.Header.IsValid)
							{
								foreach(var item in info.ComDescriptor.Resources)
								{
									Console.WriteLine("Resource Item: {0}", item.Name);
									if(item.CanRead)
										foreach(var row in item)
											Console.WriteLine("\tResource row: {0} {1}", row.Name, row.Type);
									else
										Console.WriteLine("\t---Some object---");
								}
							} else
							{
								Console.WriteLine("INVALID. MetaData Address: {0} Resources Address: {1}", info.ComDescriptor.MetaData.Directory.VirtualAddress, info.ComDescriptor.Resources.Directory.VirtualAddress);
								Console.ReadKey();
							}
						}

						var meta = info.ComDescriptor.MetaData;
						Utils.ConsoleWriteMembers("MetaData", meta.Header.Value);

						foreach(var header in meta)
						{
							Console.WriteLine(Utils.GetReflectedMembers(header.Header));
							switch(header.Header.Type)
							{
							case Cor.StreamHeaderType.StreamTable:
								var table = (StreamTables)header;

								Console.WriteLine(Utils.GetReflectedMembers(table.StreamTableHeader));

								Array enums = Enum.GetValues(typeof(Cor.MetaTableType));
								foreach(Cor.MetaTableType type in enums)
								{
									//Пробежка по всем именованным таблицам
									PropertyInfo property = table.GetType().GetProperty(type.ToString(), BindingFlags.Instance | BindingFlags.Public);
									foreach(var row in ((IEnumerable)property.GetValue(table, null)))
									{
										switch(type)
										{
										case Cor.MetaTableType.NestedClass:
											{
												NestedClassRow nestedClassRow = (NestedClassRow)row;
												TypeDefRow parentTypeRow = nestedClassRow.EnclosingClass;
												TypeDefRow childTypeRow = nestedClassRow.NestedClass;
												String typeName = parentTypeRow.TypeNamespace + "." + parentTypeRow.TypeName + " {\r\n";
												typeName += "\tclass " + childTypeRow.TypeName + " {...}\r\n";
												typeName += "}";
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.FieldRVA:
											{
												FieldRVARow fieldRVARow = (FieldRVARow)row;
												FieldRow fieldRow = fieldRVARow.Field;
												String fieldName = fieldRow.Name + " -> 0x" + fieldRVARow.RVA.ToString("X8");
												Console.WriteLine("{0}: {1}", type, fieldName);
											}
											break;
										case Cor.MetaTableType.ImplMap:
											{
												ImplMapRow implMapRow = (ImplMapRow)row;
												ModuleRefRow moduleRow = implMapRow.ImportScope;
												String moduleName = moduleRow.Name + "-> " + implMapRow.ImportName;
												Console.WriteLine("{0}: {1}", type, moduleName);
											}
											break;
										case Cor.MetaTableType.MethodImpl:
											{
												MethodImplRow methodImplRow = (MethodImplRow)row;
												TypeDefRow typeRow = methodImplRow.Class;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName;
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.PropertyMap:
											{
												PropertyMapRow propertyMapRow = (PropertyMapRow)row;
												TypeDefRow typeRow = propertyMapRow.Parent;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName + " {\r\n";
												foreach(PropertyRow propertyRow in propertyMapRow.PropertyList)
													typeName += "\t" + propertyRow.Name+";\r\n";
												typeName += "}";
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.EventMap:
											{//TODO: Не тестировано
												EventMapRow eventMapRow = (EventMapRow)row;
												TypeDefRow typeRow = eventMapRow.Parent;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName + " {\r\n";
												foreach(EventRow eventRow in eventMapRow.EventList)
													typeName += "\tevent " + eventRow.Name+";\r\n";
												typeName += "}";
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.FieldLayout:
											{
												FieldLayoutRow fieldLayoutRow = (FieldLayoutRow)row;
												FieldRow fieldRow = fieldLayoutRow.Field;
												String fieldName = fieldRow.Name;
												Console.WriteLine("{0}: {1}", type, fieldName);
											}
											break;
										case Cor.MetaTableType.ClassLayout:
											{
												ClassLayoutRow classLayoutRow = (ClassLayoutRow)row;
												TypeDefRow typeRow = classLayoutRow.Parent;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName;
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.InterfaceImpl:
											{
												InterfaceImplRow interfaceRow = (InterfaceImplRow)row;
												TypeDefRow typeRow = interfaceRow.Class;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName;
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.TypeDef:
											{
												TypeDefRow typeRow = (TypeDefRow)row;
												String typeName = typeRow.TypeNamespace + "." + typeRow.TypeName + " {\r\n";
												foreach(var fieldRow in typeRow.FieldList)
													typeName += "\t" + fieldRow.Name + ";\r\n";
												foreach(var methodRow in typeRow.MethodList)
												{
													typeName += "\t" + methodRow.Name + "(";
													foreach(var paramRow in methodRow.ParamList)
														typeName += paramRow.Name + ", ";
													typeName = typeName.TrimEnd(',', '"', ' ') + ");\r\n";
												}
												typeName += "}";
												Console.WriteLine("{0}: {1}", type, typeName);
											}
											break;
										case Cor.MetaTableType.MethodDef:
											{
												MethodDefRow methodRow = (MethodDefRow)row;
												String methodName = methodRow.Name + "(";
												foreach(var paramRow in methodRow.ParamList)
													methodName += paramRow.Name + ", ";
												methodName = methodName.TrimEnd(',', '"', ' ') + ")";
												Console.WriteLine("{0}: {1}", type, methodName);

												//TODO: Быстрый набросок чтения тела метода
												UInt32 methodLength = 0;

												UInt32 rva = methodRow.RVA;
												Byte flags = info.Header.PtrToStructure<Byte>(rva);
												rva += sizeof(Byte);
												Boolean CorILMethod_FatFormat = (UInt16)(flags & 0x3) == 0x3;
												Boolean CorILMethod_TinyFormat = (UInt16)(flags & 0x2) == 0x2;
												Boolean CorILMethod_MoreSects = (UInt16)(flags & 0x8) == 0x8;
												Boolean CorILMethod_InitLocals = (UInt16)(flags & 0x10) == 0x10;

												if(CorILMethod_FatFormat)
												{
													Byte flags2 = info.Header.PtrToStructure<Byte>(rva);
													rva += sizeof(Byte);

													Byte headerSize = Convert.ToByte((flags2 >> 4) * 4);
													UInt16 maxStack = info.Header.PtrToStructure<UInt16>(rva);
													rva += sizeof(UInt16);

													methodLength = info.Header.PtrToStructure<UInt32>(rva);
													rva += sizeof(UInt32);

													UInt32 localVarSigTok = info.Header.PtrToStructure<UInt32>(rva);
													rva += sizeof(UInt32);
												} else
													methodLength = ((UInt32)flags >> 2);

												try
												{
													foreach(var ilLine in methodRow.Body.GetMethodBody2())
													{
														StringBuilder line = new StringBuilder("IL_" + ilLine.Line.ToString("X").PadLeft(4, '0'));
														line.Append(": " + ilLine.IL.Name);
														if(ilLine.Offset != null)
															line.Append(" " + ilLine.Offset.Value);
														else if(ilLine.Token != null)
															line.Append(" [" + ilLine.Token.TableType + "]." + ilLine.Token.RowIndex);
														else if(ilLine.StrConst != null)
															line.Append(" " + ilLine.StrConst);
														Console.WriteLine(line);
													}
													//Console.ReadKey();

													Byte[] methodBody = info.Header.ReadBytes(rva, methodLength);
													Byte[] methodBody2 = methodRow.Body.GetMethodBody();

													for(Int32 loop = 0; loop < methodLength; loop++)
														if(methodBody[loop] != methodBody2[loop])
															throw new ArgumentException("Methods not equals");
												} catch(Exception exc)
												{
													Utils.ConsoleWriteError(exc, "Error reading method body", false);
												}
											}
											break;
										default:
											Utils.ConsoleWriteMembers(row);
											break;
										}
									}

									//Пробежка по всем таблицам
									MetaTable moduleTable = table[type];

									Console.WriteLine("==MetaTableType.{0} Contents:", type);
									foreach(MetaRow row in moduleTable.Rows)
									{
										StringBuilder result =new StringBuilder();
										foreach(MetaCell cell in row)
											result.AppendFormat("{0}:\t{1}", cell.Column.Name, cell.Value);
										result.AppendLine();
										Console.WriteLine(result.ToString());
									}
									Console.WriteLine("==MetaTableType.{0} End", type);
								}
								break;
							case Cor.StreamHeaderType.Guid:
								var gHeap = (GuidHeap)header;
								Guid[] guids = gHeap.Data.ToArray();
								break;
							case Cor.StreamHeaderType.Blob:
								var bHeap = (BlobHeap)header;
								Byte[][] bytes = bHeap.Data.ToArray();
								break;
							case Cor.StreamHeaderType.String:
								var sHeap = (StringHeap)header;
								String[] strings = sHeap.Data.ToArray();
								break;
							case Cor.StreamHeaderType.UnicodeSting:
								var usHeap = (USHeap)header;
								Dictionary<Int32,String> usStrings = usHeap.GetDataString().ToDictionary(p => p.Key, p => p.Value);
								break;
							}
						}
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.ExceptionTable.IsEmpty)
					{//TODO: Ошибка при чтении
						Console.WriteLine("===Exception Table===");
						try
						{
							foreach(var entry in info.ExceptionTable)
								Utils.ConsoleWriteMembers(entry);
						} catch(ArgumentOutOfRangeException exc)
						{
							Utils.ConsoleWriteError(exc, "Exception", true);
						}
						if(pauseOnDir)
							Console.ReadKey();
					}
					if(!info.Iat.IsEmpty)
					{
						Console.WriteLine("===Import Address Table===");
						foreach(UInt32 addr in info.Iat)
							Console.WriteLine("Addr: {0:X8}", addr);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Tls.IsEmpty)
					{
						Console.WriteLine("===Thread Local Storage===");
						if(pauseOnDir)
							Console.ReadKey();
					}
					if(!info.Certificate.IsEmpty)
					{
						try
						{
							var cert = info.Certificate.Certificate.Value;
							var x509 = info.Certificate.X509;
							Console.WriteLine("===Security===");
							Utils.ConsoleWriteMembers(cert);
							Console.WriteLine("Certificate: {0}", x509 == null ? "NULL" : x509.ToString());
						} catch(ArgumentOutOfRangeException exc)
						{
							Utils.ConsoleWriteError(exc, "OverflowException (Corrupted section)", true);
						}
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.DelayImport.IsEmpty)
					{
						Console.WriteLine("===Delay Import===");
						foreach(var module in info.DelayImport)
							Console.WriteLine("Module Name: {0}\tCount: {1}", module.ModuleName, module.Count());
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Relocations.IsEmpty)
					{//File contains relocation table
						Console.WriteLine("===Relocations===");
						foreach(var block in info.Relocations)
						{
							Utils.ConsoleWriteMembers(block.Block);
							foreach(var section in block)
							{
								Utils.ConsoleWriteMembers(section);
								/*if(!Enum.IsDefined(typeof(WinNT.IMAGE_REL_BASED), section.Type))
								{
									Console.WriteLine(String.Format("Enum {0} not defined", section.Type));
									Console.ReadKey();
								}*/
							}
						}
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Debug.IsEmpty)
					{//В файле есть инормация для дебага
						Console.WriteLine("===Debug info===");
						foreach(var debug in info.Debug)
							Utils.ConsoleWriteMembers(debug);
						var pdb7 = info.Debug.Pdb7CodeView;
						if(pdb7.HasValue)
							Utils.ConsoleWriteMembers(pdb7.Value);
						var pdb2 = info.Debug.Pdb2CodeView;
						if(pdb2.HasValue)
							Utils.ConsoleWriteMembers(pdb2.Value);
						var misc = info.Debug.Misc;
						if(misc.HasValue)
							Utils.ConsoleWriteMembers(misc.Value);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.LoadConfig.IsEmpty)
					{
						Console.WriteLine("===Load Config===");
						if(info.LoadConfig.Directory32.HasValue)
						{
							var directory = info.LoadConfig.Directory32.Value;
							Utils.ConsoleWriteMembers(directory);
						} else if(info.LoadConfig.Directory64.HasValue)
						{
							var directory = info.LoadConfig.Directory64.Value;
							Utils.ConsoleWriteMembers(directory);
						} else
							throw new NotImplementedException();
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.BoundImport.IsEmpty)
					{
						Console.WriteLine("===Bound Import===");
						Console.WriteLine("ModuleName: {0}", info.BoundImport.ModuleName);
						foreach(var ffRef in info.BoundImport)
							Utils.ConsoleWriteMembers(ffRef);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Export.IsEmpty)
					{//В файле есть информация о экспортируемых функциях
						Console.WriteLine("===Export Functions===");
						Console.WriteLine("Module name: {0}", info.Export.DllName);

						foreach(var func in info.Export.GetExportFunctions())
							Utils.ConsoleWriteMembers(func);
						if(pauseOnDir)
							Console.ReadKey();
					}

					if(!info.Import.IsEmpty)
					{//В файле есть информация о импортиуемых модулях
						Console.WriteLine("===Import Modules===");
						foreach(var module in info.Import)
						{
							Console.WriteLine("Module name: {0}", module.ModuleName);
							foreach(var func in module)
								Utils.ConsoleWriteMembers(func);
						}
						if(pauseOnDir)
							Console.ReadKey();
					}
				}
			}
		}
	}
}