using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronus.Enum
{
    public static class EnumHelper
    {
        public static string GetTagType(string tagType)
            => Cronos.SDK.Helper.EnumHelper.GetESLType(tagType).ToString();
    }
}
