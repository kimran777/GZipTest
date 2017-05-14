﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest.GZip
{
    abstract class Compressor
    {

        public Compressor(FileInfo inFileInfo, FileInfo outFileInfo)
        {
            if(inFileInfo == null)
            {
                throw new ArgumentNullException("inFileInfo");
            }
            if (outFileInfo == null)
            {
                throw new ArgumentNullException("outFileInfo");
            }


            InFileInfo = inFileInfo;
            OutFileInfo = outFileInfo;

            if (!InFileInfo.Exists)
            {
                throw new FileNotFoundException(string.Format("Файл {0} не найден", InFileInfo.FullName));
            }
            

        }

        protected FileInfo InFileInfo
        {
            get;
            set;
        }
        protected FileInfo OutFileInfo
        {
            get;
            set;
        }
        protected readonly int _blockSize = 1024 * 1024;

        protected long _inputLength;

        abstract public void Start();
    }
}
