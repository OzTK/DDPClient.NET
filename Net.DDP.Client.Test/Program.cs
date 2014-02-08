using System;
using System.Collections.Generic;

namespace Net.DDP.Client.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.ReadLine();
        }
    }

    public class Subscriber:IDataSubscriber
    {
        private IList<String> _ids = new List<String>();

        public void DataReceived(dynamic data)
        {
            try
            {
                if (data.type == "sub")
                {
                    Console.WriteLine(data.prodCode + ": " + data.prodName + ": collection: " + data.collection);
                    _ids.Add((string)data.id);
                }
                else if (data.type == "unset")
                {
                    foreach (string item in _ids)
                    {
                        if (item == data.id)
                            Console.WriteLine("deleleted item with id: " + data.id);
                    }
                }
                else if (data.type == "method")
                    Console.WriteLine(data.result);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
