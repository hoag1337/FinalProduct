using System.Drawing;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

namespace FinalProduct
{
    internal class Program
    {
        static void Spinning()
        {
            ConsoleSpinner spin = new ConsoleSpinner();
            while (true)
            {
                spin.Turn();
            }
        }
        public class LogisticMap
        {
            private double rate;
            public LogisticMap(double Rate)
            {
                this.rate = Rate;
            }
            public double GetSeq(double instance)
            {
                return rate * instance * (1 - instance);
            }
        }
        public class SineMap
        {
            public double rate;
            public SineMap(double Rate)
            {
                this.rate = Rate;
            }
            public double GetSeq(double instance)
            {
                return rate * Math.Sin(Math.PI * instance);
            }
        }


        public static string GetImagePath()
        {
            string dir = "D:\\PersonelProject\\FinalProduct\\plain_image";
            List<string> imageFiles = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories).Where(file => new string[] { ".jpg", ".png" }.Contains(Path.GetExtension(file))).ToList();
            for (int i = 0; i < imageFiles.Count; i++)
            {
                Console.WriteLine("{0} .{1}", i, imageFiles[i]);
            }
            Console.Write("Choose the plain image:");
            int choose = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("\nSelected image: {0}", choose);
            string plainPath = imageFiles[choose].ToString();
            return plainPath;
        }

     /*   public static void Decrypt(string plainPath)
        {
            Console.WriteLine("Decrypting function");
            byte[,] decrypted = ConvertImage(plainPath);
            byte[][] splitted = Blockize(decrypted);
            Console.WriteLine("Please fill the key");
            Co
        }*/

        public static byte[,] ConvertImage(string plainPath)
        {
            Bitmap plainImage = new Bitmap(plainPath);
            byte[,] plainByte = new byte[plainImage.Width, plainImage.Height];

            for (int i = 0; i < plainByte.GetLength(0); i++)
            {
                for (int j = 0; j < plainByte.GetLength(1); j++)
                {
                    Color pixelColor = plainImage.GetPixel(i, j);
                    plainByte[i, j] = pixelColor.R;
                }
            }
            return plainByte;
        }

        public static List<double[]> GetRandomSeq(byte[,] plainImage)
        {
            List<double[]> seq = new List<double[]>();
            double seedX = Convert.ToDouble(Math.Abs(Math.Log10(Environment.TickCount)));
            double seedY = Convert.ToDouble(Math.Abs(Math.Log2(Environment.TickCount)));

            double logistic_rate;
            Console.Write("Get the const of the logisticmap (better in range [3.6,4]:");
            logistic_rate = Convert.ToDouble(Console.ReadLine());
            LogisticMap lmap = new LogisticMap(logistic_rate);

            double sine_rate;
            Console.Write("Get the const of the sine map ( better in (0,1] ):");
            sine_rate = Convert.ToDouble(Console.ReadLine());
            SineMap smap = new SineMap(sine_rate);

            int size;
            if (plainImage.GetLength(0) > plainImage.GetLength(1))
            {
                size = plainImage.GetLength(0);
            }
            else
            {
                size = plainImage.GetLength(1);
            }

            double[] rX = new double[size];
            double[] rY = new double[size];

            rX[0] = Math.Abs(lmap.GetSeq(seedX));
            rY[0] = Math.Abs(smap.GetSeq(seedY));
            for (int i = 1; i < size; i++)
            {
                rY[i] = Math.Abs(smap.GetSeq(rX[i - 1]));
                rX[i] = Math.Abs(lmap.GetSeq(rY[i - 1]));
            }
            seq.Add(rX);
            seq.Add(rY);
            return seq;
        }

        public static List<double[]> Justify(List<double[]> seq, byte[,] plainImage)
        {
            int width = plainImage.GetLength(0);
            int height = plainImage.GetLength(1);
            Console.WriteLine("{0}  /  {1}", width, height);
            for (int i = 0; i < seq[0].Length; i++)
            {
                seq[0][i] = Math.Floor((seq[0][i] * 1000) % width);
                seq[1][i] = Math.Floor((seq[1][i] * 1000) % height);
            }
            return seq;
        }

        public static void Analyze(byte[,] array)
        {
            int i, j;
            byte[] histogram = new byte[256];
            for (i = 0; i < 256; i++)
            {
                histogram[i] = 0;
            }
            for (i = 0; i < array.GetLength(0); i++)
            {
                for (j = 0; j < array.GetLength(1); j++)
                {
                    histogram[array[i, j]]++;
                }
            }
            string name;
            Console.Write("Fill the name of the output file: ");
            name = Console.ReadLine();
            string path = "D:\\PersonelProject\\FinalProduct\\" + name + ".csv";
            //write histo gram as xlsx file with index and value
            using (System.IO.StreamWriter file =
                           new System.IO.StreamWriter(path))
            {
                for (i = 0; i < 256; i++)
                {
                    file.WriteLine(i + "," + histogram[i]);
                }
            }
        }
        public static byte[][] Blockize(byte[,] image)
        {
            int fixed_width = image.GetLength(1) - image.GetLength(1) % 32;
            byte[] change = new byte[image.GetLength(0) * fixed_width];
            int k = 0;
            for (int i = 0; i < image.GetLength(0); i++)
            {
                for (int j = 0; j < fixed_width; j++)
                {
                    change[k] = image[i, j];
                    k++;
                }
            }
            int index = 0;
            byte[][] block = new byte[fixed_width * image.GetLength(0) / 32][];
            for (int i = 0; i < block.Length; i++)
            {
                block[i] = new byte[32];
                for (int j = 0; j < 32; j++)
                {
                    block[i][j] = change[index];
                    index++;
                }

            }
            return block;
        }
        public static void Permute(byte[,] imageByte, double[] rX, double[] rY)
        {
            for (int i = 0; i < imageByte.GetLength(0); i++)
            {
                for (int j = 0; j < imageByte.GetLength(1); j++)
                {
                    int x = Convert.ToInt32(rX[i]);
                    int y = Convert.ToInt32(rY[j]);
                    byte temp = imageByte[i, j];
                    imageByte[i, j] = imageByte[x, y];
                    imageByte[x, y] = temp;
                }
            }
        }

        public static string GetNonce()
        {
            Console.Write("Please fill in the nonce: ");
            string nonce = Console.ReadLine();
            return nonce;
        }

        public static void Diffusion(List<byte[]> splittedBlocks, string nonce)
        {
            foreach (byte[] splittedBlock in splittedBlocks)
            {
                string subnonce = nonce + splittedBlocks.IndexOf(splittedBlock).ToString();
                byte[] bytes = Encoding.UTF8.GetBytes(subnonce);
                SHA256Managed sha256hashstring = new SHA256Managed();
                byte[] hash = sha256hashstring.ComputeHash(bytes);
                StringBuilder hashString = new StringBuilder();
                foreach (byte x in hash)
                {
                    hashString.Append(string.Format("{0:x2}", x));
                }
                for (int i = 0; i < 32; i++)
                {
                    splittedBlock[i] = Convert.ToByte(splittedBlock[i] ^ Convert.ToChar(hashString[i]));
                }
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Composite Map Encryption Program");

            Console.WriteLine("--------------------------------");

            //get plain image
            string path = GetImagePath();
            byte[,] plainImage = ConvertImage(path);
            Analyze(plainImage);
            List<double[]> randomSeq = GetRandomSeq(plainImage);
            Justify(randomSeq, plainImage);
            double[] rX = randomSeq[0];
            double[] rY = randomSeq[1];
            for (int i = 0; i < rX.Length; i++)
            {
                Console.WriteLine("{0} \t {1}", rX[i], rY[i]);
            }
            Permute(plainImage, rX, rY);
            Analyze(plainImage);
            string nonce = GetNonce();
            byte[][] splittedBlocks = Blockize(plainImage);
            List<byte[]> diffus = new List<byte[]>();
            for (int i = 0; i < splittedBlocks.Length; i++)
            {
                diffus.Add(splittedBlocks[i]);
            }
            Diffusion(diffus, nonce);

            
            //save the image
            Bitmap result = new Bitmap(plainImage.GetLength(0), plainImage.GetLength(1) - plainImage.GetLength(1) % 32);
            int temp = 0;
            byte[] oneLine = new byte[diffus.Count * diffus[0].Length];
            Console.WriteLine(oneLine.Length);
            for (int i = 0; i < diffus.Count; i++)
            {
                for (int j = 0; j < diffus[0].Length; j++)
                {

                    oneLine[temp] = diffus[i][j];
                    temp++;
                }
            }
            int assem = 0;
            Console.WriteLine(oneLine.Length);
            Console.WriteLine(plainImage.GetLength(0) * (plainImage.GetLength(1) - plainImage.GetLength(1) % 32));
            byte[,] encrypted = new byte[plainImage.GetLength(0), plainImage.GetLength(1) - plainImage.GetLength(1) % 32];

            for (int i = 0; i < plainImage.GetLength(0); i++)
            {
                for (int j = 0; j < plainImage.GetLength(1) - plainImage.GetLength(1) % 32; j++)
                {
                    encrypted[i, j] = oneLine[assem];
                    assem++;
                    if (assem == plainImage.GetLength(0) * (plainImage.GetLength(1) - plainImage.GetLength(1) % 32)) break;
                }
            }
            for (int i = 0; i < result.Width; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    Color pixelColor = Color.FromArgb(encrypted[i, j], encrypted[i, j], encrypted[i, j]);
                    result.SetPixel(i, j, pixelColor);
                }
            }
            Analyze(encrypted);
            Console.Write("Save the ecnrypted file as: ");
            string resultPath = Console.ReadLine();
            result.Save("D:\\PersonelProject\\FinalProduct\\FinalProduct\\" + resultPath + ".jpg");
        }
    }
}
