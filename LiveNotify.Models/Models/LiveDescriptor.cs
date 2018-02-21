using System;

namespace LiveNotify.Models
{
    public struct LiveDescriptor
    {
        public LiveDescriptor(string dataName, string label, Type dataType)
        {
            DataName = dataName;
            Label = label;
            DataType = dataType;
        }

        public override bool Equals(object obj)
        {
            if ((null == obj) || (obj.GetType() != typeof(LiveDescriptor)))
            {
                return false;
            }
            return ((LiveDescriptor)obj).DataName == DataName;
        }

        public override int GetHashCode()
        {
            return DataName.GetHashCode();
        }

        public string DataName { get; }
        public string Label { get; }
        public Type DataType { get; }
    }
}
