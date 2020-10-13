using Mirror;

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