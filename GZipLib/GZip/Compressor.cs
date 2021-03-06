﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipLib.GZip
{

    
    public abstract class Compressor
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

        protected long _inputLength;

        abstract public void Start();
    }
}
