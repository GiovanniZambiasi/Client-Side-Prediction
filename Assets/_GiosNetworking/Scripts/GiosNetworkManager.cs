using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace ClientSidePrediction
{
    public class GiosNetworkManager : NetworkManager
    {
        void Update()
        {
            if (!isNetworkActive)
                return;
        }
    }
}