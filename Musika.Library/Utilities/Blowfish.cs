using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;

namespace Musika.Library.Utilities
{
    public class Blowfish
    {
        private BlowfishCBC m_bfish;
        private static Random m_rndGen = new Random();

        private byte[] SHA1keys(string key)
        {
            byte[] keyArray;

            using (SHA1CryptoServiceProvider hashmd5 = new SHA1CryptoServiceProvider())
            {
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            }
            return keyArray;
        }

        /**
         * Creates a new Blowfish object using the specified key (oversized
         * password will be cut).
         *
         * @param password the password (treated as a real unicode array)
         */
        public Blowfish(String password) {
            // hash down the password to a 160bit key
            // setup the encryptor (use a dummy IV)

            m_bfish = new BlowfishCBC(SHA1keys(password), 0);
        }

        /**
         * Encrypts a string (treated in UNICODE) using the
         * standard Java random generator, which isn't that
         * great for creating IVs
         *
         * @param sPlainText string to encrypt
         * @return encrypted string in binhex format
         */
        public String encryptString(String sPlainText) {
            // get the IV
            long lCBCIV;
            lock (m_rndGen)
            {
                lCBCIV = ((long)m_rndGen.Next() << 32);
            }

            // map the call;
            return encStr(sPlainText, lCBCIV);
        }

        // Internal routine for string encryption

        private String encStr(String sPlainText,
                              long lNewCBCIV)
        {
            // allocate the buffer (align to the next 8 byte border plus padding)
            int nStrLen = sPlainText.Length;
            sbyte[] buf = new sbyte [((nStrLen << 1) & 0xfffffff8) + 8];

            // copy all bytes of the string into the buffer (use network byte order)
            int nI;
            int nPos = 0;
            for (nI = 0; nI < nStrLen; nI++)
            {
                char cActChar = sPlainText[nI];
                buf[nPos++] = (sbyte) ((cActChar >> 8) & 0x0ff);
                buf[nPos++] = (sbyte) (cActChar & 0x0ff) ;
            }

            // pad the rest with the PKCS5 scheme
            sbyte bPadVal = (sbyte)(buf.Length - (nStrLen << 1));
            while (nPos < buf.Length)
            {
                buf[nPos++] = bPadVal;
            }

            lock (m_bfish) {
                // create the encryptor
                m_bfish.setCBCIV(lNewCBCIV);

                // encrypt the buffer
                m_bfish.encrypt(buf);
            }

            // return the binhex string
            sbyte[] newCBCIV = new sbyte[BlowfishCBC.BLOCKSIZE];
            longToByteArray(lNewCBCIV,
                    newCBCIV,
                    0);

            return bytesToBinHex(newCBCIV, 0, BlowfishCBC.BLOCKSIZE) +
                    bytesToBinHex(buf, 0, buf.Length);
        }

        /**
         * decrypts a hexbin string (handling is case sensitive)
         * @param sCipherText hexbin string to decrypt
         * @return decrypted string (null equals an error)
         */
        public String decryptString(String sCipherText)
        {
            // get the number of estimated bytes in the string (cut off broken blocks)
            int nLen = (sCipherText.Length >> 1) & ~7;

            // does the given stuff make sense (at least the CBC IV)?
            if (nLen < BlowfishECB.BLOCKSIZE)
                return null;

            // get the CBC IV
            byte[] cbciv = new byte[BlowfishCBC.BLOCKSIZE];
            int nNumOfBytes = binHexToBytes(sCipherText,
                    cbciv,
                    0,
                    0,
                    BlowfishCBC.BLOCKSIZE);
            if (nNumOfBytes < BlowfishCBC.BLOCKSIZE)
                return null;

            // something left to decrypt?
            nLen -= BlowfishCBC.BLOCKSIZE;
            if (nLen == 0)
            {
                return "";
            }

            // get all data bytes now
            byte[] buf = new byte[nLen];

            nNumOfBytes = binHexToBytes(sCipherText,
                    buf,
                    BlowfishCBC.BLOCKSIZE * 2,
                    0,
                    nLen);

            // we cannot accept broken binhex sequences due to padding
            // and decryption
            if (nNumOfBytes < nLen)
            {
                return null;
            }

            lock (m_bfish) {
                // (got it)
                m_bfish.setCBCIV(cbciv);

                // decrypt the buffer
                m_bfish.decrypt(buf);
            }

            // get the last padding byte
            int nPadByte = (int)buf[buf.Length - 1] & 0x0ff;

            // ( try to get all information if the padding doesn't seem to be correct)
            if ((nPadByte > 8) || (nPadByte < 0))
            {
                nPadByte = 0;
            }

            // calculate the real size of this message
            nNumOfBytes -= nPadByte;
            if (nNumOfBytes < 0)
            {
                return "";
            }

            // success
            return byteArrayToUNCString(buf, 0, nNumOfBytes);
        }

        /**
         * destroys (clears) the encryption engine,
         * after that the instance is not valid anymore
         */
        public void destroy()
        {
            m_bfish.cleanUp();
        }

        /**
         * gets bytes from an array into a long
         * @param buffer where to get the bytes
         * @param nStartIndex index from where to read the data
         * @return the 64bit integer
         */
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
        private static String bytesToBinHex(sbyte[] data,
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