// created on 20/02/2003

// Npgsql.MD5.cs
// 
// Author:
//	Brar Piening (brar@gmx.de)
//  based on md5.c by Sverre H. Huseby <sverrehu@online.no>
//  which can be found in the source distribution of PostgreSQL
//  The license to use this software is provided in the file PostgreSQL.license
//	Copyright (C) 2003 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Comment: In most places this Class is a simple translation of the c-syntax
//          in md5.c to c#-syntax
//          As it's relative md5.c it is not intended to be used outside of
//          the Project it is found in.



using System;

namespace Npgsql {

	/// <summary>
	/// Class for MD5-Authentication.
	/// </summary>
	internal class MD5 {

		/// <summary>
		/// The Hex-Digit Array used by bytesToHex
		/// </summary>
		private static readonly char[] hex = new char[]{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'};
		
		private static byte[] createPaddedCopyWithLength(byte[] b) {
			uint len = ((b == null) ? 0 : (uint)b.Length);
			uint newLen448 = len + 64 - (len % 64) - 8;
			if (newLen448 <= len)
				newLen448 += 64;

			byte[] ret = new byte[newLen448 + 8];

			b.CopyTo(ret, 0);

			/* pad */
			ret[len] = 0x80;
			for (uint q = len + 1; q < newLen448; q++) {
				ret[q] = 0x00;
			}

			/* append length as a 64 bit bitcount */
			uint len_low = len;
			/* split into two 32-bit values */
			/* we only look at the bottom 32-bits */
			uint len_high = len >> 29;
			len_low <<= 3;
			ret[newLen448++] = (byte)(len_low & 0xff);
			len_low >>= 8;
			ret[newLen448++] = (byte)(len_low & 0xff);
			len_low >>= 8;
			ret[newLen448++] = (byte)(len_low & 0xff);
			len_low >>= 8;
			ret[newLen448++] = (byte)(len_low & 0xff);
			ret[newLen448++] = (byte)(len_high & 0xff);
			len_high >>= 8;
			ret[newLen448++] = (byte)(len_high & 0xff);
			len_high >>= 8;
			ret[newLen448++] = (byte)(len_high & 0xff);
			len_high >>= 8;
			ret[newLen448] = (byte)(len_high & 0xff);

			return ret;
		}

		private static uint F(uint x, uint y, uint z){
			return(((x) & (y)) | (~(x) & (z)));
		}
		private static uint G(uint x, uint y, uint z){
			return(((x) & (z)) | ((y) & ~(z)));
		}
		private static uint H(uint x, uint y, uint z){
			return((x) ^ (y) ^ (z));
		}
		private static uint I(uint x, uint y, uint z){
			return((y) ^ ((x) | ~(z)));
		}
		private static uint ROT_LEFT(uint x, uint n){
			return(((x) << (int)(n)) | ((x) >> (int)(32 - (n))));
		}

		private static void	doTheRounds(uint[] X, uint[] state) {
			UInt32 a, b, c,	d;

			a = state[0];
			b = state[1];
			c = state[2];
			d = state[3];

			/* round 1 */
			a = b + ROT_LEFT((a + F(b, c, d) + X[0] + 0xd76aa478), 7);	/* 1 */
			d = a + ROT_LEFT((d + F(a, b, c) + X[1] + 0xe8c7b756), 12); /* 2 */
			c = d + ROT_LEFT((c + F(d, a, b) + X[2] + 0x242070db), 17); /* 3 */
			b = c + ROT_LEFT((b + F(c, d, a) + X[3] + 0xc1bdceee), 22); /* 4 */
			a = b + ROT_LEFT((a + F(b, c, d) + X[4] + 0xf57c0faf), 7);	/* 5 */
			d = a + ROT_LEFT((d + F(a, b, c) + X[5] + 0x4787c62a), 12); /* 6 */
			c = d + ROT_LEFT((c + F(d, a, b) + X[6] + 0xa8304613), 17); /* 7 */
			b = c + ROT_LEFT((b + F(c, d, a) + X[7] + 0xfd469501), 22); /* 8 */
			a = b + ROT_LEFT((a + F(b, c, d) + X[8] + 0x698098d8), 7);	/* 9 */
			d = a + ROT_LEFT((d + F(a, b, c) + X[9] + 0x8b44f7af), 12); /* 10 */
			c = d + ROT_LEFT((c + F(d, a, b) + X[10] + 0xffff5bb1), 17);		/* 11 */
			b = c + ROT_LEFT((b + F(c, d, a) + X[11] + 0x895cd7be), 22);		/* 12 */
			a = b + ROT_LEFT((a + F(b, c, d) + X[12] + 0x6b901122), 7); /* 13 */
			d = a + ROT_LEFT((d + F(a, b, c) + X[13] + 0xfd987193), 12);		/* 14 */
			c = d + ROT_LEFT((c + F(d, a, b) + X[14] + 0xa679438e), 17);		/* 15 */
			b = c + ROT_LEFT((b + F(c, d, a) + X[15] + 0x49b40821), 22);		/* 16 */

			/* round 2 */
			a = b + ROT_LEFT((a + G(b, c, d) + X[1] + 0xf61e2562), 5);	/* 17 */
			d = a + ROT_LEFT((d + G(a, b, c) + X[6] + 0xc040b340), 9);	/* 18 */
			c = d + ROT_LEFT((c + G(d, a, b) + X[11] + 0x265e5a51), 14);		/* 19 */
			b = c + ROT_LEFT((b + G(c, d, a) + X[0] + 0xe9b6c7aa), 20); /* 20 */
			a = b + ROT_LEFT((a + G(b, c, d) + X[5] + 0xd62f105d), 5);	/* 21 */
			d = a + ROT_LEFT((d + G(a, b, c) + X[10] + 0x02441453), 9); /* 22 */
			c = d + ROT_LEFT((c + G(d, a, b) + X[15] + 0xd8a1e681), 14);		/* 23 */
			b = c + ROT_LEFT((b + G(c, d, a) + X[4] + 0xe7d3fbc8), 20); /* 24 */
			a = b + ROT_LEFT((a + G(b, c, d) + X[9] + 0x21e1cde6), 5);	/* 25 */
			d = a + ROT_LEFT((d + G(a, b, c) + X[14] + 0xc33707d6), 9); /* 26 */
			c = d + ROT_LEFT((c + G(d, a, b) + X[3] + 0xf4d50d87), 14); /* 27 */
			b = c + ROT_LEFT((b + G(c, d, a) + X[8] + 0x455a14ed), 20); /* 28 */
			a = b + ROT_LEFT((a + G(b, c, d) + X[13] + 0xa9e3e905), 5); /* 29 */
			d = a + ROT_LEFT((d + G(a, b, c) + X[2] + 0xfcefa3f8), 9);	/* 30 */
			c = d + ROT_LEFT((c + G(d, a, b) + X[7] + 0x676f02d9), 14); /* 31 */
			b = c + ROT_LEFT((b + G(c, d, a) + X[12] + 0x8d2a4c8a), 20);		/* 32 */

			/* round 3 */
			a = b + ROT_LEFT((a + H(b, c, d) + X[5] + 0xfffa3942), 4);	/* 33 */
			d = a + ROT_LEFT((d + H(a, b, c) + X[8] + 0x8771f681), 11); /* 34 */
			c = d + ROT_LEFT((c + H(d, a, b) + X[11] + 0x6d9d6122), 16);		/* 35 */
			b = c + ROT_LEFT((b + H(c, d, a) + X[14] + 0xfde5380c), 23);		/* 36 */
			a = b + ROT_LEFT((a + H(b, c, d) + X[1] + 0xa4beea44), 4);	/* 37 */
			d = a + ROT_LEFT((d + H(a, b, c) + X[4] + 0x4bdecfa9), 11); /* 38 */
			c = d + ROT_LEFT((c + H(d, a, b) + X[7] + 0xf6bb4b60), 16); /* 39 */
			b = c + ROT_LEFT((b + H(c, d, a) + X[10] + 0xbebfbc70), 23);		/* 40 */
			a = b + ROT_LEFT((a + H(b, c, d) + X[13] + 0x289b7ec6), 4); /* 41 */
			d = a + ROT_LEFT((d + H(a, b, c) + X[0] + 0xeaa127fa), 11); /* 42 */
			c = d + ROT_LEFT((c + H(d, a, b) + X[3] + 0xd4ef3085), 16); /* 43 */
			b = c + ROT_LEFT((b + H(c, d, a) + X[6] + 0x04881d05), 23); /* 44 */
			a = b + ROT_LEFT((a + H(b, c, d) + X[9] + 0xd9d4d039), 4);	/* 45 */
			d = a + ROT_LEFT((d + H(a, b, c) + X[12] + 0xe6db99e5), 11);		/* 46 */
			c = d + ROT_LEFT((c + H(d, a, b) + X[15] + 0x1fa27cf8), 16);		/* 47 */
			b = c + ROT_LEFT((b + H(c, d, a) + X[2] + 0xc4ac5665), 23); /* 48 */

			/* round 4 */
			a = b + ROT_LEFT((a + I(b, c, d) + X[0] + 0xf4292244), 6);	/* 49 */
			d = a + ROT_LEFT((d + I(a, b, c) + X[7] + 0x432aff97), 10); /* 50 */
			c = d + ROT_LEFT((c + I(d, a, b) + X[14] + 0xab9423a7), 15);		/* 51 */
			b = c + ROT_LEFT((b + I(c, d, a) + X[5] + 0xfc93a039), 21); /* 52 */
			a = b + ROT_LEFT((a + I(b, c, d) + X[12] + 0x655b59c3), 6); /* 53 */
			d = a + ROT_LEFT((d + I(a, b, c) + X[3] + 0x8f0ccc92), 10); /* 54 */
			c = d + ROT_LEFT((c + I(d, a, b) + X[10] + 0xffeff47d), 15);		/* 55 */
			b = c + ROT_LEFT((b + I(c, d, a) + X[1] + 0x85845dd1), 21); /* 56 */
			a = b + ROT_LEFT((a + I(b, c, d) + X[8] + 0x6fa87e4f), 6);	/* 57 */
			d = a + ROT_LEFT((d + I(a, b, c) + X[15] + 0xfe2ce6e0), 10);		/* 58 */
			c = d + ROT_LEFT((c + I(d, a, b) + X[6] + 0xa3014314), 15); /* 59 */
			b = c + ROT_LEFT((b + I(c, d, a) + X[13] + 0x4e0811a1), 21);		/* 60 */
			a = b + ROT_LEFT((a + I(b, c, d) + X[4] + 0xf7537e82), 6);	/* 61 */
			d = a + ROT_LEFT((d + I(a, b, c) + X[11] + 0xbd3af235), 10);		/* 62 */
			c = d + ROT_LEFT((c + I(d, a, b) + X[2] + 0x2ad7d2bb), 15); /* 63 */
			b = c + ROT_LEFT((b + I(c, d, a) + X[9] + 0xeb86d391), 21); /* 64 */

			state[0] += a;
			state[1] += b;
			state[2] += c;
			state[3] += d;
		}
		private static byte[] calculateDigestFromBuffer(byte[] b) {
			byte[] input;
			if ((input = createPaddedCopyWithLength(b)) == null)
				throw new Exception("Error");

			uint newI;
			uint[] workBuff = new uint[16];
			uint[] state = new uint[4];
			state[0] = 0x67452301;
			state[1] = 0xEFCDAB89;
			state[2] = 0x98BADCFE;
			state[3] = 0x10325476;
			for (uint i = 0;;) {
				if ((newI = i + 16 * 4) > input.Length)
					break;
				uint k = i + 3;
				for (uint j = 0; j < 16; j++) {
						workBuff[j] = input[k--];
						workBuff[j] <<= 8;
						workBuff[j] |= input[k--];
						workBuff[j] <<= 8;
						workBuff[j] |= input[k--];
						workBuff[j] <<= 8;
						workBuff[j] |= input[k];
						k += 7;
				}
				doTheRounds(workBuff, state);
				i = newI;
			}

			byte[] sum = new byte[16];
			for (uint i = 0, j = 0; i < 4; i++) {
				uint k = state[i];
				sum[j++] = (byte)(k & 0xff);
				k >>= 8;
				sum[j++] = (byte)(k & 0xff);
				k >>= 8;
				sum[j++] = (byte)(k & 0xff);
				k >>= 8;
				sum[j++] = (byte)(k & 0xff);
			}
			return sum;
		}
		private static String bytesToHex(byte[] b) {
			char[] s = new char[32];
			for (int q = 0, w = 0; q < 16; q++) {
				s[w++] = hex[(b[q] >> 4) & 0x0F];
				s[w++] = hex[b[q] & 0x0F];
			}
			return String.Concat("md5", new String(s));
		}

		/// <summary>
		/// Creates a Hex-Digit string of the MD5-Hashed passwd + salt concatenation
		/// </summary>
		public static String EncryptMD5(Byte[] passwd, Byte[] salt) {
			byte[] crypt_buf = new byte[passwd.Length + salt.Length];
			passwd.CopyTo(crypt_buf, 0);
			salt.CopyTo(crypt_buf, passwd.Length);
			return bytesToHex(calculateDigestFromBuffer(crypt_buf));
		}

	}
}
