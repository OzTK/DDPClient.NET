using System.Collections.Generic;
using System.Globalization;
using System.Dynamic;
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
                dynamic d= GetData(jObj);
                d.type= "sub";
                _subscriber.DataReceived(d);
            }
            else if (jObj["unset"]!=null)
            {
                dynamic entity = new ExpandoObject();
                entity.type="unset";
                entity.id= jObj["id"].ToString();
                _subscriber.DataReceived(entity);
            }
            else if (jObj["result"]!=null)
            {
                dynamic entity = new ExpandoObject();
                entity.type= "method";
                entity.requestingId=jObj["id"].ToString();
                entity.result= jObj["result"].ToString();
                _subscriber.DataReceived(entity);
            }
            else if (jObj["error"] != null)
            {
                dynamic entity = new ExpandoObject();
                entity.type = "error";
                entity.requestingId = jObj["id"].ToString();
                entity.error = jObj["error"].ToString();
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
                entity.type = jObj[DDPClient.DDP_PROPS_MESSAGE].ToString();

                _subscriber.DataReceived(entity);
            }
        }

        private dynamic GetData(JObject json)
        {
            dynamic entity = new ExpandoObject();
            ((IDictionary<string, object>)entity).Add("Id", json[DDPClient.DDP_PROPS_ID].ToString());
            entity.Collection = json[DDPClient.DDP_PROPS_ID].ToString();
            var tmp = (JObject)json[DDPClient.DDP_PROPS_FIELDS];
            foreach (var item in tmp)
                ((IDictionary<string, object>)entity).Add(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(item.Key), item.Value.ToString());

            return entity;
        }
    }
}
