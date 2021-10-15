using System;

namespace Filuet.Hardware.CashAcceptors.Common.Attributes
{
    public class CodeAttribute : Attribute
    {
        private readonly string _codeValue;

        public CodeAttribute(string codeValue)
        {
            _codeValue = codeValue;
        }

        public string DisplayCode => _codeValue;
    }
}
