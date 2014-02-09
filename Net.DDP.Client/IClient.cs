﻿namespace Net.DDP.Client
{
    public interface IClient
    {
        void AddItem(string jsonItem);
        void Connect(string url);
        void Call(string methodName, string[] args);
        int Subscribe(string methodName, string[] args);
        int GetCurrentRequestId();
    }
}
