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

            if (jObj[DDPClient.DDP_PROPS_ERROR] != null || jObj[DDPClient.DDP_PROPS_MESSAGE] != null && jObj[DDPClient.DDP_PROPS_MESSAGE].ToString() == "error")
                HandleError(jObj[DDPClient.DDP_PROPS_ERROR] ?? jObj);
            else if (jObj[DDPClient.DDP_PROPS_SESSION] != null)
                HandleConnected(jObj);
            else if (jObj[DDPClient.DDP_PROPS_MESSAGE] != null)
                HandleSubResult(jObj);
            else if (jObj[DDPClient.DDP_PROPS_RESULT] != null)
                HandleMethodResult(jObj);
        }

        private void HandleConnected(JObject jObj)
        {
            dynamic entity = new ExpandoObject();
            entity.Session = jObj[DDPClient.DDP_PROPS_SESSION].ToString();
            entity.Type = DDPType.Connected;

            _subscriber.DataReceived(entity);
        }

        private void HandleError(JToken jError)
        {
            dynamic entity = new ExpandoObject();
            entity.Type = DDPType.Error;
            entity.Error = jError["reason"].ToString();

            if (jError["error"] != null)
                entity.Code = jError["error"];
            if (jError["message"] != null)
                entity.Message = jError["message"];
        }

        private void HandleMethodResult(JObject jObj)
        {
            dynamic entity = new ExpandoObject();
            entity.Type = DDPType.MethodResult;
            entity.RequestingId = jObj["id"].ToString();
            entity.Result = jObj[DDPClient.DDP_PROPS_RESULT].ToString();
            _subscriber.DataReceived(entity);
        }

        private void HandleSubResult(JObject jObj)
        {
            dynamic entity = new ExpandoObject();

            if (jObj[DDPClient.DDP_PROPS_MESSAGE].ToString() == DDPClient.DDP_MESSAGE_TYPE_ADDED)
            {
                entity = GetMessageData(jObj);

                entity.Type = DDPType.Added;
            }
            else if (jObj[DDPClient.DDP_PROPS_MESSAGE].ToString() == DDPClient.DDP_MESSAGE_TYPE_CHANGED)
            {
                entity = GetMessageData(jObj);
                entity.Type = DDPType.Changed;
            }
            else if (jObj[DDPClient.DDP_PROPS_MESSAGE].ToString() == DDPClient.DDP_MESSAGE_TYPE_NOSUB)
            {
                HandleError(jObj[DDPClient.DDP_PROPS_ERROR]);
            }
            else if (jObj[DDPClient.DDP_PROPS_MESSAGE].ToString() == DDPClient.DDP_MESSAGE_TYPE_READY)
            {
                entity.RequestsIds = ((JArray)jObj[DDPClient.DDP_PROPS_SUBS]).Select(id => id.Value<int>()).ToArray();
                entity.Type = DDPType.Ready;
            }

            _subscriber.DataReceived(entity);
        }

        private dynamic GetMessageData(JObject json)
        {
            var tmp = (JObject)json[DDPClient.DDP_PROPS_FIELDS];
            dynamic entity = GetMessageDataRecursive(tmp);
            entity.Id = json[DDPClient.DDP_PROPS_ID].ToString();
            entity.Collection = json[DDPClient.DDP_PROPS_COLLECTION].ToString();
            
            return entity;
        }

        private dynamic GetMessageDataRecursive(JObject json)
        {
            dynamic entity = new ExpandoObject();
            var entityAsCollection = (IDictionary<string, object>) entity;

            foreach (var item in json)
            {
                string propertyName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Key);
                if (item.Value is JObject) // Property is an object
                    entityAsCollection.Add(propertyName, GetMessageDataRecursive((JObject) item.Value));
                else if (item.Value is JArray) // Property is an array...
                {
                    JArray collection = (JArray) item.Value;
                    if (collection.Count == 0)
                        continue;
                    if (collection[0] is JObject) // ... of objects
                    {
                        var entityCollection =
                            (from JObject colObj in collection select GetMessageDataRecursive(colObj)).ToList();

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
