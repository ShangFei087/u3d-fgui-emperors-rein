using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMaker
{
    public class EventData
    {
        public string name { get; private set; }
        public object value { get { return GetValue(); } }
        virtual protected object GetValue() { return null; }
        /// BAGELCODE
        /*
        public EventData(string name) {
            this.name = name;
        }
        */
        public int id;
        public EventData(string name, int id = 0)
        {
            this.name = name;
            this.id = id;
        }
        /// BAGELCODE
    }

    ///Used for events with a value
    public class EventData<T> : EventData
    {
        new public T value { get; private set; }
        protected override object GetValue() { return value; }
        public EventData(string name, T value) : base(name)
        {
            this.value = value;
        }
        /// BAGELCODE
        public EventData(string name, int id, T value) : base(name, id)
        {
            this.value = value;
        }
        /// BAGELCODE
    }

}
