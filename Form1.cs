using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace solitaire
{

	public partial class Form1 : Form
	{
		private class User
		{
			[DllImport("user32.dll")]
			public static extern IntPtr GetWindowDC(IntPtr hWnd);
			[DllImport("user32.dll")]
			public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
			[StructLayout(LayoutKind.Sequential)]
			public struct RECT
			{
				public int left;
				public int top;
				public int right;
				public int bottom;
			}
			public static RECT GetClientRect(IntPtr hWnd)
			{
				RECT result;
				GetClientRect(hWnd, out result);
				return result;
			}
			[DllImport("user32.dll")]
			static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
			[DllImport("user32.dll")]
			public static extern int GetSystemMetrics(int nIndex);
			[DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
			public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
			[DllImport("user32.dll")]
			public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
			[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
			public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int wFlags);
			public static WINDOWPLACEMENT GetPlacement(IntPtr hwnd)
			{
				WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
				placement.length = Marshal.SizeOf(placement);
				GetWindowPlacement(hwnd, ref placement);
				return placement;
			}
			[DllImport("user32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool GetWindowPlacement(
				IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
			[Serializable]
			[StructLayout(LayoutKind.Sequential)]
			internal struct WINDOWPLACEMENT
			{
				public int length;
				public int flags;
				public ShowWindowCommands showCmd;
				public System.Drawing.Point ptMinPosition;
				public System.Drawing.Point ptMaxPosition;
				public System.Drawing.Rectangle rcNormalPosition;
			}

			internal enum ShowWindowCommands : int
			{
				Hide = 0,
				Normal = 1,
				Minimized = 2,
				Maximized = 3,
			}
		}
		private class GDI32
		{

			public const int SRCCOPY = 0x00CC0020;

			[DllImport("gdi32.dll")]
			public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);

			[DllImport("gdi32.dll")]
			public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

			[DllImport("gdi32.dll")]
			public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

			[DllImport("gdi32.dll")]
			public static extern bool DeleteDC(IntPtr hDC);

			[DllImport("gdi32.dll")]
			public static extern bool DeleteObject(IntPtr hObject);

			[DllImport("gdi32.dll")]
			public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
		}
		public Form1()
		{
			InitializeComponent();
		}

		public struct min_max
		{
			public int min;
			public int max;

			public min_max (int _min, int _max) {
				this.min = _min;
				this.max = _max;
			}
		}
		public class ideal_rect
		{
			System.Collections.ArrayList l;
			public int border_t_pos;
			public int internal_border_t_pos;
			public int internal_border_b_pos;
			public int border_b_pos;

			public int border_l_pos;
			public int internal_border_l_pos;
			public int internal_border_r_pos;
			public int border_r_pos;
			public ideal_rect(int[] mas)
			{
				int i = 0;
				border_t_pos = mas[i++];
				internal_border_t_pos = mas[i++];
				internal_border_b_pos = mas[i++];
				border_b_pos = mas[i++];

				border_l_pos = mas[i++];
				internal_border_l_pos = mas[i++];
				internal_border_r_pos = mas[i++];
				border_r_pos = mas[i++];
			}
			public int[] Get() {
				int[] mas = new int[8] {
					border_t_pos,
					internal_border_t_pos,
					internal_border_b_pos,
					border_b_pos,
					
					border_l_pos,
					internal_border_l_pos,
					internal_border_r_pos,
					border_r_pos
				};

				return mas;
			}
		}
		public class line
		{
			public Point begin;
			public Point end;
			public string direction;
			public string tag;

			public line(Point b, Point e, string d, string t)
			{
				this.begin = b;
				this.end = e;
				this.direction = d;
				this.tag = t;
			}
		}

		const short SWP_NOMOVE = 0X2;
		const short SWP_NOSIZE = 1;
		const short SWP_NOZORDER = 0X4;
		const int SWP_SHOWWINDOW = 0x0040;
		const int SWP_NOREDRAW = 0x0008;
		const int SWP_HIDEWINDOW = 0x0080;

		static IntPtr WindowHandle;
		static IntPtr PrHandle;
		User.WINDOWPLACEMENT placement2;

		public static int[] find_nearest_min_max_h(int c, System.Collections.ArrayList lines, System.Collections.ArrayList list_of_marked)
		{
			int min_pos = -1;
			int max_pos = -1;
			int max = -1;
			int min = 10000;

			int y = ((line)lines[c]).begin.Y;
			int c_x = (((line)lines[c]).begin.X + ((line)lines[c]).end.X) / 2;

			for (int i = 0; i < lines.Count; i++)
			{
				int _y = ((line)lines[i]).begin.Y;
				int _c_x = (((line)lines[i]).begin.X + ((line)lines[i]).end.X) / 2;
				if (!list_of_marked.Contains(i) &&
					_c_x > c_x - 5 && _c_x < c_x + 5 &&
					_y > y - 5 && _y < y + 5 &&
					c != i
					)
				{
					list_of_marked.Add(i);
					if (_y > max)
					{
						max = _y;
						max_pos = i;
					}
					if (_y < min)
					{
						min = _y;
						min_pos = i;
					}
				}
			}
			if (min_pos == max_pos)
			{
				if (max > y)
				{
					min_pos = -1;
				}
				else
				{
					max_pos = -1;
				}
			}

			return new int[2] { min_pos, max_pos };
		}
		private void button1_Click(object sender, EventArgs e)
		{
			//ищем хэндл процесса
			Process[] p = Process.GetProcessesByName("freecell");
			if (p.Length == 0)
			{
				MessageBox.Show("Процесс не найден");
				return;
			}
			WindowHandle = p[0].MainWindowHandle;
			PrHandle = p[0].Handle;

			((Form1)((Button)sender).Parent).WindowState = FormWindowState.Minimized;

			//ицем хэндл окна
			IntPtr hwnd = User.FindWindow(null, "Солитер");
			System.Threading.Thread.Sleep(10);

			//фигачим окно в одну и ту же позицию
			var placement = User.GetPlacement(WindowHandle);
			User.ShowWindow(hwnd, 1);
			User.SetWindowPos(hwnd, 0, 0, 0, 640, 480, SWP_SHOWWINDOW);
			placement2 = User.GetPlacement(WindowHandle);

			//User.SetWindowLong32(hwnd, )
			//Определяем рамки
			var SM_CYCAPTION = User.GetSystemMetrics(4);
			var SM_CXBORDER = User.GetSystemMetrics(5);
			var SM_CYBORDER = User.GetSystemMetrics(6);
			var SM_CXEDGE = User.GetSystemMetrics(45);
			var SM_CYEDGE = User.GetSystemMetrics(46);
			var SM_CXFRAME = User.GetSystemMetrics(32);
			var SM_CYFRAME = User.GetSystemMetrics(33);
			var SM_CYMENU = User.GetSystemMetrics(15);

			//Определяем размеры рабочей области
			User.RECT t = User.GetClientRect(hwnd);

			//тут надо вычислить нормальные размеры рамок при любых условия, не ебясь со стилями, или с getsystemmetrix
			int bottom_x = (placement2.rcNormalPosition.Width - t.right) / 2;
			int bottom_y = (placement2.rcNormalPosition.Height - SM_CYCAPTION - SM_CYMENU - t.bottom) / 2;
			int x = placement2.rcNormalPosition.X + bottom_x;
			int y = placement2.rcNormalPosition.Y + SM_CYCAPTION + SM_CYMENU + bottom_y;
			int width = t.right;
			int height = t.bottom - 20;

			//Делаем скриншот
			Bitmap BMP = new Bitmap(width, height);
			System.Drawing.Graphics GFX = System.Drawing.Graphics.FromImage(BMP);
			GFX.CopyFromScreen(x, y, 0, 0, new Size(width, height));

			//если было свернуто или скрыто, то возвращаем в исходное состояние
			if (placement.showCmd == User.ShowWindowCommands.Hide)
			{
				User.ShowWindow(hwnd, 0);
			}
			else if (placement.showCmd == User.ShowWindowCommands.Minimized) 
			{
				User.ShowWindow(hwnd, 2);
			}

			//анализ пикселей
			//тупо подсчитать рамки пикселей на 640х480
			Bitmap[] BMP_Free = new Bitmap[4];
			Bitmap[] BMP_House = new Bitmap[4];
			Bitmap[,] BMP_Field = new Bitmap[8, 7];

			Bitmap s = GrayScale(BMP);
			Bitmap f = AllocationBorders(s);

			//for (int i = 0; i < 4; i++)
			//{
				//BMP_Free[i] = 
			//}

			pictureBox1.Image = f;

			//BMP.GetPixel(50, 20);
			((Form1)((Button)sender).Parent).WindowState = FormWindowState.Normal;
		}
		public static Bitmap GrayScale(Bitmap bmp)
		{
			try
			{
				// Lock the bitmap's bits.
				Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

				System.Drawing.Imaging.BitmapData bmpData =
					bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
					bmp.PixelFormat);

				// получение адреса первой строки
				IntPtr ptr = bmpData.Scan0;

				// объявление байтового массива размером с изображение
				int bytes = bmpData.Stride * bmp.Height;
				byte[] rgbValues = new byte[bytes];

				// копирование матрицы изображения в массив
				System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

				//изменение яркости
				switch (bmp.PixelFormat)
				{
					case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
					case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
					case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
						{
							int i, j;
							double yyy;
							double border_brightness = 130.0;
							int Width_1;
							if (bmp.Width * 4 % 4 == 0) Width_1 = bmp.Width * 4;
							else Width_1 = bmp.Width * 4 + 4 - bmp.Width * 4 % 4;
							for (i = 0; i < bmp.Height; i++)
							{
								for (j = 0; j < bmp.Width; j++)
								{
									yyy = (double)(0.222 * rgbValues[j * 4 + i * Width_1 + 0] +
												   0.707 * rgbValues[j * 4 + i * Width_1 + 1] +
												   0.071 * rgbValues[j * 4 + i * Width_1 + 2]);
									if (yyy > border_brightness)
									{

										rgbValues[j * 4 + i * Width_1 + 0] = (byte)255;
										rgbValues[j * 4 + i * Width_1 + 1] = (byte)255;
										rgbValues[j * 4 + i * Width_1 + 2] = (byte)255;
									}
									else
									{
										rgbValues[j * 4 + i * Width_1 + 0] = (byte)0;
										rgbValues[j * 4 + i * Width_1 + 1] = (byte)0;
										rgbValues[j * 4 + i * Width_1 + 2] = (byte)0;
									}
								}
							}
							//for (int counter = 0; counter < rgbValues.Length; counter += 3)
							//{
							//	int x = (int)(0.3 * ((int)rgbValues[counter]) + 0.59 * ((int)rgbValues[counter + 1]) + 0.11 * ((int)rgbValues[counter + 2]));
							//	rgbValues[counter] = rgbValues[counter + 1] = rgbValues[counter + 2] = (byte)x;
							//}
						} break;
					default:
						{
							MessageBox.Show("Недопустимый формат изображения!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						}
						break;
				}

				//копирование матрицы обратно в изображение
				System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

				// Unlock the bits.
				bmp.UnlockBits(bmpData);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
			return bmp;
		}
		public static Bitmap AllocationBorders(Bitmap bmp)
		{
			try
			{
				// Lock the bitmap's bits.
				Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

				System.Drawing.Imaging.BitmapData bmpData =
					bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
					bmp.PixelFormat);

				// получение адреса первой строки
				IntPtr ptr = bmpData.Scan0;

				// объявление байтового массива размером с изображение
				int bytes = bmpData.Stride * bmp.Height;
				byte[] rgbValues = new byte[bytes];

				// копирование матрицы изображения в массив
				System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

				//сам анализ
				switch (bmp.PixelFormat)
				{
					case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
					case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
					case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
						{
							int i, j, Width_1;
							if (bmp.Width * 4 % 4 == 0) Width_1 = bmp.Width * 4;
							else Width_1 = bmp.Width * 4 + 4 - bmp.Width * 4 % 4;

							//предзаполнение [2] и [3]
							for (i = 0; i < bmp.Height; i++)
							{
								for (j = 0; j < bmp.Width; j++)
								{
									rgbValues[j * 4 + i * Width_1 + 3] = 128;
									rgbValues[j * 4 + i * Width_1 + 2] = 128;
								}
							}

							//поиск точек для анализа по у
							for (i = 1; i < bmp.Height - 1; i++)
							{
								for (j = 1; j < bmp.Width - 1; j++)
								{
									if (rgbValues[j * 4 + i * Width_1] != 0)
									{
										rgbValues[(j - 1) * 4 + (i - 1) * Width_1 + 2] += 1;
										rgbValues[j * 4 + (i - 1) * Width_1 + 2] += 2;
										rgbValues[(j + 1) * 4 + (i - 1) * Width_1 + 2] += 1;

										rgbValues[(j - 1) * 4 + (i + 1) * Width_1 + 2] -= 1;
										rgbValues[j * 4 + (i + 1) * Width_1 + 2] -= 2;
										rgbValues[(j + 1) * 4 + (i + 1) * Width_1 + 2] -= 1;
									}
								}
							}
							
							//поиск точек для анализа по х
							for (j = 1; j < bmp.Width - 1; j++)
							{
								for (i = 1; i < bmp.Height - 1; i++)
								{
									if (rgbValues[j * 4 + i * Width_1] != 0)
									{
										rgbValues[(j - 1) * 4 + (i - 1) * Width_1 + 3] -= 1;
										rgbValues[(j - 1) * 4 + (i) * Width_1 + 3] -= 2;
										rgbValues[(j - 1) * 4 + (i + 1) * Width_1 + 3] -= 1;

										rgbValues[(j + 1) * 4 + (i - 1) * Width_1 + 3] += 1;
										rgbValues[(j + 1) * 4 + (i) * Width_1 + 3] += 2;
										rgbValues[(j + 1) * 4 + (i + 1) * Width_1 + 3] += 1;
									}
								}
							}
							
							//находим максимум и минимум
							byte max_x = 0, min_x = 128, max_y = 0, min_y = 128;
							for (i = 0; i < bmp.Height; i++)
							{
								for (j = 0; j < bmp.Width; j++)
								{
									if (rgbValues[j * 4 + i * Width_1 + 3] > max_x) max_x = rgbValues[j * 4 + i * Width_1 + 3];
									if (rgbValues[j * 4 + i * Width_1 + 3] < min_x) min_x = rgbValues[j * 4 + i * Width_1 + 3];
									if (rgbValues[j * 4 + i * Width_1 + 2] > max_y) max_y = rgbValues[j * 4 + i * Width_1 + 2];
									if (rgbValues[j * 4 + i * Width_1 + 2] < min_y) min_y = rgbValues[j * 4 + i * Width_1 + 2];
								}
							}
							
							//по ширине
							System.Collections.ArrayList lines = new System.Collections.ArrayList();
							for (i = 0; i < bmp.Height - 5; i++)
							{
								int first = -1, Count = 0;
								string s = "h";
								bool last_been_checked_on_one_v = false;
								for (j = 0; j < bmp.Width; j++)
								{
									if (rgbValues[j * 4 + i * Width_1 + 2] == max_y || rgbValues[j * 4 + i * Width_1 + 2] == min_y)
									{
										if (last_been_checked_on_one_v)
										{
											if (Count == 0)
											{
												first = j;
												if (rgbValues[j * 4 + i * Width_1 + 2] == max_y) s = "t";
												if (rgbValues[j * 4 + i * Width_1 + 2] == min_y) s = "b";
											}
											Count++;
										}
										else
										{
											Count = 0;
										}
										last_been_checked_on_one_v = true;
									}
									else
									{
										if (last_been_checked_on_one_v)
										{
											if (Count == 0)
											{
												first = j;
											}
											if (Count > 20)
											{
												Point p1 = new Point(first, i);
												Point p2 = new Point(j, i);
												lines.Add(new line(p1, p2, "h", s));
											}
											Count = 0;
										}
										last_been_checked_on_one_v = false;
									}
								}
							}

							//по высоте
							for (j = 0; j < bmp.Width; j++)
							{
								int first = -1, Count = 0;
								string s = "v";
								bool last_been_checked_on_one_h = false;
								for (i = 0; i < bmp.Height - 5; i++)
								{
									if (rgbValues[j * 4 + i * Width_1 + 3] == max_x || rgbValues[j * 4 + i * Width_1 + 3] == min_x)
									{
										if (last_been_checked_on_one_h)
										{
											if (Count == 0)
											{
												first = i;
												if (rgbValues[j * 4 + i * Width_1 + 3] == max_x) s = "r";
												if (rgbValues[j * 4 + i * Width_1 + 3] == min_x) s = "l";
											}
											Count++;
										}
										else
										{
											Count = 0;
										}
										last_been_checked_on_one_h = true;
									}
									else
									{
										if (last_been_checked_on_one_h)
										{
											if (Count == 0)
											{
												first = i;
											}
											if (Count > 20)
											{
												Point p1 = new Point(j, first);
												Point p2 = new Point(j, i);
												lines.Add(new line(p1, p2, "v", s));
											}
											Count = 0;
										}
										last_been_checked_on_one_h = false;
									}
								}
							}

							//чистим все
							for (i = 0; i < bmp.Height; i++)
							{
								for (j = 0; j < bmp.Width; j++)
								{
									rgbValues[j * 4 + i * Width_1 + 0] = 0;
									rgbValues[j * 4 + i * Width_1 + 1] = 0;
									rgbValues[j * 4 + i * Width_1 + 2] = 0;
									rgbValues[j * 4 + i * Width_1 + 3] = 255;
								}
							}

							//выделение отдельных границ
							System.Collections.ArrayList[] pos = new System.Collections.ArrayList[2];
							for (int m = 0; m < 2; m++)
							{
								pos[m] = new System.Collections.ArrayList();
							}
							//h, v
							System.Collections.ArrayList list_of_marked = new System.Collections.ArrayList();
							for (int c = 0; c < lines.Count; c++)
							{
								if (list_of_marked.Contains(c)) continue;
								switch (((line)lines[c]).direction)
								{
									case "h":
										{
											list_of_marked.Add(c);
											int min_pos = c;
											int max_pos = c;
											bool flag_min = true, flag_max = true;

											int[] temp = find_nearest_min_max_h(c, lines, list_of_marked);
											if (temp[0] == -1 && temp[1] == -1) {

											} else if (temp[0] == -1) {
												flag_min = false;
											} else if (temp[1] == -1) {
												flag_max = false;
											}

											if (flag_min)
											{
												int[] temp2 = find_nearest_min_max_h(temp[0], lines, list_of_marked);
												if (temp2[0] == -1)
												{
													min_pos = temp[0];
												}
												else 
												{
													min_pos = temp2[0];
												}
											}

											if (flag_max)
											{
												int[] temp3 = find_nearest_min_max_h(temp[1], lines, list_of_marked);
												if (temp3[0] == -1)
												{
													max_pos = temp[1];
												}
												else
												{
													max_pos = temp3[1];
												}
											}

											pos[0].Add(new min_max(min_pos, max_pos));
										}; break;

									case "v":
										{
											pos[1].Add(c);
										}; break;

									default: break;
								}
							}
							

							//выделяем частоту встречаемости координат
							/*
							Dictionary<int, int>[] d = new Dictionary<int, int>[2];
							for (int m = 0; m < 2; m++)
							{
								d[m] = new Dictionary<int, int>();
								for (int q = 0; q < pos[m].Count; q++)
								{
									int coord = -1;
									if (m == 0)
									{
										coord = ((line)lines[(int)pos[m][q]]).begin.Y;
									} 
									if (m == 1)
									{
										coord = ((line)lines[(int)pos[m][q]]).begin.X;
									}
									if (d[m].Keys.Contains<int>(coord))
									{
										d[m][coord]++;
									}
									else
									{
										d[m].Add(coord, 1);
									}
								}
							}*/

							//отладка - рисуем выбранные линии
							foreach (min_max item in pos[0])
							{
								switch (((line)lines[item.min]).tag)
								{
									case "t":
									case "b":
									case "h":
										{
											for (int _i = ((line)lines[item.min]).begin.X; _i <= ((line)lines[item.min]).end.X; _i++)
											{
												rgbValues[_i * 4 + ((line)lines[item.min]).begin.Y * Width_1 + 2] = 255;
											}
											for (int _i = ((line)lines[item.max]).begin.X; _i <= ((line)lines[item.max]).end.X; _i++)
											{
												rgbValues[_i * 4 + ((line)lines[item.max]).begin.Y * Width_1 + 2] = 255;
											}
										} break;

									case "r":
									case "l":
									case "v":
										{/*
											for (int _j = item.begin.Y; _j <= item.end.Y; _j++)
											{
												rgbValues[item.begin.X * 4 + _j * Width_1 + 2] = 255;
											}
										  */
										} break;

									default: break;
								}
							}
							
							//отделяем по словарю верхнюю часть от нижней
							/*
							List<int> keys = new List<int>(d[0].Keys);
							keys.Sort(); keys.Sort();
							int YY = keys[3] + 2;
							System.Collections.ArrayList[] poss = new System.Collections.ArrayList[4];
							for (int m = 0; m < 4; m++)
							{
								poss[m] = new System.Collections.ArrayList();
							}
							//верхние горизонтали, нижние горизонантали и т.д.
							for (int m = 0; m < 2; m++)
							{
								for (int q = 0; q < pos[m].Count; q++)
								{
									int p = (int)pos[m][q];
									int coord = ((line)lines[p]).end.Y;
									if (coord < YY)
									{
										poss[m * 2].Add(p);
									}
									else
									{
										poss[m * 2 + 1].Add(p);
									}
								}
							}*/
							
							//проверка на идеальный случай --- только для пустого поля
							/*
							bool flag = false;
							for (int y = 0; y < 4; y++) {
								if (poss[y].Count/2 == (int)0)
								{
									flag = true;
									break;
								}
							}
							if (flag)
							{
								//копирование матрицы обратно в изображение
								System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

								// Unlock the bits.
								bmp.UnlockBits(bmpData);

								MessageBox.Show("Случай не идеальный, попробуйте еще раз!");
								return bmp;
							}
							*/
							//сортировка горизонталей внизу

							
							//дробим прямоугольники
							/*
							ideal_rect[] mas_rect = new ideal_rect[16];
							for (j = 0; j < 16; j++)
							{
								//верхние
								if (j < 8)
								{
									int[] mas = new int[8] { 
										(int)poss[0][8*0+j], 
										(int)poss[0][8*1+j], 
										(int)poss[0][8*2+j], 
										(int)poss[0][8*3+j], 
										(int)poss[2][4*j+0], 
										(int)poss[2][4*j+1], 
										(int)poss[2][4*j+2], 
										(int)poss[2][4*j+3] 
									};
									mas_rect[j] = new ideal_rect(mas);
								}
								else
								//нижние
								{
									int[] mas = new int[8] { 
										(int)poss[1][8*0+j-8], 
										(int)poss[1][8*1+j-8], 
										(int)poss[1][8*2+j-8], 
										(int)poss[1][8*3+j-8], 
										(int)poss[3][4*(j-8)+0], 
										(int)poss[3][4*(j-8)+1], 
										(int)poss[3][4*(j-8)+2], 
										(int)poss[3][4*(j-8)+3] 
									};
									mas_rect[j] = new ideal_rect(mas);
								}	

							}
							
							//рисуем их
							for (j = 8; j < 10; j++)
							{
								int[] temp_lines_mas = mas_rect[j].Get();
								for (int k = 0; k < 8; k++)
								{
									var item = ((line)lines[temp_lines_mas[k]]);
									switch (item.tag) {
										case "t":
										case "b":
										case "h": {
											for (int _i = item.begin.X; _i <= item.end.X; _i++) {
												rgbValues[_i * 4 + item.begin.Y * Width_1 + 2] = 255;
											}
										
										} break;

										case "r":
										case "l":
										case "v": {
											for (int _j = item.begin.Y; _j <= item.end.Y; _j++)
											{
												rgbValues[item.begin.X * 4 + _j * Width_1 + 2] = 255;
											}
										} break;

										default: break;
									}
								}
							 
							}*/
							
						} break;
					default:
						{
							MessageBox.Show("Недопустимый формат изображения!", "Внимание!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						}
						break;
				}

				//копирование матрицы обратно в изображение
				System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

				// Unlock the bits.
				bmp.UnlockBits(bmpData);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
			}
			return bmp;
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

	}
}

//надо убрать выделение карты! - эмулирование фокуса на фоне или на заголовке
//забитие координат по отдельным блокам
//сделать наконец скрытие окна отладчика!!! sic!
//