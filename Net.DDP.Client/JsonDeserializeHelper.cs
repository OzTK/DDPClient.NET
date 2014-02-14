using System.Collections.Generic;
using System.Globalization;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Net.DDP.Client
{
    internal class JsonDeserializeHelper
    {
        private readonly IDataSubscriber _subscriber;

        public JsonDeserializeHelper(IDataSubscriber subscriber)
        {
            _subscriber = subscriber;
        }

        internal void Deserialize(string jsonItem)
        {
            JObject jObj = JObject.Parse(jsonItem);
            if (jObj["set"]!=null)
            {
                dynamic d = GetData(jObj);
                d.Type= "sub";
                _subscriber.DataReceived(d);
            }
            else if (jObj["unset"]!=null)
            {
                dynamic entity = new ExpandoObject();
                entity.Type="unset";
                entity.Id= jObj["id"].ToString();
                _subscriber.DataReceived(entity);
            }
            else if (jObj["result"]!=null)
            {
                dynamic entity = new ExpandoObject();
                entity.Type= "method";
                entity.RequestingId=jObj["id"].ToString();
                entity.Result= jObj["result"].ToString();
                _subscriber.DataReceived(entity);
            }
            else if (jObj["error"] != null)
            {
                dynamic entity = new ExpandoObject();
                entity.Type = "error";
                entity.RequestingId = jObj["id"].ToString();
                entity.Error = jObj["error"].ToString();
                _subscriber.DataReceived(entity);
            }
            else if (jObj[DDPClient.DDP_PROPS_MESSAGE] != null)
            {
                dynamic entity;
                if (jObj[DDPClient.DDP_PROPS_COLLECTION] != null)
                    entity = GetData(jObj);
                else if (jObj[DDPClient.DDP_PROPS_SESSION] != null)
                {
                    entity = new ExpandoObject();
                    entity.Session = jObj[DDPClient.DDP_PROPS_SESSION].ToString();
                }
                else
                {
                    entity = new ExpandoObject();
                }
                entity.Type = jObj[DDPClient.DDP_PROPS_MESSAGE].ToString();

                _subscriber.DataReceived(entity);
            }
        }

        private dynamic GetData(JObject json)
        {
            var tmp = (JObject)json[DDPClient.DDP_PROPS_FIELDS];
            dynamic entity = GetDataRecursive(tmp);
            ((IDictionary<string, object>)entity).Add("Id", json[DDPClient.DDP_PROPS_ID].ToString());
            entity.Collection = json[DDPClient.DDP_PROPS_ID].ToString();
            
            return entity;
        }

        private dynamic GetDataRecursive(JObject json)
        {
            dynamic entity = new ExpandoObject();
            var entityAsCollection = (IDictionary<string, object>) entity;
            foreach (var item in json)
            {
                string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Key);
                if (item.Value is JObject) // Property is an object
                    entityAsCollection.Add(propertyName, GetDataRecursive((JObject) item.Value));
                else if (item.Value is JArray) // Property is an array...
                {
                    JArray collection = (JArray) item.Value;
                    if (collection.Count == 0)
                        continue;
                    if (collection[0] is JObject) // ... of objects
                    {
                        var entityCollection =
                            (from JObject colObj in collection select GetDataRecursive(colObj)).ToList();

                        entityAsCollection.Add(propertyName, entityCollection);
                    }
                    else // ... of strings
                    {
                        var strColl = collection.Select(colToken => colToken.ToString()).ToList();

                        entityAsCollection.Add(propertyName, strColl);
                    }
                }
                else // Property is a string
                    entityAsCollection.Add(propertyName, item.Value.ToString());
            }

            return entityAsCollection;
        }
    }
}
