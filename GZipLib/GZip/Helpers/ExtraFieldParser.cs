using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipLib.GZip.Helpers
{
    class ExtraFieldParser
    {
        public static List<ExtraField> GetExtraFields(Stream stream)
        {           
            var beforePos = stream.Position;

            try
            {
                byte[] headerBuffer = new byte[10];
                
                int numReadBytes = 0;
                numReadBytes = stream.Read(headerBuffer, 0, 10);
                if (numReadBytes < 10)
                {
                    throw new InvalidDataException("архив повреждён");
                }

                byte id1 = headerBuffer[0];
                byte id2 = headerBuffer[1];

                if (id1 != 0x1f || id2 != 0x8b)
                {
                    throw new InvalidDataException("Неверное магическое число в заголовке файла");
                }


                byte flg = headerBuffer[3];
                if (((HeaderFLG)flg & HeaderFLG.FEXTRA) == 0)
                {
                    return new List<ExtraField>();
                }


                byte[] extraLengthBuffer = new byte[2];

                numReadBytes = stream.Read(extraLengthBuffer, 0, 2);
                if (numReadBytes < 2)
                {
                    throw new InvalidDataException("архив повреждён");
                }

                short extraLength = BitConverter.ToInt16(extraLengthBuffer, 0);
                byte[] extraFieldsBuffer = new byte[extraLength];

                numReadBytes = stream.Read(extraFieldsBuffer, 0, extraLength);
                if (numReadBytes < extraLength)
                {
                    throw new InvalidDataException("архив повреждён");
                }

                //var extraFieldsBufferList = extraFieldsBuffer.ToList();
                List<ExtraField> result = new List<ExtraField>();

                try
                {
                    for (int i = 0; i < extraLength;)
                    {
                        byte sid1 = extraFieldsBuffer[i];
                        byte sid2 = extraFieldsBuffer[i + 1];

                        short subExtraLength = BitConverter.ToInt16(extraFieldsBuffer, i + 2);
                        var data = extraFieldsBuffer.ToList().GetRange(i + 4, subExtraLength).ToArray();
                        ExtraField ef = new ExtraField(sid1, sid2, data);

                        result.Add(ef);
                        i = i + 4 + subExtraLength;
                    }
                    return result;
                }
                catch { throw new InvalidDataException("архив повреждён"); }


            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                stream.Position = beforePos;
            }
            
        }  
    }
}
