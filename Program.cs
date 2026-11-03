using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

//kept getting confused by w, h, x, and y so gemini simplified parameters to just x and y for all subroutines
namespace Ascart
{
    class Program
    {
        static void writecolmenu(int sel, string[] option)
        {
            for (int i = 0; i < option.Length; i++) { if (i == sel) { Console.Write($"\x1b[32;2;50;250;20m{option[i]}"); } else { Console.ForegroundColor = ConsoleColor.White; Console.Write(option[i]); } }Console.SetCursorPosition(0, Console.CursorTop);
        }
        static int avgpxcol(Bitmap B, int x, int y)
        {
            Color px = returnpxcol(B, x, y);
            return (px.R + px.G + px.B) / 3;
        }
        static Color returnpxcol(Bitmap B, int x, int y)
        {
            return B.GetPixel(x, y);
        }
        static char choosechar(int px, object[,] objects) { int counter = 0; for (int i = 0; i < objects.GetLength(1); i++) { if (px > Convert.ToInt32(objects[1, i])) { counter++; } } return Convert.ToChar(objects[0, counter]); } //myself
        [STAThread] //Single Thread Apartment Thread - Windows Uses this for clipboard or something; only the thread that has created the data can access it.

        static void Main(string[] args)
        {
            bool exit = false;
            object[,] Blocks = { { ' ', '.', ',', ':', ';', '¦', '/', 'X', '#', '@', '█' }, { 10, 50, 75, 100, 125, 150, 175, 200, 225, 250, 255 } };

            string[] option = { "Greyscale ", "Colour (CharVar™) ", "Colour (Regular)" };
            while(!exit)
{
                Console.ResetColor();
                Console.WriteLine("\nEnter path or command... > ");
                string path = Convert.ToString(Console.ReadLine());
                if (path != null) { path = path.Replace("\"", "").Trim(); }

                if (path.Equals("c", StringComparison.OrdinalIgnoreCase))
                {
                    Console.Clear();
                    continue;
                }
                else if (path.Equals("x", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Program will close in 1 second.");
                    Thread.Sleep(1000);
                    exit = true;
                    break;
                }
                else if (path.Equals("nuke", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Launching new console window...");
                    ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", $"/c start \"\" \"{Environment.ProcessPath}\"")
                    {
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                    exit = true;
                    break;
                }

                //Clipboard, URLs, File Paths - gemini
                Bitmap bitmap = null;

                if (path.Equals("clip", StringComparison.OrdinalIgnoreCase) || path.Equals("clipboard", StringComparison.OrdinalIgnoreCase))
                {
                    if (Clipboard.ContainsImage())
                    {
                        using (Image clipImg = Clipboard.GetImage()) { bitmap = new Bitmap(clipImg); }
                    }
                    else if (Clipboard.ContainsFileDropList())
                    {
                        var files = Clipboard.GetFileDropList();
                        if (files.Count > 0) { bitmap = new Bitmap(files[0]); }
                        else { Console.WriteLine("Error: Clipboard file list is empty!"); continue; }
                    }
                    else { Console.WriteLine("Error: No image or screenshot found in clipboard!"); continue; }
                }
                else if (path.StartsWith("http://") || path.StartsWith("https://"))
                {
                    using (HttpClient client = new HttpClient())
                    {
                        byte[] imageBytes = client.GetByteArrayAsync(path).Result;
                        using (MemoryStream ms = new MemoryStream(imageBytes)) { bitmap = new Bitmap(ms); }
                    }
                }
                else
                {
                    try
                    {
                        bitmap = new Bitmap(path);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error: Invalid path or command!");
                        continue;
                    }
                }
                int sel = 0;
                bool choosecolopt = true;
                writecolmenu(sel, option);

                while (choosecolopt)
                {
                    ConsoleKeyInfo k = Console.ReadKey(true);
                    if (k.Key == ConsoleKey.LeftArrow && sel > 0) { sel--; writecolmenu(sel, option); }
                    else if (k.Key == ConsoleKey.RightArrow && sel < option.Length - 1) { sel++; writecolmenu(sel, option); }
                    else if (k.Key == ConsoleKey.Enter) { choosecolopt = false; }
                }
                Console.ForegroundColor = ConsoleColor.White;
                int step; Console.WriteLine("");
                Console.WriteLine("Enter step: "); //how many pixels the thing will jump before the next RGB mean is evaluated
                step = Convert.ToInt32(Console.ReadLine());

                if (bitmap != null)
                {
                    double aspectRatio = 0.5; // 0.5 is better than 2/3 strangely, even though a character width is 2/3 its height

                    if (sel == 1 || sel == 2) // Colour option
                    {
                        for (double y = 0; y < bitmap.Height; y += (step / aspectRatio))
                        {
                            for (double x = 0; x < bitmap.Width; x += step)
                            {
                                int intX = (int)x;
                                int intY = (int)y;

                                if (intX >= bitmap.Width || intY >= bitmap.Height) continue;

                                Color pxColor = returnpxcol(bitmap, intX, intY);
                                char asciiChar = choosechar(avgpxcol(bitmap, intX, intY), Blocks);
                                if (sel == 1)
                                { Console.Write($"\x1b[38;2;{pxColor.R};{pxColor.G};{pxColor.B}m{asciiChar}"); }
                                else { Console.Write($"\x1b[38;2;{pxColor.R};{pxColor.G};{pxColor.B}m█"); }
                            }
                            Console.WriteLine();
                        }
                    }
                    else // Greyscale option
                    {
                        for (double y = 0; y < bitmap.Height; y += (step / aspectRatio))
                        {
                            for (double x = 0; x < bitmap.Width; x += step)
                            {
                                int intX = (int)x;
                                int intY = (int)y;

                                if (intX >= bitmap.Width || intY >= bitmap.Height) continue;

                                char asciiChar = choosechar(avgpxcol(bitmap, intX, intY), Blocks);
                                Console.Write(asciiChar);
                            }
                            Console.WriteLine();
                        }
                    }
                    Console.Write("\x1b[0m"); // Reset term colour
                }
            }
        }
    } 
}