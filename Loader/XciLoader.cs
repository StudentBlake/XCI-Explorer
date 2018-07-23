using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using XCI.Explorer.DTO;
using XCI.Explorer.Helpers;
using XCI.Model;

namespace XCI.Explorer.Loader
{
    public class XciLoader : ILoader
    {
        private readonly GameDto _gameDto;
        //Don't know what this value is representing
        private const long MagicalSecureSize = -9223372036854775808L;
        //Don't know what this value is representing (apart from being int16.MaxValue + 1)
        private const int MagicNumber = 32768;


        public XciLoader(GameDto gameDto)
        {
            _gameDto = gameDto;
        }
        public void LoadRom(string filePath)
        {
            double fileSize = new FileInfo(filePath).Length;

            _gameDto.ExactSize = "(" + fileSize + " bytes)";
            var num2 = 0;
            while (fileSize >= 1024.0 && num2 < Util.SizeCategories.Length - 1)
            {
                num2++;
                fileSize /= 1024.0;
            }
            _gameDto.Size = $"{fileSize:0.##} {Util.SizeCategories[num2]}";
            var num3 = _gameDto.UsedSize = Xci.XciHeaders[0].CardSize2 * 512 + 512;
            _gameDto.ExactUsedSpace = "(" + num3 + " bytes)";
            num2 = 0;
            while (num3 >= 1024.0 && num2 < Util.SizeCategories.Length - 1)
            {
                num2++;
                num3 /= 1024.0;
            }
            _gameDto.UsedSpace = $"{num3:0.##} {Util.SizeCategories[num2]}";
            _gameDto.Capacity = Util.GetCapacity(Xci.XciHeaders[0].CardSize1);
        }

        public void LoadPartitions(string filePath)
        {
            
        }

        public void LoadNca()
        {
            throw new NotImplementedException();
        }

        public void LoadInfos()
        {
            throw new NotImplementedException();
        }
    }
}