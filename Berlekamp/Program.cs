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
        static char Xor(char input, char key)
        {
            return input == key ? '0' : '1';
        }
        static char Xor(params char[] input)
        {
            for (int i=1; i<input.Length; i++)
            {
                input[i] = Xor(input[i], input[i - 1]);
            }
            return input[input.Length - 1];
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
        static void Temp(string keybinary)
        {
            double temp = Math.Truncate(Math.Sqrt(keybinary.Length)) + 1;
            Console.WriteLine("|sqrt(N)|+1={0}", temp);
        }
        static int Move(string keybinary)
        {
            string bytes = keybinary;
            //Console.WriteLine(bytes);
            int CtMin = PAKF(bytes);
            int RtMin = AAKF(bytes);
            int len = keybinary.Length;
            for (int i = 0; i < keybinary.Length; i++)
            {
                string last = bytes[len - 1].ToString();
                bytes = bytes.Insert(0, last);
                bytes = bytes.Remove(len);
                if (CtMin > PAKF(bytes)) CtMin = PAKF(bytes);
                if (RtMin > AAKF(bytes)) RtMin = AAKF(bytes);
                //Console.WriteLine(bytes);
            }
            Console.WriteLine("|Ct|={0}", CtMin);
            Console.WriteLine("|Rt|={0}", RtMin);
            return 0;
        }
        static int PAKF(string keybinary)
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
            //Console.WriteLine(String.Format("C: {0}", String.Join(",", C)));
            C.RemoveAt(0);
            int Ct = -1;
            for (int i = 0; i < C.Count; i++)
            {
                if (Ct < Math.Abs(C[i])) Ct = Math.Abs(C[i]);
            }
            return Ct;
        }
        static int AAKF(string keybinary)
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
            //Console.WriteLine(String.Format("R: {0}", String.Join(",", R)));
            R.RemoveAt(0);
            int Rt = -1;
            for (int i=0; i < R.Count; i++)
            {
                if (Rt < Math.Abs(R[i])) Rt = Math.Abs(R[i]);
            }
            return Rt;
        }
        static string GenerateKey (int len)
        {
            List<List<int>> Dictionary = new List<List<int>>()
            {
                new List<int>{ 8, 3, 1, 0 },            //2^3    //1 символ
                new List<int>{ 16, 4, 1, 0},            //2^4    //2 символа
                new List<int>{ 32, 5, 3, 0},            //2^5    //4 символа
                new List<int>{ 64, 6, 1, 0},            //2^6    //8 символов
                new List<int>{ 128, 7, 3, 0},           //2^7    //16 символов
                new List<int>{ 256, 8, 4, 3, 2, 0},     //2^8    //32 символа
                new List<int>{ 512, 9, 4, 0},           //2^9    //64 символа
                new List<int>{ 1024, 10, 3, 0 },        //2^10   //128 символов
                new List<int>{ 2048, 11, 2, 0},         //2^11
                new List<int>{ 4096, 12, 6, 4, 1, 0},   //2^12
                new List<int>{ 8192, 13, 4, 3, 1, 0},   //2^13
                new List<int>{ 16384, 14, 10, 6, 1, 0}, //2^14
                new List<int>{ 32768, 15, 1, 0},        //2^15
                new List<int>{ 65536, 16, 12, 3, 1, 0}, //2^16
                new List<int>{ 131072, 17, 3, 0},       //2^17
                new List<int>{ 262144, 18, 7, 0},       //2^18
                new List<int>{ 524288, 19, 5, 2, 1, 0}, //2^19
                new List<int>{ 1048576, 20, 3, 0},      //2^20
                new List<int>{ 2097152, 21, 2, 0},      //2^21
                new List<int>{ 4194304, 22, 1, 0},      //2^22
                new List<int>{ 8388608, 23, 5, 0},      //2^23
                new List<int>{ 16777216, 24, 7, 2, 1, 0}//2^24
            };
            int rem = len;
            string output = "";
            while (rem != 0)
            {
                for (int i = Dictionary.Count() - 1; i >= 0; i--)
                {
                    if (rem >= Dictionary[i][0])
                    {
                        rem -= Dictionary[i][0];
                        List<int> temp = new List<int>(Dictionary[i]);
                        temp.RemoveAt(0);
                        output += GenerateSequence(temp);
                        break;
                    }
                }
            }
            return output;

        }
        static string GenerateSequence (List<int> polynomial)
        {
            string bytes = "";
            bytes = bytes.PadLeft(polynomial[0] - 1, '0');
            bytes += '1';
            for (int i = polynomial[0]; i < Math.Pow(2, polynomial[0]); i++)
            {
                char ch = new char();
                for (int j=2; j<polynomial.Count(); j++)
                {
                    ch = Xor(bytes[i-polynomial[0]+polynomial[j]],bytes[i-polynomial[0]+polynomial[j-1]]);
                }
                bytes += ch;
            }
            return bytes;
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Введите исходный текст: ");
                string inputstring = Console.ReadLine();
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
                Console.WriteLine(new string('—', 20));
                Console.WriteLine("Проверка ключа");
                Berlekamp(keybinary);
                Temp(keybinary);
                PAKF(keybinary);
                AAKF(keybinary);
                Move(keybinary);
                Console.WriteLine();
            }
        }
    }
}
