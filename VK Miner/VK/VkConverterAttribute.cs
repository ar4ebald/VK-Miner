using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace VK_Miner.VK
{
    [AttributeUsage(AttributeTargets.Property)]
    class VkConverterAttribute : Attribute
    {
        public string ConverterName { get; private set; }

        public VkConverterAttribute(string converterName)
        {
            ConverterName = converterName;
        }
    }
}
