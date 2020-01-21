using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace  Musika.Library.Utilities
{
    public class BlowfishCBC : BlowfishECB
    {
        // here we hold the CBC IV
        long m_lCBCIV;

        public BlowfishCBC()
        {

        }

        /**
         * get the current CBC IV (for cipher resets)
         * @return current CBC IV
         */
        public long getCBCIV()
        {
            return m_lCBCIV;
        }

        /**
         * get the current CBC IV (for cipher resets)
         * @param dest wher eto put current CBC IV in network byte ordered array
         */
        public void getCBCIV(byte[] dest)
        {
            longToByteArray(m_lCBCIV, dest, 0);
        }

        /**
         * set the current CBC IV (for cipher resets)
         * @param lNewCBCIV the new CBC IV
         */
        public void setCBCIV(long lNewCBCIV)
        {
            m_lCBCIV = lNewCBCIV;
        }

        /**
         * set the current CBC IV (for cipher resets)
         * @param newCBCIV the new CBC IV  in network byte ordered array
         */
        public void setCBCIV(byte[] newCBCIV)
        {
            m_lCBCIV = byteArrayToLong(newCBCIV, 0);
        }

        /**
         * constructor, stores a zero CBC IV
         * @param bfkey key material, up to MAXKEYLENGTH bytes
         */
        public BlowfishCBC(byte[] bfkey)
            : base(bfkey)
        {
            // store zero CBCB IV
            setCBCIV(0);
        }

        /**
         * constructor
         * @param bfkey key material, up to MAXKEYLENGTH bytes
         * @param lInitCBCIV the CBC IV
         */
        public BlowfishCBC(byte[] bfkey,
                           long lInitCBCIV)
            : base(bfkey)
        {
            // store the CBCB IV
            setCBCIV(lInitCBCIV);
        }

        /**
         * constructor
         * @param bfkey key material, up to MAXKEYLENGTH bytes
         * @param initCBCIV the CBC IV (array with min. BLOCKSIZE bytes)
         */
        public BlowfishCBC(byte[] bfkey,
                           byte[] initCBCIV)
            : base(bfkey)
        {
            // store the CBCB IV
            setCBCIV(initCBCIV);
        }

        /**
         * cleans up all critical internals,
         * call this if you don't need an instance anymore
         */
        //@Override
		public void cleanUp()
        {
            m_lCBCIV = 0;
            base.cleanUp();
        }

        // internal routine to encrypt a block in CBC mode
        private long encryptBlockCBC(long lPlainblock)
        {
            // chain with the CBC IV
            lPlainblock ^= m_lCBCIV;

            // encrypt the block
            lPlainblock = base.encryptBlock(lPlainblock);

            // the encrypted block is the new CBC IV
            return (m_lCBCIV = lPlainblock);
        }

        // internal routine to decrypt a block in CBC mode
        private long decryptBlockCBC(long lCipherblock)
        {
            // save the current block
            long lTemp = lCipherblock;

            // decrypt the block
            lCipherblock = base.decryptBlock(lCipherblock);

            // dechain the block
            lCipherblock ^= m_lCBCIV;

            // set the new CBC IV
            m_lCBCIV = lTemp;

            // return the decrypted block
            return lCipherblock;
        }

        /**
         * encrypts a byte buffer (should be aligned to an 8 byte border)
         * to another buffer (of the same size or bigger)
         * @param inbuffer buffer with plaintext data
         * @param outbuffer buffer to get the ciphertext data
         */
        //@Override
		public void encrypt(byte[] inbuffer,
                            byte[] outbuffer)
        {
            int nLen = inbuffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=8)
            {
                // encrypt a temporary 64bit block
                lTemp = byteArrayToLong(inbuffer, nI);
                lTemp = encryptBlockCBC(lTemp);
                longToByteArray(lTemp, outbuffer, nI);
            }
        }

        /**
         * encrypts a byte buffer (should be aligned to an 8 byte border) to itself
         * @param buffer buffer to encrypt
         */
        //@Override
		public void encrypt(byte[] buffer)
        {

            int nLen = buffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=8)
            {
                // encrypt a temporary 64bit block
                lTemp = byteArrayToLong(buffer, nI);
                lTemp = encryptBlockCBC(lTemp);
                longToByteArray(lTemp, buffer, nI);
            }
        }

        public void encrypt(sbyte[] buffer)
        {

            int nLen = buffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI += 8)
            {
                // encrypt a temporary 64bit block
                lTemp = byteArrayToLong(buffer, nI);
                lTemp = encryptBlockCBC(lTemp);
                longToByteArray(lTemp, buffer, nI);
            }
        }

        /**
         * encrypts an int buffer (should be aligned to an
         * two integer border) to another int buffer (of the same
         * size or bigger)
         * @param inbuffer buffer with plaintext data
         * @param outbuffer buffer to get the ciphertext data
         */
        //@Override
		public void encrypt(int[] inbuffer,
                            int[] outbuffer)
        {
            int nLen = inbuffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=2)
            {
                // encrypt a temporary 64bit block
                lTemp = intArrayToLong(inbuffer, nI);
                lTemp = encryptBlockCBC(lTemp);
                longToIntArray(lTemp, outbuffer, nI);
            }
        }

        /**
         * encrypts an integer buffer (should be aligned to an
         * @param buffer buffer to encrypt
         */
        //@Override
		public void encrypt(int[] buffer)
        {
            int nLen = buffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=2)
            {
                // encrypt a temporary 64bit block
                lTemp = intArrayToLong(buffer, nI);
                lTemp = encryptBlockCBC(lTemp);
                longToIntArray(lTemp, buffer, nI);
            }
        }

        /**
         * encrypts a long buffer to another long buffer (of the same size or bigger)
         * @param inbuffer buffer with plaintext data
         * @param outbuffer buffer to get the ciphertext data
         */
       // @Override
		public void encrypt(long[] inbuffer,
                            long[] outbuffer)
        {
            int nLen = inbuffer.Length;
            for (int nI = 0; nI < nLen; nI++)
            {
                outbuffer[nI] = encryptBlockCBC(inbuffer[nI]);
            }
        }

        /**
         * encrypts a long buffer to itself
         * @param buffer buffer to encrypt
         */
        //@Override
		public void encrypt(long[] buffer)
        {
            int nLen = buffer.Length;
            for (int nI = 0; nI < nLen; nI++)
            {
                buffer[nI] = encryptBlockCBC(buffer[nI]);
            }
        }

        /**
         * decrypts a byte buffer (should be aligned to an 8 byte border)
         * to another buffer (of the same size or bigger)
         * @param inbuffer buffer with ciphertext data
         * @param outbuffer buffer to get the plaintext data
         */
        //@Override
		public void decrypt(byte[] inbuffer,
                            byte[] outbuffer)
        {
            int nLen = inbuffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=8)
            {
                // decrypt a temporary 64bit block
                lTemp = byteArrayToLong(inbuffer, nI);
                lTemp = decryptBlockCBC(lTemp);
                longToByteArray(lTemp, outbuffer, nI);
            }
        }

        /**
         * decrypts a byte buffer (should be aligned to an 8 byte border) to itself
         * @param buffer buffer to decrypt
         */
        //@Override
		public void  decrypt(byte[] buffer)
        {
            int nLen = buffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=8)
            {
                // decrypt over a temporary 64bit block
                lTemp = byteArrayToLong(buffer, nI);
                lTemp = decryptBlockCBC(lTemp);
                longToByteArray(lTemp, buffer, nI);
            }
        }

        /**
         * decrypts an integer buffer (should be aligned to an
         * two integer border) to another int buffer (of the same size or bigger)
         * @param inbuffer buffer with ciphertext data
         * @param outbuffer buffer to get the plaintext data
         */
        //@Override
		public void decrypt(int[] inbuffer,
                            int[] outbuffer)
        {

            int nLen = inbuffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=2)
            {
                // decrypt a temporary 64bit block
                lTemp = intArrayToLong(inbuffer, nI);
                lTemp = decryptBlockCBC(lTemp);
                longToIntArray(lTemp, outbuffer, nI);
            }
        }

        /**
         * decrypts an int buffer (should be aligned to a
         * two integer border)
         * @param buffer buffer to decrypt
         */
        //@Override
		public void decrypt(int[] buffer)
        {
            int nLen = buffer.Length;
            long lTemp;
            for (int nI = 0; nI < nLen; nI +=2)
            {
                // decrypt a temporary 64bit block
                lTemp = intArrayToLong(buffer, nI);
                lTemp = decryptBlockCBC(lTemp);
                longToIntArray(lTemp, buffer, nI);
            }
        }

        /**
         * decrypts a long buffer to another long buffer (of the same size or bigger)
         * @param inbuffer buffer with ciphertext data
         * @param outbuffer buffer to get the plaintext data
         */
        //@Override
		public void decrypt(long[] inbuffer,
                            long[] outbuffer)
        {
            int nLen = inbuffer.Length;
            for (int nI = 0; nI < nLen; nI++)
            {
                outbuffer[nI] = decryptBlockCBC(inbuffer[nI]);
            }
        }

        /**
         * decrypts a long buffer to itself
         * @param buffer buffer to decrypt
         */
        //@Override
		public void decrypt(long[] buffer)
        {
            int nLen = buffer.Length;
            for (int nI = 0; nI < nLen; nI++)
            {
                buffer[nI] = decryptBlockCBC(buffer[nI]);
            }
        }

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

        private static long byteArrayToLong(sbyte[] buffer,
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
            buffer[nStartIndex + 7] = (byte)lValue;
        }

        private static void longToByteArray(long lValue,
                                           sbyte[] buffer,
                                           int nStartIndex)
        {
            buffer[nStartIndex] = (sbyte)((ulong)lValue >> 56);
            buffer[nStartIndex + 1] = (sbyte)(((ulong)lValue >> 48) & 0x0ff);
            buffer[nStartIndex + 2] = (sbyte)(((ulong)lValue >> 40) & 0x0ff);
            buffer[nStartIndex + 3] = (sbyte)(((ulong)lValue >> 32) & 0x0ff);
            buffer[nStartIndex + 4] = (sbyte)(((ulong)lValue >> 24) & 0x0ff);
            buffer[nStartIndex + 5] = (sbyte)(((ulong)lValue >> 16) & 0x0ff);
            buffer[nStartIndex + 6] = (sbyte)(((ulong)lValue >> 8) & 0x0ff);
            buffer[nStartIndex + 7] = (sbyte)lValue;
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
            return (((long)buffer[nStartIndex]) << 32) |
                    (((long)buffer[nStartIndex + 1]) & 0x0ffffffffL);
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
            buffer[nStartIndex + 1] = (int)lValue;
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