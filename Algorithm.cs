using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EncryptionDecryptionAlgo
{
    class Program
    {

		public static int[] SBOX1 = { 102534, 109904, 114069, 129821, 132944, 139741, 144436, 147462, 154247, 166746,
			195250, 202765, 217500, 225809, 227763, 228463, 228747, 240723, 244510, 248819, 266970, 270418, 272050,
			282807, 287533, 295705, 345530, 346220, 346527, 355031, 363207, 370554, 381067, 386297, 387949, 393021,
			402043, 414366, 430355, 432255, 437300, 444034, 454873, 468860, 472518, 478945, 491676, 496690, 522952,
			530608, 532881, 536353, 537610, 583757, 585678, 586575, 590619, 597911, 598560, 643485, 645661, 649714,
			655755, 676984, 685288, 689834, 689986, 695269, 702991, 705566, 746171, 750898, 752469, 754367, 773074,
			781104, 781997, 796906, 804648, 807686, 812255, 818578, 839335, 839735, 840837, 841272, 843869, 844334,
			858204, 880432, 897904, 920558, 925127, 938485, 941478, 950686, 953393, 957328, 986281, 989299};
		
		public static int seed = 02;
		public static uint[] inversemod;
		public static string Globalcipher = "a";
		public static uint[] keys;

		public static void Main()
		{
			Console.WriteLine("Enter the text you wish to encrypt: ");
			var text = Console.ReadLine();

			Console.WriteLine("Do you wish to use defualt key (press a) or random key (press b) : ");
			var keyType = Console.ReadLine();

			KeyGenerator KEY = new KeyGenerator();
            //preparing the keys.
            switch (keyType)
            {
				case "b" :
					keys = KEY.Create();
					break;
				default:
					keys = KEY.Defualt();
					break;
			}
			Console.WriteLine("____________________________________________________________________________ \n Encryption process");
			encrypt(text); // contains a the chain of methods needed for encryption.

			Console.WriteLine("____________________________________________________________________________ \n Decryption process");
			decrypt(Globalcipher); // contains a the chain of methods needed for decryption.

			Console.WriteLine("____________________________________________________________________________ \n Keys Generated" +
				"\nKey 1                             Key 2");
			var key1 = string.Join(" ", keys);
			var key2 = key1 +" "+ string.Join(" ", inversemod);

			Console.WriteLine($"key1 : {key1}\nkey2 : {key2}");
		}

		public static void encrypt(string text)
		{
			var result = GetModBlocks(text);

			var sResult = GetSBox(result);
            //to make sure that i have no repeated numbers i used linq querry and concatenated two arrrays then checked in their are any duplicate values


			var ciphernum = GetCipher(sResult);

			//var ciphertext = ConvertToText(ciphernum);

			Console.WriteLine("\n\nResulting Cipher: " + ciphernum);
			//Console.WriteLine("the output is this " + ciphertext);

			
		}

		public static uint[] GetModBlocks(string text)
		{
			Console.WriteLine("Mod operation");
			int inverse = 0;
			byte[] bytes = new byte[text.Length];
            //
            for (int i = 0; i < text.Length; i++)
            {
				bytes[i] = (byte)text[i];
            }

			var result = new uint[bytes.Length];
			inversemod = new uint[bytes.Length];
			Random rnd = new Random();
			int n = rnd.Next(100);
			for (int i = 0; i < bytes.Length; i++)
			{
				result[i] =(uint) ((int)bytes[i] % n);
				
				TryModInverse((int)bytes[i], 977, out inverse);

				inversemod[i] = (uint)inverse ^ result[i];
				Console.WriteLine(" value for x is: " + inverse);
			}

			for (int i = 0; i<result.Length; i++)
            {
				Console.WriteLine($"  pliantext block          mod block  \n  {bytes[i]}                       {result[i]}");
            }
            
			return result;
		}

		public static uint[] GetSBox(uint[] result)
        {
			Console.WriteLine("Expansion SBOX operation\n");
			int index = 0;
            //this performs expansion s box on the code.
            for (int i = 0; i < result.Length; i++)
            {	
				index = (int)result[i];

				result[i] = (uint)SBOX1[index];

				Console.WriteLine($"  {index} maps to {result[i]}");
			}
			
			return result;
        }

		

		public static string GetCipher(uint[] result)
        {
			Console.WriteLine("\n5 Rounds encruption");
			string cipher="";
            for (int i = 0; i < result.Length; i++)
            {
				Console.WriteLine($"\n  Block {i} :");
				result[i] = Convert.ToUInt32(round(result[i], i));
					
				

				Console.WriteLine("\n");
			}

            foreach (var item in result)
            {
				cipher += item.ToString().PadLeft(7, '0');
            }
			Globalcipher = cipher;

			return cipher;
		}

		public static string round(uint pblock,int location)
        {
			

			
			var cblock = 0;
			var shiftedcblock = "";

            for (int i = 0; i < 5; i++)
            {
				
				seed = (location%20 < 0)? 1 : location%20;

				cblock = (int)pblock ^ (int)keys[i];

				

				shiftedcblock = Convert.ToString(cblock, 2).PadLeft(20, '0'); //we pad values with zero so that we dont lose and information


				shiftedcblock = shiftedcblock[seed..] + shiftedcblock[..seed]; //shits block to the right according to seed
				Console.WriteLine($"   Round {i} output is : {shiftedcblock}(base 2) and {Convert.ToInt32(shiftedcblock, 2)}(base 10)");

				cblock = Convert.ToInt32(shiftedcblock, 2);
			}

			seed = 7;
			return cblock.ToString();
		}

		public static string ConvertToText(string cipher)
        {
			cipher = cipher.TrimStart();

			var temp = "";
			
            
			return temp;

		}

		////////////////////////////////////////////// DECRYPTION METHODS /////////////////////////////////////////

		public static void decrypt(string cipher)
        {
			//split cipher text into blocks so decryption process may begin
			var cBlocks = cipherBlocks(cipher);

			Console.WriteLine("Cipher split into blocks is: \n");
			foreach (var item in cBlocks)
			{
				Console.WriteLine("   seperated ciphers : " + item);
			}
			Console.WriteLine("");


			//5 decrypt operation rounds
			getSubstitutedCipher(cBlocks); 


        }

		
		public static string[] cipherBlocks(string cipher)
        {
			//prepare the array that will store the encrypted blocks
			string[] cblocks = new string[(int)Math.Ceiling(((double)cipher.Length / 7))];
			cipher = cipher.TrimStart();

            for (int i = 0; i < cblocks.Length; i++)
            {
				if(i<cblocks.Length-1)
					cblocks[i] = cipher.Substring(i * 7, 7);
				else
					cblocks[i] = cipher.Substring(i * 7);
			}
			
			return cblocks;
        }
		

		public static void getSubstitutedCipher(string[] cblock)
        {
			uint[] roundCblock = new uint[cblock.Length];

			Console.WriteLine("Round decryption process:\n");
			
            
			for (int i = 0; i < cblock.Length; i++)
			{
				
				roundCblock[i]=Uround(cblock[i],i);
				Console.WriteLine($"   Block {i} : \n   Inputed block {cblock[i]} outputed block {roundCblock[i]} \n");
				

			}

			Console.WriteLine("");

			ReverseGetSbox(roundCblock);
		}
		public static uint Uround(string cblock, int location)
		{
			
			var shiftedcblock = Convert.ToString(Convert.ToInt32(cblock), 2).PadLeft(20, '0');
			
			
			var answer = 0;
			
			for (int i = 4; i >3; i--)
            {
				seed = (location % 20 < 0) ? 1 : location % 20; //we pad values with zero so that we dont lose and information
				
				shiftedcblock = Convert.ToString(Convert.ToInt32(cblock), 2).PadLeft(20, '0');
				
				shiftedcblock = shiftedcblock[^seed..] + shiftedcblock[..^seed]; //shits block to the left according to seed

				answer = Convert.ToInt32(shiftedcblock, 2);
				answer = answer ^ (int)keys[i];
				
				cblock = answer.ToString();
			}
			seed = 7;
			
			return (uint)answer;
		}
		public static void ReverseGetSbox(uint[] cipher)
		{
			int temp = 0;
			byte[] Cipher = new byte[cipher.Length];

			Console.WriteLine("Compression SBOX:\n");

			for (int i = 0; i < cipher.Length; i++)
			{
				temp = Array.FindIndex(SBOX1, val => val.Equals((int)cipher[i])); //find where where the value is stored in the array
				
                
					Cipher[i] = (byte)temp; // allocates maps value and stores it

				Console.WriteLine($"   Input {cipher[i]} is mapped to {temp}\n");
			}
			
			ReverseMod((Cipher));
		}

		public static void ReverseMod(byte[] cipher)
		{

			Console.WriteLine("Reversing mod operation\n");
			uint temp = 0;
			int cipherSubstitute = 0;

			for (int i = 0; i < cipher.Length; i++)
            {
				temp = inversemod[i] ^ cipher[i];
				Console.WriteLine($"   Block {i}:\n   value of x for {cipher[i]} is : {temp}");
				TryModInverse((int)temp, 977, out cipherSubstitute);
				Console.WriteLine($"   value of x solves {cipher[i]} to get the plaintext ascii code : {cipherSubstitute}\n");
				cipher[i] = (byte)cipherSubstitute;
				
            }
			// we could have used the method Encoding.ASCII.GetString(cipher) but does not encode for ¬ and extended ascii;

			GetPlaintext(cipher);

		}

		public static void GetPlaintext(byte[] cipher)
        {
			string pliantext = "";
            foreach (var item in cipher)
            {
				pliantext += (item==63)? '¬' : (char)item; //c# does not recognise 63 as the character ¬, so hard coded it
            }

			Console.WriteLine("\nPlaintext is " + pliantext);
        }

		public static bool TryModInverse(int number, int modulo, out int result)
		{
			if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
			if (modulo < 2) throw new ArgumentOutOfRangeException(nameof(modulo));
			int n = number;
			int m = modulo, v = 0, d = 1;
			while (n > 0)
			{
				int t = m / n, x = n;
				n = m % x;
				m = x;
				x = d;
				d = checked(v - t * x); // Just in case
				v = x;
			}
			result = v % modulo;
			if (result < 0) result += modulo;
			if ((long)number * result % modulo == 1L) return true;
			result = default;
			return false;
		}

	}
	public class KeyGenerator
	{
		public uint[] Create()
        {
			uint[] arr = new uint[5];
			var rnd = new RNGCryptoServiceProvider();
			var b = new byte[5]; //bytes range from 0 to 255
			rnd.GetNonZeroBytes(b); // will generate 5 subkeys 

            for (int i = 0; i < 5; i++)
            {
				arr[i] = (uint)((b[i] < 100) ? b[i] + 100 : b[i]); //makes sure each subkey is of five digits.
            }
			return arr;
		}
		public uint[] Defualt()
        {
			uint[] arr = { 112, 944, 618, 777, 333 };
			return arr;
        }
    }
}
