using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace Musika.Library.Utilities
{
    public class BlowfishECB
    {
        /** maximum possible key length */
        public readonly static int MAXKEYLENGTH = 56;

        /** block size of this cipher (in bytes) */
        public readonly static int BLOCKSIZE = 8;

        // size of the single boxes
        readonly static int PBOX_ENTRIES = 18;
        readonly static int SBOX_ENTRIES = 256;

        // the boxes
        int[] m_pbox;
        int[] m_sbox1;
        int[] m_sbox2;
        int[] m_sbox3;
        int[] m_sbox4;

        public BlowfishECB()
        {

        }

        /**
         * default constructor
         * @param bfkey key material, up to MAXKEYLENGTH bytes
         */
        public BlowfishECB(byte[] bfkey)
        {
            // create the boxes
            int nI;

            m_pbox = new int[PBOX_ENTRIES];

            for (nI = 0; nI < PBOX_ENTRIES; nI++)
            {
                m_pbox[nI] = pbox_init[nI];
            }

            m_sbox1 = new int[SBOX_ENTRIES];
            m_sbox2 = new int[SBOX_ENTRIES];
            m_sbox3 = new int[SBOX_ENTRIES];
            m_sbox4 = new int[SBOX_ENTRIES];

            for (nI = 0; nI < SBOX_ENTRIES; nI++)
            {
                m_sbox1[nI] = sbox_init_1[nI];
                m_sbox2[nI] = sbox_init_2[nI];
                m_sbox3[nI] = sbox_init_3[nI];
                m_sbox4[nI] = sbox_init_4[nI];
            }

            // xor the key over the p-boxes

            int nLen = bfkey.Length;
            if (nLen == 0) return; // such a setup is also valid (zero key "encryption" is possible)
            int nKeyPos = 0;
            int nBuild = 0;
            int nJ;

            for (nI = 0; nI < PBOX_ENTRIES; nI++)
            {
                for (nJ = 0; nJ < 4; nJ++)
                {
                    nBuild = (nBuild << 8) | (((int) bfkey[nKeyPos]) & 0x0ff);

                    if (++nKeyPos == nLen)
                    {
                        nKeyPos = 0;
                    }
                }
                m_pbox[nI] ^= nBuild;
            }

            // encrypt all boxes with the all zero string
            long lZero = 0;

            // (same as above)
            for (nI = 0; nI < PBOX_ENTRIES; nI += 2)
            {
                lZero = encryptBlock(lZero);
                m_pbox[nI] = (int)((ulong)lZero >> 32);
                m_pbox[nI+1] = (int) (lZero & 0x0ffffffffL);
            }
            for (nI = 0; nI < SBOX_ENTRIES; nI += 2)
            {
                lZero = encryptBlock(lZero);
                m_sbox1[nI] = (int)((ulong)lZero >> 32);
                m_sbox1[nI+1] = (int) (lZero & 0x0ffffffffL);
            }
            for (nI = 0; nI < SBOX_ENTRIES; nI += 2)
            {
                lZero = encryptBlock(lZero);
                m_sbox2[nI] = (int)((ulong)lZero >> 32);
                m_sbox2[nI+1] = (int) (lZero & 0x0ffffffffL);
            }
            for (nI = 0; nI < SBOX_ENTRIES; nI += 2)
            {
                lZero = encryptBlock(lZero);
                m_sbox3[nI] = (int)((ulong)lZero >> 32);
                m_sbox3[nI+1] = (int) (lZero & 0x0ffffffffL);
            }
            for (nI = 0; nI < SBOX_ENTRIES; nI += 2)
            {
                lZero = encryptBlock(lZero);
                m_sbox4[nI] = (int)((ulong)lZero >> 32);
                m_sbox4[nI+1] = (int) (lZero & 0x0ffffffffL);
            }
        }

        /**
         * to clear data in the boxes before an instance is freed
         */
        public void cleanUp()
        {
            int nI;

            for (nI = 0; nI < PBOX_ENTRIES; nI++)
            {
                m_pbox[nI] = 0;
            }

            for (nI = 0; nI < SBOX_ENTRIES; nI++)
            {
                m_sbox1[nI] = m_sbox2[nI] = m_sbox3[nI] = m_sbox4[nI] = 0;
            }
        }

        // internal routine to encrypt a 64bit block
        protected long encryptBlock(long lPlainBlock)
        {
            // split the block in two 32 bit halves

            int nHi = longHi32(lPlainBlock);
            int nLo = longLo32(lPlainBlock);

            // encrypt the block, gain more speed by unrooling the loop
            // (we avoid swapping by using nHi and nLo alternating at
            // odd an even loop nubers) and using local references

            int[] sbox1 = m_sbox1;
            int[] sbox2 = m_sbox2;
            int[] sbox3 = m_sbox3;
            int[] sbox4 = m_sbox4;

            int[] pbox = m_pbox;

            nHi ^= pbox[0];
            nLo ^= (((sbox1[(uint)nHi >> 24] + sbox2[((uint)nHi >> 16) & 0x0ff]) ^ sbox3[((uint)nHi >> 8) & 0x0ff]) + sbox4[nHi & 0x0ff]) ^ pbox[1];
            nHi ^= (((sbox1[(uint)nLo >> 24] + sbox2[((uint)nLo >> 16) & 0x0ff]) ^ sbox3[((uint)nLo >> 8) & 0x0ff]) + sbox4[nLo & 0x0ff]) ^ pbox[2];
            nLo ^= (((sbox1[(uint)nHi >> 24] + sbox2[((uint)nHi >> 16) & 0x0ff]) ^ sbox3[((uint)nHi >> 8) & 0x0ff]) + sbox4[nHi & 0x0ff]) ^ pbox[3];
            nHi ^= (((sbox1[(uint)nLo >> 24] + sbox2[((uint)nLo >> 16) & 0x0ff]) ^ sbox3[((uint)nLo >> 8) & 0x0ff]) + sbox4[nLo & 0x0ff]) ^ pbox[4];
            nLo ^= (((sbox1[(uint)nHi >> 24] + sbox2[((uint)nHi >> 16) & 0x0ff]) ^ sbox3[((uint)nHi >> 8) & 0x0ff]) + sbox4[nHi & 0x0ff]) ^ pbox[5];
            nHi ^= (((sbox1[(uint)nLo >> 24] + sbox2[((uint)nLo >> 16) & 0x0ff]) ^ sbox3[((uint)nLo >> 8) & 0x0ff]) + sbox4[nLo & 0x0ff]) ^ pbox[6];
            nLo ^= (((sbox1[(uint)nHi >> 24] + sbox2[((uint)nHi >> 16) & 0x0ff]) ^ sbox3[((uint)nHi >> 8) & 0x0ff]) + sbox4[nHi & 0x0ff]) ^ pbox[7];
            nHi ^= (((sbox1[(uint)nLo >> 24] + sbox2[((uint)nLo >> 16) & 0x0ff]) ^ sbox3[((uint)nLo >> 8) & 0x0ff]) + sbox4[nLo & 0x0ff]) ^ pbox[8];
            nLo ^= (((sbox1[(uint)nHi >> 24] + sbox2[((uint)nHi >> 16) & 0x0ff]) ^ sbox3[((uint)nHi >> 8) & 0x0ff]) + sbox4[nHi & 0x0ff]) ^ pbox[9];
            nHi ^= (((sbox1[(uint)nLo >> 24] + sbox2[((uint)nLo >> 16) & 0x0ff]) ^ sbox3[((uint)nLo >> 8) & 0x0ff]) + sbox4[nLo & 0x0ff]) ^ pbox[10];
            nLo ^= (((sbox1[(uint)nHi >> 24] + sbox2[((uint)nHi >> 16) & 0x0ff]) ^ sbox3[((uint)nHi >> 8) & 0x0ff]) + sbox4[nHi & 0x0ff]) ^ pbox[11];
            nHi ^= (((sbox1[(uint)nLo >> 24] + sbox2[((uint)nLo >> 16) & 0x0ff]) ^ sbox3[((uint)nLo >> 8) & 0x0ff]) + sbox4[nLo & 0x0ff]) ^ pbox[12];
            nLo ^= (((sbox1[(uint)nHi >> 24] + sbox2[((uint)nHi >> 16) & 0x0ff]) ^ sbox3[((uint)nHi >> 8) & 0x0ff]) + sbox4[nHi & 0x0ff]) ^ pbox[13];
            nHi ^= (((sbox1[(uint)nLo >> 24] + sbox2[((uint)nLo >> 16) & 0x0ff]) ^ sbox3[((uint)nLo >> 8) & 0x0ff]) + sbox4[nLo & 0x0ff]) ^ pbox[14];
            nLo ^= (((sbox1[(uint)nHi >> 24] + sbox2[((uint)nHi >> 16) & 0x0ff]) ^ sbox3[((uint)nHi >> 8) & 0x0ff]) + sbox4[nHi & 0x0ff]) ^ pbox[15];
            nHi ^= (((sbox1[(uint)nLo >> 24] + sbox2[((uint)nLo >> 16) & 0x0ff]) ^ sbox3[((uint)nLo >> 8) & 0x0ff]) + sbox4[nLo & 0x0ff]) ^ pbox[16];

            // finalize, cross and return the reassembled block
            return makeLong(nHi, nLo ^ pbox[17]);
        }

        // internal routine to decrypt a 64bit block
        protected long decryptBlock(long lCipherBlock) {
            // (same as above)

            int nHi = longHi32(lCipherBlock);
            int nLo = longLo32(lCipherBlock);

            nHi ^= m_pbox[17];
            nLo ^= (((m_sbox1[(uint)nHi >> 24] + m_sbox2[((uint)nHi >> 16) & 0x0ff]) ^ m_sbox3[((uint)nHi >> 8) & 0x0ff]) + m_sbox4[nHi & 0x0ff]) ^ m_pbox[16];
            nHi ^= (((m_sbox1[(uint)nLo >> 24] + m_sbox2[((uint)nLo >> 16) & 0x0ff]) ^ m_sbox3[((uint)nLo >> 8) & 0x0ff]) + m_sbox4[nLo & 0x0ff]) ^ m_pbox[15];
            nLo ^= (((m_sbox1[(uint)nHi >> 24] + m_sbox2[((uint)nHi >> 16) & 0x0ff]) ^ m_sbox3[((uint)nHi >> 8) & 0x0ff]) + m_sbox4[nHi & 0x0ff]) ^ m_pbox[14];
            nHi ^= (((m_sbox1[(uint)nLo >> 24] + m_sbox2[((uint)nLo >> 16) & 0x0ff]) ^ m_sbox3[((uint)nLo >> 8) & 0x0ff]) + m_sbox4[nLo & 0x0ff]) ^ m_pbox[13];
            nLo ^= (((m_sbox1[(uint)nHi >> 24] + m_sbox2[((uint)nHi >> 16) & 0x0ff]) ^ m_sbox3[((uint)nHi >> 8) & 0x0ff]) + m_sbox4[nHi & 0x0ff]) ^ m_pbox[12];
            nHi ^= (((m_sbox1[(uint)nLo >> 24] + m_sbox2[((uint)nLo >> 16) & 0x0ff]) ^ m_sbox3[((uint)nLo >> 8) & 0x0ff]) + m_sbox4[nLo & 0x0ff]) ^ m_pbox[11];
            nLo ^= (((m_sbox1[(uint)nHi >> 24] + m_sbox2[((uint)nHi >> 16) & 0x0ff]) ^ m_sbox3[((uint)nHi >> 8) & 0x0ff]) + m_sbox4[nHi & 0x0ff]) ^ m_pbox[10];
            nHi ^= (((m_sbox1[(uint)nLo >> 24] + m_sbox2[((uint)nLo >> 16) & 0x0ff]) ^ m_sbox3[((uint)nLo >> 8) & 0x0ff]) + m_sbox4[nLo & 0x0ff]) ^ m_pbox[9];
            nLo ^= (((m_sbox1[(uint)nHi >> 24] + m_sbox2[((uint)nHi >> 16) & 0x0ff]) ^ m_sbox3[((uint)nHi >> 8) & 0x0ff]) + m_sbox4[nHi & 0x0ff]) ^ m_pbox[8];
            nHi ^= (((m_sbox1[(uint)nLo >> 24] + m_sbox2[((uint)nLo >> 16) & 0x0ff]) ^ m_sbox3[((uint)nLo >> 8) & 0x0ff]) + m_sbox4[nLo & 0x0ff]) ^ m_pbox[7];
            nLo ^= (((m_sbox1[(uint)nHi >> 24] + m_sbox2[((uint)nHi >> 16) & 0x0ff]) ^ m_sbox3[((uint)nHi >> 8) & 0x0ff]) + m_sbox4[nHi & 0x0ff]) ^ m_pbox[6];
            nHi ^= (((m_sbox1[(uint)nLo >> 24] + m_sbox2[((uint)nLo >> 16) & 0x0ff]) ^ m_sbox3[((uint)nLo >> 8) & 0x0ff]) + m_sbox4[nLo & 0x0ff]) ^ m_pbox[5];
            nLo ^= (((m_sbox1[(uint)nHi >> 24] + m_sbox2[((uint)nHi >> 16) & 0x0ff]) ^ m_sbox3[((uint)nHi >> 8) & 0x0ff]) + m_sbox4[nHi & 0x0ff]) ^ m_pbox[4];
            nHi ^= (((m_sbox1[(uint)nLo >> 24] + m_sbox2[((uint)nLo >> 16) & 0x0ff]) ^ m_sbox3[((uint)nLo >> 8) & 0x0ff]) + m_sbox4[nLo & 0x0ff]) ^ m_pbox[3];
            nLo ^= (((m_sbox1[(uint)nHi >> 24] + m_sbox2[((uint)nHi >> 16) & 0x0ff]) ^ m_sbox3[((uint)nHi >> 8) & 0x0ff]) + m_sbox4[nHi & 0x0ff]) ^ m_pbox[2];
            nHi ^= (((m_sbox1[(uint)nLo >> 24] + m_sbox2[((uint)nLo >> 16) & 0x0ff]) ^ m_sbox3[((uint)nLo >> 8) & 0x0ff]) + m_sbox4[nLo & 0x0ff]) ^ m_pbox[1];

            return makeLong(nHi, nLo ^ m_pbox[0]);
        }

        /**
         * Encrypts a byte buffer (should be aligned to an 8 byte border) to another
         * buffer (of the same size or bigger)
         *
         * @param inbuffer buffer with plaintext data
         * @param outbuffer buffer to get the ciphertext data
         */
        public void encrypt(byte[] inbuffer, byte[] outbuffer) {
            int nLen = inbuffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=8)
            {
                // encrypt a temporary 64bit block
                lTemp = byteArrayToLong(inbuffer, nI);
                lTemp = encryptBlock(lTemp);
                longToByteArray(lTemp, outbuffer, nI);
            }
        }

        /**
         * encrypts a byte buffer (should be aligned to an 8 byte border) to itself
         * @param buffer buffer to encrypt
         */
        public void encrypt(byte[] buffer)
        {
            int nLen = buffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=8)
            {
                // encrypt a temporary 64bit block
                lTemp = byteArrayToLong(buffer, nI);
                lTemp = encryptBlock(lTemp);
                longToByteArray(lTemp, buffer, nI);
            }
        }

        /**
         * encrypts an integer buffer (should be aligned to an
         * two integer border) to another int buffer (of the
         * same size or bigger)
         * @param inbuffer buffer with plaintext data
         * @param outbuffer buffer to get the ciphertext data
         */
        public void encrypt(int[] inbuffer, int[] outbuffer) {
            int nLen = inbuffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=2)
            {
                // encrypt a temporary 64bit block
                lTemp = intArrayToLong(inbuffer, nI);
                lTemp = encryptBlock(lTemp);
                longToIntArray(lTemp, outbuffer, nI);
            }
        }

        /**
         * encrypts an int buffer (should be aligned to a
         * two integer border)
         * @param buffer buffer to encrypt
         */
        public void encrypt(int[] buffer) {
            int nLen = buffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=2)
            {
                // encrypt a temporary 64bit block
                lTemp = intArrayToLong(buffer, nI);
                lTemp = encryptBlock(lTemp);
                longToIntArray(lTemp, buffer, nI);
            }
        }

        /**
         * encrypts a long buffer to another long buffer (of the same size or bigger)
         * @param inbuffer buffer with plaintext data
         * @param outbuffer buffer to get the ciphertext data
         */
        public void encrypt(long[] inbuffer, long[] outbuffer) {
            int nLen = inbuffer.Length;
            for (int nI = 0; nI < nLen; nI++)
            {
                outbuffer[nI] = encryptBlock(inbuffer[nI]);
            }
        }

        /**
         * encrypts a long buffer to itself
         * @param buffer buffer to encrypt
         */
        public void encrypt(long[] buffer) {
            int nLen = buffer.Length;
            for (int nI = 0; nI < nLen; nI++)
            {
                buffer[nI] = encryptBlock(buffer[nI]);
            }
        }

        /**
         * decrypts a byte buffer (should be aligned to an 8 byte border)
         * to another byte buffer (of the same size or bigger)
         * @param inbuffer buffer with ciphertext data
         * @param outbuffer buffer to get the plaintext data
         */
        public void decrypt(byte[] inbuffer,
                            byte[] outbuffer)
        {
            int nLen = inbuffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=8)
            {
                // decrypt a temporary 64bit block
                lTemp = byteArrayToLong(inbuffer, nI);
                lTemp = decryptBlock(lTemp);
                longToByteArray(lTemp, outbuffer, nI);
            }
        }

        /**
         * decrypts a byte buffer (should be aligned to an 8 byte border) to itself
         * @param buffer buffer to decrypt
         */
        public void decrypt(byte[] buffer)
        {
            int nLen = buffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=8)
            {
                // decrypt over a temporary 64bit block
                lTemp = byteArrayToLong(buffer, nI);
                lTemp = decryptBlock(lTemp);
                longToByteArray(lTemp, buffer, nI);
            }
        }

        /**
         * decrypts an integer buffer (should be aligned to an
         * two integer border) to another int buffer (of the same size or bigger)
         * @param inbuffer buffer with ciphertext data
         * @param outbuffer buffer to get the plaintext data
         */
        public void decrypt(int[] inbuffer,
                            int[] outbuffer)
        {
            int nLen = inbuffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=2)
            {
                // decrypt a temporary 64bit block
                lTemp = intArrayToLong(inbuffer, nI);
                lTemp = decryptBlock(lTemp);
                longToIntArray(lTemp, outbuffer, nI);
            }
        }

        /**
         * decrypts an int buffer (should be aligned to an
         * two integer border)
         * @param buffer buffer to decrypt
         */
        public void decrypt(int[] buffer)
        {
            int nLen = buffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=2)
            {
                // decrypt a temporary 64bit block
                lTemp = intArrayToLong(buffer, nI);
                lTemp = decryptBlock(lTemp);
                longToIntArray(lTemp, buffer, nI);
            }
        }

        /**
         * decrypts a long buffer to another long buffer (of the same size or bigger)
         * @param inbuffer buffer with ciphertext data
         * @param outbuffer buffer to get the plaintext data
         */
        public void decrypt(long[] inbuffer,
                            long[] outbuffer)
        {
            int nLen = inbuffer.Length;
            for (int nI = 0; nI < nLen; nI++)
            {
                outbuffer[nI] = decryptBlock(inbuffer[nI]);
            }
        }

        /**
         * decrypts a long buffer to itself
         * @param buffer buffer to decrypt
         */
        public void decrypt(long[] buffer) {

            int nLen = buffer.Length;
            for (int nI = 0; nI < nLen; nI++)
            {
                buffer[nI] = decryptBlock(buffer[nI]);
            }
        }

        // the boxes init. data,
        // FIXME: it might be better to create them at runtime to make the class
        //        file smaller, e.g. by calculating the hexdigits of pi (default)
        //        or just a fixed random sequence (out of the standard)

        readonly static int[] pbox_init = {

            Convert.ToInt32("0x243f6a88", 16), Convert.ToInt32("0x85a308d3", 16), Convert.ToInt32("0x13198a2e", 16), Convert.ToInt32("0x03707344", 16), Convert.ToInt32("0xa4093822", 16), Convert.ToInt32("0x299f31d0", 16),
            Convert.ToInt32("0x082efa98", 16), Convert.ToInt32("0xec4e6c89", 16), Convert.ToInt32("0x452821e6", 16), Convert.ToInt32("0x38d01377", 16), Convert.ToInt32("0xbe5466cf", 16), Convert.ToInt32("0x34e90c6c", 16),
            Convert.ToInt32("0xc0ac29b7", 16), Convert.ToInt32("0xc97c50dd", 16), Convert.ToInt32("0x3f84d5b5", 16), Convert.ToInt32("0xb5470917", 16), Convert.ToInt32("0x9216d5d9", 16), Convert.ToInt32("0x8979fb1b", 16)  };

        readonly static int[] sbox_init_1 = {

            Convert.ToInt32("0xd1310ba6", 16),   Convert.ToInt32("0x98dfb5ac", 16),   Convert.ToInt32("0x2ffd72db", 16),   Convert.ToInt32("0xd01adfb7", 16),   Convert.ToInt32("0xb8e1afed", 16),   Convert.ToInt32("0x6a267e96", 16),
            Convert.ToInt32("0xba7c9045", 16),   Convert.ToInt32("0xf12c7f99", 16),   Convert.ToInt32("0x24a19947", 16),   Convert.ToInt32("0xb3916cf7", 16),   Convert.ToInt32("0x0801f2e2", 16),   Convert.ToInt32("0x858efc16", 16),
            Convert.ToInt32("0x636920d8", 16),   Convert.ToInt32("0x71574e69", 16),   Convert.ToInt32("0xa458fea3", 16),   Convert.ToInt32("0xf4933d7e", 16),   Convert.ToInt32("0x0d95748f", 16),   Convert.ToInt32("0x728eb658", 16),
            Convert.ToInt32("0x718bcd58", 16),   Convert.ToInt32("0x82154aee", 16),   Convert.ToInt32("0x7b54a41d", 16),   Convert.ToInt32("0xc25a59b5", 16),   Convert.ToInt32("0x9c30d539", 16),   Convert.ToInt32("0x2af26013", 16),
            Convert.ToInt32("0xc5d1b023", 16),   Convert.ToInt32("0x286085f0", 16),   Convert.ToInt32("0xca417918", 16),   Convert.ToInt32("0xb8db38ef", 16),   Convert.ToInt32("0x8e79dcb0", 16),   Convert.ToInt32("0x603a180e", 16),
            Convert.ToInt32("0x6c9e0e8b", 16),   Convert.ToInt32("0xb01e8a3e", 16),   Convert.ToInt32("0xd71577c1", 16),   Convert.ToInt32("0xbd314b27", 16),   Convert.ToInt32("0x78af2fda", 16),   Convert.ToInt32("0x55605c60", 16),
            Convert.ToInt32("0xe65525f3", 16),   Convert.ToInt32("0xaa55ab94", 16),   Convert.ToInt32("0x57489862", 16),   Convert.ToInt32("0x63e81440", 16),   Convert.ToInt32("0x55ca396a", 16),   Convert.ToInt32("0x2aab10b6", 16),
            Convert.ToInt32("0xb4cc5c34", 16),   Convert.ToInt32("0x1141e8ce", 16),   Convert.ToInt32("0xa15486af", 16),   Convert.ToInt32("0x7c72e993", 16),   Convert.ToInt32("0xb3ee1411", 16),   Convert.ToInt32("0x636fbc2a", 16),
            Convert.ToInt32("0x2ba9c55d", 16),   Convert.ToInt32("0x741831f6", 16),   Convert.ToInt32("0xce5c3e16", 16),   Convert.ToInt32("0x9b87931e", 16),   Convert.ToInt32("0xafd6ba33", 16),   Convert.ToInt32("0x6c24cf5c", 16),
            Convert.ToInt32("0x7a325381", 16),   Convert.ToInt32("0x28958677", 16),   Convert.ToInt32("0x3b8f4898", 16),   Convert.ToInt32("0x6b4bb9af", 16),   Convert.ToInt32("0xc4bfe81b", 16),   Convert.ToInt32("0x66282193", 16),
            Convert.ToInt32("0x61d809cc", 16),   Convert.ToInt32("0xfb21a991", 16),   Convert.ToInt32("0x487cac60", 16),   Convert.ToInt32("0x5dec8032", 16),   Convert.ToInt32("0xef845d5d", 16),   Convert.ToInt32("0xe98575b1", 16),
            Convert.ToInt32("0xdc262302", 16),   Convert.ToInt32("0xeb651b88", 16),   Convert.ToInt32("0x23893e81", 16),   Convert.ToInt32("0xd396acc5", 16),   Convert.ToInt32("0x0f6d6ff3", 16),   Convert.ToInt32("0x83f44239", 16),
            Convert.ToInt32("0x2e0b4482", 16),   Convert.ToInt32("0xa4842004", 16),   Convert.ToInt32("0x69c8f04a", 16),   Convert.ToInt32("0x9e1f9b5e", 16),   Convert.ToInt32("0x21c66842", 16),   Convert.ToInt32("0xf6e96c9a", 16),
            Convert.ToInt32("0x670c9c61", 16),   Convert.ToInt32("0xabd388f0", 16),   Convert.ToInt32("0x6a51a0d2", 16),   Convert.ToInt32("0xd8542f68", 16),   Convert.ToInt32("0x960fa728", 16),   Convert.ToInt32("0xab5133a3", 16),
            Convert.ToInt32("0x6eef0b6c", 16),   Convert.ToInt32("0x137a3be4", 16),   Convert.ToInt32("0xba3bf050", 16),   Convert.ToInt32("0x7efb2a98", 16),   Convert.ToInt32("0xa1f1651d", 16),   Convert.ToInt32("0x39af0176", 16),
            Convert.ToInt32("0x66ca593e", 16),   Convert.ToInt32("0x82430e88", 16),   Convert.ToInt32("0x8cee8619", 16),   Convert.ToInt32("0x456f9fb4", 16),   Convert.ToInt32("0x7d84a5c3", 16),   Convert.ToInt32("0x3b8b5ebe", 16),
            Convert.ToInt32("0xe06f75d8", 16),   Convert.ToInt32("0x85c12073", 16),   Convert.ToInt32("0x401a449f", 16),   Convert.ToInt32("0x56c16aa6", 16),   Convert.ToInt32("0x4ed3aa62", 16),   Convert.ToInt32("0x363f7706", 16),
            Convert.ToInt32("0x1bfedf72", 16),   Convert.ToInt32("0x429b023d", 16),   Convert.ToInt32("0x37d0d724", 16),   Convert.ToInt32("0xd00a1248", 16),   Convert.ToInt32("0xdb0fead3", 16),   Convert.ToInt32("0x49f1c09b", 16),
            Convert.ToInt32("0x075372c9", 16),   Convert.ToInt32("0x80991b7b", 16),   Convert.ToInt32("0x25d479d8", 16),   Convert.ToInt32("0xf6e8def7", 16),   Convert.ToInt32("0xe3fe501a", 16),   Convert.ToInt32("0xb6794c3b", 16),
            Convert.ToInt32("0x976ce0bd", 16),   Convert.ToInt32("0x04c006ba", 16),   Convert.ToInt32("0xc1a94fb6", 16),   Convert.ToInt32("0x409f60c4", 16),   Convert.ToInt32("0x5e5c9ec2", 16),   Convert.ToInt32("0x196a2463", 16),
            Convert.ToInt32("0x68fb6faf", 16),   Convert.ToInt32("0x3e6c53b5", 16),   Convert.ToInt32("0x1339b2eb", 16),   Convert.ToInt32("0x3b52ec6f", 16),   Convert.ToInt32("0x6dfc511f", 16),   Convert.ToInt32("0x9b30952c", 16),
            Convert.ToInt32("0xcc814544", 16),   Convert.ToInt32("0xaf5ebd09", 16),   Convert.ToInt32("0xbee3d004", 16),   Convert.ToInt32("0xde334afd", 16),   Convert.ToInt32("0x660f2807", 16),   Convert.ToInt32("0x192e4bb3", 16),
            Convert.ToInt32("0xc0cba857", 16),   Convert.ToInt32("0x45c8740f", 16),   Convert.ToInt32("0xd20b5f39", 16),   Convert.ToInt32("0xb9d3fbdb", 16),   Convert.ToInt32("0x5579c0bd", 16),   Convert.ToInt32("0x1a60320a", 16),
            Convert.ToInt32("0xd6a100c6", 16),   Convert.ToInt32("0x402c7279", 16),   Convert.ToInt32("0x679f25fe", 16),   Convert.ToInt32("0xfb1fa3cc", 16),   Convert.ToInt32("0x8ea5e9f8", 16),   Convert.ToInt32("0xdb3222f8", 16),
            Convert.ToInt32("0x3c7516df", 16),   Convert.ToInt32("0xfd616b15", 16),   Convert.ToInt32("0x2f501ec8", 16),   Convert.ToInt32("0xad0552ab", 16),   Convert.ToInt32("0x323db5fa", 16),   Convert.ToInt32("0xfd238760", 16),
            Convert.ToInt32("0x53317b48", 16),   Convert.ToInt32("0x3e00df82", 16),   Convert.ToInt32("0x9e5c57bb", 16),   Convert.ToInt32("0xca6f8ca0", 16),   Convert.ToInt32("0x1a87562e", 16),   Convert.ToInt32("0xdf1769db", 16),
            Convert.ToInt32("0xd542a8f6", 16),   Convert.ToInt32("0x287effc3", 16),   Convert.ToInt32("0xac6732c6", 16),   Convert.ToInt32("0x8c4f5573", 16),   Convert.ToInt32("0x695b27b0", 16),   Convert.ToInt32("0xbbca58c8", 16),
            Convert.ToInt32("0xe1ffa35d", 16),   Convert.ToInt32("0xb8f011a0", 16),   Convert.ToInt32("0x10fa3d98", 16),   Convert.ToInt32("0xfd2183b8", 16),   Convert.ToInt32("0x4afcb56c", 16),   Convert.ToInt32("0x2dd1d35b", 16),
            Convert.ToInt32("0x9a53e479", 16),   Convert.ToInt32("0xb6f84565", 16),   Convert.ToInt32("0xd28e49bc", 16),   Convert.ToInt32("0x4bfb9790", 16),   Convert.ToInt32("0xe1ddf2da", 16),   Convert.ToInt32("0xa4cb7e33", 16),
            Convert.ToInt32("0x62fb1341", 16),   Convert.ToInt32("0xcee4c6e8", 16),   Convert.ToInt32("0xef20cada", 16),   Convert.ToInt32("0x36774c01", 16),   Convert.ToInt32("0xd07e9efe", 16),   Convert.ToInt32("0x2bf11fb4", 16),
            Convert.ToInt32("0x95dbda4d", 16),   Convert.ToInt32("0xae909198", 16),   Convert.ToInt32("0xeaad8e71", 16),   Convert.ToInt32("0x6b93d5a0", 16),   Convert.ToInt32("0xd08ed1d0", 16),   Convert.ToInt32("0xafc725e0", 16),
            Convert.ToInt32("0x8e3c5b2f", 16),   Convert.ToInt32("0x8e7594b7", 16),   Convert.ToInt32("0x8ff6e2fb", 16),   Convert.ToInt32("0xf2122b64", 16),   Convert.ToInt32("0x8888b812", 16),   Convert.ToInt32("0x900df01c", 16),
            Convert.ToInt32("0x4fad5ea0", 16),   Convert.ToInt32("0x688fc31c", 16),   Convert.ToInt32("0xd1cff191", 16),   Convert.ToInt32("0xb3a8c1ad", 16),   Convert.ToInt32("0x2f2f2218", 16),   Convert.ToInt32("0xbe0e1777", 16),
            Convert.ToInt32("0xea752dfe", 16),   Convert.ToInt32("0x8b021fa1", 16),   Convert.ToInt32("0xe5a0cc0f", 16),   Convert.ToInt32("0xb56f74e8", 16),   Convert.ToInt32("0x18acf3d6", 16),   Convert.ToInt32("0xce89e299", 16),
            Convert.ToInt32("0xb4a84fe0", 16),   Convert.ToInt32("0xfd13e0b7", 16),   Convert.ToInt32("0x7cc43b81", 16),   Convert.ToInt32("0xd2ada8d9", 16),   Convert.ToInt32("0x165fa266", 16),   Convert.ToInt32("0x80957705", 16),
            Convert.ToInt32("0x93cc7314", 16),   Convert.ToInt32("0x211a1477", 16),   Convert.ToInt32("0xe6ad2065", 16),   Convert.ToInt32("0x77b5fa86", 16),   Convert.ToInt32("0xc75442f5", 16),   Convert.ToInt32("0xfb9d35cf", 16),
            Convert.ToInt32("0xebcdaf0c", 16),   Convert.ToInt32("0x7b3e89a0", 16),   Convert.ToInt32("0xd6411bd3", 16),   Convert.ToInt32("0xae1e7e49", 16),   Convert.ToInt32("0x00250e2d", 16),   Convert.ToInt32("0x2071b35e", 16),
            Convert.ToInt32("0x226800bb", 16),   Convert.ToInt32("0x57b8e0af", 16),   Convert.ToInt32("0x2464369b", 16),   Convert.ToInt32("0xf009b91e", 16),   Convert.ToInt32("0x5563911d", 16),   Convert.ToInt32("0x59dfa6aa", 16),
            Convert.ToInt32("0x78c14389", 16),   Convert.ToInt32("0xd95a537f", 16),   Convert.ToInt32("0x207d5ba2", 16),   Convert.ToInt32("0x02e5b9c5", 16),   Convert.ToInt32("0x83260376", 16),   Convert.ToInt32("0x6295cfa9", 16),
            Convert.ToInt32("0x11c81968", 16),   Convert.ToInt32("0x4e734a41", 16),   Convert.ToInt32("0xb3472dca", 16),   Convert.ToInt32("0x7b14a94a", 16),   Convert.ToInt32("0x1b510052", 16),   Convert.ToInt32("0x9a532915", 16),
            Convert.ToInt32("0xd60f573f", 16),   Convert.ToInt32("0xbc9bc6e4", 16),   Convert.ToInt32("0x2b60a476", 16),   Convert.ToInt32("0x81e67400", 16),   Convert.ToInt32("0x08ba6fb5", 16),   Convert.ToInt32("0x571be91f", 16),
            Convert.ToInt32("0xf296ec6b", 16),   Convert.ToInt32("0x2a0dd915", 16),   Convert.ToInt32("0xb6636521", 16),   Convert.ToInt32("0xe7b9f9b6", 16),   Convert.ToInt32("0xff34052e", 16),   Convert.ToInt32("0xc5855664", 16),
            Convert.ToInt32("0x53b02d5d", 16),   Convert.ToInt32("0xa99f8fa1", 16),   Convert.ToInt32("0x08ba4799", 16),   Convert.ToInt32("0x6e85076a", 16) };


        readonly static int[] sbox_init_2 = {

            Convert.ToInt32("0x4b7a70e9", 16),   Convert.ToInt32("0xb5b32944", 16),
            Convert.ToInt32("0xdb75092e", 16),   Convert.ToInt32("0xc4192623", 16),   Convert.ToInt32("0xad6ea6b0", 16),   Convert.ToInt32("0x49a7df7d", 16),   Convert.ToInt32("0x9cee60b8", 16),   Convert.ToInt32("0x8fedb266", 16),
            Convert.ToInt32("0xecaa8c71", 16),   Convert.ToInt32("0x699a17ff", 16),   Convert.ToInt32("0x5664526c", 16),   Convert.ToInt32("0xc2b19ee1", 16),   Convert.ToInt32("0x193602a5", 16),   Convert.ToInt32("0x75094c29", 16),
            Convert.ToInt32("0xa0591340", 16),   Convert.ToInt32("0xe4183a3e", 16),   Convert.ToInt32("0x3f54989a", 16),   Convert.ToInt32("0x5b429d65", 16),   Convert.ToInt32("0x6b8fe4d6", 16),   Convert.ToInt32("0x99f73fd6", 16),
            Convert.ToInt32("0xa1d29c07", 16),   Convert.ToInt32("0xefe830f5", 16),   Convert.ToInt32("0x4d2d38e6", 16),   Convert.ToInt32("0xf0255dc1", 16),   Convert.ToInt32("0x4cdd2086", 16),   Convert.ToInt32("0x8470eb26", 16),
            Convert.ToInt32("0x6382e9c6", 16),   Convert.ToInt32("0x021ecc5e", 16),   Convert.ToInt32("0x09686b3f", 16),   Convert.ToInt32("0x3ebaefc9", 16),   Convert.ToInt32("0x3c971814", 16),   Convert.ToInt32("0x6b6a70a1", 16),
            Convert.ToInt32("0x687f3584", 16),   Convert.ToInt32("0x52a0e286", 16),   Convert.ToInt32("0xb79c5305", 16),   Convert.ToInt32("0xaa500737", 16),   Convert.ToInt32("0x3e07841c", 16),   Convert.ToInt32("0x7fdeae5c", 16),
            Convert.ToInt32("0x8e7d44ec", 16),   Convert.ToInt32("0x5716f2b8", 16),   Convert.ToInt32("0xb03ada37", 16),   Convert.ToInt32("0xf0500c0d", 16),   Convert.ToInt32("0xf01c1f04", 16),   Convert.ToInt32("0x0200b3ff", 16),
            Convert.ToInt32("0xae0cf51a", 16),   Convert.ToInt32("0x3cb574b2", 16),   Convert.ToInt32("0x25837a58", 16),   Convert.ToInt32("0xdc0921bd", 16),   Convert.ToInt32("0xd19113f9", 16),   Convert.ToInt32("0x7ca92ff6", 16),
            Convert.ToInt32("0x94324773", 16),   Convert.ToInt32("0x22f54701", 16),   Convert.ToInt32("0x3ae5e581", 16),   Convert.ToInt32("0x37c2dadc", 16),   Convert.ToInt32("0xc8b57634", 16),   Convert.ToInt32("0x9af3dda7", 16),
            Convert.ToInt32("0xa9446146", 16),   Convert.ToInt32("0x0fd0030e", 16),   Convert.ToInt32("0xecc8c73e", 16),   Convert.ToInt32("0xa4751e41", 16),   Convert.ToInt32("0xe238cd99", 16),   Convert.ToInt32("0x3bea0e2f", 16),
            Convert.ToInt32("0x3280bba1", 16),   Convert.ToInt32("0x183eb331", 16),   Convert.ToInt32("0x4e548b38", 16),   Convert.ToInt32("0x4f6db908", 16),   Convert.ToInt32("0x6f420d03", 16),   Convert.ToInt32("0xf60a04bf", 16),
            Convert.ToInt32("0x2cb81290", 16),   Convert.ToInt32("0x24977c79", 16),   Convert.ToInt32("0x5679b072", 16),   Convert.ToInt32("0xbcaf89af", 16),   Convert.ToInt32("0xde9a771f", 16),   Convert.ToInt32("0xd9930810", 16),
            Convert.ToInt32("0xb38bae12", 16),   Convert.ToInt32("0xdccf3f2e", 16),   Convert.ToInt32("0x5512721f", 16),   Convert.ToInt32("0x2e6b7124", 16),   Convert.ToInt32("0x501adde6", 16),   Convert.ToInt32("0x9f84cd87", 16),
            Convert.ToInt32("0x7a584718", 16),   Convert.ToInt32("0x7408da17", 16),   Convert.ToInt32("0xbc9f9abc", 16),   Convert.ToInt32("0xe94b7d8c", 16),   Convert.ToInt32("0xec7aec3a", 16),   Convert.ToInt32("0xdb851dfa", 16),
            Convert.ToInt32("0x63094366", 16),   Convert.ToInt32("0xc464c3d2", 16),   Convert.ToInt32("0xef1c1847", 16),   Convert.ToInt32("0x3215d908", 16),   Convert.ToInt32("0xdd433b37", 16),   Convert.ToInt32("0x24c2ba16", 16),
            Convert.ToInt32("0x12a14d43", 16),   Convert.ToInt32("0x2a65c451", 16),   Convert.ToInt32("0x50940002", 16),   Convert.ToInt32("0x133ae4dd", 16),   Convert.ToInt32("0x71dff89e", 16),   Convert.ToInt32("0x10314e55", 16),
            Convert.ToInt32("0x81ac77d6", 16),   Convert.ToInt32("0x5f11199b", 16),   Convert.ToInt32("0x043556f1", 16),   Convert.ToInt32("0xd7a3c76b", 16),   Convert.ToInt32("0x3c11183b", 16),   Convert.ToInt32("0x5924a509", 16),
            Convert.ToInt32("0xf28fe6ed", 16),   Convert.ToInt32("0x97f1fbfa", 16),   Convert.ToInt32("0x9ebabf2c", 16),   Convert.ToInt32("0x1e153c6e", 16),   Convert.ToInt32("0x86e34570", 16),   Convert.ToInt32("0xeae96fb1", 16),
            Convert.ToInt32("0x860e5e0a", 16),   Convert.ToInt32("0x5a3e2ab3", 16),   Convert.ToInt32("0x771fe71c", 16),   Convert.ToInt32("0x4e3d06fa", 16),   Convert.ToInt32("0x2965dcb9", 16),   Convert.ToInt32("0x99e71d0f", 16),
            Convert.ToInt32("0x803e89d6", 16),   Convert.ToInt32("0x5266c825", 16),   Convert.ToInt32("0x2e4cc978", 16),   Convert.ToInt32("0x9c10b36a", 16),   Convert.ToInt32("0xc6150eba", 16),   Convert.ToInt32("0x94e2ea78", 16),
            Convert.ToInt32("0xa5fc3c53", 16),   Convert.ToInt32("0x1e0a2df4", 16),   Convert.ToInt32("0xf2f74ea7", 16),   Convert.ToInt32("0x361d2b3d", 16),   Convert.ToInt32("0x1939260f", 16),   Convert.ToInt32("0x19c27960", 16),
            Convert.ToInt32("0x5223a708", 16),   Convert.ToInt32("0xf71312b6", 16),   Convert.ToInt32("0xebadfe6e", 16),   Convert.ToInt32("0xeac31f66", 16),   Convert.ToInt32("0xe3bc4595", 16),   Convert.ToInt32("0xa67bc883", 16),
            Convert.ToInt32("0xb17f37d1", 16),   Convert.ToInt32("0x018cff28", 16),   Convert.ToInt32("0xc332ddef", 16),   Convert.ToInt32("0xbe6c5aa5", 16),   Convert.ToInt32("0x65582185", 16),   Convert.ToInt32("0x68ab9802", 16),
            Convert.ToInt32("0xeecea50f", 16),   Convert.ToInt32("0xdb2f953b", 16),   Convert.ToInt32("0x2aef7dad", 16),   Convert.ToInt32("0x5b6e2f84", 16),   Convert.ToInt32("0x1521b628", 16),   Convert.ToInt32("0x29076170", 16),
            Convert.ToInt32("0xecdd4775", 16),   Convert.ToInt32("0x619f1510", 16),   Convert.ToInt32("0x13cca830", 16),   Convert.ToInt32("0xeb61bd96", 16),   Convert.ToInt32("0x0334fe1e", 16),   Convert.ToInt32("0xaa0363cf", 16),
            Convert.ToInt32("0xb5735c90", 16),   Convert.ToInt32("0x4c70a239", 16),   Convert.ToInt32("0xd59e9e0b", 16),   Convert.ToInt32("0xcbaade14", 16),   Convert.ToInt32("0xeecc86bc", 16),   Convert.ToInt32("0x60622ca7", 16),
            Convert.ToInt32("0x9cab5cab", 16),   Convert.ToInt32("0xb2f3846e", 16),   Convert.ToInt32("0x648b1eaf", 16),   Convert.ToInt32("0x19bdf0ca", 16),   Convert.ToInt32("0xa02369b9", 16),   Convert.ToInt32("0x655abb50", 16),
            Convert.ToInt32("0x40685a32", 16),   Convert.ToInt32("0x3c2ab4b3", 16),   Convert.ToInt32("0x319ee9d5", 16),   Convert.ToInt32("0xc021b8f7", 16),   Convert.ToInt32("0x9b540b19", 16),   Convert.ToInt32("0x875fa099", 16),
            Convert.ToInt32("0x95f7997e", 16),   Convert.ToInt32("0x623d7da8", 16),   Convert.ToInt32("0xf837889a", 16),   Convert.ToInt32("0x97e32d77", 16),   Convert.ToInt32("0x11ed935f", 16),   Convert.ToInt32("0x16681281", 16),
            Convert.ToInt32("0x0e358829", 16),   Convert.ToInt32("0xc7e61fd6", 16),   Convert.ToInt32("0x96dedfa1", 16),   Convert.ToInt32("0x7858ba99", 16),   Convert.ToInt32("0x57f584a5", 16),   Convert.ToInt32("0x1b227263", 16),
            Convert.ToInt32("0x9b83c3ff", 16),   Convert.ToInt32("0x1ac24696", 16),   Convert.ToInt32("0xcdb30aeb", 16),   Convert.ToInt32("0x532e3054", 16),   Convert.ToInt32("0x8fd948e4", 16),   Convert.ToInt32("0x6dbc3128", 16),
            Convert.ToInt32("0x58ebf2ef", 16),   Convert.ToInt32("0x34c6ffea", 16),   Convert.ToInt32("0xfe28ed61", 16),   Convert.ToInt32("0xee7c3c73", 16),   Convert.ToInt32("0x5d4a14d9", 16),   Convert.ToInt32("0xe864b7e3", 16),
            Convert.ToInt32("0x42105d14", 16),   Convert.ToInt32("0x203e13e0", 16),   Convert.ToInt32("0x45eee2b6", 16),   Convert.ToInt32("0xa3aaabea", 16),   Convert.ToInt32("0xdb6c4f15", 16),   Convert.ToInt32("0xfacb4fd0", 16),
            Convert.ToInt32("0xc742f442", 16),   Convert.ToInt32("0xef6abbb5", 16),   Convert.ToInt32("0x654f3b1d", 16),   Convert.ToInt32("0x41cd2105", 16),   Convert.ToInt32("0xd81e799e", 16),   Convert.ToInt32("0x86854dc7", 16),
            Convert.ToInt32("0xe44b476a", 16),   Convert.ToInt32("0x3d816250", 16),   Convert.ToInt32("0xcf62a1f2", 16),   Convert.ToInt32("0x5b8d2646", 16),   Convert.ToInt32("0xfc8883a0", 16),   Convert.ToInt32("0xc1c7b6a3", 16),
            Convert.ToInt32("0x7f1524c3", 16),   Convert.ToInt32("0x69cb7492", 16),   Convert.ToInt32("0x47848a0b", 16),   Convert.ToInt32("0x5692b285", 16),   Convert.ToInt32("0x095bbf00", 16),   Convert.ToInt32("0xad19489d", 16),
            Convert.ToInt32("0x1462b174", 16),   Convert.ToInt32("0x23820e00", 16),   Convert.ToInt32("0x58428d2a", 16),   Convert.ToInt32("0x0c55f5ea", 16),   Convert.ToInt32("0x1dadf43e", 16),   Convert.ToInt32("0x233f7061", 16),
            Convert.ToInt32("0x3372f092", 16),   Convert.ToInt32("0x8d937e41", 16),   Convert.ToInt32("0xd65fecf1", 16),   Convert.ToInt32("0x6c223bdb", 16),   Convert.ToInt32("0x7cde3759", 16),   Convert.ToInt32("0xcbee7460", 16),
            Convert.ToInt32("0x4085f2a7", 16),   Convert.ToInt32("0xce77326e", 16),   Convert.ToInt32("0xa6078084", 16),   Convert.ToInt32("0x19f8509e", 16),   Convert.ToInt32("0xe8efd855", 16),   Convert.ToInt32("0x61d99735", 16),
            Convert.ToInt32("0xa969a7aa", 16),   Convert.ToInt32("0xc50c06c2", 16),   Convert.ToInt32("0x5a04abfc", 16),   Convert.ToInt32("0x800bcadc", 16),   Convert.ToInt32("0x9e447a2e", 16),   Convert.ToInt32("0xc3453484", 16),
            Convert.ToInt32("0xfdd56705", 16),   Convert.ToInt32("0x0e1e9ec9", 16),   Convert.ToInt32("0xdb73dbd3", 16),   Convert.ToInt32("0x105588cd", 16),   Convert.ToInt32("0x675fda79", 16),   Convert.ToInt32("0xe3674340", 16),
            Convert.ToInt32("0xc5c43465", 16),   Convert.ToInt32("0x713e38d8", 16),   Convert.ToInt32("0x3d28f89e", 16),   Convert.ToInt32("0xf16dff20", 16),   Convert.ToInt32("0x153e21e7", 16),   Convert.ToInt32("0x8fb03d4a", 16),
            Convert.ToInt32("0xe6e39f2b", 16),   Convert.ToInt32("0xdb83adf7", 16) };

        readonly static int[] sbox_init_3 = {

            Convert.ToInt32("0xe93d5a68", 16),   Convert.ToInt32("0x948140f7", 16),   Convert.ToInt32("0xf64c261c", 16),   Convert.ToInt32("0x94692934", 16),
            Convert.ToInt32("0x411520f7", 16),   Convert.ToInt32("0x7602d4f7", 16),   Convert.ToInt32("0xbcf46b2e", 16),   Convert.ToInt32("0xd4a20068", 16),   Convert.ToInt32("0xd4082471", 16),   Convert.ToInt32("0x3320f46a", 16),
            Convert.ToInt32("0x43b7d4b7", 16),   Convert.ToInt32("0x500061af", 16),   Convert.ToInt32("0x1e39f62e", 16),   Convert.ToInt32("0x97244546", 16),   Convert.ToInt32("0x14214f74", 16),   Convert.ToInt32("0xbf8b8840", 16),
            Convert.ToInt32("0x4d95fc1d", 16),   Convert.ToInt32("0x96b591af", 16),   Convert.ToInt32("0x70f4ddd3", 16),   Convert.ToInt32("0x66a02f45", 16),   Convert.ToInt32("0xbfbc09ec", 16),   Convert.ToInt32("0x03bd9785", 16),
            Convert.ToInt32("0x7fac6dd0", 16),   Convert.ToInt32("0x31cb8504", 16),   Convert.ToInt32("0x96eb27b3", 16),   Convert.ToInt32("0x55fd3941", 16),   Convert.ToInt32("0xda2547e6", 16),   Convert.ToInt32("0xabca0a9a", 16),
            Convert.ToInt32("0x28507825", 16),   Convert.ToInt32("0x530429f4", 16),   Convert.ToInt32("0x0a2c86da", 16),   Convert.ToInt32("0xe9b66dfb", 16),   Convert.ToInt32("0x68dc1462", 16),   Convert.ToInt32("0xd7486900", 16),
            Convert.ToInt32("0x680ec0a4", 16),   Convert.ToInt32("0x27a18dee", 16),   Convert.ToInt32("0x4f3ffea2", 16),   Convert.ToInt32("0xe887ad8c", 16),   Convert.ToInt32("0xb58ce006", 16),   Convert.ToInt32("0x7af4d6b6", 16),
            Convert.ToInt32("0xaace1e7c", 16),   Convert.ToInt32("0xd3375fec", 16),   Convert.ToInt32("0xce78a399", 16),   Convert.ToInt32("0x406b2a42", 16),   Convert.ToInt32("0x20fe9e35", 16),   Convert.ToInt32("0xd9f385b9", 16),
            Convert.ToInt32("0xee39d7ab", 16),   Convert.ToInt32("0x3b124e8b", 16),   Convert.ToInt32("0x1dc9faf7", 16),   Convert.ToInt32("0x4b6d1856", 16),   Convert.ToInt32("0x26a36631", 16),   Convert.ToInt32("0xeae397b2", 16),
            Convert.ToInt32("0x3a6efa74", 16),   Convert.ToInt32("0xdd5b4332", 16),   Convert.ToInt32("0x6841e7f7", 16),   Convert.ToInt32("0xca7820fb", 16),   Convert.ToInt32("0xfb0af54e", 16),   Convert.ToInt32("0xd8feb397", 16),
            Convert.ToInt32("0x454056ac", 16),   Convert.ToInt32("0xba489527", 16),   Convert.ToInt32("0x55533a3a", 16),   Convert.ToInt32("0x20838d87", 16),   Convert.ToInt32("0xfe6ba9b7", 16),   Convert.ToInt32("0xd096954b", 16),
            Convert.ToInt32("0x55a867bc", 16),   Convert.ToInt32("0xa1159a58", 16),   Convert.ToInt32("0xcca92963", 16),   Convert.ToInt32("0x99e1db33", 16),   Convert.ToInt32("0xa62a4a56", 16),   Convert.ToInt32("0x3f3125f9", 16),
            Convert.ToInt32("0x5ef47e1c", 16),   Convert.ToInt32("0x9029317c", 16),   Convert.ToInt32("0xfdf8e802", 16),   Convert.ToInt32("0x04272f70", 16),   Convert.ToInt32("0x80bb155c", 16),   Convert.ToInt32("0x05282ce3", 16),
            Convert.ToInt32("0x95c11548", 16),   Convert.ToInt32("0xe4c66d22", 16),   Convert.ToInt32("0x48c1133f", 16),   Convert.ToInt32("0xc70f86dc", 16),   Convert.ToInt32("0x07f9c9ee", 16),   Convert.ToInt32("0x41041f0f", 16),
            Convert.ToInt32("0x404779a4", 16),   Convert.ToInt32("0x5d886e17", 16),   Convert.ToInt32("0x325f51eb", 16),   Convert.ToInt32("0xd59bc0d1", 16),   Convert.ToInt32("0xf2bcc18f", 16),   Convert.ToInt32("0x41113564", 16),
            Convert.ToInt32("0x257b7834", 16),   Convert.ToInt32("0x602a9c60", 16),   Convert.ToInt32("0xdff8e8a3", 16),   Convert.ToInt32("0x1f636c1b", 16),   Convert.ToInt32("0x0e12b4c2", 16),   Convert.ToInt32("0x02e1329e", 16),
            Convert.ToInt32("0xaf664fd1", 16),   Convert.ToInt32("0xcad18115", 16),   Convert.ToInt32("0x6b2395e0", 16),   Convert.ToInt32("0x333e92e1", 16),   Convert.ToInt32("0x3b240b62", 16),   Convert.ToInt32("0xeebeb922", 16),
            Convert.ToInt32("0x85b2a20e", 16),   Convert.ToInt32("0xe6ba0d99", 16),   Convert.ToInt32("0xde720c8c", 16),   Convert.ToInt32("0x2da2f728", 16),   Convert.ToInt32("0xd0127845", 16),   Convert.ToInt32("0x95b794fd", 16),
            Convert.ToInt32("0x647d0862", 16),   Convert.ToInt32("0xe7ccf5f0", 16),   Convert.ToInt32("0x5449a36f", 16),   Convert.ToInt32("0x877d48fa", 16),   Convert.ToInt32("0xc39dfd27", 16),   Convert.ToInt32("0xf33e8d1e", 16),
            Convert.ToInt32("0x0a476341", 16),   Convert.ToInt32("0x992eff74", 16),   Convert.ToInt32("0x3a6f6eab", 16),   Convert.ToInt32("0xf4f8fd37", 16),   Convert.ToInt32("0xa812dc60", 16),   Convert.ToInt32("0xa1ebddf8", 16),
            Convert.ToInt32("0x991be14c", 16),   Convert.ToInt32("0xdb6e6b0d", 16),   Convert.ToInt32("0xc67b5510", 16),   Convert.ToInt32("0x6d672c37", 16),   Convert.ToInt32("0x2765d43b", 16),   Convert.ToInt32("0xdcd0e804", 16),
            Convert.ToInt32("0xf1290dc7", 16),   Convert.ToInt32("0xcc00ffa3", 16),   Convert.ToInt32("0xb5390f92", 16),   Convert.ToInt32("0x690fed0b", 16),   Convert.ToInt32("0x667b9ffb", 16),   Convert.ToInt32("0xcedb7d9c", 16),
            Convert.ToInt32("0xa091cf0b", 16),   Convert.ToInt32("0xd9155ea3", 16),   Convert.ToInt32("0xbb132f88", 16),   Convert.ToInt32("0x515bad24", 16),   Convert.ToInt32("0x7b9479bf", 16),   Convert.ToInt32("0x763bd6eb", 16),
            Convert.ToInt32("0x37392eb3", 16),   Convert.ToInt32("0xcc115979", 16),   Convert.ToInt32("0x8026e297", 16),   Convert.ToInt32("0xf42e312d", 16),   Convert.ToInt32("0x6842ada7", 16),   Convert.ToInt32("0xc66a2b3b", 16),
            Convert.ToInt32("0x12754ccc", 16),   Convert.ToInt32("0x782ef11c", 16),   Convert.ToInt32("0x6a124237", 16),   Convert.ToInt32("0xb79251e7", 16),   Convert.ToInt32("0x06a1bbe6", 16),   Convert.ToInt32("0x4bfb6350", 16),
            Convert.ToInt32("0x1a6b1018", 16),   Convert.ToInt32("0x11caedfa", 16),   Convert.ToInt32("0x3d25bdd8", 16),   Convert.ToInt32("0xe2e1c3c9", 16),   Convert.ToInt32("0x44421659", 16),   Convert.ToInt32("0x0a121386", 16),
            Convert.ToInt32("0xd90cec6e", 16),   Convert.ToInt32("0xd5abea2a", 16),   Convert.ToInt32("0x64af674e", 16),   Convert.ToInt32("0xda86a85f", 16),   Convert.ToInt32("0xbebfe988", 16),   Convert.ToInt32("0x64e4c3fe", 16),
            Convert.ToInt32("0x9dbc8057", 16),   Convert.ToInt32("0xf0f7c086", 16),   Convert.ToInt32("0x60787bf8", 16),   Convert.ToInt32("0x6003604d", 16),   Convert.ToInt32("0xd1fd8346", 16),   Convert.ToInt32("0xf6381fb0", 16),
            Convert.ToInt32("0x7745ae04", 16),   Convert.ToInt32("0xd736fccc", 16),   Convert.ToInt32("0x83426b33", 16),   Convert.ToInt32("0xf01eab71", 16),   Convert.ToInt32("0xb0804187", 16),   Convert.ToInt32("0x3c005e5f", 16),
            Convert.ToInt32("0x77a057be", 16),   Convert.ToInt32("0xbde8ae24", 16),   Convert.ToInt32("0x55464299", 16),   Convert.ToInt32("0xbf582e61", 16),   Convert.ToInt32("0x4e58f48f", 16),   Convert.ToInt32("0xf2ddfda2", 16),
            Convert.ToInt32("0xf474ef38", 16),   Convert.ToInt32("0x8789bdc2", 16),   Convert.ToInt32("0x5366f9c3", 16),   Convert.ToInt32("0xc8b38e74", 16),   Convert.ToInt32("0xb475f255", 16),   Convert.ToInt32("0x46fcd9b9", 16),
            Convert.ToInt32("0x7aeb2661", 16),   Convert.ToInt32("0x8b1ddf84", 16),   Convert.ToInt32("0x846a0e79", 16),   Convert.ToInt32("0x915f95e2", 16),   Convert.ToInt32("0x466e598e", 16),   Convert.ToInt32("0x20b45770", 16),
            Convert.ToInt32("0x8cd55591", 16),   Convert.ToInt32("0xc902de4c", 16),   Convert.ToInt32("0xb90bace1", 16),   Convert.ToInt32("0xbb8205d0", 16),   Convert.ToInt32("0x11a86248", 16),   Convert.ToInt32("0x7574a99e", 16),
            Convert.ToInt32("0xb77f19b6", 16),   Convert.ToInt32("0xe0a9dc09", 16),   Convert.ToInt32("0x662d09a1", 16),   Convert.ToInt32("0xc4324633", 16),   Convert.ToInt32("0xe85a1f02", 16),   Convert.ToInt32("0x09f0be8c", 16),
            Convert.ToInt32("0x4a99a025", 16),   Convert.ToInt32("0x1d6efe10", 16),   Convert.ToInt32("0x1ab93d1d", 16),   Convert.ToInt32("0x0ba5a4df", 16),   Convert.ToInt32("0xa186f20f", 16),   Convert.ToInt32("0x2868f169", 16),
            Convert.ToInt32("0xdcb7da83", 16),   Convert.ToInt32("0x573906fe", 16),   Convert.ToInt32("0xa1e2ce9b", 16),   Convert.ToInt32("0x4fcd7f52", 16),   Convert.ToInt32("0x50115e01", 16),   Convert.ToInt32("0xa70683fa", 16),
            Convert.ToInt32("0xa002b5c4", 16),   Convert.ToInt32("0x0de6d027", 16),   Convert.ToInt32("0x9af88c27", 16),   Convert.ToInt32("0x773f8641", 16),   Convert.ToInt32("0xc3604c06", 16),   Convert.ToInt32("0x61a806b5", 16),
            Convert.ToInt32("0xf0177a28", 16),   Convert.ToInt32("0xc0f586e0", 16),   Convert.ToInt32("0x006058aa", 16),   Convert.ToInt32("0x30dc7d62", 16),   Convert.ToInt32("0x11e69ed7", 16),   Convert.ToInt32("0x2338ea63", 16),
            Convert.ToInt32("0x53c2dd94", 16),   Convert.ToInt32("0xc2c21634", 16),   Convert.ToInt32("0xbbcbee56", 16),   Convert.ToInt32("0x90bcb6de", 16),   Convert.ToInt32("0xebfc7da1", 16),   Convert.ToInt32("0xce591d76", 16),
            Convert.ToInt32("0x6f05e409", 16),   Convert.ToInt32("0x4b7c0188", 16),   Convert.ToInt32("0x39720a3d", 16),   Convert.ToInt32("0x7c927c24", 16),   Convert.ToInt32("0x86e3725f", 16),   Convert.ToInt32("0x724d9db9", 16),
            Convert.ToInt32("0x1ac15bb4", 16),   Convert.ToInt32("0xd39eb8fc", 16),   Convert.ToInt32("0xed545578", 16),   Convert.ToInt32("0x08fca5b5", 16),   Convert.ToInt32("0xd83d7cd3", 16),   Convert.ToInt32("0x4dad0fc4", 16),
            Convert.ToInt32("0x1e50ef5e", 16),   Convert.ToInt32("0xb161e6f8", 16),   Convert.ToInt32("0xa28514d9", 16),   Convert.ToInt32("0x6c51133c", 16),   Convert.ToInt32("0x6fd5c7e7", 16),   Convert.ToInt32("0x56e14ec4", 16),
            Convert.ToInt32("0x362abfce", 16),   Convert.ToInt32("0xddc6c837", 16),   Convert.ToInt32("0xd79a3234", 16),   Convert.ToInt32("0x92638212", 16),   Convert.ToInt32("0x670efa8e", 16),   Convert.ToInt32("0x406000e0", 16) };


        readonly static int[] sbox_init_4 = {

            Convert.ToInt32("0x3a39ce37", 16),   Convert.ToInt32("0xd3faf5cf", 16),   Convert.ToInt32("0xabc27737", 16),   Convert.ToInt32("0x5ac52d1b", 16),   Convert.ToInt32("0x5cb0679e", 16),   Convert.ToInt32("0x4fa33742", 16),
            Convert.ToInt32("0xd3822740", 16),   Convert.ToInt32("0x99bc9bbe", 16),   Convert.ToInt32("0xd5118e9d", 16),   Convert.ToInt32("0xbf0f7315", 16),   Convert.ToInt32("0xd62d1c7e", 16),   Convert.ToInt32("0xc700c47b", 16),
            Convert.ToInt32("0xb78c1b6b", 16),   Convert.ToInt32("0x21a19045", 16),   Convert.ToInt32("0xb26eb1be", 16),   Convert.ToInt32("0x6a366eb4", 16),   Convert.ToInt32("0x5748ab2f", 16),   Convert.ToInt32("0xbc946e79", 16),
            Convert.ToInt32("0xc6a376d2", 16),   Convert.ToInt32("0x6549c2c8", 16),   Convert.ToInt32("0x530ff8ee", 16),   Convert.ToInt32("0x468dde7d", 16),   Convert.ToInt32("0xd5730a1d", 16),   Convert.ToInt32("0x4cd04dc6", 16),
            Convert.ToInt32("0x2939bbdb", 16),   Convert.ToInt32("0xa9ba4650", 16),   Convert.ToInt32("0xac9526e8", 16),   Convert.ToInt32("0xbe5ee304", 16),   Convert.ToInt32("0xa1fad5f0", 16),   Convert.ToInt32("0x6a2d519a", 16),
            Convert.ToInt32("0x63ef8ce2", 16),   Convert.ToInt32("0x9a86ee22", 16),   Convert.ToInt32("0xc089c2b8", 16),   Convert.ToInt32("0x43242ef6", 16),   Convert.ToInt32("0xa51e03aa", 16),   Convert.ToInt32("0x9cf2d0a4", 16),
            Convert.ToInt32("0x83c061ba", 16),   Convert.ToInt32("0x9be96a4d", 16),   Convert.ToInt32("0x8fe51550", 16),   Convert.ToInt32("0xba645bd6", 16),   Convert.ToInt32("0x2826a2f9", 16),   Convert.ToInt32("0xa73a3ae1", 16),
            Convert.ToInt32("0x4ba99586", 16),   Convert.ToInt32("0xef5562e9", 16),   Convert.ToInt32("0xc72fefd3", 16),   Convert.ToInt32("0xf752f7da", 16),   Convert.ToInt32("0x3f046f69", 16),   Convert.ToInt32("0x77fa0a59", 16),
            Convert.ToInt32("0x80e4a915", 16),   Convert.ToInt32("0x87b08601", 16),   Convert.ToInt32("0x9b09e6ad", 16),   Convert.ToInt32("0x3b3ee593", 16),   Convert.ToInt32("0xe990fd5a", 16),   Convert.ToInt32("0x9e34d797", 16),
            Convert.ToInt32("0x2cf0b7d9", 16),   Convert.ToInt32("0x022b8b51", 16),   Convert.ToInt32("0x96d5ac3a", 16),   Convert.ToInt32("0x017da67d", 16),   Convert.ToInt32("0xd1cf3ed6", 16),   Convert.ToInt32("0x7c7d2d28", 16),
            Convert.ToInt32("0x1f9f25cf", 16),   Convert.ToInt32("0xadf2b89b", 16),   Convert.ToInt32("0x5ad6b472", 16),   Convert.ToInt32("0x5a88f54c", 16),   Convert.ToInt32("0xe029ac71", 16),   Convert.ToInt32("0xe019a5e6", 16),
            Convert.ToInt32("0x47b0acfd", 16),   Convert.ToInt32("0xed93fa9b", 16),   Convert.ToInt32("0xe8d3c48d", 16),   Convert.ToInt32("0x283b57cc", 16),   Convert.ToInt32("0xf8d56629", 16),   Convert.ToInt32("0x79132e28", 16),
            Convert.ToInt32("0x785f0191", 16),   Convert.ToInt32("0xed756055", 16),   Convert.ToInt32("0xf7960e44", 16),   Convert.ToInt32("0xe3d35e8c", 16),   Convert.ToInt32("0x15056dd4", 16),   Convert.ToInt32("0x88f46dba", 16),
            Convert.ToInt32("0x03a16125", 16),   Convert.ToInt32("0x0564f0bd", 16),   Convert.ToInt32("0xc3eb9e15", 16),   Convert.ToInt32("0x3c9057a2", 16),   Convert.ToInt32("0x97271aec", 16),   Convert.ToInt32("0xa93a072a", 16),
            Convert.ToInt32("0x1b3f6d9b", 16),   Convert.ToInt32("0x1e6321f5", 16),   Convert.ToInt32("0xf59c66fb", 16),   Convert.ToInt32("0x26dcf319", 16),   Convert.ToInt32("0x7533d928", 16),   Convert.ToInt32("0xb155fdf5", 16),
            Convert.ToInt32("0x03563482", 16),   Convert.ToInt32("0x8aba3cbb", 16),   Convert.ToInt32("0x28517711", 16),   Convert.ToInt32("0xc20ad9f8", 16),   Convert.ToInt32("0xabcc5167", 16),   Convert.ToInt32("0xccad925f", 16),
            Convert.ToInt32("0x4de81751", 16),   Convert.ToInt32("0x3830dc8e", 16),   Convert.ToInt32("0x379d5862", 16),   Convert.ToInt32("0x9320f991", 16),   Convert.ToInt32("0xea7a90c2", 16),   Convert.ToInt32("0xfb3e7bce", 16),
            Convert.ToInt32("0x5121ce64", 16),   Convert.ToInt32("0x774fbe32", 16),   Convert.ToInt32("0xa8b6e37e", 16),   Convert.ToInt32("0xc3293d46", 16),   Convert.ToInt32("0x48de5369", 16),   Convert.ToInt32("0x6413e680", 16),
            Convert.ToInt32("0xa2ae0810", 16),   Convert.ToInt32("0xdd6db224", 16),   Convert.ToInt32("0x69852dfd", 16),   Convert.ToInt32("0x09072166", 16),   Convert.ToInt32("0xb39a460a", 16),   Convert.ToInt32("0x6445c0dd", 16),
            Convert.ToInt32("0x586cdecf", 16),   Convert.ToInt32("0x1c20c8ae", 16),   Convert.ToInt32("0x5bbef7dd", 16),   Convert.ToInt32("0x1b588d40", 16),   Convert.ToInt32("0xccd2017f", 16),   Convert.ToInt32("0x6bb4e3bb", 16),
            Convert.ToInt32("0xdda26a7e", 16),   Convert.ToInt32("0x3a59ff45", 16),   Convert.ToInt32("0x3e350a44", 16),   Convert.ToInt32("0xbcb4cdd5", 16),   Convert.ToInt32("0x72eacea8", 16),   Convert.ToInt32("0xfa6484bb", 16),
            Convert.ToInt32("0x8d6612ae", 16),   Convert.ToInt32("0xbf3c6f47", 16),   Convert.ToInt32("0xd29be463", 16),   Convert.ToInt32("0x542f5d9e", 16),   Convert.ToInt32("0xaec2771b", 16),   Convert.ToInt32("0xf64e6370", 16),
            Convert.ToInt32("0x740e0d8d", 16),   Convert.ToInt32("0xe75b1357", 16),   Convert.ToInt32("0xf8721671", 16),   Convert.ToInt32("0xaf537d5d", 16),   Convert.ToInt32("0x4040cb08", 16),   Convert.ToInt32("0x4eb4e2cc", 16),
            Convert.ToInt32("0x34d2466a", 16),   Convert.ToInt32("0x0115af84", 16),   Convert.ToInt32("0xe1b00428", 16),   Convert.ToInt32("0x95983a1d", 16),   Convert.ToInt32("0x06b89fb4", 16),   Convert.ToInt32("0xce6ea048", 16),
            Convert.ToInt32("0x6f3f3b82", 16),   Convert.ToInt32("0x3520ab82", 16),   Convert.ToInt32("0x011a1d4b", 16),   Convert.ToInt32("0x277227f8", 16),   Convert.ToInt32("0x611560b1", 16),   Convert.ToInt32("0xe7933fdc", 16),
            Convert.ToInt32("0xbb3a792b", 16),   Convert.ToInt32("0x344525bd", 16),   Convert.ToInt32("0xa08839e1", 16),   Convert.ToInt32("0x51ce794b", 16),   Convert.ToInt32("0x2f32c9b7", 16),   Convert.ToInt32("0xa01fbac9", 16),
            Convert.ToInt32("0xe01cc87e", 16),   Convert.ToInt32("0xbcc7d1f6", 16),   Convert.ToInt32("0xcf0111c3", 16),   Convert.ToInt32("0xa1e8aac7", 16),   Convert.ToInt32("0x1a908749", 16),   Convert.ToInt32("0xd44fbd9a", 16),
            Convert.ToInt32("0xd0dadecb", 16),   Convert.ToInt32("0xd50ada38", 16),   Convert.ToInt32("0x0339c32a", 16),   Convert.ToInt32("0xc6913667", 16),   Convert.ToInt32("0x8df9317c", 16),   Convert.ToInt32("0xe0b12b4f", 16),
            Convert.ToInt32("0xf79e59b7", 16),   Convert.ToInt32("0x43f5bb3a", 16),   Convert.ToInt32("0xf2d519ff", 16),   Convert.ToInt32("0x27d9459c", 16),   Convert.ToInt32("0xbf97222c", 16),   Convert.ToInt32("0x15e6fc2a", 16),
            Convert.ToInt32("0x0f91fc71", 16),   Convert.ToInt32("0x9b941525", 16),   Convert.ToInt32("0xfae59361", 16),   Convert.ToInt32("0xceb69ceb", 16),   Convert.ToInt32("0xc2a86459", 16),   Convert.ToInt32("0x12baa8d1", 16),
            Convert.ToInt32("0xb6c1075e", 16),   Convert.ToInt32("0xe3056a0c", 16),   Convert.ToInt32("0x10d25065", 16),   Convert.ToInt32("0xcb03a442", 16),   Convert.ToInt32("0xe0ec6e0e", 16),   Convert.ToInt32("0x1698db3b", 16),
            Convert.ToInt32("0x4c98a0be", 16),   Convert.ToInt32("0x3278e964", 16),   Convert.ToInt32("0x9f1f9532", 16),   Convert.ToInt32("0xe0d392df", 16),   Convert.ToInt32("0xd3a0342b", 16),   Convert.ToInt32("0x8971f21e", 16),
            Convert.ToInt32("0x1b0a7441", 16),   Convert.ToInt32("0x4ba3348c", 16),   Convert.ToInt32("0xc5be7120", 16),   Convert.ToInt32("0xc37632d8", 16),   Convert.ToInt32("0xdf359f8d", 16),   Convert.ToInt32("0x9b992f2e", 16),
            Convert.ToInt32("0xe60b6f47", 16),   Convert.ToInt32("0x0fe3f11d", 16),   Convert.ToInt32("0xe54cda54", 16),   Convert.ToInt32("0x1edad891", 16),   Convert.ToInt32("0xce6279cf", 16),   Convert.ToInt32("0xcd3e7e6f", 16),
            Convert.ToInt32("0x1618b166", 16),   Convert.ToInt32("0xfd2c1d05", 16),   Convert.ToInt32("0x848fd2c5", 16),   Convert.ToInt32("0xf6fb2299", 16),   Convert.ToInt32("0xf523f357", 16),   Convert.ToInt32("0xa6327623", 16),
            Convert.ToInt32("0x93a83531", 16),   Convert.ToInt32("0x56cccd02", 16),   Convert.ToInt32("0xacf08162", 16),   Convert.ToInt32("0x5a75ebb5", 16),   Convert.ToInt32("0x6e163697", 16),   Convert.ToInt32("0x88d273cc", 16),
            Convert.ToInt32("0xde966292", 16),   Convert.ToInt32("0x81b949d0", 16),   Convert.ToInt32("0x4c50901b", 16),   Convert.ToInt32("0x71c65614", 16),   Convert.ToInt32("0xe6c6c7bd", 16),   Convert.ToInt32("0x327a140a", 16),
            Convert.ToInt32("0x45e1d006", 16),   Convert.ToInt32("0xc3f27b9a", 16),   Convert.ToInt32("0xc9aa53fd", 16),   Convert.ToInt32("0x62a80f00", 16),   Convert.ToInt32("0xbb25bfe2", 16),   Convert.ToInt32("0x35bdd2f6", 16),
            Convert.ToInt32("0x71126905", 16),   Convert.ToInt32("0xb2040222", 16),   Convert.ToInt32("0xb6cbcf7c", 16),   Convert.ToInt32("0xcd769c2b", 16),   Convert.ToInt32("0x53113ec0", 16),   Convert.ToInt32("0x1640e3d3", 16),
            Convert.ToInt32("0x38abbd60", 16),   Convert.ToInt32("0x2547adf0", 16),   Convert.ToInt32("0xba38209c", 16),   Convert.ToInt32("0xf746ce76", 16),   Convert.ToInt32("0x77afa1c5", 16),   Convert.ToInt32("0x20756060", 16),
            Convert.ToInt32("0x85cbfe4e", 16),   Convert.ToInt32("0x8ae88dd8", 16),   Convert.ToInt32("0x7aaaf9b0", 16),   Convert.ToInt32("0x4cf9aa7e", 16),   Convert.ToInt32("0x1948c25c", 16),   Convert.ToInt32("0x02fb8a8c", 16),
            Convert.ToInt32("0x01c36ae4", 16),   Convert.ToInt32("0xd6ebe1f9", 16),   Convert.ToInt32("0x90d4f869", 16),   Convert.ToInt32("0xa65cdea0", 16),   Convert.ToInt32("0x3f09252d", 16),   Convert.ToInt32("0xc208e69f", 16),
            Convert.ToInt32("0xb74e6132", 16),   Convert.ToInt32("0xce77e25b", 16),   Convert.ToInt32("0x578fdfe3", 16),   Convert.ToInt32("0x3ac372e6", 16) };
    
        private static long byteArrayToLong(byte[] buffer,
                                           int nStartIndex)
        {
            return (((long)buffer[nStartIndex]) << 56) |
                    (((long)buffer[nStartIndex + 1] & 0x0ffL) << 48) |
                    (((long)buffer[nStartIndex + 2] & 0x0ffL) << 40) |
                    (((long)buffer[nStartIndex + 3] & 0x0ffL) << 32) |
                    (((long)buffer[nStartIndex + 4] & 0x0ffL) << 24) |
                    (((long)buffer[nStartIndex + 5] & 0x0ffL) << 16) |
                    (((long)buffer[nStartIndex + 6] & 0x0ffL) << 8) |
                    ((long)buffer[nStartIndex + 7] & 0x0ff);
        }

        /**
         * converts a long o bytes which are put into a given array
         * @param lValue the 64bit integer to convert
         * @param buffer the target buffer
         * @param nStartIndex where to place the bytes in the buffer
         */
        private static void longToByteArray(long lValue,
                                           byte[] buffer,
                                           int nStartIndex)
        {
            buffer[nStartIndex] = (byte)((ulong)lValue >> 56);
            buffer[nStartIndex + 1] = (byte)(((ulong)lValue >> 48) & 0x0ff);
            buffer[nStartIndex + 2] = (byte)(((ulong)lValue >> 40) & 0x0ff);
            buffer[nStartIndex + 3] = (byte)(((ulong)lValue >> 32) & 0x0ff);
            buffer[nStartIndex + 4] = (byte)(((ulong)lValue >> 24) & 0x0ff);
            buffer[nStartIndex + 5] = (byte)(((ulong)lValue >> 16) & 0x0ff);
            buffer[nStartIndex + 6] = (byte)(((ulong)lValue >> 8) & 0x0ff);
            buffer[nStartIndex + 7] = (byte) lValue;
        }

        /**
         * converts values from an integer array to a long
         * @param buffer where to get the bytes
         * @param nStartIndex index from where to read the data
         * @return the 64bit integer
         */
        private static long intArrayToLong(int[] buffer,
                                          int nStartIndex)
        {
            return (((long) buffer[nStartIndex]) << 32) |
                    (((long) buffer[nStartIndex + 1]) & 0x0ffffffffL);
        }

        /**
         * converts a long to integers which are put into a given array
         * @param lValue the 64bit integer to convert
         * @param buffer the target buffer
         * @param nStartIndex where to place the bytes in the buffer
         */
        private static void longToIntArray(long lValue,
                                          int[] buffer,
                                          int nStartIndex)
        {
            buffer[nStartIndex] = (int)((ulong)lValue >> 32);
            buffer[nStartIndex + 1] = (int) lValue;
        }

        /**
         * makes a long from two integers (treated unsigned)
         * @param nLo lower 32bits
         * @param nHi higher 32bits
         * @return the built long
         */
        private static long makeLong(long nLo,
                                    long nHi)
        {
            return (((long)nHi << 32) |
                    ((long)nLo & 0x00000000ffffffffL));
        }

        /**
         * gets the lower 32 bits of a long
         * @param lVal the long integer
         * @return lower 32 bits
         */
        private static int longLo32(long lVal)
        {
            return (int)lVal;
        }

        /**
         * gets the higher 32 bits of a long
         * @param lVal the long integer
         * @return higher 32 bits
         */
        private static int longHi32(long lVal)
        {
            return (int)(((ulong)lVal >> 32));
        }

        // our table for binhex conversion
        readonly static char[] HEXTAB = { '0', '1', '2', '3', '4', '5', '6', '7',
                                       '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        /**
         * converts a byte array to a binhex string
         * @param data the byte array
         * @param nStartPos start index where to get the bytes
         * @param nNumOfBytes number of bytes to convert
         * @return the binhex string
         */
        private static String bytesToBinHex(byte[] data,
                                           int nStartPos,
                                           int nNumOfBytes)
        {
            string[] bytesToBinHex = new string[nNumOfBytes << 1];
            int nPos = 0;
            String value = string.Empty;

            for (int nI = 0; nI < nNumOfBytes; nI++)
            {
                bytesToBinHex[nPos++] = HEXTAB[(data[nI + nStartPos] >> 4) & 0x0f].ToString();
                bytesToBinHex[nPos++] = HEXTAB[data[nI + nStartPos] & 0x0f].ToString();
            }

            for (int byteI = 0; byteI < bytesToBinHex.Length; byteI++)
                value += bytesToBinHex[byteI];

            return value;
        }

        /**
         * converts a binhex string back into a byte array (invalid codes will be skipped)
         * @param sBinHex binhex string
         * @param data the target array
         * @param nSrcPos from which character in the string the conversion should begin,
         *                remember that (nSrcPos modulo 2) should equals 0 normally
         * @param nDstPos to store the bytes from which position in the array
         * @param nNumOfBytes number of bytes to extract
         * @return number of extracted bytes
         */
        private static int binHexToBytes(String sBinHex,
                                        byte[] data,
                                        int nSrcPos,
                                        int nDstPos,
                                        int nNumOfBytes)
        {
            // check for correct ranges
            int nStrLen = sBinHex.Length;

            int nAvailBytes = (nStrLen - nSrcPos) >> 1;
            if (nAvailBytes < nNumOfBytes)
            {
                nNumOfBytes = nAvailBytes;
            }

            int nOutputCapacity = data.Length - nDstPos;
            if (nNumOfBytes > nOutputCapacity)
            {
                nNumOfBytes = nOutputCapacity;
            }

            // convert now
            int nResult = 0;
            for (int nI = 0; nI < nNumOfBytes; nI++)
            {
                byte bActByte = 0;
                bool blConvertOK = true;
                for (int nJ = 0; nJ < 2; nJ++)
                {
                    bActByte <<= 4;
                    char cActChar = sBinHex[nSrcPos++];

                    if ((cActChar >= 'a') && (cActChar <= 'f'))
                    {
                        bActByte |= (byte)((cActChar - 'a') + 10);
                    }
                    else
                    {
                        if ((cActChar >= '0') && (cActChar <= '9'))
                        {
                            bActByte |= (byte)(cActChar - '0');
                        }
                        else
                        {
                            blConvertOK = false;
                        }
                    }
                }
                if (blConvertOK)
                {
                    data[nDstPos++] = bActByte;
                    nResult++;
                }
            }

            return nResult;
        }

        /**
         * converts a byte array into an UNICODE string
         * @param data the byte array
         * @param nStartPos where to begin the conversion
         * @param nNumOfBytes number of bytes to handle
         * @return the string
         */
        private static String byteArrayToUNCString(byte[] data,
                                                  int nStartPos,
                                                  int nNumOfBytes)
        {
            // we need two bytes for every character
            nNumOfBytes &= ~1;

            // enough bytes in the buffer?
            int nAvailCapacity = data.Length - nStartPos;

            if (nAvailCapacity < nNumOfBytes)
            {
                nNumOfBytes = nAvailCapacity;
            }

            string[] byteArrayToUNCString = new string[nNumOfBytes >> 1];
            int nSBufPos = 0;
            String value = string.Empty;
            while (nNumOfBytes > 0)
            {
                byteArrayToUNCString[nSBufPos++] = ((char)(((int)data[nStartPos] << 8) | ((int)data[nStartPos + 1] & 0x0ff))).ToString();
                nStartPos += 2;
                nNumOfBytes -= 2;
            }

            for (int byteI = 0; byteI < byteArrayToUNCString.Length; byteI++)
            {
                value += byteArrayToUNCString[byteI];
            }
            return value;
        }
    }
}