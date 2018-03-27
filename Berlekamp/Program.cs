using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Berlekamp
{
    class Program
    {
        static Encoding cp866 = Encoding.GetEncoding("cp866");
        static Random random = new Random();
        static string BytesToBinary(byte[] bytes)
        {
            string str = "";
            foreach (byte b in bytes)
            {
                str += Convert.ToString(b, 2).PadLeft(8, '0');

            }
            return str;
        }
        static byte[] BinaryToBytes(string str)
        {
            int len = str.Length / 8;
            byte[] bytes = new byte[len];
            for(int i = 0; i < len; ++i)
            {
                bytes[i] = Convert.ToByte(str.Substring(8 * i, 8), 2);
            }
            return bytes;
        }
        static void Shuffle<T>(T[] array)
        {
            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                int r = i + random.Next(n - i);
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }
        static string GenerateKey (int len) 
        {
            string y = new string('1', len / 2);
            string n = new string('0', len / 2);
            string key = y + n;
            char[] chararr = key.ToCharArray();
            Shuffle(chararr);
            Shuffle(chararr);
            return new string(chararr);
        }
        static byte[] Xor(byte[] input, byte[] key)
        {
            byte[] bytes = new byte[input.Length];
            for (int i = 0; i<input.Length; i++)
            {
                bytes[i] = (byte)(input[i]^key[i]);
            }
            return bytes;
        }
        static string Xor(string input, string key)
        {
            string binary = "";
            for (int i = 0; i < input.Length; i++)
            {
                binary += input[i]==key[i]?"0":"1";
            }
            return binary;
        }
        static void Berlekamp(string keybinary)
        {
            byte[] bytes = new byte[keybinary.Length];
            for (int i = 0; i < keybinary.Length; i++)
            {
                bytes[i] = Convert.ToByte(keybinary[i] - 48);
            }
            int L, N, m, d;
            int n = bytes.Length;
            byte[] c = new byte[n];
            byte[] b = new byte[n];
            byte[] t = new byte[n];
            
            b[0] = c[0] = 1;
            N = L = 0;
            m = -1;
            
            while (N < n)
            {
                d = bytes[N];
                for (int i = 1; i <= L; i++)
                    d ^= c[i] & bytes[N - i];            //(d+=c[i]*s[N-i] mod 2)
                if (d == 1)
                {
                    Array.Copy(c, t, n);    //T(D)<-C(D)
                    for (int i = 0; (i + N - m) < n; i++)
                        c[i + N - m] ^= b[i];
                    if (L <= (N >> 1))
                    {
                        L = N + 1 - L;
                        m = N;
                        Array.Copy(t, b, n);    //B(D)<-T(D)
                    }
                }
                N++;

            }
            Console.WriteLine("Максимальная степень уравнения: {0}", L);
            List<string> list = new List<string>();
            for (int y = 0; y < c.Length; y++)
            {
                if (c[y] == 1)
                {
                    for (int q = 0; q <= L; q++)
                    {
                        if (c[q] == 1)
                        {
                            if ((L - q) != 0) list.Add(String.Format("x^{0}", L - q));
                            else list.Add("1");
                        }
                        y++;
                    }
                }
                else
                {
                    y++;
                }
            }
            Console.WriteLine(String.Format("Уравнение : {0}",String.Join("+", list)));
        }
        static void PAKF(string keybinary)
        {
            string bytes = keybinary;
            int len = bytes.Length;
            List<int> C = new List<int>();
            for (int i=0; i< keybinary.Length; i++)
            {
                //Console.WriteLine(bytes);
                string xor = Xor(keybinary, bytes);
                int w = xor.Count(ch => ch == '1');
                C.Add(len - 2 * w);
                string last = bytes[len - 1].ToString();
                bytes = bytes.Insert(0, last);
                bytes = bytes.Remove(len);
            }
            C.RemoveAt(0);
            double Ct = C.Max();
            double temp = Math.Sqrt(Convert.ToDouble(keybinary.Length))+1;
            Console.WriteLine("|sqrt(N)|+1={0}", temp);
            Console.WriteLine("|Ct|={0}", Ct);
        }
        static void AAKF(string keybinary)
        {
            string bytes = keybinary;
            string oldbytes = keybinary;
            int len = bytes.Length;
            List<int> R = new List<int>();
            for (int i = 0; i < keybinary.Length; i++)
            {
                //Console.WriteLine(bytes);
                string xor = Xor(oldbytes, bytes);
                int w = xor.Count(ch => ch == '1');
                R.Add(len - 2 * w);
                bytes = bytes.Remove(--len);
                oldbytes = oldbytes.Remove(0,1);
            }
            R.RemoveAt(0);
            double Rt = R.Max();
            Console.WriteLine("|Rt|={0}", Rt);
            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Введите исходный текст: He");
            string inputstring = /*Console.ReadLine()*/ "He";
            byte[] inputbytes = cp866.GetBytes(inputstring);
            string inputbinary = BytesToBinary(inputbytes);
            Console.WriteLine("Текст в двоичном виде : {0}", inputbinary);
            string keybinary = GenerateKey(inputbinary.Length);
            byte[] keybytes = BinaryToBytes(keybinary);
            Console.WriteLine("Ключ                  : {0}", keybinary);
            byte[] encodedbytes = Xor(inputbytes, keybytes);
            Console.WriteLine("Зашифрованная строка  : {0}", BytesToBinary(encodedbytes));
            byte[] decodedbytes = Xor(encodedbytes, keybytes);
            Console.WriteLine("Зашифрованный текст   : {0}", cp866.GetString(encodedbytes));

            Console.WriteLine("Расшифрованная строка : {0}", BytesToBinary(decodedbytes));
            string decodedstring = cp866.GetString(decodedbytes);
            Console.WriteLine("Итоговый текст        : {0}", decodedstring);
            string str2 = cp866.GetString(inputbytes);
            Console.WriteLine(new string('—',20));
            Console.WriteLine("Проверка ключа");
            Berlekamp(keybinary);
            PAKF(keybinary);
            AAKF(keybinary);//0001101
            Console.ReadLine();


        }
    }
}
