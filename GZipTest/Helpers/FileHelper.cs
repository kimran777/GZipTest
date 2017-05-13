using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.File
{
    class FileHelper
    {
        public static FileStream CreateFileToWrite(FileInfo fileInfo)
        {

            if (!fileInfo.Exists)
            {
                return fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.Write);
            }

            while (true)
            {

                Console.Write("Файл {0} уже существует. Заменить файл? [да/нет]: ", fileInfo);

                var answer = Console.ReadLine().ToLower();


                switch (answer)
                {
                    case "да":
                    case "д":
                        {
                            return fileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.Write);
                        }
                    case "нет":
                    case "н":
                        {
                            throw new OperationCanceledException();
                        }
                }

            }

        }
    }
}
