using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.File
{
    class FileHelper
    {
        public static void CheckOutputExist(FileInfo outputFile)
        {
            if (!outputFile.Exists)
            {
                return;
            }

            while (true)
            {

                Console.Write("Файл {0} уже существует. Заменить файл? [да/нет]: ", outputFile.FullName);

                var answer = Console.ReadLine().ToLower();


                switch (answer)
                {
                    case "да":
                    case "д":
                        {
                            return;
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
