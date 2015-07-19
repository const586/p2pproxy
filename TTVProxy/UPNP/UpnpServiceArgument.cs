using System;

namespace P2pProxy.UPNP
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class UpnpServiceVariable : Attribute
    {
        private readonly string name;
        private readonly string dataType;
        private readonly bool sendEvents;
        private readonly string[] allowedValue;

        public UpnpServiceVariable(string name, string dataType, bool sendEvents, params string[] allowedValue)
        {
            this.name = name;
            this.dataType = dataType;
            this.sendEvents = sendEvents;
            this.allowedValue = allowedValue;
        }

        public UpnpServiceVariable(string name, string dataType, bool sendEvents) : this(name, dataType, sendEvents, new string[0]) { }

        public string Name
        {
            get { return name; }
        }

        public string DataType
        {
            get { return dataType; }
        }

        public bool SendEvents
        {
            get { return sendEvents; }
        }

        public string[] AllowedValue
        {
            get { return allowedValue; }
        }
    }

    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true)]
    public class UpnpServiceArgument : Attribute
    {
        private readonly int index;
        private readonly string name;
        private readonly string relatedStateVariable;

        public UpnpServiceArgument(int index, string name, string relatedStateVariable)
        {
            this.index = index;
            this.name = name;
            this.relatedStateVariable = relatedStateVariable;
        }

        public UpnpServiceArgument(string relatedStateVariable)
        {
            this.relatedStateVariable = relatedStateVariable;
        }

        public int Index
        {
            get { return index; }
        }

        public string Name
        {
            get { return name; }
        }

        public string RelatedStateVariable
        {
            get { return relatedStateVariable; }
        }
    }
}